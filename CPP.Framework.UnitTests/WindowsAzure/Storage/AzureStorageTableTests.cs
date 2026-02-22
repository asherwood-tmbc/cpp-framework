using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CPP.Framework.DependencyInjection;
using CPP.Framework.UnitTests.Testing;
using CPP.Framework.WindowsAzure.Storage.Entities;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NSubstitute;

namespace CPP.Framework.WindowsAzure.Storage
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1601:PartialElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [ExcludeFromCodeCoverage]
    [TestClass]
    public partial class AzureStorageTableTests
    {
        private static readonly HashSet<ITableEntity> _MetadataValueSet = new HashSet<ITableEntity>(new EntityEqualityComparer());

        [TestInitialize]
        public void OnTestStartup()
        {
        }

        [TestCleanup]
        public void OnTestCleanup()
        {
            _MetadataValueSet.Clear();
            ServiceLocator.Unload();
        }

        private static AzureStorageAccount CreateStorageAccountStub(bool register)
        {
            var account = Substitute.For<AzureStorageAccount>("UseDevelopmentStorage=true", "TestStorageAccount");

            void InitMetadataTable<TValue>()
            {
                var existing = default(AzureStorageTableTests.MetadataPropertyTableStub<SampleEntity, TValue>);
                if (!ServiceLocator.IsRegistered<AzureStorageTableTests.MetadataPropertyTableStub<SampleEntity, TValue>>())
                {
                    existing = new AzureStorageTableTests.MetadataPropertyTableStub<SampleEntity, TValue>(account)
                    {
                        Exists = true,
                    };
                    ServiceLocator.Register(existing);
                }
                else ServiceLocator.TryGetInstance(out existing);

                if (!ServiceLocator.IsRegistered<AzureStorageTable<AzureStorageTable<SampleEntity>.MetadataPropertyEntity<TValue>>>())
                {
                    ServiceLocator.Register<AzureStorageTable<AzureStorageTable<SampleEntity>.MetadataPropertyEntity<TValue>>>(existing);
                    account.GetStorageTable<AzureStorageTable<SampleEntity>.MetadataPropertyEntity<TValue>>().Returns(existing);
                }
            }

            InitMetadataTable<Guid>();
            InitMetadataTable<bool>();
            InitMetadataTable<byte>();
            InitMetadataTable<sbyte>();
            InitMetadataTable<char>();
            InitMetadataTable<decimal>();
            InitMetadataTable<double>();
            InitMetadataTable<float>();
            InitMetadataTable<int>();
            InitMetadataTable<uint>();
            InitMetadataTable<long>();
            InitMetadataTable<ulong>();
            InitMetadataTable<short>();
            InitMetadataTable<ushort>();
            InitMetadataTable<string>();

            var table = new AzureStorageTableStub<SampleEntity>(account)
            {
                Exists = true,
            };
            account.GetStorageTable<SampleEntity>().Returns(table);

            return (register ? account.RegisterServiceStub() : account);
        }

        #region MetadataPropertyTableStub Class Declaration

        // ReSharper disable once PossibleInfiniteInheritance
        private sealed class MetadataPropertyTableStub<TEntity, TValue> :
            AzureStorageTableStub<AzureStorageTable<TEntity>.MetadataPropertyEntity<TValue>>
            where TEntity : class, ITableEntity, new()
        {
            public MetadataPropertyTableStub(AzureStorageAccount account) : base(account) { }

            public override bool Delete()
            {
                if (base.Delete())
                {
                    _MetadataValueSet.Clear();
                    return true;
                }
                return false;
            }

            public override AzureStorageTable<TEntity>.MetadataPropertyEntity<TValue> GetEntity(AzureTableEntityKey entityKey)
            {
                ArgumentValidator.ValidateNotNull(() => entityKey);

                var partitionKey = entityKey.GeneratePartitionKey();
                if (string.IsNullOrWhiteSpace(partitionKey)) throw new ArgumentException();
                var rowKey = entityKey.GenerateRowKey();
                if (string.IsNullOrWhiteSpace(rowKey)) throw new ArgumentException();

                var entity = _MetadataValueSet
                    .Where(obj => (obj.PartitionKey == partitionKey))
                    .Where(obj => (obj.RowKey == rowKey))
                    .SingleOrDefault();
                if (entity is AzureStorageTable<TEntity>.MetadataPropertyEntity<TValue> candidate)
                {
                    return candidate;
                }
                if (entity != null)
                {
                    throw new StorageException(string.Empty, new InvalidOperationException());
                }
                return null;
            }

            [Obsolete("Please use GetEntity(AzureTableEntityKey) instead.", true)]
            public override AzureStorageTable<TEntity>.MetadataPropertyEntity<TValue> GetEntity(string partitionKey, string rowKey)
            {
                var entity = _MetadataValueSet
                    .Where(obj => (obj.PartitionKey == partitionKey))
                    .Where(obj => (obj.RowKey == rowKey))
                    .FirstOrDefault();
                if (entity is AzureStorageTable<TEntity>.MetadataPropertyEntity<TValue> candidate)
                {
                    return candidate;
                }
                if (entity != null)
                {
                    throw new StorageException(string.Empty, new InvalidOperationException());
                }
                return null;
            }

            public override IEnumerable<AzureStorageTable<TEntity>.MetadataPropertyEntity<TValue>> GetEntities()
            {
                return _MetadataValueSet.OfType<AzureStorageTable<TEntity>.MetadataPropertyEntity<TValue>>();
            }

            public override void InsertOrReplaceEntity(AzureStorageTable<TEntity>.MetadataPropertyEntity<TValue> entity)
            {
                if (!_MetadataValueSet.Add(entity))
                {
                    _MetadataValueSet.Remove(entity);
                    _MetadataValueSet.Add(entity);
                }
            }

            public override void Truncate() { _MetadataValueSet.Clear(); }
        }

        #endregion // MetadataPropertyTableStub Class Declaration

        #region SampleEntity Class Declaration

        private sealed class SampleModel
        {
            public SampleModel()
            {
                this.Value = Guid.NewGuid();
            }
            public Guid Value { [UsedImplicitly]get; }
        }

        private sealed class SampleEntity : AzureTableEntity<SampleModel>
        {
            [UsedImplicitly]
            public SampleEntity() : this(new SampleModel()) { }
            public SampleEntity(SampleModel model) : base(model, true) { }
            protected override AzureTableEntityKey CreateEntityKey()
            {
                throw new NotImplementedException();
            }
        }

        #endregion // SampleEntity Class Declaration
    }

    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    internal static class AzureStorageTableTestsExtensions
    {
        internal static AzureStorageTable<TEntity> StubMetadataValue<TEntity, TValue>(this AzureStorageTable<TEntity> table, string propertyName, TValue value)
            where TEntity : class, ITableEntity, new()
        {
            var metadata = ServiceLocator.GetInstance<AzureStorageTable<AzureStorageTable<TEntity>.MetadataPropertyEntity<TValue>>>();
            if (!string.IsNullOrWhiteSpace(propertyName) && (metadata != null))
            {
                var model = new AzureStorageTableStub<TEntity>.MetadataPropertyModel<TValue>()
                {
                    Name = propertyName,
                    Value = value,
                };
                metadata.InsertOrReplaceEntity(new AzureStorageTable<TEntity>.MetadataPropertyEntity<TValue>(model));
            }
            return table;
        }
    }
}
