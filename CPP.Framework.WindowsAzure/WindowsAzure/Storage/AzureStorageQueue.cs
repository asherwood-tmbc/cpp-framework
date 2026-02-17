using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Encapsulates operations against a queue in the Windows Azure Cloud.
    /// </summary>
    public class AzureStorageQueue : AzureStorageObject
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="account">The <see cref="AzureStorageAccount"/> where the object is stored.</param>
        /// <param name="objectName">The name of the storage object.</param>
        public AzureStorageQueue(AzureStorageAccount account, string objectName) : base(account, objectName.SafeToLower()) { }

        /// <summary>
        /// Adds a message to the queue.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The message to add to the queue.</param>
        /// <param name="scheduleAt">The optional date and time that the message should be scheduled for delivery, which cannot be more than 7 days in the future. A value of null means that the message is send immediately.</param>
        public virtual void AddMessage<TContent>(TContent content, DateTime? scheduleAt = null) where TContent : class
        {
            ArgumentValidator.ValidateNotNull(() => content);
            if ((scheduleAt != null) && (scheduleAt >= DateTimeService.Current.UtcNow.AddDays(7)))
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => scheduleAt, ErrorStrings.MaxScheduleDateExceeded, scheduleAt);
            }
            var options = this.RequestOptions.CreateOptions<QueueRequestOptions>();
            var queue = this.GetCloudQueue();
            var jsmsg = JsonConvert.SerializeObject(content);

            var now = DateTimeService.Current.UtcNow;
            var delay = default(TimeSpan?);
            if (scheduleAt.HasValue && (scheduleAt.Value >= now))
            {
                delay = (scheduleAt - now);
            }
            queue.CreateIfNotExists(options);
            queue.AddMessage(new CloudQueueMessage(jsmsg), options: options, initialVisibilityDelay: delay);
        }

        /// <summary>
        /// Adds a message to the queue.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The message to add to the queue.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async virtual Task AddMessageAsync<TContent>(TContent content) where TContent : class
        {
            await this.AddMessageAsync(content, null, CancellationToken.None);
        }

        /// <summary>
        /// Adds a message to the queue.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The message to add to the queue.</param>
        /// <param name="scheduleAt">The optional date and time that the message should be scheduled for delivery, which cannot be more than 7 days in the future. A value of null means that the message is send immediately.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async virtual Task AddMessageAsync<TContent>(TContent content, DateTime? scheduleAt) where TContent : class
        {
            await this.AddMessageAsync(content, scheduleAt, CancellationToken.None);
        }

        /// <summary>
        /// Adds a message to the queue.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The message to add to the queue.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <param name="scheduleAt">The optional date and time that the message should be scheduled for delivery, which cannot be more than 7 days in the future. A value of null means that the message is send immediately.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async virtual Task AddMessageAsync<TContent>(TContent content, DateTime? scheduleAt, CancellationToken cancellationToken) where TContent : class
        {
            ArgumentValidator.ValidateNotNull(() => content);
            if ((scheduleAt != null) && (scheduleAt >= DateTimeService.Current.UtcNow.AddDays(7)))
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => scheduleAt, ErrorStrings.MaxScheduleDateExceeded, scheduleAt);
            }
            var options = this.RequestOptions.CreateOptions<QueueRequestOptions>();
            var queue = this.GetCloudQueue();
            var jsmsg = JsonConvert.SerializeObject(content);

            var now = DateTimeService.Current.UtcNow;
            var delay = default(TimeSpan?);
            if (scheduleAt.HasValue && (scheduleAt.Value >= now))
            {
                delay = (scheduleAt - now);
            }
            await queue.CreateIfNotExistsAsync(options, null, cancellationToken);
            await queue.AddMessageAsync(new CloudQueueMessage(jsmsg), null, delay, options, null, cancellationToken);
        }

        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <returns>True if the queue exists and was deleted; otherwise, false.</returns>
        public override bool Delete()
        {
            var options = this.RequestOptions.CreateOptions<QueueRequestOptions>();
            return this.GetCloudQueue().DeleteIfExists(options);
        }

        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async override Task<bool> DeleteAsync(CancellationToken cancellationToken)
        {
            var options = this.RequestOptions.CreateOptions<QueueRequestOptions>();
            return await this.GetCloudQueue().DeleteIfExistsAsync(options, null, cancellationToken);
        }

        /// <summary>
        /// Creates an instance of the underlying Windows Azure queue object.
        /// </summary>
        /// <returns>A <see cref="CloudQueue"/> instance.</returns>
        internal CloudQueue GetCloudQueue()
        {
            var account = (this.Account.UseDevelopmentStorage
                ? CloudStorageAccount.DevelopmentStorageAccount
                : this.Account.OpenStorageAccount());
            var qclient = account.CreateCloudQueueClient();
            return qclient.GetQueueReference(this.ObjectName);
        }

        /// <summary>
        /// Gets the next message from the queue and removes it, if available.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <returns>The message contents, or null if there are no messages available.</returns>
        public virtual TContent GetMessage<TContent>() where TContent : class
        {
            var queue = this.GetCloudQueue();
            var options = this.RequestOptions.CreateOptions<QueueRequestOptions>();
            queue.CreateIfNotExists(options);

            var message = queue.GetMessage(options: options);
            if (message != null)
            {
                var contents = JsonConvert.DeserializeObject<TContent>(message.AsString);
                queue.DeleteMessage(message, options);
                return contents;
            }
            return default(TContent);
        }

        /// <summary>
        /// Gets the next message from the queue and removes it, if available.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async virtual Task<TContent> GetMessageAsync<TContent>() where TContent : class
        {
            return await this.GetMessageAsync<TContent>(CancellationToken.None);
        }

        /// <summary>
        /// Gets the next message from the queue and removes it, if available.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async virtual Task<TContent> GetMessageAsync<TContent>(CancellationToken cancellationToken) where TContent : class
        {
            var queue = this.GetCloudQueue();
            var options = this.RequestOptions.CreateOptions<QueueRequestOptions>();
            await queue.CreateIfNotExistsAsync(options, null, cancellationToken);

            var message = await queue.GetMessageAsync(null, options, null, cancellationToken);
            if (message != null)
            {
                var contents = JsonConvert.DeserializeObject<TContent>(message.AsString);
                await queue.DeleteMessageAsync(message, options, null, cancellationToken);
                return contents;
            }
            return default(TContent);
        }

        /// <summary>
        /// Gets the next message from the queue, if available, but does not remove it.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <returns>The message contents, or null if there are no messages available.</returns>
        public virtual TContent PeekMessage<TContent>() where TContent : class
        {
            var queue = this.GetCloudQueue();
            var options = this.RequestOptions.CreateOptions<QueueRequestOptions>();
            queue.CreateIfNotExists(options);

            var message = queue.PeekMessage(options);
            if (message != null)
            {
                return JsonConvert.DeserializeObject<TContent>(message.AsString);
            }
            return default(TContent);
        }

        /// <summary>
        /// Gets the next message from the queue, if available, but does not remove it.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async virtual Task<TContent> PeekMessageAsync<TContent>() where TContent : class
        {
            return await this.PeekMessageAsync<TContent>(CancellationToken.None);
        }

        /// <summary>
        /// Gets the next message from the queue, if available, but does not remove it.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public async virtual Task<TContent> PeekMessageAsync<TContent>(CancellationToken cancellationToken) where TContent : class
        {
            var queue = this.GetCloudQueue();
            var options = this.RequestOptions.CreateOptions<QueueRequestOptions>();
            await queue.CreateIfNotExistsAsync(options, null, cancellationToken);

            var message = await queue.PeekMessageAsync(options, null, cancellationToken);
            if (message != null)
            {
                return JsonConvert.DeserializeObject<TContent>(message.AsString);
            }
            return default(TContent);
        }
    }
}
