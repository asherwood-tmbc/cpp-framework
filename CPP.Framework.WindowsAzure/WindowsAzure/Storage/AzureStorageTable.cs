using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.Diagnostics;
using CPP.Framework.WindowsAzure.Storage.Entities;
using CPP.Framework.WindowsAzure.Storage.Filters;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;

namespace CPP.Framework.WindowsAzure.Storage
{
    #region QueryCombineOperator Enum Declaration

    /// <summary>
    /// Operators for joining clauses for storage table queries.
    /// </summary>
    public enum QueryCombineOperator
    {
        /// <summary>
        /// Represents an AND join condition.
        /// </summary>
        And,

        /// <summary>
        /// Represents an OR join condition.
        /// </summary>
        Or,
        
        /// <summary>
        /// Represents the NOT join condition.
        /// </summary>
        Not,
    }

    #endregion // QueryCombineOperator Enum Declaration

    #region QueryOperator Enum Declaration

    /// <summary>
    /// Defines the set of comparison operators that may be used for constructing filter conditions.
    /// </summary>
    public enum QueryOperator
    {
        /// <summary>
        /// Represents the Equal operator.
        /// </summary>
        Equal,

        /// <summary>
        /// Represents the Not Equal operator.
        /// </summary>
        NotEqual,
        
        /// <summary>
        /// Represents the Greater Than operator.
        /// </summary>
        GreaterThan,
        
        /// <summary>
        /// Represents the Greater Than or Equal operator.
        /// </summary>
        GreaterThanOrEqual,
        
        /// <summary>
        /// Represents the Less Than operator.
        /// </summary>
        LessThan,
        
        /// <summary>
        /// Represents the Less Than or Equal operator.
        /// </summary>
        LessThanOrEqual,
    }

    #endregion // QueryOperator Enum Declaration

    /// <summary>
    /// Abstract base class for an Azure Storage Table object.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity model.</typeparam>
    public class AzureStorageTable<TEntity> :
        AzureStorageObject
        where TEntity : class, ITableEntity, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageTable{TEntity}"/> class.
        /// </summary>
        /// <param name="account">The <see cref="AzureStorageAccount"/> where the object is stored.</param>
        /// <param name="objectName">The name of the storage object.</param>
        public AzureStorageTable(AzureStorageAccount account, string objectName) : base(account, objectName) { }

        /// <summary>
        /// Gets the name of the Windows Azure Storage table name for the entity.
        /// </summary>
        /// <returns>The storage table name.</returns>
        public static string GetTableName()
        {
            var attribute = typeof(TEntity)
                .GetCustomAttributes<AzureTableNameAttribute>(false, true)
                .FirstOrDefault();
            return ((attribute == null) ? typeof(TEntity).Name : attribute.TableName);
        }

        /// <summary>
        /// Creates a new filter by joining two existing <see cref="AzureTableFilter"/> objects.
        /// </summary>
        /// <param name="filterA">
        /// The <see cref="AzureTableFilter"/> on the left side of the join statement.
        /// </param>
        /// <param name="operator">
        /// A <see cref="CombineOperator"/> value that indicates how to combine 
        /// <paramref name="filterA"/> and <paramref name="filterB"/>.
        /// </param>
        /// <param name="filterB">
        /// The <see cref="AzureTableFilter"/> on the right side of the join statement.
        /// </param>
        /// <returns>A <see cref="TablePropertyFilter"/> object.</returns>
        public AzureTableFilter CreateFilter(AzureTableFilter filterA, CombineOperator @operator, AzureTableFilter filterB)
        {
            ArgumentValidator.ValidateNotNull(() => filterA);
            ArgumentValidator.ValidateNotNull(() => filterB);
            return new TableFilterGroup(filterA, @operator, filterB);
        }

        /// <summary>
        /// Creates a filter against an entity property.
        /// </summary>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being filtered.</param>
        /// <param name="operator">An <see cref="ComparisonOperator"/> value that specifies how filter value is compared.</param>
        /// <param name="value">The value to filter <paramref name="expression"/> against.</param>
        /// <returns>A <see cref="TablePropertyFilter"/> object.</returns>
        public TablePropertyFilter CreateFilter(Expression<Func<TEntity, bool?>> expression, ComparisonOperator @operator, bool value)
        {
            return new BoolPropertyFilter(expression, @operator, value);
        }

        /// <summary>
        /// Creates a filter against an entity property.
        /// </summary>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being filtered.</param>
        /// <param name="operator">An <see cref="ComparisonOperator"/> value that specifies how filter value is compared.</param>
        /// <param name="value">The value to filter <paramref name="expression"/> against.</param>
        /// <returns>A <see cref="TablePropertyFilter"/> object.</returns>
        public TablePropertyFilter CreateFilter(Expression<Func<TEntity, byte[]>> expression, ComparisonOperator @operator, byte[] value)
        {
            return new BinaryPropertyFilter(expression, @operator, value);
        }

