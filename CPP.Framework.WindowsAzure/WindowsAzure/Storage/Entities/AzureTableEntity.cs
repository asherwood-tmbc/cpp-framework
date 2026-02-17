using System;
using System.Collections.Generic;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace CPP.Framework.WindowsAzure.Storage.Entities
{
    /// <summary>
    /// Represents an entity in table storage.
    /// </summary>
    public abstract class AzureTableEntity : ITableEntity
    {
        /// <summary>
        /// The <see cref="AzureTableEntityKey"/> for the currenty entity.
        /// </summary>
        private AzureTableEntityKey _entityKey;

        /// <summary>
        /// The entity's partition key.
        /// </summary>
        private string _partitionKey;

        /// <summary>
        /// The entity's row key.
        /// </summary>
        private string _rowKey;

        /// <summary>
        /// Gets or sets the entity's current ETag.  Set this value to '*' in order to blindly 
        /// overwrite an entity as part of an update operation.
        /// </summary>
        string ITableEntity.ETag { get; set; }

        /// <summary>
        /// Gets or sets the entity's partition key.
        /// </summary>
        string ITableEntity.PartitionKey
        {
            get => _partitionKey;
            set => _partitionKey = value;
        }

        /// <summary>
        /// Gets or sets the entity's row key.
        /// </summary>
        string ITableEntity.RowKey
        {
            get => _rowKey;
            set => _rowKey = value;
        }

        /// <summary>
        /// Gets or sets the entity's timestamp.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Called by the base class to create an instance of the <see cref="AzureTableEntityKey"/> 
        /// for the current entity.
        /// </summary>
        /// <returns>An <see cref="AzureTableEntityKey"/> object.</returns>
        protected abstract AzureTableEntityKey CreateEntityKey();

        /// <summary>
        /// Gets the <see cref="AzureTableEntityKey"/> for the current entity.
        /// </summary>
        /// <returns>An <see cref="AzureTableEntityKey"/> object.</returns>
        protected internal AzureTableEntityKey GetEntityKey()
        {
            return (_entityKey ?? (_entityKey = this.CreateEntityKey()));
        }

        /// <summary>
        /// Called by the framework to generate the partition key for the entity.
        /// </summary>
        /// <returns>A string that contains a new partition key.</returns>
        [Obsolete("Please use AzureTableEntityKey.GeneratePartitionKey (via AzureTableEntity.CreateEntityKey) instead.", true)]
        protected virtual string GeneratePartitionKey() { throw new NotImplementedException(); }

        /// <summary>
        /// Called by the framework to generate the row key value for the entity.
        /// </summary>
        /// <returns>A string that contains the row key value.</returns>
        [Obsolete("Please use AzureTableEntityKey.GenerateRowKey (via AzureTableEntity.CreateEntityKey) instead.", true)]
        protected virtual string GenerateRowKey() { throw new NotImplementedException(); }

        /// <summary>
        /// Initializes the partition key for the entity.
        /// </summary>
        internal void InitializeEntityKeys()
        {
            var entityKey = this.GetEntityKey();
            if (string.IsNullOrWhiteSpace(_partitionKey))
            {
                var candidate = entityKey.GeneratePartitionKey();
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    candidate = GuidGeneratorService.Current.NewGuid(this).ToString();
                }
                _partitionKey = candidate;
            }
            if (string.IsNullOrWhiteSpace(_rowKey))
            {
                var candidate = entityKey.GenerateRowKey();
                if (string.IsNullOrWhiteSpace(candidate))
                {
                    candidate = GuidGeneratorService.Current.NewGuid(this).ToString();
                }
                _rowKey = candidate;
            }
        }

        /// <summary>
        /// Populates the entity's properties from the <see cref="EntityProperty"/> data values in 
        /// the <paramref name="properties"/> dictionary. 
        /// </summary>
        /// <param name="properties">The dictionary of string property names to <see cref="EntityProperty"/> data values to deserialize and store in this table entity instance.</param>
        /// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
        void ITableEntity.ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            this.ReadEntity(properties, operationContext);
        }

        /// <summary>
        /// Populates the entity's properties from the <see cref="EntityProperty"/> data values in 
        /// the <paramref name="properties"/> dictionary. 
        /// </summary>
        /// <param name="properties">The dictionary of string property names to <see cref="EntityProperty"/> data values to deserialize and store in this table entity instance.</param>
        /// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
        protected virtual void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext) { }

        /// <summary>
        /// Serializes the <see cref="IDictionary{TKey,TValue}"/> of property names mapped to 
        /// <see cref="EntityProperty"/> data values from the entity instance.
        /// </summary>
        /// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> object of property names to <see cref="EntityProperty"/> data typed values created by serializing this table entity instance.</returns>
        IDictionary<string, EntityProperty> ITableEntity.WriteEntity(OperationContext operationContext)
        {
            return this.WriteEntity(operationContext);
        }

        /// <summary>
        /// Serializes the <see cref="IDictionary{TKey,TValue}"/> of property names mapped to 
        /// <see cref="EntityProperty"/> data values from the entity instance.
        /// </summary>
        /// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> object of property names to <see cref="EntityProperty"/> data typed values created by serializing this table entity instance.</returns>
        protected virtual IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return new Dictionary<string, EntityProperty>();
        }
    }
}
