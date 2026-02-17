using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.Configuration;
using CPP.Framework.Threading.Tasks;
using CPP.Framework.WindowsAzure.Storage;

using Microsoft.ServiceBus.Messaging;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    /// <summary>
    /// Represents a subscription to a Windows Azure Service Bus topic.
    /// </summary>
    public class AzureServiceBusSubscription :
        AzureServiceBusObject,
        ICancellableResource,
        IEquatable<AzureServiceBusSubscription>
    {
        /// <summary>
        /// The name of the event property for the <see cref="BrokeredMessage"/> object.
        /// </summary>
        internal const string EventPropertyName = "Event";

        /// <summary>
        /// The name of the target maching property for the <see cref="BrokeredMessage"/> object.
        /// </summary>
        internal const string TargetMachinePropertyName = "TargetMachine";

        /// <summary>
        /// The name of the default filtering rule for subscriptions to the topic.
        /// </summary>
        internal const string DefaultRuleName = "DefaultRule";

        /// <summary>
        /// The countdown event used to detect shutdown for the queue.
        /// </summary>
        private readonly CountdownEvent _handlerCountdownEvent = new CountdownEvent(1);

        /// <summary>
        /// The name of the lock file.
        /// </summary>
        private readonly string _lockFileName;

        /// <summary>
        /// The <see cref="SubscriptionClient"/> instance.
        /// </summary>
        private readonly Lazy<SubscriptionClient> _subscriptionClient;

        /// <summary>
        /// The lock file handle for the current instance.
        /// </summary>
        private FileStream _instanceFileLock;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusSubscription"/> class.
        /// </summary>
        /// <param name="topic">The <see cref="AzureServiceBusTopic"/> that is the target of the subscription.</param>
        /// <param name="subscriptionName">The name of the subscription.</param>
        /// <param name="eventName">The name of the event that are handled by the subscription.</param>
        public AzureServiceBusSubscription(AzureServiceBusTopic topic, string subscriptionName, string eventName)
            : base(GetServiceBus(topic), subscriptionName)
        {
            ArgumentValidator.ValidateNotNull(() => topic);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => subscriptionName);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);

            var targetEvent = eventName;
            if (string.IsNullOrWhiteSpace(targetEvent) || (targetEvent == AzureServiceBusTopic.AnyEventName))
            {
                this.EventName = AzureServiceBusTopic.AnyEventName;
                targetEvent = Guid.NewGuid().ToString("N");
            }
            else targetEvent = eventName;

            this.EventName = eventName;
            this.Topic = topic;
            _lockFileName = topic.GetLockFilePath(subscriptionName, targetEvent);

            _subscriptionClient = new Lazy<SubscriptionClient>(
                () =>
                {
                    var subscription = SubscriptionClient.CreateFromConnectionString(
                        this.ServiceBus.ConnectionString,
                        this.Topic.ObjectName,
                        this.GetResolvedSubscriptionName());
                    return subscription;
                },
                LazyThreadSafetyMode.PublicationOnly);
            this.CancellationToken = CancellationToken.None;
        }

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the <see cref="CancellationToken" /> object to monitor for cancellation
        /// requests.
        /// </summary>
        CancellationToken ICancellableResource.CancellationToken
        {
            get => this.CancellationToken;
            set => this.CancellationToken = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="CancellationToken" /> object to monitor for cancellation
        /// requests.
        /// </summary>
        protected CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Gets the name of the events that are handled by the subscription.
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Gets the <see cref="AzureServiceBusTopic"/> associated with the subscription.
        /// </summary>
        public AzureServiceBusTopic Topic { get; }

        /// <summary>
        /// Gets a value indicating whether or not to use the local subscription for debugging.
        /// </summary>
        protected bool UseLocalSubscription
        {
            get
            {
                var lockfile = _lockFileName;
                if (this.EventName == AzureServiceBusTopic.AnyEventName)
                {
                    lockfile = this.Topic.GetLockFilePath(this.ObjectName, this.EventName);
                }
                var path = Path.GetDirectoryName(lockfile);
                var file = Path.GetFileName(lockfile);

                if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(file))
                {
                    return false;
                }

                if (!Directory.Exists(path))
                    return false;
                return Directory.GetFiles(path, file).Any();
            }
        }

        /// <summary>
        /// Automatically renews the lock for a <see cref="BrokeredMessage"/> in a loop until 
        /// cancelled.
        /// </summary>
        /// <param name="message">The message to automatically renew.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation.</param>
        /// <returns>True if the loop executed successfully; otherwise, false if the lock timed out at some point before it could be renewed.</returns>
        internal static async Task<bool> AutoRenewLockAsync(BrokeredMessage message, CancellationToken cancellationToken)
        {
            while (true)
            {
                // if the lock has already expired, then exit.
                var now = DateTimeService.Current.UtcNow;
                if (message.LockedUntilUtc <= now) return false;

                // wait until the next renewal period.
                var timeout = unchecked((int)((message.LockedUntilUtc - now).TotalMilliseconds / 2));
                try
                {
                    await Task.Delay(timeout, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                // attempt to renew the lock.
                if (cancellationToken.IsCancellationRequested) break;
                try
                {
                    await message.RenewLockAsync();
                }
                catch (MessageLockLostException)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Stops receiving new messages from the queue, and waits for any existing handlers to
        /// rollback and abort.
        /// </summary>
        private void Close()
        {
            try
            {
                if (_subscriptionClient.IsValueCreated && (!_subscriptionClient.Value.IsClosed))
                {
                    _subscriptionClient.Value.Close();
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
                if (_subscriptionClient.IsValueCreated && (!_subscriptionClient.Value.IsClosed))
                {
                    await _subscriptionClient.Value.CloseAsync();
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
                /* ignored */
            }
        }

        /// <summary>
        /// Ensures that the object has been created in the service bus.
        /// </summary>
        /// <returns>True if the object was created successfully; otherwise, false if it already exists.</returns>
        internal override bool CreateIfNotExists()
        {
            this.Topic.CreateIfNotExists(); // always ensure that the topic has been created first

            // setup the filter rules for the subscription first.
            var filter = new SqlFilter(this.GenerateEventFilterRule());
            var defaultRule = new RuleDescription(DefaultRuleName, filter);

            // now create the subscription if it doesn't already exist.
            var subscriptionName = this.GetResolvedSubscriptionName();
            if (!this.ServiceBus.NamespaceManager.SubscriptionExists(this.Topic.ObjectName, subscriptionName))
            {
                this.ServiceBus.NamespaceManager.CreateSubscription(this.Topic.ObjectName, subscriptionName, defaultRule);
                return true;
            }


            return false;
        }

        /// <summary>
        /// Ensures that the object has been created in the service bus.
        /// </summary>
        /// <returns>True if the object was created successfully; otherwise, false if it already exists.</returns>
        internal override async Task<bool> CreateIfNotExistsAsync()
        {
            await this.Topic.CreateIfNotExistsAsync(); // always ensure that the topic has been created first

            // setup the filter rules for the subscription first.
            var filter = new SqlFilter(this.GenerateEventFilterRule());
            var defaultRule = new RuleDescription(DefaultRuleName, filter);

            // now create the subscription if it doesn't already exist.
            var subscriptionName = this.GetResolvedSubscriptionName();
            if (!await this.ServiceBus.NamespaceManager.SubscriptionExistsAsync(this.Topic.ObjectName, subscriptionName))
            {
                await this.ServiceBus.NamespaceManager.CreateSubscriptionAsync(this.Topic.ObjectName, subscriptionName, defaultRule);
                return true;
            }


            return false;
        }

        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <returns>True if the object exists and was deleted; otherwise, false.</returns>
        public override bool Delete()
        {
            var subscriptionName = this.GetResolvedSubscriptionName();
            if (this.ServiceBus.NamespaceManager.SubscriptionExists(this.Topic.ObjectName, subscriptionName))
            {
                this.ServiceBus.NamespaceManager.DeleteSubscription(this.Topic.ObjectName, subscriptionName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override async Task<bool> DeleteAsync(CancellationToken cancellationToken)
        {
            var subscriptionName = this.GetResolvedSubscriptionName();
            if (await this.ServiceBus.NamespaceManager.SubscriptionExistsAsync(this.Topic.ObjectName, subscriptionName))
            {
                await this.ServiceBus.NamespaceManager.DeleteSubscriptionAsync(this.Topic.ObjectName, subscriptionName);
                return true;
            }
            return false;
        }

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
                    if (File.Exists(_lockFileName))
                    {
                        File.Delete(_lockFileName);
                    }
                }
                catch { /* we're making our best effort to clean up, but we don't want to fail the process over it */ }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that"/> parameter; otherwise, false.</returns>
        public override bool Equals(AzureServiceBusObject that)
        {
            return this.Equals(that as AzureServiceBusSubscription);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that"/> parameter; otherwise, false.</returns>
        public virtual bool Equals(AzureServiceBusSubscription that)
        {
            if (ReferenceEquals(null, that)) return false;
            if (ReferenceEquals(this, that)) return true;
            return (base.Equals(that) && this.Topic.Equals(that.Topic));
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that"/> parameter; otherwise, false.</returns>
        public override bool Equals(AzureStorageObject that)
        {
            return this.Equals(that as AzureServiceBusSubscription);
        }

        /// <summary>
        /// Generates the rule text for the event filter associated with the subscription.
        /// </summary>
        /// <returns>The rule filter text.</returns>
        protected internal string GenerateEventFilterRule()
        {
            var sb = new StringBuilder();
            if (this.EventName != AzureServiceBusTopic.AnyEventName)
            {
                sb.AppendFormat("({0} = '{1}')", EventPropertyName, this.EventName);
            }
#if USES_TARGETED_SBMESSAGES
            var machineFilterFormat = (this.UseLocalSubscription
                ? "({0} = '{1}')"
                : "({0} IS NULL OR {0} IN ('', '*'))");
            if (sb.Length != 0) sb.Append(" AND ");
            sb.AppendFormat(machineFilterFormat, TargetMachinePropertyName, Environment.MachineName);
#endif // USES_TARGETED_SBMESSAGES
            if (sb.Length == 0) sb.Append("(1 = 1)");
            return sb.ToString();
        }

        /// <summary>
        /// Receives and then removes the next available message from the subscription queue.
        /// </summary>
        /// <param name="timeout">The maximum amount of time to wait to receive a message.</param>
        /// <returns>A <see cref="BrokeredMessage"/> object, or null if there are no messages available.</returns>
        protected internal BrokeredMessage GetMessage(TimeSpan? timeout = null)
        {
            this.CreateIfNotExists();

            var message = default(BrokeredMessage);
            if ((timeout == null) || (timeout.Value < TimeSpan.Zero))
            {
                message = _subscriptionClient.Value.Receive();
            }
            else message = _subscriptionClient.Value.Receive(timeout.Value);

            return message;
        }

        /// <summary>
        /// Receives and then removes the next available message from the subscription queue.
        /// </summary>
        /// <param name="timeout">The maximum amount of time to wait to receive a message.</param>
        /// <returns>A <see cref="BrokeredMessage"/> object, or null if there are no messages available.</returns>
        protected internal async Task<BrokeredMessage> GetMessageAsync(TimeSpan? timeout = null)
        {
            await this.CreateIfNotExistsAsync();

            var message = default(BrokeredMessage);
            if ((timeout == null) || (timeout.Value < TimeSpan.Zero))
            {
                message = await _subscriptionClient.Value.ReceiveAsync();
            }
            else message = await _subscriptionClient.Value.ReceiveAsync(timeout.Value);

            return message;
        }

        /// <summary>
        /// Gets the resolved name of the subscription.
        /// </summary>
        /// <returns>The subscription name.</returns>
        private string GetResolvedSubscriptionName()
        {
            var subscriptionName = this.ObjectName;
            if (this.UseLocalSubscription)
            {
                subscriptionName = AzureServiceBusNameResolver.Current.Resolve(subscriptionName);
            }
            return subscriptionName;
        }

        /// <summary>
        /// Gets the <see cref="AzureServiceBus"/> object associated with a topic.
        /// </summary>
        /// <param name="topic">An <see cref="AzureServiceBusTopic"/> object.</param>
        /// <returns>An <see cref="AzureServiceBus"/> object.</returns>
        private static AzureServiceBus GetServiceBus(AzureServiceBusTopic topic)
        {
            ArgumentValidator.ValidateNotNull(() => topic);
            return topic.ServiceBus;
        }

        /// <summary>
        /// Marks the service bus subscription as available and listening for new messages.
        /// </summary>
        public virtual void Listen()
        {
            // if we are debugging, then take out a file lock for the subscription so that the code
            // knows to redirect to the local machine service bus subscription instead of using the
            // global one for the environment.
            if (Debugger.IsAttached)
            {
                var path = Path.GetDirectoryName(_lockFileName);
                if ((path != null) && (!Directory.Exists(path)))
                {
                    Directory.CreateDirectory(path);
                }
                Interlocked.CompareExchange(
                    ref _instanceFileLock,
                    File.Open(_lockFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete),
                    null);
            }
            this.CreateIfNotExists();   // ensure the the subscription exists.
        }

        /// <summary>
        /// Marks the service bus subscription as available and listening for new messages.
        /// </summary>
        /// <returns>A <see cref="Task"/> that can be used to monitor the operation.</returns>
        public virtual async Task ListenAsync()
        {
            // if we are debugging, then take out a file lock for the subscription so that the code
            // knows to redirect to the local machine service bus subscription instead of using the
            // global one for the environment.
            if (Debugger.IsAttached)
            {
                var path = Path.GetDirectoryName(_lockFileName);
                if ((path != null) && (!Directory.Exists(path)))
                {
                    Directory.CreateDirectory(path);
                }
                Interlocked.CompareExchange(
                    ref _instanceFileLock,
                    File.Open(_lockFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete),
                    null);
            }
            await this.CreateIfNotExistsAsync();    // ensure the the subscription exists.
        }

        /// <summary>
        /// Retrieves and processes the next available message from the subscription, if available.
        /// </summary>
        /// <param name="callback">The delegate to call to process the message, which should return true if the message was processed successfully.</param>
        /// <returns>True if a message was received; otherwise, false.</returns>
        /// <remarks>
        ///     <para>
        ///         If the <paramref name="callback"/> delegate returns a value of true, then the 
        ///         message will be marked as completed and taken off the queue. Otherwise, the 
        ///         message will be left in the queue and will be picked up again once the receive
        ///         lock has expired.
        ///     </para>
        /// </remarks>
        protected internal bool NextMessage(Func<BrokeredMessage, bool> callback)
        {
            return this.NextMessage(callback, null);
        }

        /// <summary>
        /// Retrieves and processes the next available message from the subscription, if available.
        /// </summary>
        /// <param name="callback">The delegate to call to process the message, which should return true if the message was processed successfully.</param>
        /// <param name="timeout">The maximum amount of time to wait to receive a message.</param>
        /// <returns>True if a message was received; otherwise, false.</returns>
        /// <remarks>
        ///     <para>
        ///         If the <paramref name="callback"/> delegate returns a value of true, then the 
        ///         message will be marked as completed and taken off the queue. Otherwise, the 
        ///         message will be left in the queue and will be picked up again once the receive
        ///         lock has expired.
        ///     </para>
        /// </remarks>
        protected internal bool NextMessage(Func<BrokeredMessage, bool> callback, TimeSpan? timeout)
        {
            ArgumentValidator.ValidateNotNull(() => callback);
            var message = this.GetMessage(timeout);
            return ((message != null) && ProcessMessage(message, callback));
        }

        /// <summary>
        /// Retrieves and processes the next available message from the subscription, if available.
        /// </summary>
        /// <param name="callback">The delegate to call to process the message, which should return true if the message was processed successfully.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        /// <remarks>
        ///     <para>
        ///         If the <paramref name="callback"/> delegate returns a value of true, then the 
        ///         message will be marked as completed and taken off the queue. Otherwise, the 
        ///         message will be left in the queue and will be picked up again once the receive
        ///         lock has expired.
        ///     </para>
        /// </remarks>
        protected internal async Task<bool> NextMessageAsync(Func<BrokeredMessage, Task<bool>> callback)
        {
            return await this.NextMessageAsync((async (msg, tok) => await callback(msg)), null, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves and processes the next available message from the subscription, if available.
        /// </summary>
        /// <param name="callback">The delegate to call to process the message, which should return true if the message was processed successfully.</param>
        /// <param name="timeout">The maximum amount of time to wait to receive a message.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        /// <remarks>
        ///     <para>
        ///         If the <paramref name="callback"/> delegate returns a value of true, then the 
        ///         message will be marked as completed and taken off the queue. Otherwise, the 
        ///         message will be left in the queue and will be picked up again once the receive
        ///         lock has expired.
        ///     </para>
        /// </remarks>
        protected internal async Task<bool> NextMessageAsync(Func<BrokeredMessage, Task<bool>> callback, TimeSpan? timeout)
        {
            return await this.NextMessageAsync((async (msg, tok) => await callback(msg)), timeout, CancellationToken.None);
        }

        /// <summary>
        /// Retrieves and processes the next available message from the subscription, if available.
        /// </summary>
        /// <param name="callback">The delegate to call to process the message, which should return true if the message was processed successfully.</param>
        /// <param name="timeout">The maximum amount of time to wait to receive a message.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        /// <remarks>
        ///     <para>
        ///         If the <paramref name="callback"/> delegate returns a value of true, then the 
        ///         message will be marked as completed and taken off the queue. Otherwise, the 
        ///         message will be left in the queue and will be picked up again once the receive
        ///         lock has expired.
        ///     </para>
        /// </remarks>
        protected internal async Task<bool> NextMessageAsync(Func<BrokeredMessage, CancellationToken, Task<bool>> callback, TimeSpan? timeout, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => callback);
            var message = await this.GetMessageAsync(timeout);
            return ((message != null) && await ProcessMessageAsync(message, callback, cancellationToken));
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
            this.Listen();

            var options = new OnMessageOptions
            {
                AutoComplete = true,
                AutoRenewTimeout = ConfigSettingProvider.Current.GetSetting("ServiceBusAutoRenewTimeout", TimeSpan.Parse, "3:00:00"),
                MaxConcurrentCalls = ConfigSettingProvider.Current.GetSetting("ServiceBusMaxConcurrentCalls", (s => int.Parse(s)), "16"),
            };
            _subscriptionClient.Value.OnMessage(
                (msg) =>
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
                            action(msg);
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
            this.Listen();

            var options = new OnMessageOptions
            {
                AutoComplete = true,
                AutoRenewTimeout = ConfigSettingProvider.Current.GetSetting("ServiceBusAutoRenewTimeout", TimeSpan.Parse, "3:00:00"),
                MaxConcurrentCalls = ConfigSettingProvider.Current.GetSetting("ServiceBusMaxConcurrentCalls", (s => int.Parse(s)), "16"),
            };
            _subscriptionClient.Value.OnMessageAsync(
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
        /// Processes a message from the subscription.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <param name="callback">The delegate to call to process the message, which should return true if the message was processed successfully.</param>
        /// <returns>True if a message was processed; otherwise, false.</returns>
        /// <remarks>
        ///     <para>
        ///         If the <paramref name="callback"/> delegate returns a value of true, then the 
        ///         message will be marked as completed and taken off the queue. Otherwise, the 
        ///         message will be left in the queue and will be picked up again once the receive
        ///         lock has expired.
        ///     </para>
        /// </remarks>
        protected internal static bool ProcessMessage(BrokeredMessage message, Func<BrokeredMessage, bool> callback)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            ArgumentValidator.ValidateNotNull(() => callback);

            var source = new CancellationTokenSource();
            var locmsg = message;
            Task.Run(async () => await AzureServiceBusSubscription.AutoRenewLockAsync(locmsg, source.Token), CancellationToken.None).ConfigureAwait(false);

            try
            {
                if (callback(message))
                {
                    // MessagingCommunicationException is thrown if the message is already removed,
                    // usually because someone else already called Complete on this message object.
                    try
                    {
                        message.Complete();
                    }
                    catch (MessagingCommunicationException)
                    {
                        /* ignored */
                    }
                    return true;
                }
                else
                {
                    try
                    {
                        message.Abandon();
                    }
                    catch (Exception)
                    {
                        /* ignored */
                    }
                    return false;
                }
            }
            finally
            {
                source.Cancel();
            }
        }

        /// <summary>
        /// Processes a message from the subscription.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <param name="callback">The delegate to call to process the message, which should return true if the message was processed successfully.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        /// <remarks>
        ///     <para>
        ///         If the <paramref name="callback"/> delegate returns a value of true, then the 
        ///         message will be marked as completed and taken off the queue. Otherwise, the 
        ///         message will be left in the queue and will be picked up again once the receive
        ///         lock has expired.
        ///     </para>
        /// </remarks>
        protected internal static async Task<bool> ProcessMessageAsync(BrokeredMessage message, Func<BrokeredMessage, Task<bool>> callback)
        {
            return await ProcessMessageAsync(message, (async (msg, tok) => await callback(msg)), CancellationToken.None);
        }

        /// <summary>
        /// Processes a message from the subscription.
        /// </summary>
        /// <param name="message">The message to process.</param>
        /// <param name="callback">The delegate to call to process the message, which should return true if the message was processed successfully.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        /// <remarks>
        ///     <para>
        ///         If the <paramref name="callback"/> delegate returns a value of true, then the 
        ///         message will be marked as completed and taken off the queue. Otherwise, the 
        ///         message will be left in the queue and will be picked up again once the receive
        ///         lock has expired.
        ///     </para>
        /// </remarks>
        protected internal static async Task<bool> ProcessMessageAsync(BrokeredMessage message, Func<BrokeredMessage, CancellationToken, Task<bool>> callback, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            ArgumentValidator.ValidateNotNull(() => callback);

            var locmsg = message;
            var source = new CancellationTokenSource();
            var handled = (await Task.WhenAll(
                callback(locmsg, cancellationToken)
                    .ContinueWith(
                        task =>
                        {
                            source.Cancel();
                            return task.Result;
                        },
                        CancellationToken.None),
                    Task.Run(async () => await AzureServiceBusSubscription.AutoRenewLockAsync(locmsg, source.Token), CancellationToken.None)))
                .FirstOrDefault();

            if (handled)
            {
                // MessagingCommunicationException is thrown if the message is already removed,
                // usually because someone else already called Complete on this message object.
                try
                {
                    await message.CompleteAsync();
                }
                catch (MessagingCommunicationException)
                {
                    /* ignored */
                }
            }
            else
            {
                try
                {
                    await message.AbandonAsync();
                }
                catch (Exception)
                {
                    /* ignored */
                }
            }
            return handled;
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
