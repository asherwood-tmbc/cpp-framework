using System;
using System.Collections.Generic;
using CPP.Framework.WindowsAzure.Storage.Entities;
using Microsoft.WindowsAzure.Storage.Table;

namespace CPP.Framework.WindowsAzure.Storage
{
    internal sealed class EntityEqualityComparer : IEqualityComparer<ITableEntity>
    {
        public bool Equals(ITableEntity x, ITableEntity y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (!ReferenceEquals(null, x) && ReferenceEquals(null, y)) return false;
            if (!ReferenceEquals(null, y) && ReferenceEquals(null, x)) return false;

            InitializeEntityKeys(x);
            InitializeEntityKeys(y);

            return ((x.PartitionKey == y.PartitionKey) && (x.RowKey == y.RowKey));
        }

        public int GetHashCode(ITableEntity entity)
        {
            ArgumentValidator.ValidateNotNull(() => entity);
            InitializeEntityKeys(entity);

            var hash = (entity.PartitionKey ?? String.Empty).GetHashCode();
            hash ^= (hash * 927) ^ (entity.RowKey ?? String.Empty).GetHashCode();
            return hash;
        }

        private static void InitializeEntityKeys(ITableEntity entity) => (entity as AzureTableEntity)?.InitializeEntityKeys();
    }
}
