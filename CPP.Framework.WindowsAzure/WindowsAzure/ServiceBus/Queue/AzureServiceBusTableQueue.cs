using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPP.Framework.WindowsAzure.Storage;
using CPP.Framework.WindowsAzure.Storage.Entities;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace CPP.Framework.WindowsAzure.ServiceBus.Queue
{
    /// <summary>
    /// Abstract base class for objects that manage access to Azure Service Bus Queues whose 
    /// message contents are persisted using table storage.
    /// </summary>
    /// <typeparam name="TModel">The type of the model for the queue message.</typeparam>
    /// <typeparam name="TEntity">The type of the table entity for <typeparamref name="TModel"/>.</typeparam>
    public abstract class AzureServiceBusTableQueue<TModel, TEntity> :
        AzureTableQueue<TModel, TEntity>
        where TModel : class, IAzureTableQueueModel
        where TEntity : AzureTableEntity<TModel>, ITableEntity, new()
    {
        /// <summary>
        /// The default name for the container used to store the reference counts for the queue.
        /// </summary>
        private const string StorageContainerName = "sbtqref";

        /// <summary>
        /// The map of event names to their queue handlers.
        /// </summary>
        private readonly ConcurrentDictionary<string, AzureServiceBusTableQueueHandler<TModel, TEntity>> _messageHandlerMap = new ConcurrentDictionary<string, AzureServiceBusTableQueueHandler<TModel, TEntity>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// The topic associated with the queue.
        /// </summary>
        private readonly Lazy<AzureServiceBusTopic> _topic; 

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusTableQueue{TModel,TEntity}"/>
        /// class.
        /// </summary>
        /// <param name="storageAccountName">The <see cref="AzureStorageAccount"/> to use for the connections to the queue.</param>
        /// <param name="topicName">The name of the service bus topic where messages are sent or received.</param>
        /// <param name="eventName">The name of the event for the service bus messages.</param>
        protected AzureServiceBusTableQueue(string storageAccountName, string topicName, string eventName)
            : base(storageAccountName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => topicName);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);            
            _topic = new Lazy<AzureServiceBusTopic>(
                () =>
                {
                    return this.StorageAccount.GetServiceBus().GetTopic(topicName);
                },
                LazyThreadSafetyMode.PublicationOnly);
            this.EventName = eventName;
            this.TopicName = topicName;
        }

        /// <summary>
        /// Gets the name of the default event being monitored for messages.
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Gets a reference to the service bus topic associated with the queue.
        /// </summary>
        protected AzureServiceBusTopic Topic => _topic.Value;

        /// <summary>
        /// Gets the name of the service bus topic where messages are sent or received.
        /// </summary>
        public string TopicName { get; }

        /// <summary>
        /// Sets the initialize reference count for a message model.
        /// </summary>
        /// <param name="content">The content for the message.</param>
        /// <param name="eventName">The name of the event that triggered the message.</param>
        /// <returns>A <see cref="BrokeredMessage"/> object.</returns>
        protected virtual BrokeredMessage CreateBrokeredMessage(QueueMessageInfo content, string eventName)
        {
            var message = this.Topic.CreateBrokeredMessage(content, eventName, content.ScheduledDeliveryDate);
            var subscribers = this.Topic.GetMessageSubscribers(message);
            this.UpdateMessageReferences(content, subscribers, null);
            return message;
        }

        /// <summary>
        /// Sets the initialize reference count for a message model.
        /// </summary>
        /// <param name="content">The content for the message.</param>
        /// <param name="eventName">The name of the event that triggered the message.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected virtual async Task<BrokeredMessage> CreateBrokeredMessageAsync(QueueMessageInfo content, string eventName)
        {
            return await this.CreateBrokeredMessageAsync(content, eventName, CancellationToken.None);
        }

        /// <summary>
        /// Sets the initialize reference count for a message model.
        /// </summary>
        /// <param name="content">The content for the message.</param>
        /// <param name="eventName">The name of the event that triggered the message.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected virtual async Task<BrokeredMessage> CreateBrokeredMessageAsync(QueueMessageInfo content, string eventName, CancellationToken cancellationToken)
        {
            var message = this.Topic.CreateBrokeredMessage(content, eventName, content.ScheduledDeliveryDate);
            var subscribers = this.Topic.GetMessageSubscribers(message);
            await this.UpdateMessageReferencesAsync(content, subscribers, null, cancellationToken);
            return message;
        }

        /// <summary>
        /// Gets the queue message handler for a given service bus subscription.
        /// </summary>
        /// <param name="subscription">The service bus subscription.</param>
        /// <returns>An <see cref="AzureServiceBusTableQueueHandler{TModel,TEntity}"/> object.</returns>
        public AzureServiceBusTableQueueHandler<TModel, TEntity> GetMessageHandler(AzureServiceBusSubscription subscription)
        {
            ArgumentValidator.ValidateNotNull(() => subscription);
            return this.GetMessageHandler(subscription.ObjectName);
        }

        /// <summary>
        /// Gets the queue message handler for a given service bus subscription.
        /// </summary>
        /// <param name="subscriptionName">The name of the service bus subscription.</param>
        /// <returns>An <see cref="AzureServiceBusTableQueueHandler{TModel,TEntity}"/> object.</returns>
        public AzureServiceBusTableQueueHandler<TModel, TEntity> GetMessageHandler(string subscriptionName)
        {
            var handler = _messageHandlerMap.GetOrAdd(
                subscriptionName, 
                (key) => new AzureServiceBusTableQueueHandler<TModel, TEntity>(this, key));
            return handler;
        }

        /// <summary>
        /// Creates a service subscription that references the current queue.
        /// </summary>
        /// <param name="subscriptionName">The name of the subscription.</param>
        /// <returns>An <see cref="AzureServiceBusSubscription"/> object.</returns>
        public virtual AzureServiceBusSubscription CreateSubscription(string subscriptionName)
        {
            return this.Topic.GetSubscription(subscriptionName, this.EventName);
        }

        /// <summary>
        /// Retrieves the contents for a message, and immediately removes it from the service bus 
        /// queue.
        /// </summary>
        /// <param name="subscriptionName">The name of the subscription that received the message.</param>
        /// <param name="message">The message received from the storage queue.</param>
        /// <returns>The contents of the message, or null if the contents are not available.</returns>
        protected internal TModel Dequeue(string subscriptionName, BrokeredMessage message)
        {
            var model = default(TModel);
            this.ProcessMessage(
                subscriptionName,
                message,
                (content) =>
                {
                    model = content;
                    return true;
                });
            return model;
        }

        /// <summary>
        /// Retrieves the contents for a message, and immediately removes it from the service bus 
        /// queue.
        /// </summary>
        /// <param name="subscriptionName">The name of the subscription that received the message.</param>
        /// <param name="message">The message received from the storage queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected internal async Task<TModel> DequeueAsync(string subscriptionName, BrokeredMessage message)
        {
            return await this.DequeueAsync(subscriptionName, message, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves the contents for a message, and immediately removes it from the service bus 
        /// queue.
        /// </summary>
        /// <param name="subscriptionName">The name of the subscription that received the message.</param>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected internal async Task<TModel> DequeueAsync(string subscriptionName, BrokeredMessage message, CancellationToken cancellationToken)
        {
            var model = default(TModel);
            await this.ProcessMessageAsync(
                subscriptionName,
                message,
                async (content, token) =>
                {
                    model = content;
                    return await Task.FromResult(true);
                },
                cancellationToken);
            return model;
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        public void Enqueue(TModel request)
        {
            ArgumentValidator.ValidateNotNull(() => request);
            this.ScheduleQueueMessage(request, (entity => new ServiceBusMessageInfo(entity, this.EventName)));
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task EnqueueAsync(TModel request)
        {
            await this.EnqueueAsync(request, CancellationToken.None);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task EnqueueAsync(TModel request, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => request);
            await this.ScheduleQueueMessageAsync(request, (entity => new ServiceBusMessageInfo(entity, this.EventName)), cancellationToken);
        }

        /// <summary>
        /// Called by the base class to send the actual message to the underlying queue.
        /// </summary>
        /// <param name="message">The message to send to the queue.</param>
        protected override void OnSendQueueMessage(QueueMessageInfo message)
        {
            var eventName = this.EventName;
            if (message is ServiceBusMessageInfo info)
            {
                eventName = info.EventName;
            }
            var brokered = this.CreateBrokeredMessage(message, eventName);
            this.Topic.SendMessage(brokered);
        }

        /// <summary>
        /// Called by the base class to send the actual message to the underlying queue.
        /// </summary>
        /// <param name="message">The message to send to the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected override sealed async Task OnSendQueueMessageAsync(QueueMessageInfo message, CancellationToken cancellationToken)
        {
            var eventName = this.EventName;
            if (message is ServiceBusMessageInfo info)
            {
                eventName = info.EventName;
            }
            var brokered = this.CreateBrokeredMessage(message, eventName);
            await this.Topic.SendMessageAsync(brokered);
        }

        /// <summary>
        /// Processes the contents of a message from the queue, and then deletes the message from
        /// the queue if the processing succeeds.
        /// </summary>
        /// <param name="subscriptionName">The name of the subscription that received the message.</param>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="callback">The delegate to call when the contents of <paramref name="message"/> is ready for processing. The delegate should return true if the message was handled successfully; otherwise, false.</param>
        /// <returns>True if a message was received and handled successfully; otherwise, false.</returns>
        protected internal bool ProcessMessage(string subscriptionName, BrokeredMessage message, Func<TModel, bool> callback)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => subscriptionName);
            ArgumentValidator.ValidateNotNull(() => message);
            ArgumentValidator.ValidateNotNull(() => callback);

            var handled = false; // assume failure
            var cblocal = callback;
            if (message != null)
            {
                var msginfo = default(QueueMessageInfo);
                handled = AzureServiceBusSubscription.ProcessMessage(
                    message,
                    (msg) =>
                    {
                        msginfo = AzureServiceBus.GetMessageBody<QueueMessageInfo>(msg);
                        try
                        {
                            var content = this.GetMessageContents(msginfo);
                            return ((content != null) && cblocal(content));
                        }
                        catch (InvalidMessageContentException)
                        {
                            return true;
                        }
                    });

                if (handled && (msginfo != null))
                {
                    // ReSharper disable EmptyGeneralCatchClause
                    try
                    {
                        var resolved = this.Topic.ResolveSubscriptionName(subscriptionName, this.EventName);
                        if (!this.UpdateMessageReferences(msginfo, null, new[] { resolved }).Any())
                        {
                            var table = this.StorageAccount.GetStorageTable<TEntity>();
                            var entity = table.GetEntity(msginfo);
                            if (entity != null) table.DeleteEntity(entity);
                        }
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
        /// <param name="subscriptionName">The name of the subscription that received the message.</param>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="callback">The delegate to call when the contents of <paramref name="message"/> is ready for processing. The delegate should return true if the message was handled successfully; otherwise, false.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected internal async Task<bool> ProcessMessageAsync(string subscriptionName, BrokeredMessage message, Func<TModel, Task<bool>> callback)
        {
            return await this.ProcessMessageAsync(subscriptionName, message, (async (msg, tok) => await callback(msg)), CancellationToken.None);
        }

        /// <summary>
        /// Processes the contents of a message from the queue, and then deletes the message from
        /// the queue if the processing succeeds.
        /// </summary>
        /// <param name="subscriptionName">The name of the subscription that received the message.</param>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="callback">The delegate to call when the contents of <paramref name="message"/> is ready for processing. The delegate should return true if the message was handled successfully; otherwise, false.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected internal async Task<bool> ProcessMessageAsync(string subscriptionName, BrokeredMessage message, Func<TModel, CancellationToken, Task<bool>> callback, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => subscriptionName);
            ArgumentValidator.ValidateNotNull(() => message);
            ArgumentValidator.ValidateNotNull(() => callback);

            var handled = false; // assume failure
            var cblocal = callback;
            if (message != null)
            {
                var msginfo = default(QueueMessageInfo);
                handled = await AzureServiceBusSubscription.ProcessMessageAsync(
                    message,
                    async (msg, tok) =>
                    {
                        msginfo = AzureServiceBus.GetMessageBody<QueueMessageInfo>(msg);
                        try
                        {
                            var content = await this.GetMessageContentsAsync(msginfo, tok);
                            return ((content != null) && await cblocal(content, cancellationToken));
                        }
                        catch (InvalidMessageContentException)
                        {
                            return true;
                        }
                    },
                    cancellationToken);

                if (handled && (msginfo != null))
                {
                    // ReSharper disable EmptyGeneralCatchClause
                    try
                    {
                        var resolved = this.Topic.ResolveSubscriptionName(subscriptionName, this.EventName);
                        if (!(await this.UpdateMessageReferencesAsync(msginfo, null, new[] { resolved }, cancellationToken)).Any())
                        {
                            var table = this.StorageAccount.GetStorageTable<TEntity>();
                            var entity = await table.GetEntityAsync(msginfo, cancellationToken);
                            if (entity != null) await table.DeleteEntityAsync(entity, cancellationToken);
                        }
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
        /// Updates the subscription references to a queue message.
        /// </summary>
        /// <param name="message">The message to update.</param>
        /// <param name="insert">An optional <see cref="IEnumerable{T}"/> object that contains a sequence of subscription names to insert.</param>
        /// <param name="delete">An optional <see cref="IEnumerable{T}"/> object that contains a sequence of subscription names to delete.</param>
        /// <returns>A <see cref="HashSet{T}"/> that contains the current list of subscription references.</returns>
        protected HashSet<string> UpdateMessageReferences(QueueMessageInfo message, IEnumerable<string> insert, IEnumerable<string> delete)
        {
            ArgumentValidator.ValidateNotNull(() => message);

            var references = new HashSet<string>();
            var file = $"{message.PartitionKey}-{message.RowKey}.json";
            var path = AzureStoragePath.Combine(this.Topic.ObjectName, this.EventName, file);

            using (var blob = this.StorageAccount.GetStorageBlockBlob(StorageContainerName, path))
            {
                using (AzureStorageLease.Acquire(blob))
                using (var stream = new MemoryStream())
                {
                    if (!blob.Exists())
                    {
                        // pull the existing list from the blob in storage.
                        blob.DownloadToStream(stream);
                        var serialized = Encoding.UTF8.GetString(stream.ToArray());
                        if (!string.IsNullOrWhiteSpace(serialized))
                        {
                            var existing = JsonConvert.DeserializeObject<string[]>(serialized);
                            references.UnionWith(existing);
                        }
                    }

                    // now synchronize the list with subscript names, if needed.
                    if ((delete != null) || (insert != null))
                    {
                        if (delete != null) references.ExceptWith(delete);
                        if (insert != null) references.UnionWith(insert);
                    }

                    if (references.Any())
                    {
                        // and now write the updated list back to table storage.
                        var deserilized = JsonConvert.SerializeObject(references.ToArray());
                        var buffer = Encoding.UTF8.GetBytes(deserilized);

                        stream.SetLength(0);
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Seek(0, SeekOrigin.Begin);

                        blob.UpdateFromStream(stream);
                    }
                    else blob.Delete();
                }
            }
            return references;
        }

        /// <summary>
        /// Updates the subscription references to a queue message.
        /// </summary>
        /// <param name="message">The message to update.</param>
        /// <param name="insert">An optional <see cref="IEnumerable{T}"/> object that contains a sequence of subscription names to insert.</param>
        /// <param name="delete">An optional <see cref="IEnumerable{T}"/> object that contains a sequence of subscription names to delete.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected async Task<HashSet<string>> UpdateMessageReferencesAsync(QueueMessageInfo message, IEnumerable<string> insert, IEnumerable<string> delete)
        {
            return await this.UpdateMessageReferencesAsync(message, insert, delete, CancellationToken.None);
        }

        /// <summary>
        /// Updates the subscription references to a queue message.
        /// </summary>
        /// <param name="message">The message to update.</param>
        /// <param name="insert">An optional <see cref="IEnumerable{T}"/> object that contains a sequence of subscription names to insert.</param>
        /// <param name="delete">An optional <see cref="IEnumerable{T}"/> object that contains a sequence of subscription names to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected async Task<HashSet<string>> UpdateMessageReferencesAsync(QueueMessageInfo message, IEnumerable<string> insert, IEnumerable<string> delete, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => message);

            var references = new HashSet<string>();
            var file = $"{message.PartitionKey}-{message.RowKey}.json";
            var path = AzureStoragePath.Combine(this.Topic.ObjectName, this.EventName, file);

            using (var blob = this.StorageAccount.GetStorageBlockBlob(StorageContainerName, path))
            {
                using (await AzureStorageLease.AcquireAsync(blob, cancellationToken))
                using (var stream = new MemoryStream())
                {
                    // pull the existing list from the blob in storage.
                    await blob.DownloadToStreamAsync(stream, cancellationToken);
                    var serialized = Encoding.UTF8.GetString(stream.ToArray());
                    if (!string.IsNullOrWhiteSpace(serialized))
                    {
                        var existing = JsonConvert.DeserializeObject<string[]>(serialized);
                        references.UnionWith(existing);
                    }

                    // now synchronize the list with subscript names, if needed.
                    if ((delete != null) || (insert != null))
                    {
                        if (delete != null) references.ExceptWith(delete);
                        if (insert != null) references.UnionWith(insert);
                    }

                    if (references.Any())
                    {
                        // and now write the updated list back to table storage.
                        var deserilized = JsonConvert.SerializeObject(references.ToArray());
                        var buffer = Encoding.UTF8.GetBytes(deserilized);

                        stream.SetLength(0);
                        stream.Write(buffer, 0, buffer.Length);
                        stream.Seek(0, SeekOrigin.Begin);

                        await blob.UpdateFromStreamAsync(stream, cancellationToken);
                    }
                    else await blob.DeleteAsync(cancellationToken);
                }
            }
            return references;
        }

        #region ServiceBusMessageInfo Clas Declaration

        /// <summary>
        /// Contains the basic information needed to send and receive message content using a 
        /// service bus topic.
        /// </summary>
        protected class ServiceBusMessageInfo : QueueMessageInfo
        {
            /// <summary>
            /// The name of the event for the service bus.
            /// </summary>
            private string _eventName;

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceBusMessageInfo"/> class.
            /// </summary>
            public ServiceBusMessageInfo() { }

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceBusMessageInfo"/> class.
            /// </summary>
            /// <param name="entity">The table entity to associate with the queue message.</param>
            /// <param name="eventName">The name of the event for the service bus message.</param>
            public ServiceBusMessageInfo(TEntity entity, string eventName) : base(entity)
            {
                ArgumentValidator.ValidateNotNull(() => entity);
                ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);
                this.EventName = eventName;
                this.PartitionKey = entity.PartitionKey;
                this.RowKey = entity.RowKey;
            }

            /// <summary>
            /// Gets or sets the name of the event for the service bus message.
            /// </summary>
            public string EventName
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_eventName))
                    {
                        _eventName = AzureServiceBusTopic.AnyEventName;
                    }
                    return _eventName;
                }
                set
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        value = AzureServiceBusTopic.AnyEventName;
                    }
                    _eventName = value;
                }
            }
        }

        #endregion // ServiceBusMessageInfo Clas Declaration
    }
}
