using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CPP.Framework.WindowsAzure.Storage;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace CPP.Framework.WindowsAzure.ServiceBus.Queue
{
    /// <summary>
    /// Abstract base class for all standard Azure Service Bus queues.
    /// </summary>
    /// <typeparam name="TModel">The base type of the messages sent through the queue.</typeparam>
    public abstract class AzureServiceBusSubscriptionQueue<TModel> : SingletonServiceBase where TModel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusSubscriptionQueue{TModel}"/>
        /// class.
        /// </summary>
        /// <param name="storageAccountName">
        /// The <see cref="AzureStorageAccount"/> to use for the connections to the queue.
        /// </param>
        /// <param name="topicName">
        /// The name of the service bus topic where messages are sent or received.
        /// </param>
        protected AzureServiceBusSubscriptionQueue(string storageAccountName, string topicName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => topicName);
            this.TopicName = topicName;
            this.StorageAccountName = storageAccountName;
        }

        /// <summary>
        /// Gets the name of the storage account associated with the queue.
        /// </summary>
        protected string StorageAccountName { get; }

        /// <summary>
        /// Gets the name of the service bus topic where messages are sent or received.
        /// </summary>
        public string TopicName { get; }

        /// <summary>
        /// Creates a new <see cref="BrokeredMessage"/> instance for the request content.
        /// </summary>
        /// <param name="payload">The content for the message.</param>
        /// <returns>A <see cref="BrokeredMessage"/> object.</returns>
        protected virtual BrokeredMessage CreateBrokeredMessage(TModel payload)
        {
            ArgumentValidator.ValidateNotNull(() => payload);
            var content = JsonConvert.SerializeObject(payload);
            var buffer = Encoding.UTF8.GetBytes(content);
            return new BrokeredMessage(new MemoryStream(buffer), true) { ContentType = AzureServiceBus.JsonContentType };
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        public void Enqueue(TModel request) { this.Enqueue(request, null, null); }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="eventName">The name of the event that triggered the request.</param>
        public void Enqueue(TModel request, string eventName) { this.Enqueue(request, eventName, null); }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="scheduled">The date and time when the message should be delivered to the queue.</param>
        public void Enqueue(TModel request, DateTime? scheduled) { this.Enqueue(request, null, scheduled); }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="eventName">The name of the event that triggered the request.</param>
        /// <param name="scheduled">The date and time when the message should be delivered to the queue.</param>
        public virtual void Enqueue(TModel request, string eventName, DateTime? scheduled)
        {
            ArgumentValidator.ValidateNotNull(() => request);
            using (var account = AzureStorageAccount.GetInstance(this.StorageAccountName))
            {
                eventName = (eventName ?? this.GetRequestEventName(request));
                if (string.IsNullOrWhiteSpace(eventName))
                {
                    eventName = AzureServiceBusTopic.AnyEventName;
                }
                var topic = account.GetServiceBus().GetTopic(this.TopicName);
                topic.SendEventMessage(eventName, request);
            }
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task EnqueueAsync(TModel request)
        {
            await this.EnqueueAsync(request, null, null, CancellationToken.None);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="eventName">The name of the event that triggered the request.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task EnqueueAsync(TModel request, string eventName)
        {
            await this.EnqueueAsync(request, null, null, CancellationToken.None);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="scheduled">The date and time when the message should be delivered to the queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task EnqueueAsync(TModel request, DateTime? scheduled)
        {
            await this.EnqueueAsync(request, null, scheduled, CancellationToken.None);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="eventName">The name of the event that triggered the request.</param>
        /// <param name="scheduled">The date and time when the message should be delivered to the queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task EnqueueAsync(TModel request, string eventName, DateTime? scheduled)
        {
            await this.EnqueueAsync(request, null, scheduled, CancellationToken.None);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task EnqueueAsync(TModel request, CancellationToken cancellationToken)
        {
            await this.EnqueueAsync(request, null, null, cancellationToken);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="eventName">The name of the event that triggered the request.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task EnqueueAsync(TModel request, string eventName, CancellationToken cancellationToken)
        {
            await this.EnqueueAsync(request, eventName, null, cancellationToken);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="scheduled">The date and time when the message should be delivered to the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task EnqueueAsync(TModel request, DateTime? scheduled, CancellationToken cancellationToken)
        {
            await this.EnqueueAsync(request, null, scheduled, cancellationToken);
        }

        /// <summary>
        /// Sends a notification request to the project notification queue.
        /// </summary>
        /// <param name="request">The request details to send to the queue.</param>
        /// <param name="eventName">The name of the event that triggered the request.</param>
        /// <param name="scheduled">The date and time when the message should be delivered to the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task EnqueueAsync(TModel request, string eventName, DateTime? scheduled, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => request);
            using (var account = AzureStorageAccount.GetInstance(this.StorageAccountName))
            {
                eventName = (eventName ?? this.GetRequestEventName(request));
                if (string.IsNullOrWhiteSpace(eventName))
                {
                    eventName = AzureServiceBusTopic.AnyEventName;
                }
                var topic = account.GetServiceBus().GetTopic(this.TopicName);
                var message = this.CreateBrokeredMessage(request);
                await topic.SendMessageAsync(topic.ConfigureMessage(message, eventName, scheduled));
            }
        }

        /// <summary>
        /// Gets the default event name for a given request model.
        /// </summary>
        /// <param name="request">The request model instance.</param>
        /// <returns>A string that contains the event name.</returns>
        protected internal virtual string GetRequestEventName(TModel request)
        {
            var eventName = AzureServiceBusTopic.AnyEventName;
            if (request != null)
            {
                eventName = DefaultEventNameAttribute.GetDefaultEventName(request.GetType());
            }
            return eventName;
        }
    }
}
