using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.DependencyInjection;
using CPP.Framework.DependencyInjection.Resolvers;
using CPP.Framework.WindowsAzure.Storage;
using CPP.Framework.WindowsAzure.Storage.Entities;

using Microsoft.WindowsAzure.Storage.Table;

using Newtonsoft.Json;

namespace CPP.Framework.WindowsAzure
{
    /// <summary>
    /// Abstrat base class for classes that manage access to Azure Storage Queues whose message 
    /// content is stored using Azure Storage Tables.
    /// </summary>
    /// <typeparam name="TModel">The type of the model for the queue message.</typeparam>
    /// <typeparam name="TEntity">The type of the table entity for <typeparamref name="TModel"/>.</typeparam>
    public abstract class AzureTableQueue<TModel, TEntity> :
        SingletonServiceBase
        where TModel : class, IAzureTableQueueModel
        where TEntity : AzureTableEntity<TModel>, ITableEntity, new()
    {
        /// <summary>
        /// The reference to the <see cref="AzureStorageAccount"/> associated with the queue.
        /// </summary>
        private readonly Lazy<AzureStorageAccount> _storageAccount; 

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableQueue{TModel,TEntity}"/> class.
        /// </summary>
        /// <param name="storageAccountName">The <see cref="AzureStorageAccount"/> to use for the connections to the queue.</param>
        protected AzureTableQueue(string storageAccountName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => storageAccountName);
            _storageAccount = new Lazy<AzureStorageAccount>(
                () =>
                {
                    return ServiceLocator.GetInstance<AzureStorageAccount>(storageAccountName);
                },
                LazyThreadSafetyMode.PublicationOnly);
            this.StorageAccountName = storageAccountName;
        }

        /// <summary>
        /// Gets the name of the storage account used by the current instance.
        /// </summary>
        public string StorageAccountName { get; }

        /// <summary>
        /// Gets the reference to the <see cref="AzureStorageAccount"/> associated with the queue.
        /// </summary>
        protected AzureStorageAccount StorageAccount => _storageAccount.Value;

        /// <summary>
        /// Called by the base class to determine if a message can be delivered or not, or if it
        /// needs to be requeued and handled later.
        /// </summary>
        /// <param name="message">The queue message to check.</param>
        /// <returns>True if the message content can be delivered; otherwise, false if it needs to be requeued.</returns>
        protected virtual bool CanDeliverMessage(QueueMessageInfo message) { return true; }

