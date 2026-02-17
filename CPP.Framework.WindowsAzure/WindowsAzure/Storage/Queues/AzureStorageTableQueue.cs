using System;
using System.Threading;
using System.Threading.Tasks;
using CPP.Framework.WindowsAzure.Storage.Entities;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace CPP.Framework.WindowsAzure.Storage.Queues
{
    /// <summary>
    /// Abstract base class for objects that manage access to Azure Storage Queues whose message 
    /// contents are persisted using table storage.
    /// </summary>
    /// <typeparam name="TModel">The type of the model for the queue message.</typeparam>
    /// <typeparam name="TEntity">The type of the table entity for <typeparamref name="TModel"/>.</typeparam>
    public abstract class AzureStorageTableQueue<TModel, TEntity> :
        AzureTableQueue<TModel, TEntity>
        where TModel : class, IAzureTableQueueModel
        where TEntity : AzureTableEntity<TModel>, ITableEntity, new()
    {
        /// <summary>
        /// The name of the storage queue.
        /// </summary>
        private readonly string _queueName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageTableQueue{TModel,TEntity}"/> 
        /// class.
        /// </summary>
        /// <param name="storageAccountName">The <see cref="AzureStorageAccount"/> to use for the connections to the queue.</param>
        /// <param name="queueName">The name of the queue where messages are sent or received.</param>
        protected AzureStorageTableQueue(string storageAccountName, string queueName)
            : base(storageAccountName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => queueName);
            _queueName = queueName;
        }

        /// <summary>
        /// Gets the name of the queue being managed by the current instance.
        /// </summary>
        public string QueueName => this.GetResolvedQueueName();

        /// <summary>
        /// Called by the base class to determine if a message can be delivered or not, or if it
        /// needs to be requeued and handled later.
        /// </summary>
        /// <param name="message">The queue message to check.</param>
        /// <returns>True if the message content can be delivered; otherwise, false if it needs to be requeued.</returns>
        protected override bool CanDeliverMessage(QueueMessageInfo message)
        {
            if (message.ScheduledDeliveryDate > DateTimeService.Current.UtcNow)
            {
                return false;
            }
            return base.CanDeliverMessage(message);
        }

        /// <summary>
        /// Retrieves the contents for a message from the queue.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <returns>The model instance associated with message, or null if the message expired or could not be found.</returns>
        public TModel Dequeue(string message) 
        {
            var result = default(TModel);
            this.ProcessMessage(
                message,
                (model) =>
                {
                    result = model;
                    return true;
                });
            return result;
        }

        /// <summary>
        /// Retrieves the contents for a message from the queue.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task<TModel> DequeueAsync(string message)
        {
            return await this.DequeueAsync(message, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves the contents for a message from the queue.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task<TModel> DequeueAsync(string message, CancellationToken cancellationToken)
        {
            var result = default(TModel);
            await this.ProcessMessageAsync(
                message,
                async (model, token) =>
                {
                    result = model;
                    return await Task.FromResult(true);
                },
                cancellationToken);           
            return result;
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        public void Enqueue(TModel request) { this.ScheduleQueueMessage(request); }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task EnqueueAsync(TModel request)
        {
            await this.ScheduleQueueMessageAsync(request, null, CancellationToken.None);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task EnqueueAsync(TModel request, CancellationToken cancellationToken)
        {
            await this.ScheduleQueueMessageAsync(request, null, cancellationToken);
        }

        /// <summary>
        /// Called by the base class to resolve or expand the queue name, if needed.
        /// </summary>
        /// <returns>The resolved queue name.</returns>
        protected internal virtual string GetResolvedQueueName() { return _queueName; }

        /// <summary>
        /// Called by the base class to send the actual message to the underlying queue.
        /// </summary>
        /// <param name="message">The message to send to the queue.</param>
        protected override void OnSendQueueMessage(QueueMessageInfo message)
        {
            var scheduleAt = message.ScheduledDeliveryDate;
            var maxScheduleDate = DateTimeService.Current.UtcNow.AddDays(7);
            if (message.ScheduledDeliveryDate > maxScheduleDate)
            {
                scheduleAt = maxScheduleDate;
            }
            var queue = this.StorageAccount.GetStorageQueue(this.QueueName);
            queue.AddMessage(message, scheduleAt);
        }

        /// <summary>
        /// Called by the base class to send the actual message to the underlying queue.
        /// </summary>
        /// <param name="message">The message to send to the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected override sealed async Task OnSendQueueMessageAsync(QueueMessageInfo message, CancellationToken cancellationToken)
        {
            var scheduleAt = message.ScheduledDeliveryDate;
            var maxScheduleDate = DateTimeService.Current.UtcNow.AddDays(7);
            if (message.ScheduledDeliveryDate > maxScheduleDate)
            {
                scheduleAt = maxScheduleDate;
            }
            var queue = this.StorageAccount.GetStorageQueue(this.QueueName);
            await queue.AddMessageAsync(message, scheduleAt, cancellationToken);
        }

        /// <summary>
        /// Processes the contents of a message from the queue, and then deletes the message from
        /// the queue if the processing succeeds.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="callback">The delegate to call when the contents of <paramref name="message"/> is ready for processing. The delegate should return true if the message was handled successfully; otherwise, false.</param>
        /// <returns>True if a message was received and handled successfully; otherwise, false.</returns>
        public bool ProcessMessage(string message, Func<TModel, bool> callback)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            ArgumentValidator.ValidateNotNull(() => callback);

            var handled = false; // assume failure
            var cblocal = callback;
            if (message != null)
            {
                var msginfo = default(QueueMessageInfo);
                try
                {
                    msginfo = JsonConvert.DeserializeObject<QueueMessageInfo>(message);
                    var content = this.GetMessageContents(message);
                    handled = ((content != null) && cblocal(content));
                }
                catch (InvalidMessageContentException)
                {
                    return true;
                }

                if (handled && (msginfo != null))
                {
                    // ReSharper disable EmptyGeneralCatchClause
                    try
                    {
                        var table = this.StorageAccount.GetStorageTable<TEntity>();
                        var entity = table.GetEntity(msginfo);
                        if (entity != null) table.DeleteEntity(entity);
                    }
                    catch (Exception)
                    {
                        /* ignored */
                    }
                    // ReSharper restore EmptyGeneralCatchClause
                }
            }
            return handled;
        }

        /// <summary>
        /// Processes the contents of a message from the queue, and then deletes the message from
        /// the queue if the processing succeeds.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="callback">The delegate to call when the contents of <paramref name="message"/> is ready for processing. The delegate should return true if the message was handled successfully; otherwise, false.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task<bool> ProcessMessageAsync(string message, Func<TModel, Task<bool>> callback)
        {
            return await this.ProcessMessageAsync(message, (async (msg, tok) => await callback(msg)), CancellationToken.None);
        }

        /// <summary>
        /// Processes the contents of a message from the queue, and then deletes the message from
        /// the queue if the processing succeeds.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="callback">The delegate to call when the contents of <paramref name="message"/> is ready for processing. The delegate should return true if the message was handled successfully; otherwise, false.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task<bool> ProcessMessageAsync(string message, Func<TModel, CancellationToken, Task<bool>> callback, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            ArgumentValidator.ValidateNotNull(() => callback);

            var handled = false; // assume failure
            var cblocal = callback;
            if (message != null)
            {
                var msginfo = default(QueueMessageInfo);
                try
                {
                    msginfo = JsonConvert.DeserializeObject<QueueMessageInfo>(message);
                    var content = await this.GetMessageContentsAsync(message, cancellationToken);
                    handled = ((content != null) && await cblocal(content, cancellationToken));
                }
                catch (InvalidMessageContentException)
                {
                    return true;
                }

                if (handled && (msginfo != null))
                {
                    // ReSharper disable EmptyGeneralCatchClause
                    try
                    {
                        var table = this.StorageAccount.GetStorageTable<TEntity>();
                        var entity = await table.GetEntityAsync(msginfo, cancellationToken);
                        if (entity != null) await table.DeleteEntityAsync(entity, cancellationToken);
                    }
                    catch (Exception)
                    {
                        /* ignored */
                    }
                    // ReSharper restore EmptyGeneralCatchClause
                }
            }
            return handled;
        }
    }
}
