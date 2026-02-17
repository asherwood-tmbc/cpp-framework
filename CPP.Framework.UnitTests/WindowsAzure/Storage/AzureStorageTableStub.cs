using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CPP.Framework.WindowsAzure.Storage.Entities;
using CPP.Framework.WindowsAzure.Storage.Filters;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CPP.Framework.WindowsAzure.Storage
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [ExcludeFromCodeCoverage]
    internal class AzureStorageTableStub<TEntity> : AzureStorageTable<TEntity> where TEntity : class, ITableEntity, new()
    {
        private readonly HashSet<TEntity> _storedEntitySet = new HashSet<TEntity>(new EntityEqualityComparer());

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageTableStub{TEntity}"/> class.
        /// </summary>
        /// <param name="account">The <see cref="AzureStorageAccount"/> where the object is stored.</param>
        public AzureStorageTableStub(AzureStorageAccount account) : base(account, AzureStorageTable<TEntity>.GetTableName()) { }

        /// <summary>
        /// Gets or sets a value indicating whether or not the table exists.
        /// </summary>
        public bool Exists { get; set; }

        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <returns>True if the queue exists and was deleted; otherwise, false.</returns>
        public override bool Delete()
        {
            if (this.Exists)
            {
                this.Exists = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override Task<bool> DeleteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(this.Delete());
        }

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public override void DeleteEntity(TEntity entity)
        {
            InitializeEntityKeys(entity);
            _storedEntitySet.Remove(entity);
        }

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            this.DeleteEntity(entity);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Retrieves all of the entities in the table.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> that can be used to iterator over the returned entities.</returns>
        public override IEnumerable<TEntity> GetEntities()
        {
            return _storedEntitySet;
        }

        /// <summary>
        /// Retrieves one or more entities that match a filter condition.
        /// </summary>
        /// <param name="filter">An <see cref="AzureTableFilter"/> object that represents the filter condition.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that can be used to iterator over the returned entities.</returns>
        public override IEnumerable<TEntity> GetEntities(AzureTableFilter filter)
        {
            foreach (var entity in _storedEntitySet)
            {
                if (ExecuteFilter(entity, filter)) yield return entity;
            }
        }

        /// <summary>
        /// Retrieves one or more entities that match a filter condition.
        /// </summary>
        /// <param name="filter">A <see cref="AzureTableFilter"/> object that represents the filter condition.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override Task<IEnumerable<TEntity>> GetEntitiesAsync(AzureTableFilter filter, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.GetEntities(filter));
        }

        /// <summary>
        /// Retrieves an entity from the table based on it's partition and row key.
        /// </summary>
        /// <param name="entityKey">
        /// The <see cref="AzureTableEntityKey"/> of the entity to retrieve.
        /// </param>
        /// <returns>The retrieved entity, or null if a matching entity was not found.</returns>
        public override TEntity GetEntity(AzureTableEntityKey entityKey)
        {
            ArgumentValidator.ValidateNotNull(() => entityKey);

            var partitionKey = entityKey.GeneratePartitionKey();
            if (string.IsNullOrWhiteSpace(partitionKey)) throw new ArgumentException();
            var rowKey = entityKey.GenerateRowKey();
            if (string.IsNullOrWhiteSpace(rowKey)) throw new ArgumentException();

            var entity = _storedEntitySet
                .Where(obj => (obj.PartitionKey == partitionKey))
                .Where(obj => (obj.RowKey == rowKey))
                .SingleOrDefault();
            return entity;
        }

        /// <summary>
        /// Retrieves an entity from the table based on it's partition and row key.
        /// </summary>
        /// <param name="partitionKey">The partion key of the target entity.</param>
        /// <param name="rowKey">The row key of the target entity.</param>
        /// <returns>The retrieved entity, or null if a matching entity was not found.</returns>
        [Obsolete("Please use GetEntity(AzureTableEntityKey) instead.", true)]
        public override TEntity GetEntity(string partitionKey, string rowKey)
        {
            var entity = _storedEntitySet
                .Where(obj => (obj.PartitionKey == partitionKey))
                .Where(obj => (obj.RowKey == rowKey))
                .FirstOrDefault();
            return entity;
        }

        /// <summary>
        /// Retrieves an entity from the table based on it's partition and row key.
        /// </summary>
        /// <param name="entityKey">
        /// The <see cref="AzureTableEntityKey"/> of the entity to retrieve.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> object to observe for cancellation requests.
        /// </param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override Task<TEntity> GetEntityAsync(AzureTableEntityKey entityKey, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.GetEntity(entityKey));
        }

        /// <summary>
        /// Retrieves an entity from the table based on it's partition and row key.
        /// </summary>
        /// <param name="partitionKey">The partion key of the target entity.</param>
        /// <param name="rowKey">The row key of the target entity.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        [Obsolete("Please use GetEntitiesAsync(AzureTableEntityKey, CancellationToken) instead.", true)]
        public override Task<TEntity> GetEntityAsync(string partitionKey, string rowKey, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.GetEntity(partitionKey, rowKey));
        }

        /// <summary>
        /// Initializes the <see cref="ITableEntity.PartitionKey"/> and 
        /// <see cref="ITableEntity.RowKey"/> values for entities derived from the
        /// <see cref="AzureTableEntity"/> class.
        /// </summary>
        /// <param name="entity">The entity to initialize.</param>
        private static void InitializeEntityKeys(TEntity entity)
        {
            (entity as AzureTableEntity)?.InitializeEntityKeys();
        }

        /// <summary>
        /// Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        public override void InsertEntity(TEntity entity)
        {
            InitializeEntityKeys(entity);
            if (_storedEntitySet.Add(entity))
            {
                return;
            }
            throw new StorageException("The specified key already exists in the collection.");
        }

        /// <summary>
        /// Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override Task InsertEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            this.InsertEntity(entity);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Inserts a new entity into the table, merging it with any existing entries that match 
        /// the entity id.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        public override void InsertOrMergeEntity(TEntity entity)
        {
            ArgumentValidator.ValidateNotNull(() => entity);
            InitializeEntityKeys(entity);
            var existing = this.GetEntity(new DefaultTableEntityKey(entity));

            if (existing != null)
            {
                var properties = new HashSet<PropertyInfo>(existing
                    .GetType()
                    .GetProperties());
                foreach (var pi in entity.GetType().GetProperties())
                {
                    if (!properties.Contains(pi)) continue;
                    var value = pi.GetValue(entity);
                    pi.SetValue(existing, value);
                }
            }
            else _storedEntitySet.Add(entity);
        }

        /// <summary>
        /// Inserts a new entity into the table, merging it with any existing entries that match 
        /// the entity id.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override Task InsertOrMergeEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            this.InsertOrMergeEntity(entity);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Inserts a new entity into the table, replacing any existing entries that match the
        /// entity id.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        public override void InsertOrReplaceEntity(TEntity entity)
        {
            ArgumentValidator.ValidateNotNull(() => entity);
            InitializeEntityKeys(entity);
            if (_storedEntitySet.Contains(entity))
            {
                _storedEntitySet.Remove(entity);
            }
            _storedEntitySet.Add(entity);
        }

        /// <summary>
        /// Inserts a new entity into the table, replacing any existing entries that match the
        /// entity id.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override Task InsertOrReplaceEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            this.InsertOrReplaceEntity(entity);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Truncates the contents of the table by deleting and then recreating it.
        /// </summary>
        public override void Truncate()
        {
            _storedEntitySet.Clear();
        }

        /// <summary>
        /// Truncates the contents of the table by deleting and then recreating it.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override Task TruncateAsync(CancellationToken cancellationToken)
        {
            this.Truncate();
            return Task.FromResult(0);
        }

        #region TableFilter Execution Helpers

        private static bool ExecuteFilter(TEntity entity, AzureTableFilter filter)
        {
            if (filter is TableFilterGroup group)
            {
                return ExecuteFilter(entity, group);
            }
            if (filter is TablePropertyFilter propertyFilter)
            {
                return ExecuteFilter(entity, propertyFilter);
            }
            throw new StorageException();
        }

        private static bool ExecuteFilter(TEntity entity, TableFilterGroup filter)
        {
            var conditions = new[]
            {
                ExecuteFilter(entity, filter.FilterA),
                ExecuteFilter(entity, filter.FilterB),
            };
            switch (filter.Operator)
            {
                case CombineOperator.Not:
                    {
                        conditions[1] = (!conditions[1]);
                    }
                    goto case CombineOperator.And;
                case CombineOperator.And: return (conditions[0] && conditions[1]);
                case CombineOperator.Or: return (conditions[0] || conditions[1]);
                default: throw new InvalidOperationException();
            }
        }

        private static bool ExecuteFilter(TEntity entity, TablePropertyFilter filter)
        {
            if (filter is BinaryPropertyFilter propertyFilter)
            {
                var propertyValue = ((byte[])filter.Property.GetValue(entity));
                var converted = propertyFilter;
                if ((filter.Operator == ComparisonOperator.Equal) || (filter.Operator == ComparisonOperator.NotEqual))
                {
                    if ((converted.FilterValue != null) && (propertyValue != null) &&
                        (converted.FilterValue.Length == propertyValue.Length))
                    {
                        var isOperatorEquals = (filter.Operator == ComparisonOperator.Equal);
                        for (var i = 0; i < propertyValue.Length; i++)
                        {
                            var equals = (propertyValue[i] == converted.FilterValue[i]);
                            if ((!isOperatorEquals) && (!equals)) return true;
                            if ((isOperatorEquals) && (!equals)) return false;
                        }
                        return (isOperatorEquals);
                    }
                    return (filter.Operator == ComparisonOperator.NotEqual);
                }
            }
            else
            {
                if (TryExecuteFilter(entity, (filter as BoolPropertyFilter), out var match)) return match;
                if (TryExecuteFilter(entity, (filter as DateTimePropertyFilter), out match)) return match;
                if (TryExecuteFilter(entity, (filter as DoublePropertyFilter), out match)) return match;
                if (TryExecuteFilter(entity, (filter as GuidPropertyFilter), out match)) return match;
                if (TryExecuteFilter(entity, (filter as IntPropertyFilter), out match)) return match;
                if (TryExecuteFilter(entity, (filter as LongPropertyFilter), out match)) return match;
                if (TryExecuteFilter(entity, (filter as StringPropertyFilter), out match)) return match;
            }
            throw new StorageException();
        }

        private static bool TryExecuteFilter<TValue>(TEntity entity, TablePropertyFilter<TValue> filter, out bool match)
            where TValue : IComparable<TValue>
        {
            match = false;
            if (filter == null) return false;

            var propertyValue = ((TValue)filter.Property.GetValue(entity));
            var comparison = filter.FilterValue.CompareTo(propertyValue);

            if (filter is BoolPropertyFilter)
            {
                switch (filter.Operator)
                {
                    case ComparisonOperator.Equal:
                        {
                            match = (comparison == 0);
                        }
                        break;

                    case ComparisonOperator.NotEqual:
                        {
                            match = (comparison != 0);
                        }
                        break;

                    default: throw new StorageException();
                }
            }
            else
            {
                switch (filter.Operator)
                {
                    case ComparisonOperator.Equal:
                        {
                            match = (comparison == 0);
                        }
                        break;

                    case ComparisonOperator.GreaterThan:
                        {
                            match = (comparison > 0);
                        }
                        break;

                    case ComparisonOperator.GreaterThanOrEqual:
                        {
                            match = (comparison >= 0);
                        }
                        break;

                    case ComparisonOperator.LessThan:
                        {
                            match = (comparison < 0);
                        }
                        break;

                    case ComparisonOperator.LessThanOrEqual:
                        {
                            match = (comparison <= 0);
                        }
                        break;

                    case ComparisonOperator.NotEqual:
                        {
                            match = (comparison != 0);
                        }
                        break;

                    default: throw new StorageException();
                }
            }
            return true;
        }

        #endregion

        #region DefaultTableEntityKey Class Declaration

        private sealed class DefaultTableEntityKey : AzureTableEntityKey
        {
            private readonly string _partitionKey, _rowKey;

            public DefaultTableEntityKey(ITableEntity entity)
            {
                if (entity is AzureTableEntity azure)
                {
                    azure.InitializeEntityKeys();
                }
                _partitionKey = entity.PartitionKey;
                _rowKey = entity.RowKey;
            }

            protected internal override string GeneratePartitionKey() => _partitionKey;

            protected internal override string GenerateRowKey() => _rowKey;
        }

        #endregion // DefaultTableEntityKey Class Declaration
    }
}