        /// <summary>
        /// Creates a filter against an entity property.
        /// </summary>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being filtered.</param>
        /// <param name="operator">An <see cref="ComparisonOperator"/> value that specifies how filter value is compared.</param>
        /// <param name="value">The value to filter <paramref name="expression"/> against.</param>
        /// <returns>A <see cref="TablePropertyFilter"/> object.</returns>
        public TablePropertyFilter CreateFilter(Expression<Func<TEntity, DateTime?>> expression, ComparisonOperator @operator, DateTime value)
        {
            return new DateTimePropertyFilter(expression, @operator, value);
        }

        /// <summary>
        /// Creates a filter against an entity property.
        /// </summary>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being filtered.</param>
        /// <param name="operator">An <see cref="ComparisonOperator"/> value that specifies how filter value is compared.</param>
        /// <param name="value">The value to filter <paramref name="expression"/> against.</param>
        /// <returns>A <see cref="TablePropertyFilter"/> object.</returns>
        public TablePropertyFilter CreateFilter(Expression<Func<TEntity, double?>> expression, ComparisonOperator @operator, double value)
        {
            return new DoublePropertyFilter(expression, @operator, value);
        }

        /// <summary>
        /// Creates a filter against an entity property.
        /// </summary>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being filtered.</param>
        /// <param name="operator">An <see cref="ComparisonOperator"/> value that specifies how filter value is compared.</param>
        /// <param name="value">The value to filter <paramref name="expression"/> against.</param>
        /// <returns>A <see cref="TablePropertyFilter"/> object.</returns>
        public TablePropertyFilter CreateFilter(Expression<Func<TEntity, Enum>> expression, ComparisonOperator @operator, Enum value)
        {
            return new StringPropertyFilter(expression, @operator, value.ToString());
        }

        /// <summary>
        /// Creates a filter against an entity property.
        /// </summary>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being filtered.</param>
        /// <param name="operator">An <see cref="CombineOperator"/> value that specifies how filter value is compared.</param>
        /// <param name="value">The value to filter <paramref name="expression"/> against.</param>
        /// <returns>A <see cref="TablePropertyFilter"/> object.</returns>
        public TablePropertyFilter CreateFilter(Expression<Func<TEntity, Guid?>> expression, ComparisonOperator @operator, Guid value)
        {
            return new GuidPropertyFilter(expression, @operator, value);
        }

        /// <summary>
        /// Creates a filter against an entity property.
        /// </summary>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being filtered.</param>
        /// <param name="operator">An <see cref="CombineOperator"/> value that specifies how filter value is compared.</param>
        /// <param name="value">The value to filter <paramref name="expression"/> against.</param>
        /// <returns>A <see cref="TablePropertyFilter"/> object.</returns>
        public TablePropertyFilter CreateFilter(Expression<Func<TEntity, int?>> expression, ComparisonOperator @operator, int value)
        {
            return new IntPropertyFilter(expression, @operator, value);
        }

        /// <summary>
        /// Creates a filter against an entity property.
        /// </summary>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being filtered.</param>
        /// <param name="operator">An <see cref="CombineOperator"/> value that specifies how filter value is compared.</param>
        /// <param name="value">The value to filter <paramref name="expression"/> against.</param>
        /// <returns>A <see cref="TablePropertyFilter"/> object.</returns>
        public TablePropertyFilter CreateFilter(Expression<Func<TEntity, long?>> expression, ComparisonOperator @operator, long value)
        {
            return new LongPropertyFilter(expression, @operator, value);
        }

        /// <summary>
        /// Creates a filter against an entity property.
        /// </summary>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being filtered.</param>
        /// <param name="operator">An <see cref="CombineOperator"/> value that specifies how filter value is compared.</param>
        /// <param name="value">The value to filter <paramref name="expression"/> against.</param>
        /// <returns>A <see cref="TablePropertyFilter"/> object.</returns>
        public TablePropertyFilter CreateFilter(Expression<Func<TEntity, string>> expression, ComparisonOperator @operator, string value)
        {
            return new StringPropertyFilter(expression, @operator, value);
        }

        /// <summary>
        /// Creates a new request context that includes retries for the optimistic concurrency 
        /// failures.
        /// </summary>
        /// <returns>An <see cref="AzureRequestContext"/> object.</returns>
        private AzureRequestContext CreateOptimisticConcurrencyRequestContext()
        {
            var context = AzureRequestContext.Create(this.RequestOptions);
            context.IncludeHttpStatus(HttpStatusCode.PreconditionFailed);
            return context;
        }

        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <returns>True if the queue exists and was deleted; otherwise, false.</returns>
        public override bool Delete()
        {
            var options = this.RequestOptions.CreateOptions<TableRequestOptions>();
            return this.GetCloudTable().DeleteIfExists(options);
        }

        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override async Task<bool> DeleteAsync(CancellationToken cancellationToken)
        {
            var options = this.RequestOptions.CreateOptions<TableRequestOptions>();
            return await this.GetCloudTable().DeleteIfExistsAsync(options, null, cancellationToken);
        }

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        public virtual void DeleteEntity(TEntity entity)
        {
            ArgumentValidator.ValidateNotNull(() => entity);
            var instance = (entity as AzureTableEntity);
            instance?.InitializeEntityKeys();
            this.Execute(TableOperation.Delete(entity));
        }

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task DeleteEntityAsync(TEntity entity)
        {
            await this.DeleteEntityAsync(entity, CancellationToken.None);
        }

