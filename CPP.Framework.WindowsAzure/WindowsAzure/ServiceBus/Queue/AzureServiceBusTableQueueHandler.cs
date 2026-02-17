using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using CPP.Framework.WindowsAzure.Storage.Entities;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Table;

namespace CPP.Framework.WindowsAzure.ServiceBus.Queue
{
    /// <summary>
    /// Helper class used to handle <see cref="AzureServiceBusTableQueue{TModel,TEntity}"/>
    /// messages for a given service bus subscription.
    /// </summary>
    /// <typeparam name="TModel">The type of the model for the queue message.</typeparam>
    /// <typeparam name="TEntity">The type of the table entity for <typeparamref name="TModel"/>.</typeparam>
    [ExcludeFromCodeCoverage]
    public sealed class AzureServiceBusTableQueueHandler<TModel, TEntity>
        where TModel : class, IAzureTableQueueModel
        where TEntity : AzureTableEntity<TModel>, ITableEntity, new()
    {
        private readonly AzureServiceBusTableQueue<TModel, TEntity> _MessageQueue;
        private readonly string _SubscriptionName;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="queue">The queue associated with the target messages.</param>
        /// <param name="subscriptionName">The name of the subscription receiving the messages.</param>
        internal AzureServiceBusTableQueueHandler(AzureServiceBusTableQueue<TModel, TEntity> queue, string subscriptionName)
        {
            ArgumentValidator.ValidateNotNull(() => queue);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => subscriptionName);
            _MessageQueue = queue;
            _SubscriptionName = subscriptionName;
        }

        /// <summary>
        /// Retrieves the contents for a message, and immediately removes it from the service bus 
        /// queue.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <returns>The contents of the message, or null if the contents are not available.</returns>
        public TModel Dequeue(BrokeredMessage message)
        {
            return _MessageQueue.Dequeue(_SubscriptionName, message);
        }

        /// <summary>
        /// Retrieves the contents for a message, and immediately removes it from the service bus 
        /// queue.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task<TModel> DequeueAsync(BrokeredMessage message)
        {
            return await this.DequeueAsync(message, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves the contents for a message, and immediately removes it from the service bus 
        /// queue.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task<TModel> DequeueAsync(BrokeredMessage message, CancellationToken cancellationToken)
        {
            return await _MessageQueue.DequeueAsync(_SubscriptionName, message, cancellationToken);
        }

        /// <summary>
        /// Processes the contents of a message from the queue, and then deletes the message from
        /// the queue if the processing succeeds.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="callback">The delegate to call when the contents of <paramref name="message"/> is ready for processing. The delegate should return true if the message was handled successfully; otherwise, false.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public bool ProcessMessage(BrokeredMessage message, Func<TModel, bool> callback)
        {
            return _MessageQueue.ProcessMessage(_SubscriptionName, message, callback);
        }

        /// <summary>
        /// Processes the contents of a message from the queue, and then deletes the message from
        /// the queue if the processing succeeds.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="callback">The delegate to call when the contents of <paramref name="message"/> is ready for processing. The delegate should return true if the message was handled successfully; otherwise, false.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task<bool> ProcessMessageAsync(BrokeredMessage message, Func<TModel, Task<bool>> callback)
        {
            return await _MessageQueue.ProcessMessageAsync(_SubscriptionName, message, callback);
        }

        /// <summary>
        /// Processes the contents of a message from the queue, and then deletes the message from
        /// the queue if the processing succeeds.
        /// </summary>
        /// <param name="message">The message received from the storage queue.</param>
        /// <param name="callback">The delegate to call when the contents of <paramref name="message"/> is ready for processing. The delegate should return true if the message was handled successfully; otherwise, false.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async Task<bool> ProcessMessageAsync(BrokeredMessage message, Func<TModel, CancellationToken, Task<bool>> callback, CancellationToken cancellationToken)
        {
            return await _MessageQueue.ProcessMessageAsync(_SubscriptionName, message, callback, cancellationToken);
        }
    }
}
