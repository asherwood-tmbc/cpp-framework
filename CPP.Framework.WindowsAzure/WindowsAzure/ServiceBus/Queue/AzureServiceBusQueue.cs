using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.Configuration;
using CPP.Framework.Threading.Tasks;
using CPP.Framework.WindowsAzure.Storage;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

using Newtonsoft.Json;

namespace CPP.Framework.WindowsAzure.ServiceBus.Queue
{
    /// <inheritdoc cref="AzureServiceBusObject"/>
    /// <summary>
    /// Represents a basic Azure Service Bus Queue that is based on a push/pull model, instead of a 
    /// publish/subscribe model (i.e. Topic/Subscription).
    /// </summary>
    public class AzureServiceBusQueue :
        AzureServiceBusObject,
        ICancellableResource,
        IEquatable<AzureServiceBusQueue>
    {
        /// <summary>
        /// The map of <see cref="AzureServiceBusQueueAttribute"/> instance to content model types.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, AzureServiceBusQueueAttribute> _QueueAttributeMap = new ConcurrentDictionary<Type, AzureServiceBusQueueAttribute>();

        /// <summary>
        /// The countdown event used to detect shutdown for the queue.
        /// </summary>
        private readonly CountdownEvent _handlerCountdownEvent = new CountdownEvent(1);

        /// <summary>
        /// The name of the instance lock file.
        /// </summary>
        private readonly Lazy<string> _lockFileName;

        /// <summary>
        /// The <see cref="QueueClient"/> for the object.
        /// </summary>
        private readonly Lazy<QueueClient> _queueClient;

        /// <summary>
        /// The file lock handle for the instance.
        /// </summary>
        private FileStream _instanceFileLock;

        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusQueue"/> class.
        /// </summary>
        /// <param name="serviceBus">The <see cref="T:CPP.Framework.WindowsAzure.ServiceBus.AzureServiceBus" /> that owns the object.</param>
        /// <param name="queueName">The name of the service bus queue.</param>
        public AzureServiceBusQueue(AzureServiceBus serviceBus, string queueName) : base(serviceBus, queueName)
        {
            ArgumentValidator.ValidateNotNull(() => serviceBus);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => queueName);

            _queueClient = new Lazy<QueueClient>(
                () =>
                {
                    var resolved = this.GetResolvedQueueName();
                    this.CreateIfNotExists();
                    var client = QueueClient.CreateFromConnectionString(serviceBus.ConnectionString, resolved);
                    client.RetryPolicy = RetryPolicy.Default;
                    client.PrefetchCount = ConfigSettingProvider.Current.GetSetting("ServiceBusPrefetchCount", int.Parse, "0");
                    return client;
                },
                LazyThreadSafetyMode.PublicationOnly);
            _lockFileName = new Lazy<string>(this.GetLockFilePath);
            this.CancellationToken = CancellationToken.None;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the <see cref="CancellationToken" /> object to monitor for cancellation
        /// requests.
        /// </summary>
        CancellationToken ICancellableResource.CancellationToken { get => this.CancellationToken; set => this.CancellationToken = value; }

        /// <summary>
        /// Gets or sets the <see cref="CancellationToken" /> object to monitor for cancellation
        /// requests.
        /// </summary>
        protected CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not to use the local queue for debugging.
        /// </summary>
        protected bool UseLocalQueue
        {
            get
            {
                var lockfile = _lockFileName.Value;
                var path = Path.GetDirectoryName(lockfile);
                var file = Path.GetFileName(lockfile);

                if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(file))
                {
                    return false;
                }

                if (Directory.Exists(path))
                {
                    return Directory.GetFiles(path, file).Any();
                }
                return false;
            }
        }