        /// <summary>
        /// Deletes an entity from the table.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => entity);
            var instance = (entity as AzureTableEntity);
            instance?.InitializeEntityKeys();
            await this.ExecuteAsync(TableOperation.Delete(entity), cancellationToken);
        }

        /// <summary>
        /// Executes an operation again the table.
        /// </summary>
        /// <param name="operation">The <see cref="TableOperation"/> to execute.</param>
        /// <returns>A <see cref="TableResult"/> instance.</returns>
        protected TableResult Execute(TableOperation operation)
        {
            try
            {
                var table = this.GetCloudTable();
                var options = this.RequestOptions.CreateOptions<TableRequestOptions>();
                table.CreateIfNotExists(options);
                return table.Execute(operation, options);
            }
            catch (StorageException ex)
            {
                Journal.WriteError(
                    "Azure Storage Failure: {0} ({1})",
                    ex.RequestInformation.ExtendedErrorInformation.ErrorCode,
                    ex.RequestInformation.HttpStatusCode);
                throw;
            }
        }

        /// <summary>
        /// Executes an operation again the table.
        /// </summary>
        /// <param name="operation">The <see cref="TableOperation"/> to execute.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await this.ExecuteAsync(operation, CancellationToken.None);
        }

        /// <summary>
        /// Executes an operation again the table.
        /// </summary>
        /// <param name="operation">The <see cref="TableOperation"/> to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected async Task<TableResult> ExecuteAsync(TableOperation operation, CancellationToken cancellationToken)
        {
            var table = this.GetCloudTable();
            var options = this.RequestOptions.CreateOptions<TableRequestOptions>();
            await table.CreateIfNotExistsAsync(options, null, cancellationToken);
            return await table.ExecuteAsync(operation, options, null, cancellationToken);
        }

        /// <summary>
        /// Creates a reference to a cloud storage table.
        /// </summary>
        /// <returns>A <see cref="CloudTable"/> object.</returns>
        private CloudTable GetCloudTable()
        {
            var account = this.Account.OpenStorageAccount();
            var tclient = account.CreateCloudTableClient();
            var table = tclient.GetTableReference(this.ObjectName);
            return table;
        }

        /// <summary>
        /// Retrieves all of the entities in the table.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> that can be used to iterator over the returned entities.</returns>
        public virtual IEnumerable<TEntity> GetEntities()
        {
            foreach (var entity in this.GetEntities(default(AzureTableFilter))) yield return entity;
        }

        /// <summary>
        /// Retrieves one or more entities bases on their key.
        /// </summary>
        /// <param name="entityKey">
        /// The <see cref="AzureTableEntityKey"/> of the entities to retrieve.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> that can be used to iterator over the returned entities.
        /// </returns>
        public virtual IEnumerable<TEntity> GetEntities(AzureTableEntityKey entityKey)
        {
            ArgumentValidator.ValidateNotNull(() => entityKey);

            var partitionKey = entityKey.GeneratePartitionKey();
            var rowKey = entityKey.GenerateRowKey();
            var iterator = default(IEnumerable<TEntity>);

            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                iterator = this.GetEntities();
            }
            else if (string.IsNullOrWhiteSpace(rowKey))
            {
                var filter = this.CreateFilter(x => x.PartitionKey, ComparisonOperator.Equal, partitionKey);
                iterator = this.GetEntities(filter);
            }
            else
            {
                var entity = this.GetEntity(entityKey);
                iterator = ((entity == null) ? Enumerable.Empty<TEntity>() : new[] { entity });
            }
            foreach (var entity in iterator) yield return entity;
        }

        /// <summary>
        /// Retrieves one or more entities that match a filter condition.
        /// </summary>
        /// <param name="filter">An <see cref="AzureTableFilter"/> object that represents the filter condition.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that can be used to iterator over the returned entities.</returns>
        public virtual IEnumerable<TEntity> GetEntities(AzureTableFilter filter)
        {
            // get a reference to the target table for the query, and create it if it does not 
            // already exist.
            var criteria = filter?.GenerateFilterString();
            var options = this.RequestOptions.CreateOptions<TableRequestOptions>();
            var query = new TableQuery<TEntity>
            {
                FilterString = criteria,
            };
            var table = this.GetCloudTable();
            table.CreateIfNotExists(options);

            var continuation = default(TableContinuationToken);
            do
            {
                // get the next batch of results, and dispose of the previous enumerator
                // (if available).
                var results = table.ExecuteQuerySegmented(query, continuation, options);

                // grab the continuation token for the next bactch, then return the results from 
                // the current response.
                continuation = results.ContinuationToken;
                foreach (var entity in results) yield return entity;
            }
            while (continuation != null);
        }

        /// <summary>
        /// Retrieves one or more entities bases on their key.
        /// </summary>
        /// <param name="entityKey">
        /// The <see cref="AzureTableEntityKey"/> of the entities to retrieve.
        /// </param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task<IEnumerable<TEntity>> GetEntitiesAsync(AzureTableEntityKey entityKey)
        {
            return await this.GetEntitiesAsync(entityKey, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves one or more entities bases on their key.
        /// </summary>
        /// <param name="entityKey">
        /// The <see cref="AzureTableEntityKey"/> of the entities to retrieve.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> object to observe for cancellation requests.
        /// </param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task<IEnumerable<TEntity>> GetEntitiesAsync(AzureTableEntityKey entityKey, CancellationToken cancellationToken)
        {
            var partitionKey = entityKey.GeneratePartitionKey();
            var rowKey = entityKey.GenerateRowKey();
            var iterator = default(IEnumerable<TEntity>);

            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                iterator = await this.GetEntitiesAsync(default(AzureTableFilter), cancellationToken);
            }
            else if (string.IsNullOrWhiteSpace(rowKey))
            {
                var filter = this.CreateFilter(x => x.PartitionKey, ComparisonOperator.Equal, partitionKey);
                iterator = await this.GetEntitiesAsync(filter, cancellationToken);
            }
            else
            {
                var entity = await this.GetEntityAsync(entityKey, cancellationToken);
                iterator = ((entity == null) ? Enumerable.Empty<TEntity>() : new[] { entity });
            }

            return iterator;
        }

        /// <summary>
        /// Retrieves one or more entities that match a filter condition.
        /// </summary>
        /// <param name="filter">A <see cref="AzureTableFilter"/> object that represents the filter condition.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task<IEnumerable<TEntity>> GetEntitiesAsync(AzureTableFilter filter)
        {
            return await this.GetEntitiesAsync(filter, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves one or more entities that match a filter condition.
        /// </summary>
        /// <param name="filter">A <see cref="AzureTableFilter"/> object that represents the filter condition.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task<IEnumerable<TEntity>> GetEntitiesAsync(AzureTableFilter filter, CancellationToken cancellationToken)
        {
            // get a reference to the target table for the query, and create it if it does not 
            // already exist.
            var criteria = filter?.GenerateFilterString();
            var options = this.RequestOptions.CreateOptions<TableRequestOptions>();
            var query = new TableQuery<TEntity>
            {
                FilterString = criteria,
            };
            var table = this.GetCloudTable();
            await table.CreateIfNotExistsAsync(options, null, cancellationToken);

            var continuation = default(TableContinuationToken);
            var results = new List<TEntity>();
            do
            {
                // get the next batch of results, and dispose of the previous enumerator
                // (if available).
                var response = await table.ExecuteQuerySegmentedAsync(query, continuation, options, null, cancellationToken);

                // grab the continuation token for the next bactch, then add the results from the 
                // current response to the output.
                continuation = response.ContinuationToken;
                results.AddRange(response);
            }
            while (continuation != null);

            return results; // return the results to the caller
        }

        /// <summary>
        /// Retrieves an entity from the table based on it's partition and row key.
        /// </summary>
        /// <param name="entityKey">
        /// The <see cref="AzureTableEntityKey"/> of the entity to retrieve.
        /// </param>
        /// <returns>The retrieved entity, or null if a matching entity was not found.</returns>
        public virtual TEntity GetEntity(AzureTableEntityKey entityKey)
        {
            ArgumentValidator.ValidateNotNull(() => entityKey);

            var partitionKey = entityKey.GeneratePartitionKey();
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => entityKey, ErrorStrings.InvalidAzureTablePartitionKey);
            }

            var rowKey = entityKey.GenerateRowKey();
            if (string.IsNullOrWhiteSpace(rowKey))
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => entityKey, ErrorStrings.InvalidAzureTableRowKey);
            }

            var result = this.Execute(TableOperation.Retrieve<TEntity>(partitionKey, rowKey));
            if ((result.HttpStatusCode < 200) || (result.HttpStatusCode >= 400))
            {
                return default(TEntity);
            }
            return (result.Result as TEntity);
        }

        /// <summary>
        /// Retrieves an entity from the table based on it's partition and row key.
        /// </summary>
        /// <param name="partitionKey">The partion key of the target entity.</param>
        /// <param name="rowKey">The row key of the target entity.</param>
        /// <returns>The retrieved entity, or null if a matching entity was not found.</returns>
        [Obsolete("Please use GetEntity(AzureTableEntityKey) instead.", true)]
        public virtual TEntity GetEntity(string partitionKey, string rowKey)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => partitionKey);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => rowKey);
            var result = this.Execute(TableOperation.Retrieve<TEntity>(partitionKey, rowKey));
            if ((result.HttpStatusCode < 200) || (result.HttpStatusCode >= 400))
            {
                return default(TEntity);
            }
            return (result.Result as TEntity);
        }

        /// <summary>
        /// Retrieves an entity from the table based on it's partition and row key.
        /// </summary>
        /// <param name="entityKey">
        /// The <see cref="AzureTableEntityKey"/> of the entity to retrieve.
        /// </param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task<TEntity> GetEntityAsync(AzureTableEntityKey entityKey)
        {
            return (await this.GetEntitiesAsync(entityKey, CancellationToken.None)).SingleOrDefault();
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
        public virtual async Task<TEntity> GetEntityAsync(AzureTableEntityKey entityKey, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => entityKey);

            var partitionKey = entityKey.GeneratePartitionKey();
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => entityKey, ErrorStrings.InvalidAzureTablePartitionKey);
            }

            var rowKey = entityKey.GenerateRowKey();
            if (string.IsNullOrWhiteSpace(rowKey))
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => entityKey, ErrorStrings.InvalidAzureTableRowKey);
            }

            var result = await this.ExecuteAsync(TableOperation.Retrieve<TEntity>(partitionKey, rowKey), cancellationToken);
            if ((result.HttpStatusCode < 200) || (result.HttpStatusCode >= 400))
            {
                return default(TEntity);
            }
            return (result.Result as TEntity);
        }

        /// <summary>
        /// Retrieves an entity from the table based on it's partition and row key.
        /// </summary>
        /// <param name="partitionKey">The partion key of the target entity.</param>
        /// <param name="rowKey">The row key of the target entity.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        [Obsolete("Please use GetEntityAsync(AzureTableEntity) instead.", true)]
        public virtual async Task<TEntity> GetEntityAsync(string partitionKey, string rowKey)
        {
            return await this.GetEntityAsync(partitionKey, rowKey, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves an entity from the table based on it's partition and row key.
        /// </summary>
        /// <param name="partitionKey">The partion key of the target entity.</param>
        /// <param name="rowKey">The row key of the target entity.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        [Obsolete("Please use GetEntitiesAsync(AzureTableEntityKey, CancellationToken) instead.", true)]
        public virtual async Task<TEntity> GetEntityAsync(string partitionKey, string rowKey, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => partitionKey);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => rowKey);
            var result = await this.ExecuteAsync(TableOperation.Retrieve<TEntity>(partitionKey, rowKey), cancellationToken);
            if ((result.HttpStatusCode < 200) || (result.HttpStatusCode >= 400))
            {
                return default(TEntity);
            }
            return (result.Result as TEntity);
        }

        /// <summary>
        /// Retrieves the value of a metadata property associated with the current storage table.
        /// </summary>
        /// <typeparam name="TValue">The data type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property value.</returns>
        public virtual TValue GetTableProperty<TValue>(string propertyName)
            where TValue : struct
        {
            try 
            {
                // the latest version of the code is storing the metadata value using the target
                // data type, so attempt to get the data that way first (most likely sceanario).
                return this.GetTablePropertyCore<TValue>(propertyName);
            }
            catch (StorageException ex) when (ex.InnerException is InvalidOperationException)
            {
                // otherwise, older versions of the code were storing the value as a string and 
                // then coercing the value after the fact, so attempt that as a backup.
                try
                {
                    var value = this.GetTablePropertyCore<string>(propertyName);
                    if (Convert.ChangeType(value, typeof(TValue)) is TValue coerced)
                    {
                        // be sure to set the value using the target type, so that we don't have to
                        // go through this extra work again next time.
                        this.SetTableProperty(propertyName, coerced);
                        return coerced;
                    }
                }
                catch (StorageException se) when (se.InnerException is InvalidOperationException)
                {
                    /* ignored */
                } 
                catch (FormatException)
                {
                    /* ignored */
                }
                catch (InvalidCastException)
                {
                    /* ignored */
                }
                catch (OverflowException)
                {
                    /* ignored */
                }

                // if we got this far, the conversion failed and the requested type is wrong anyway
                throw new InvalidCastException(string.Format(ErrorStrings.InvalidAzureMetadataPropertyValueCast, typeof(TValue).FullName), ex);
            }
        }

        /// <summary>
        /// Retrieves the value of a metadata property associated with the current storage table.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property value.</returns>
        public virtual string GetTableProperty(string propertyName)
        {
            return this.GetTablePropertyCore<string>(propertyName);
        }

        /// <summary>
        /// Retrieves the value of a metadata property associated with the current storage table.
        /// </summary>
        /// <typeparam name="TValue">The data type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The property value.</returns>
        private TValue GetTablePropertyCore<TValue>(string propertyName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => propertyName);
            var table = this.Account.GetStorageTable<MetadataPropertyEntity<TValue>>();
            var entityKey = new MetadataPropertyEntityKey<TValue>(this.ObjectName, propertyName);
            var entity = table.GetEntity(entityKey);

            if (entity == null)
            {
                throw new AzureTablePropertyNotFoundException(this.ObjectName, propertyName);
            }
            return entity.Model.Value;
        }

        /// <summary>
        /// Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        public virtual void InsertEntity(TEntity entity)
        {
            ArgumentValidator.ValidateNotNull(() => entity);
            var instance = (entity as AzureTableEntity);
            instance?.InitializeEntityKeys();
            this.Execute(TableOperation.Insert(entity));
        }

        /// <summary>
        /// Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task InsertEntityAsync(TEntity entity)
        {
            await this.InsertEntityAsync(entity, CancellationToken.None);
        }

        /// <summary>
        /// Inserts a new entity into the table.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task InsertEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => entity);
            var instance = (entity as AzureTableEntity);
            instance?.InitializeEntityKeys();
            await this.ExecuteAsync(TableOperation.Insert(entity), cancellationToken);
        }

        /// <summary>
        /// Inserts a new entity into the table, merging it with any existing entries that match 
        /// the entity id.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        public virtual void InsertOrMergeEntity(TEntity entity)
        {
            ArgumentValidator.ValidateNotNull(() => entity);
            var instance = (entity as AzureTableEntity);
            instance?.InitializeEntityKeys();
            this.Execute(TableOperation.InsertOrMerge(entity));
        }

        /// <summary>
        /// Inserts a new entity into the table, merging it with any existing entries that match 
        /// the entity id.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task InsertOrMergeEntityAsync(TEntity entity)
        {
            await this.InsertOrMergeEntityAsync(entity, CancellationToken.None);
        }

        /// <summary>
        /// Inserts a new entity into the table, merging it with any existing entries that match 
        /// the entity id.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task InsertOrMergeEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => entity);
            var instance = (entity as AzureTableEntity);
            instance?.InitializeEntityKeys();
            await this.ExecuteAsync(TableOperation.InsertOrMerge(entity), cancellationToken);
        }

        /// <summary>
        /// Inserts a new entity into the table, replacing any existing entries that match the
        /// entity id.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        public virtual void InsertOrReplaceEntity(TEntity entity)
        {
            ArgumentValidator.ValidateNotNull(() => entity);
            var instance = (entity as AzureTableEntity);
            instance?.InitializeEntityKeys();
            this.Execute(TableOperation.InsertOrReplace(entity));
        }

        /// <summary>
        /// Inserts a new entity into the table, replacing any existing entries that match the
        /// entity id.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task InsertOrReplaceEntityAsync(TEntity entity)
        {
            await this.InsertOrReplaceEntityAsync(entity, CancellationToken.None);
        }

        /// <summary>
        /// Inserts a new entity into the table, replacing any existing entries that match the
        /// entity id.
        /// </summary>
        /// <param name="entity">The entity to insert.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task InsertOrReplaceEntityAsync(TEntity entity, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => entity);
            var instance = (entity as AzureTableEntity);
            instance?.InitializeEntityKeys();
            await this.ExecuteAsync(TableOperation.InsertOrReplace(entity), cancellationToken);
        }

        /// <summary>
        /// Updates or merges with the value of an existing entity using optimistic concurrency. If
        /// the update fails due to a concurrency failure, the method will use the retry policy set
        /// in options of the <see cref="AzureStorageObject.RequestOptions"/> property to determine
        /// the number of times to retry the operation, starting with the lasted copy of the entity.
        /// </summary>
        /// <param name="entityKey">
        /// The <see cref="AzureTableEntityKey"/> of the entity to update.
        /// </param>
        /// <param name="updateAction">
        /// A delegate that is called to update or replace the entity value. This delegate is 
        /// passed the existing (if present), and should return the updated entity. This allows the
        /// method to handle both inserts and updates, as the value passed will be null on insert.
        /// </param>
        public void OptimisticInsertOrMergeEntity(AzureTableEntityKey entityKey, Func<TEntity, TEntity> updateAction)
        {
            ArgumentValidator.ValidateNotNull(() => entityKey);
            ArgumentValidator.ValidateNotNull(() => updateAction);

            var context = this.CreateOptimisticConcurrencyRequestContext();
            do
            {
                var entity = updateAction(this.GetEntity(entityKey));
                try
                {
                    this.InsertOrMergeEntity(entity);
                    break;
                }
                catch (StorageException ex)
                {
                    if (!context.ShouldRetry(ex)) throw;
                }
            }
            while (true);
        }

        /// <summary>
        /// Updates or merges with the value of an existing entity using optimistic concurrency. If
        /// the update fails due to a concurrency failure, the method will use the retry policy set
        /// in options of the <see cref="AzureStorageObject.RequestOptions"/> property to determine
        /// the number of times to retry the operation, starting with the lasted copy of the entity.
        /// </summary>
        /// <param name="entityKey">
        /// The <see cref="AzureTableEntityKey"/> of the entity to update.
        /// </param>
        /// <param name="updateAction">
        /// A delegate that is called to update or replace the entity value. This delegate is 
        /// passed the existing (if present), and should return the updated entity. This allows the
        /// method to handle both inserts and updates, as the value passed will be null on insert.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> to monitor for cancellation requests for the task.
        /// </param>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the task.</returns>
        public virtual async Task OptimisticInsertOrMergeEntityAsync(AzureTableEntityKey entityKey, Func<TEntity, CancellationToken, Task<TEntity>> updateAction, CancellationToken cancellationToken = default(CancellationToken))
        {
            ArgumentValidator.ValidateNotNull(() => entityKey);
            ArgumentValidator.ValidateNotNull(() => updateAction);

            var context = this.CreateOptimisticConcurrencyRequestContext();
            do
            {
                var entity = await updateAction(await this.GetEntityAsync(entityKey, cancellationToken), cancellationToken);
                try
                {
                    await this.InsertOrMergeEntityAsync(entity, cancellationToken);
                    break;
                }
                catch (StorageException ex)
                {
                    if (!await context.ShouldRetryAsync(ex, cancellationToken)) throw;
                }
            }
            while (true);
        }

        /// <summary>
        /// Updates or replaces the value of an existing entity using optimistic concurrency. If
        /// the update fails due to a concurrency failure, the method will use the retry policy set
        /// in options of the <see cref="AzureStorageObject.RequestOptions"/> property to determine
        /// the number of times to retry the operation, starting with the lasted copy of the entity.
        /// </summary>
        /// <param name="entityKey">
        /// The <see cref="AzureTableEntityKey"/> of the entity to update.
        /// </param>
        /// <param name="updateAction">
        /// A delegate that is called to update or replace the entity value. This delegate is 
        /// passed the existing (if present), and should return the updated entity. This allows the
        /// method to handle both inserts and updates, as the value passed will be null on insert.
        /// </param>
        public void OptimisticInsertOrReplaceEntity(AzureTableEntityKey entityKey, Func<TEntity, TEntity> updateAction)
        {
            ArgumentValidator.ValidateNotNull(() => entityKey);
            ArgumentValidator.ValidateNotNull(() => updateAction);

            var context = this.CreateOptimisticConcurrencyRequestContext();
            do
            {
                var entity = updateAction(this.GetEntity(entityKey));
                try
                {
                    this.InsertOrReplaceEntity(entity);
                    break;
                }
                catch (StorageException ex)
                {
                    if (!context.ShouldRetry(ex)) throw;
                }
            }
            while (true);
        }

        /// <summary>
        /// Updates or replaces the value of an existing entity using optimistic concurrency. If
        /// the update fails due to a concurrency failure, the method will use the retry policy set
        /// in options of the <see cref="AzureStorageObject.RequestOptions"/> property to determine
        /// the number of times to retry the operation, starting with the lasted copy of the entity.
        /// </summary>
        /// <param name="entityKey">
        /// The <see cref="AzureTableEntityKey"/> of the entity to update.
        /// </param>
        /// <param name="updateAction">
        /// A delegate that is called to update or replace the entity value. This delegate is 
        /// passed the existing (if present), and should return the updated entity. This allows the
        /// method to handle both inserts and updates, as the value passed will be null on insert.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> to monitor for cancellation requests for the task.
        /// </param>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the task.</returns>
        public virtual async Task OptimisticInsertOrReplaceEntityAsync(AzureTableEntityKey entityKey, Func<TEntity, CancellationToken, Task<TEntity>> updateAction, CancellationToken cancellationToken = default(CancellationToken))
        {
            ArgumentValidator.ValidateNotNull(() => entityKey);
            ArgumentValidator.ValidateNotNull(() => updateAction);

            var context = this.CreateOptimisticConcurrencyRequestContext();
            do
            {
                var entity = await updateAction(await this.GetEntityAsync(entityKey, cancellationToken), cancellationToken);
                try
                {
                    await this.InsertOrReplaceEntityAsync(entity, cancellationToken);
                    break;
                }
                catch (StorageException ex)
                {
                    if (!await context.ShouldRetryAsync(ex, cancellationToken)) throw;
                }
            }
            while (true);
        }

        /// <summary>
        /// Assigns the value of a metadata property associated with the current storage table.
        /// </summary>
        /// <typeparam name="TValue">The data type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="newValue">The new value for the property.</param>
        public virtual void SetTableProperty<TValue>(string propertyName, TValue newValue)
            where TValue : struct
        {
            this.SetTablePropertyCore(propertyName, newValue);
        }

        /// <summary>
        /// Assigns the value of a metadata property associated with the current storage table.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="newValue">The new value for the property.</param>
        public virtual void SetTableProperty(string propertyName, string newValue)
        {
            this.SetTablePropertyCore(propertyName, (newValue ?? string.Empty));
        }

        /// <summary>
        /// Assigns the value of a metadata property associated with the current storage table.
        /// </summary>
        /// <typeparam name="TValue">The data type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="newValue">The new value for the property.</param>
        private void SetTablePropertyCore<TValue>(string propertyName, TValue newValue)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => propertyName);
            var table = this.Account.GetStorageTable<MetadataPropertyEntity<TValue>>();

            var model = new MetadataPropertyModel<TValue> 
            {
                Name = propertyName,
                Value = newValue,
            };
            var entity = new MetadataPropertyEntity<TValue>(model) { OwnerTableName = this.ObjectName };

            table.InsertOrReplaceEntity(entity);
        }

        /// <summary>
        /// Truncates the contents of the table by deleting and then recreating it.
        /// </summary>
        public virtual void Truncate()
        {
            var table = this.GetCloudTable();
            var options = this.RequestOptions.CreateOptions<TableRequestOptions>();
            table.DeleteIfExists(options);

            var created = false;
            do
            {
                try
                {
                    table.CreateIfNotExists(options);
                    created = true;
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == ((int)HttpStatusCode.Conflict))
                    {
                        var code = ex.RequestInformation.ExtendedErrorInformation.ErrorCode;
                        if (code != TableErrorCodeStrings.TableBeingDeleted) throw;
                    }
                    else throw;
                }
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            while (!created);
        }

        /// <summary>
        /// Truncates the contents of the table by deleting and then recreating it.
        /// </summary>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task TruncateAsync()
        {
            await this.TruncateAsync(CancellationToken.None);
        }

        /// <summary>
        /// Truncates the contents of the table by deleting and then recreating it.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task TruncateAsync(CancellationToken cancellationToken)
        {
            var table = this.GetCloudTable();
            var options = this.RequestOptions.CreateOptions<TableRequestOptions>();
            await table.DeleteIfExistsAsync(options, null, cancellationToken);

            var created = false;
            do
            {
                try
                {
                    await table.CreateIfNotExistsAsync(options, null, cancellationToken);
                    created = true;
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == ((int)HttpStatusCode.Conflict))
                    {
                        var code = ex.RequestInformation.ExtendedErrorInformation.ErrorCode;
                        if (code != TableErrorCodeStrings.TableBeingDeleted) throw;
                    }
                    else throw;
                }
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            while (!created);
        }

        #region MetadataPropertyModel Class Declaration

        /// <summary>
        /// Model used to represent a metadata property value that is associated with an
        /// <see cref="AzureStorageTable{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        internal class MetadataPropertyModel<TValue>
        {
            /// <summary>
            /// Gets or sets the name of the property.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the value of the property.
            /// </summary>
            public TValue Value { get; set; }
        }

        #endregion // MetadataPropertyModel Class Declaration

        #region MetadataPropertyEntity Class Declaration

        /// <summary>
        /// Entity used to represent a metadata property value that is associated with an
        /// <see cref="AzureStorageTable{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        [AzureTableName("SysMetaDataGlobal")]
        internal class MetadataPropertyEntity<TValue> : AzureTableEntity<MetadataPropertyModel<TValue>>
        {
            /// <summary>
            /// The name of the table that owns the metadata.
            /// </summary>
            // ReSharper disable once StaticFieldInGenericType
            private string _ownerTableName;

            /// <summary>
            /// Initializes a new instance of the <see cref="MetadataPropertyEntity{TValue}"/> class.
            /// </summary>
            public MetadataPropertyEntity() : this(new MetadataPropertyModel<TValue>()) { }

            /// <summary>
            /// Initializes a new instance of the <see cref="MetadataPropertyEntity{TValue}"/> class.
            /// </summary>
            /// <param name="model">The object to assign to the entity model.</param>
            public MetadataPropertyEntity(MetadataPropertyModel<TValue> model) : base(model, true)
            {
                _ownerTableName = GetTableName();
            }

            /// <summary>
            /// Gets or sets the name of the table that owns the metadata.
            /// </summary>
            public string OwnerTableName
            {
                get => _ownerTableName;
                set => _ownerTableName = value ?? GetTableName();
            }

            /// <summary>
            /// Called by the base class to create an instance of the <see cref="AzureTableEntityKey"/> 
            /// for the current entity.
            /// </summary>
            /// <returns>An <see cref="AzureTableEntityKey"/> object.</returns>
            protected override AzureTableEntityKey CreateEntityKey()
            {
                return new MetadataPropertyEntityKey<TValue>(this);
            }

            /// <summary>
            /// Called by the base class to load the properties for the model from the values in table 
            /// storage.
            /// </summary>
            /// <param name="serializer">The <see cref="AzureTableEntity{TModel}.PropertySerializer"/> used to store the property values.</param>
            protected override void LoadModelProperties(PropertySerializer serializer)
            {
                base.LoadModelProperties(serializer);
                if (string.IsNullOrWhiteSpace(this.Model.Name))
                {
                    this.Model.Name = ((ITableEntity)this).RowKey;
                }
                OwnerTableName = ((ITableEntity)this).PartitionKey;
            }
        }

        #endregion // MetadataPropertyEntity Class Declaration

        #region MetadataPropertyEntityKey Class Declaration

        /// <summary>
        /// Represents a key value for a <see cref="MetadataPropertyEntity{TValue}"/> object.
        /// </summary>
        /// <typeparam name="TValue">The type of the metatdata value.</typeparam>
        internal class MetadataPropertyEntityKey<TValue> : AzureTableEntityKey
        {
            /// <summary>
            /// The partition key for the entity.
            /// </summary>
            private readonly string _partitionKey;

            /// <summary>
            /// The row key for the entity.
            /// </summary>
            private readonly string _rowKey;

            /// <summary>
            /// Initializes a new instance of the <see cref="MetadataPropertyEntityKey{TValue}"/> 
            /// class.
            /// </summary>
            /// <param name="entity">The entity associated with the key.</param>
            public MetadataPropertyEntityKey(MetadataPropertyEntity<TValue> entity)
            {
                ArgumentValidator.ValidateNotNull(() => entity);
                _partitionKey = entity.OwnerTableName;
                _rowKey = entity.Model.Name;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MetadataPropertyEntityKey{TValue}"/> 
            /// class.
            /// </summary>
            /// <param name="objectName">The name of the table that owns the metadata.</param>
            /// <param name="propertyName">The name of the metadata property.</param>
            public MetadataPropertyEntityKey(string objectName, string propertyName)
            {
                ArgumentValidator.ValidateNotNullOrWhiteSpace(() => objectName);
                ArgumentValidator.ValidateNotNullOrWhiteSpace(() => propertyName);
                _partitionKey = objectName;
                _rowKey = propertyName;
            }

            /// <summary>
            /// Called by the framework to generate the partition key for the entity.
            /// </summary>
            /// <returns>A string that contains a new partition key.</returns>
            protected internal override string GeneratePartitionKey() => _partitionKey;

            /// <summary>
            /// Called by the framework to generate the row key value for the entity.
            /// </summary>
            /// <returns>A string that contains the row key value.</returns>
            protected internal override string GenerateRowKey() => _rowKey;
        }

        #endregion // MetadataPropertyEntityKey Class Declaration
    }
}