        /// <summary>
        /// Gets the contents of a message from table storage.
        /// </summary>
        /// <param name="data">The message data received from the queue.</param>
        /// <returns>The model instance associated with message, or null if the message expired or could not be found.</returns>
        protected TModel GetMessageContents(string data)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => data);
            var message = JsonConvert.DeserializeObject<QueueMessageInfo>(data);
            return this.GetMessageContents(message);
        }

        /// <summary>
        /// Gets the contents of a message from table storage.
        /// </summary>
        /// <param name="message">The message data received from the queue.</param>
        /// <returns>The model instance associated with message, or null if the message expired or could not be found.</returns>
        protected internal virtual TModel GetMessageContents(QueueMessageInfo message)
        {
            ArgumentValidator.ValidateNotNull(() => message);

            var table = this.StorageAccount.GetStorageTable<TEntity>();
            var entity = table.GetEntity(message);

            var model = default(TModel);
            if (entity != null)
            {
                model = entity.Model;   // assume success
                if (!this.CanDeliverMessage(message))
                {
                    model = null;
                    this.OnSendQueueMessage(message);
        }
            }
            else throw new InvalidMessageContentException();
            return model;
        }

        /// <summary>
        /// Gets the contents of a message from table storage.
        /// </summary>
        /// <param name="data">The message data received from the queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected async Task<TModel> GetMessageContentsAsync(string data)
        {
            return await this.GetMessageContentsAsync(data, CancellationToken.None);
        }

        /// <summary>
        /// Gets the contents of a message from table storage.
        /// </summary>
        /// <param name="data">The message data received from the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected async Task<TModel> GetMessageContentsAsync(string data, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => data);
            var message = JsonConvert.DeserializeObject<QueueMessageInfo>(data);
            return await this.GetMessageContentsAsync(message, cancellationToken);
        }

        /// <summary>
        /// Gets the contents of a message from table storage.
        /// </summary>
        /// <param name="message">The message data received from the queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected async Task<TModel> GetMessageContentsAsync(QueueMessageInfo message)
        {
            return await this.GetMessageContentsAsync(message, CancellationToken.None);
        }

        /// <summary>
        /// Gets the contents of a message from table storage.
        /// </summary>
        /// <param name="message">The message data received from the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected virtual async Task<TModel> GetMessageContentsAsync(QueueMessageInfo message, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => message);

            var table = this.StorageAccount.GetStorageTable<TEntity>();
            var entity = await table.GetEntityAsync(message, cancellationToken);

            var model = default(TModel);
            if (entity != null)
            {
                model = entity.Model;   // assume success
                if (!this.CanDeliverMessage(message))
                {
                    model = null;
                    await this.OnSendQueueMessageAsync(message, cancellationToken);
                }
            }
            else throw new InvalidMessageContentException();
            return model;
        }

        /// <summary>
        /// Called by the base class to send the actual message to the underlying queue.
        /// </summary>
        /// <param name="message">The message to send to the queue.</param>
        protected abstract void OnSendQueueMessage(QueueMessageInfo message);

        /// <summary>
        /// Called by the base class to send the actual message to the underlying queue.
        /// </summary>
        /// <param name="message">The message to send to the queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected async Task OnSendQueueMessageAsync(QueueMessageInfo message)
        {
            await this.OnSendQueueMessageAsync(message, CancellationToken.None);
        }

        /// <summary>
        /// Called by the base class to send the actual message to the underlying queue.
        /// </summary>
        /// <param name="message">The message to send to the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected abstract Task OnSendQueueMessageAsync(QueueMessageInfo message, CancellationToken cancellationToken);

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="model">The model that contains the details for the queue message.</param>
        protected void ScheduleQueueMessage(TModel model)
        {
            this.ScheduleQueueMessage(model, null);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="model">The model that contains the details for the queue message.</param>
        /// <param name="factory">An optional factory method that is used to generate the <see cref="QueueMessageInfo"/> object that is placed on the actual queue.</param>
        protected internal virtual void ScheduleQueueMessage(TModel model, Func<TEntity, QueueMessageInfo> factory)
        {
            ArgumentValidator.ValidateNotNull(() => model);
            factory = (factory ?? (e => new QueueMessageInfo(e)));

            // create the table entity and the queue message.
            var resolvers = new ServiceResolver[]
            {
                new DependencyResolver(typeof(TModel), model),
            };
            var entity = ServiceLocator.GetInstance<TEntity>(resolvers);
            var message = factory(entity);

            // put the message data in storage, and send the queue message.
            var table = this.StorageAccount.GetStorageTable<TEntity>();
            table.InsertOrReplaceEntity(entity);

            ExceptionDispatchInfo captured = null;
            try
            {
                this.OnSendQueueMessage(message);
            }
            catch (Exception ex)
            {
                captured = ExceptionDispatchInfo.Capture(ex);
            }

            if (captured != null)
            {
                // ReSharper disable EmptyGeneralCatchClause
                try
                {
                    table.DeleteEntity(entity);
                }
                catch
                {
                    /* ignored */
                }
                // ReSharper restore EmptyGeneralCatchClause
                captured.Throw();
            }
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="model">The model that contains the details for the queue message.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected async Task ScheduleQueueMessageAsync(TModel model)
        {
            await this.ScheduleQueueMessageAsync(model, null, CancellationToken.None);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="model">The model that contains the details for the queue message.</param>
        /// <param name="factory">An optional factory method that is used to generate the <see cref="QueueMessageInfo"/> object that is placed on the actual queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected async Task ScheduleQueueMessageAsync(TModel model, Func<TEntity, QueueMessageInfo> factory)
        {
            await this.ScheduleQueueMessageAsync(model, factory, CancellationToken.None);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="model">The model that contains the details for the queue message.</param>
        /// <param name="factory">An optional factory method that is used to generate the <see cref="QueueMessageInfo"/> object that is placed on the actual queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected virtual async Task ScheduleQueueMessageAsync(TModel model, Func<TEntity, QueueMessageInfo> factory, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => model);
            factory = (factory ?? (e => new QueueMessageInfo(e)));

            // create the table entity and the queue message.
            var resolvers = new ServiceResolver[]
            {
                new DependencyResolver(typeof(TModel), model),
            };
            var entity = ServiceLocator.GetInstance<TEntity>(resolvers);
            var message = factory(entity);

            ExceptionDispatchInfo captured = null;
            //// put the message data in storage, and send the queue message.
            var table = this.StorageAccount.GetStorageTable<TEntity>();
            try
            {
                await table.InsertOrReplaceEntityAsync(entity, cancellationToken);
                
                await this.OnSendQueueMessageAsync(message, cancellationToken);
            }
            catch (Exception ex) 
            { 
                captured = ExceptionDispatchInfo.Capture(ex); 
            }

            if (captured != null)
            {
                // ReSharper disable EmptyGeneralCatchClause
                try
                {
                    await table.DeleteEntityAsync(entity, cancellationToken);
                }
                catch
                {
                    /* ignored */
                }
                // ReSharper restore EmptyGeneralCatchClause
                captured.Throw();
            }
        }

        #region QueueMessageInfo Class Declaration

        /// <summary>
        /// Contains the basic information needed to send and receive message content using storage
        /// queue.
        /// </summary>
        protected internal class QueueMessageInfo : AzureTableEntityKey
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="QueueMessageInfo"/> class.
            /// </summary>
            public QueueMessageInfo() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="QueueMessageInfo"/> class.
            /// </summary>
            /// <param name="entity">The table entity to associate with the queue message.</param>
            public QueueMessageInfo(TEntity entity)
            {
                ArgumentValidator.ValidateNotNull(() => entity);

                // check if the model supports scheduling, and if so, get the delivery date.
                this.ScheduledDeliveryDate = (entity.Model.ScheduledDeliveryDate ?? DateTimeService.Current.UtcNow);
                entity.InitializeEntityKeys();
                this.PartitionKey = entity.PartitionKey;
                this.RowKey = entity.RowKey;
            }

            /// <summary>
            /// Gets or sets the partition key of the target table entity.
            /// </summary>
            public string PartitionKey { get; set; }

            /// <summary>
            /// Gets or sets the row key of the target table entity.
            /// </summary>
            public string RowKey { get; set; }

            /// <summary>
            /// Gets or sets the optional scheduled delivery date for the message.
            /// </summary>
            public DateTime ScheduledDeliveryDate { get; set; }

            /// <summary>
            /// Called by the framework to generate the partition key for the entity.
            /// </summary>
            /// <returns>A string that contains a new partition key.</returns>
            protected internal override string GeneratePartitionKey() => this.PartitionKey;

            /// <summary>
            /// Called by the framework to generate the row key value for the entity.
            /// </summary>
            /// <returns>A string that contains the row key value.</returns>
            protected internal override string GenerateRowKey() => this.RowKey;
        }

        #endregion // QueueMessageDetails
    }
}