        /// <summary>
        /// Stops receiving new messages from the queue, and waits for any existing handlers to
        /// rollback and abort.
        /// </summary>
        private void Close()
        {
            try
            {
                if (_queueClient.IsValueCreated && (!_queueClient.Value.IsClosed))
                {
                    _queueClient.Value.Close();
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
                /* ignored */
            }
        }

        /// <summary>
        /// Stops receiving new messages from the queue, and waits for any existing handlers to
        /// rollback and abort.
        /// </summary>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the operation.</returns>
        private async Task CloseAsync()
        {
            try
            {
                if (_queueClient.IsValueCreated && (!_queueClient.Value.IsClosed))
                {
                    await _queueClient.Value.CloseAsync();
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
                /* ignored */
            }
        }

        /// <summary>
        /// Configures the metadata necessary to deliver a <see cref="BrokeredMessage"/> to the 
        /// correct handler(s) at the correct date and time.
        /// </summary>
        /// <param name="message">The message to configure.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        /// <returns>A reference to <paramref name="message"/>.</returns>
        internal virtual BrokeredMessage ConfigureMessage(BrokeredMessage message, DateTime? scheduled)
        {
            if (scheduled.HasValue)
            {
                message.ScheduledEnqueueTimeUtc = scheduled.Value;
            }
            return message;
        }

        /// <summary>
        /// Creates a <see cref="BrokeredMessage"/> object for a given value.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The content for message.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        /// <returns>A <see cref="BrokeredMessage"/> object.</returns>
        internal virtual BrokeredMessage CreateBrokeredMessage<TContent>(TContent content, DateTime? scheduled)
        {
            var payload = JsonConvert.SerializeObject(content);
            var buffer = Encoding.UTF8.GetBytes(payload);
            var reader = MessagePropertyReader.GetPropertyReader<TContent>();
            var message = new BrokeredMessage(new MemoryStream(buffer), true)
            {
                ContentType = AzureServiceBus.JsonContentType,
            };
            foreach (var property in reader.GetPropertyValues(content))
            {
                message.Properties[property.Name] = property.Value;
            }
            return ConfigureMessage(message, scheduled);
        }

        /// <inheritdoc />
        /// <summary>
        /// Ensures that the object has been created in the service bus.
        /// </summary>
        /// <returns>True if the object was created successfully; otherwise, false if it already exists.</returns>
        internal override bool CreateIfNotExists()
        {
            var queueName = this.GetResolvedQueueName();
            if (!this.NamespaceManager.QueueExists(queueName))
            {
                var description = this.GenerateDescription(queueName);
                this.NamespaceManager.CreateQueue(description);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        /// <summary>
        /// Ensures that the object has been created in the service bus.
        /// </summary>
        /// <returns>True if the object was created successfully; otherwise, false if it already exists.</returns>
        internal override async Task<bool> CreateIfNotExistsAsync()
        {
            var queueName = this.GetResolvedQueueName();
            if (!await this.NamespaceManager.QueueExistsAsync(queueName))
            {
                var description = this.GenerateDescription(queueName);
                await this.NamespaceManager.CreateQueueAsync(description);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <returns>True if the object exists and was deleted; otherwise, false.</returns>
        public override bool Delete()
        {
            var queueName = this.GetResolvedQueueName();
            if (this.NamespaceManager.QueueExists(queueName))
            {
                this.NamespaceManager.DeleteQueue(queueName);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="T:System.Threading.CancellationToken" /> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> object that represents the asynchronous operation.</returns>
        public override async Task<bool> DeleteAsync(CancellationToken cancellationToken)
        {
            var queueName = this.GetResolvedQueueName();
            if (await this.NamespaceManager.QueueExistsAsync(queueName))
            {
                await this.NamespaceManager.DeleteQueueAsync(queueName);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if the object is being disposed explicitly; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            this.Close();           // shutdown the client first
            var file = Interlocked.Exchange(ref _instanceFileLock, null);
            if (file != null)
            {
                file.Dispose();
                try
                {
                    if (_lockFileName.IsValueCreated && File.Exists(_lockFileName.Value))
                    {
                        File.Delete(_lockFileName.Value);
                    }
                }
                catch { /* we're making our best effort to clean up, but we don't want to fail the process over it */ }
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that" /> parameter; otherwise, false.</returns>
        public override bool Equals(AzureServiceBusObject that)
        {
            return this.Equals(that as AzureServiceBusQueue);
        }

        /// <inheritdoc />
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that" /> parameter; otherwise, false.</returns>
        public virtual bool Equals(AzureServiceBusQueue that)
        {
            if (ReferenceEquals(null, that)) return false;
            if (ReferenceEquals(this, that)) return true;
            return base.Equals(that);
        }

        /// <inheritdoc />
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that" /> parameter; otherwise, false.</returns>
        public override bool Equals(AzureStorageObject that)
        {
            return this.Equals(that as AzureServiceBusQueue);
        }

        /// <summary>
        /// Generates the default <see cref="QueueDescription"/> for the current topic.
        /// </summary>
        /// <param name="queueName">The name of the queue to query.</param>
        /// <returns>
        /// A <see cref="QueueDescription"/> object.
        /// </returns>
        private QueueDescription GenerateDescription(string queueName)
        {
            var description = new QueueDescription(queueName)
            {
                DefaultMessageTimeToLive = TimeSpan.FromDays(14),
                DuplicateDetectionHistoryTimeWindow = TimeSpan.FromSeconds(30),
                EnableBatchedOperations = false,
                MaxSizeInMegabytes = 1024,
                RequiresDuplicateDetection = false,
                EnableExpress = true,
                EnablePartitioning = true,
            };
            return description;
        }

        /// <summary>
        /// Gets the <see cref="AzureServiceBusQueueAttribute"/> instance associated with
        /// a content  model type.
        /// </summary>
        /// <typeparam name="TContent">The type of the content model.</typeparam>
        /// <param name="content">An optional instance of the content model.</param>
        /// <returns>A <see cref="AzureServiceBusQueueAttribute"/> instance.</returns>
        protected static AzureServiceBusQueueAttribute GetContentModelMetadata<TContent>(TContent content)
        {
            var attribute = _QueueAttributeMap.GetOrAdd(
                (content?.GetType() ?? typeof(TContent)),
                (type) =>
                    {
                        var found = type.GetCustomAttributes(typeof(AzureServiceBusQueueAttribute), true)
                            .OfType<AzureServiceBusQueueAttribute>()
                            .SingleOrDefault() ?? throw new ArgumentException(string.Format(ErrorStrings.InvalidAzureQueueMessageMetadata, typeof(TContent)));
                        return found;
                    });
            return attribute;
        }

        /// <summary>
        /// Gets the path to a lock file for the queue.
        /// </summary>
        /// <returns>The fully qualified path to the lock file.</returns>
        protected internal virtual string GetLockFilePath()
        {
            var location = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var filename = string.Format($"{this.ObjectName}.lock");
            return Path.Combine(location, "CPP", "ServiceBus", "Queues", filename);
        }

        /// <summary>
        /// Gets the resolved name of the subscription.
        /// </summary>
        /// <returns>The subscription name.</returns>
        protected string GetResolvedQueueName()
        {
            var queueName = this.ObjectName;
            if (this.UseLocalQueue)
            {
                queueName = AzureServiceBusNameResolver.Current.Resolve(queueName);
            }
            return queueName;
        }

        /// <summary>
        /// Called by the framework to notify the object that a cancellation has been requested, so
        /// that it can perform any addition wait operations or cleanup tasks.
        /// </summary>
        /// <param name="source">
        /// The <see cref="CancellableResourceManager"/> that requested the cancellation.
        /// </param>
        void ICancellableResource.OnCancelRequested(CancellableResourceManager source)
        {
            this.Close();
            this.WaitForMessageHandlers();
        }

        /// <inheritdoc />
        /// <summary>
        /// Called by the framework to notify the object that a cancellation has been requested, so
        /// that it can perform any addition wait operations or cleanup tasks.
        /// </summary>
        /// <param name="source">
        /// The <see cref="CancellableResourceManager"/> that requested the cancellation.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> object that can be used to monitor the progress of the operation.</returns>
        async Task ICancellableResource.OnCancelRequestedAsync(CancellableResourceManager source)
        {
            await this.CloseAsync();
            this.WaitForMessageHandlers();
        }

        /// <summary>
        /// Creates a listener for the service bus queue that asynchronously executes an action for 
        /// each message that is received.
        /// </summary>
        /// <param name="action">The action to execute when a message is received.</param>
        public virtual void OnMessageReceived(Action<BrokeredMessage> action)
        {
            ArgumentValidator.ValidateNotNull(() => action);

            // if we are debugging, then take out a file lock for the subscription so that the code
            // knows to redirect to the local machine service bus subscription instead of using the
            // global one for the environment.
            if (Debugger.IsAttached && (!this.UseLocalQueue))
            {
                var lockfile = _lockFileName.Value;
                var path = Path.GetDirectoryName(lockfile);
                if ((path != null) && (!Directory.Exists(path)))
                {
                    Directory.CreateDirectory(path);
                }
                Interlocked.CompareExchange(
                    ref _instanceFileLock,
                    File.Open(lockfile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete),
                    null);
            }
            this.CreateIfNotExists();   // ensure the the queue exists.

            var options = new OnMessageOptions
            {
                AutoComplete = true,
                AutoRenewTimeout = ConfigSettingProvider.Current.GetSetting("ServiceBusAutoRenewTimeout", TimeSpan.Parse, "3:00:00"),
                MaxConcurrentCalls = ConfigSettingProvider.Current.GetSetting("ServiceBusMaxConcurrentCalls", (s => int.Parse(s)), "16"),
            };
            _queueClient.Value.OnMessage(action, options);
        }

        /// <summary>
        /// Creates a listener for the service bus queue that asynchronously executes an action for 
        /// each message that is received.
        /// </summary>
        /// <typeparam name="TModel">The type of the message model.</typeparam>
        /// <param name="action">
        /// The action to execute when a message is received.
        /// </param>
        public virtual void OnMessageReceived<TModel>(Action<TModel> action)
        {
            ArgumentValidator.ValidateNotNull(() => action);
            this.OnMessageReceived(msg =>
            {
                var model = AzureServiceBus.GetMessageBody<TModel>(msg);
                action(model);
            });
        }

        /// <summary>
        /// Creates a listener for the service bus queue that asynchronously executes an action for 
        /// each message that is received.
        /// </summary>
        /// <param name="action">The action to execute when a message is received.</param>
        public virtual void OnMessageReceivedAsync(Func<BrokeredMessage, CancellationToken, Task> action)
        {
            this.OnMessageReceivedAsync(action, this.CancellationToken);
        }

        /// <summary>
        /// Creates a listener for the service bus queue that asynchronously executes an action for 
        /// each message that is received.
        /// </summary>
        /// <param name="action">The action to execute when a message is received.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that is signaled when the request is cancelled.</param>
        private void OnMessageReceivedAsync(Func<BrokeredMessage, CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => action);

            // if we are debugging, then take out a file lock for the subscription so that the code
            // knows to redirect to the local machine service bus subscription instead of using the
            // global one for the environment.
            if (Debugger.IsAttached && (!this.UseLocalQueue))
            {
                var lockfile = _lockFileName.Value;
                var path = Path.GetDirectoryName(lockfile);
                if ((path != null) && (!Directory.Exists(path)))
                {
                    Directory.CreateDirectory(path);
                }
                Interlocked.CompareExchange(
                    ref _instanceFileLock,
                    File.Open(lockfile, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete),
                    null);
            }
            this.CreateIfNotExists();   // ensure the the queue exists.

            var options = new OnMessageOptions
            {
                AutoComplete = true,
                AutoRenewTimeout = ConfigSettingProvider.Current.GetSetting("ServiceBusAutoRenewTimeout", TimeSpan.Parse, "3:00:00"),
                MaxConcurrentCalls = ConfigSettingProvider.Current.GetSetting("ServiceBusMaxConcurrentCalls", (s => int.Parse(s)), "16"),
            };
            _queueClient.Value.OnMessageAsync(
                async msg =>
                {
                    if (!_handlerCountdownEvent.TryAddCount())
                    {
                        var error = string.Format(
                            ErrorStrings.AzureServiceBusConnectionShutdown,
                            msg.MessageId);
                        throw new InvalidOperationException(error);
                    }
                    try
                    {
                        await action(msg, cancellationToken);
                    }
                    finally
                    {
                        _handlerCountdownEvent.Signal();
                    }
                },
                options);
        }

        /// <summary>
        /// Creates a listener for the service bus queue that asynchronously executes an action for 
        /// each message that is received.
        /// </summary>
        /// <typeparam name="TModel">The type of the message model.</typeparam>
        /// <param name="action">
        /// The action to execute when a message is received.
        /// </param>
        public virtual void OnMessageReceivedAsync<TModel>(Func<TModel, CancellationToken, Task> action)
        {
            ArgumentValidator.ValidateNotNull(() => action);
            this.OnMessageReceivedAsync(
                async (msg, token) =>
                {
                    var model = AzureServiceBus.GetMessageBody<TModel>(msg);
                    await action(model, token);
                },
                this.CancellationToken);
        }

        /// <summary>
        /// Sends a message to the topic.
        /// </summary>
        /// <param name="message">The message to send.</param>
        internal virtual void SendMessage(BrokeredMessage message)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            _queueClient.Value.Send(message);
        }

        /// <summary>
        /// Sends an event message to the topic.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The message content.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        public virtual void SendMessage<TContent>(TContent content, DateTime? scheduled = null)
        {
            var message = this.CreateBrokeredMessage(content, scheduled);
            _queueClient.Value.Send(message);
        }

        /// <summary>
        /// Sends a message to the topic.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the operation.</returns>
        internal virtual async Task SendMessageAsync(BrokeredMessage message)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            await _queueClient.Value.SendAsync(message);
        }

        /// <summary>
        /// Sends an event message to the topic.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The message content.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the operation.</returns>
        public virtual async Task SendMessageAsync<TContent>(TContent content, DateTime? scheduled = null)
        {
            var message = this.CreateBrokeredMessage(content, scheduled);
            await _queueClient.Value.SendAsync(message);
        }

        /// <summary>
        /// Sends an event message to the topic.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The message content.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        /// <param name="existing">
        /// An optional reference to an existing <see cref="AzureStorageAccount"/> session to use
        /// for the model. If this value is null, then a new local session is created instead.
        /// </param>
        public static void SendQueueMessage<TContent>(TContent content, DateTime? scheduled = null, AzureStorageAccount existing = null)
        {
            var account = (existing ?? AzureStorageAccount.GetInstance(content));
            try
            {
                var metadata = GetContentModelMetadata(content);
                using (var serviceBus = account.GetServiceBus())
                using (var queue = serviceBus.GetQueue(metadata.QueueName))
                {
                    queue.SendMessage(content, scheduled);
                }
            }
            finally
            {
                if (existing == null)
                {
                    ((IDisposable)account).Dispose();
                }
            }
        }

        /// <summary>
        /// Sends an event message to the topic.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The message content.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        /// <param name="existing">
        /// An optional reference to an existing <see cref="AzureStorageAccount"/> session to use
        /// for the model. If this value is null, then a new local session is created instead.
        /// </param>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the operation.</returns>
        public static async Task SendQueueMessageAsync<TContent>(TContent content, DateTime? scheduled = null, AzureStorageAccount existing = null)
        {
            var account = (existing ?? AzureStorageAccount.GetInstance(content));
            try
            {
                var metadata = GetContentModelMetadata(content);
                using (var serviceBus = account.GetServiceBus())
                using (var queue = serviceBus.GetQueue(metadata.QueueName))
                {
                    await queue.SendMessageAsync(content, scheduled);
                }
            }
            finally
            {
                if (existing == null)
                {
                    ((IDisposable)account).Dispose();
                }
            }
        }

        /// <summary>
        /// Signals a final shutdown of all the currently executing message handlers, and then 
        /// waits for them to complete.
        /// </summary>
        private void WaitForMessageHandlers()
        {
            try
            {
                _handlerCountdownEvent.Signal();
                _handlerCountdownEvent.Wait(CancellationToken.None);  // wait for any pending handlers to complete
            }
            catch (InvalidOperationException)
            {
                /* ignored */
            }
        }
    }
}
