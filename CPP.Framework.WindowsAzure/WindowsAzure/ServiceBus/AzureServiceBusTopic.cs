using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.DependencyInjection;
using CPP.Framework.DependencyInjection.Resolvers;
using CPP.Framework.WindowsAzure.Storage;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

using Newtonsoft.Json;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    /// <summary>
    /// Represents a Windows Azure Service Bus topic.
    /// </summary>
    public class AzureServiceBusTopic :
        AzureServiceBusObject,
        IEquatable<AzureServiceBusTopic>
    {
        /// <summary>
        /// The value for wildcard names.
        /// </summary>
        private const string WildCardValue = "*";

        /// <summary>
        /// Used to indicate any available event name.
        /// </summary>
        public const string AnyEventName = WildCardValue;

        /// <summary>
        /// Used to indicate any available subscription name.
        /// </summary>
        public const string AnySubscriptionName = WildCardValue;

        /// <summary>
        /// The map of <see cref="TopicSubscriberInfo"/> to topic names.
        /// </summary>
        private static readonly ConcurrentDictionary<string, TopicSubscriberInfo> _SubscriptionEventMap = new ConcurrentDictionary<string, TopicSubscriberInfo>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// The map of <see cref="AzureServiceBusTopicAttribute"/> instance to content model types.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, AzureServiceBusTopicAttribute> _TopicAttributeMap = new ConcurrentDictionary<Type, AzureServiceBusTopicAttribute>();

        /// <summary>
        /// The <see cref="TopicClient" /> instance.
        /// </summary>
        private readonly Lazy<TopicClient> _topicClient;
 
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusTopic"/> class.
        /// </summary>
        /// <param name="serviceBus">The <see cref="AzureServiceBus"/> that owns the object.</param>
        /// <param name="topicName">The name of the service bus topic.</param>
        public AzureServiceBusTopic(AzureServiceBus serviceBus, string topicName) : base(serviceBus, topicName)
        {
            ArgumentValidator.ValidateNotNull(() => serviceBus);
            _topicClient = new Lazy<TopicClient>(() =>
            {
                this.CreateIfNotExists();
                var client = TopicClient.CreateFromConnectionString(serviceBus.ConnectionString, this.ObjectName);
                client.RetryPolicy = RetryPolicy.Default;
                return client;
            });
        }

        /// <summary>
        /// Creates a <see cref="BrokeredMessage"/> object for a given value.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The content for message.</param>
        /// <param name="eventName">The name of the target event for the message.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        /// <returns>A <see cref="BrokeredMessage"/> object.</returns>
        internal virtual BrokeredMessage CreateBrokeredMessage<TContent>(TContent content, string eventName, DateTime? scheduled)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);

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
            return ConfigureMessage(message, eventName, scheduled);
        }

        /// <summary>
        /// Configures the metadata necessary to deliver a <see cref="BrokeredMessage"/> to the 
        /// correct handler(s) at the correct date and time.
        /// </summary>
        /// <param name="message">The message to configure.</param>
        /// <param name="eventName">The name of the target event for the message.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        /// <returns>A reference to <paramref name="message"/>.</returns>
        internal virtual BrokeredMessage ConfigureMessage(BrokeredMessage message, string eventName, DateTime? scheduled)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);

            if (eventName != AnyEventName)
            {
                message.Properties[AzureServiceBusSubscription.EventPropertyName] = eventName;
            }

            // if there is a local process being debugged that is listening to the event, then
            // we need to target to the local topic, rather than the global one.
            var lockFile = this.GetLockFilePath(AnySubscriptionName, eventName);
            var path = Path.GetDirectoryName(lockFile);
            var searchPattern = Path.GetFileName(lockFile);

            if ((path != null) && (searchPattern != null) &&
                Directory.Exists(path) &&
                Directory.GetFiles(path, searchPattern).Any())
            {
                message.Properties[AzureServiceBusSubscription.TargetMachinePropertyName] = Environment.MachineName;
            }
            if (scheduled.HasValue) message.ScheduledEnqueueTimeUtc = scheduled.Value;

            return message;
        }

        /// <summary>
        /// Ensures that the object has been created in the service bus.
        /// </summary>
        /// <returns>True if the object was created successfully; otherwise, false if it already exists.</returns>
        internal override bool CreateIfNotExists()
        {
            if (!this.ServiceBus.NamespaceManager.TopicExists(this.ObjectName))
            {
                var description = GenerateDescription();
                this.ServiceBus.NamespaceManager.CreateTopic(description);
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
            if (!await this.ServiceBus.NamespaceManager.TopicExistsAsync(this.ObjectName))
            {
                var description = this.GenerateDescription();
                await this.ServiceBus.NamespaceManager.CreateTopicAsync(description);
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
            if (this.ServiceBus.NamespaceManager.TopicExists(this.ObjectName))
            {
                this.ServiceBus.NamespaceManager.DeleteTopic(this.ObjectName);
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
            if (await this.ServiceBus.NamespaceManager.TopicExistsAsync(this.ObjectName))
            {
                await this.ServiceBus.NamespaceManager.DeleteTopicAsync(this.ObjectName);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that"/> parameter; otherwise, false.</returns>
        public override bool Equals(AzureServiceBusObject that)
        {
            return this.Equals(that as AzureServiceBusTopic);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that"/> parameter; otherwise, false.</returns>
        public virtual bool Equals(AzureServiceBusTopic that)
        {
            if (ReferenceEquals(null, that)) return false;
            if (ReferenceEquals(this, that)) return true;
            return base.Equals(that);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that"/> parameter; otherwise, false.</returns>
        public override bool Equals(AzureStorageObject that)
        {
            return this.Equals(that as AzureServiceBusTopic);
        }

        /// <summary>
        /// Generates the default <see cref="TopicDescription"/> for the current topic.
        /// </summary>
        /// <returns>A <see cref="TopicDescription"/> object.</returns>
        private TopicDescription GenerateDescription()
        {
            var description = new TopicDescription(this.ObjectName)
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
        /// Gets the <see cref="AzureServiceBusTopicAttribute"/> instance associated with a content 
        /// model type.
        /// </summary>
        /// <typeparam name="TContent">The type of the content model.</typeparam>
        /// <param name="content">An optional instance of the content model.</param>
        /// <returns>A <see cref="AzureServiceBusTopicAttribute"/> instance.</returns>
        protected static AzureServiceBusTopicAttribute GetContentModelMetadata<TContent>(TContent content)
        {
            var attribute = _TopicAttributeMap.GetOrAdd(
                (content?.GetType() ?? typeof(TContent)),
                (type) =>
                    {
                        var found = type.GetCustomAttributes(typeof(AzureServiceBusTopicAttribute), true)
                            .OfType<AzureServiceBusTopicAttribute>()
                            .SingleOrDefault() ?? throw new ArgumentException(string.Format(ErrorStrings.InvalidAzureTopicMessageMetadata, typeof(TContent)));
                        return found;
                    });
            return attribute;
        }

        /// <summary>
        /// Gets the path to a lock file for a subscription.
        /// </summary>
        /// <param name="subscriptionName">The name of the subscription, which can be null.</param>
        /// <param name="eventName">The name of the subscription event, which can be null.</param>
        /// <returns>The fully qualified path to the lock file.</returns>
        protected internal virtual string GetLockFilePath(string subscriptionName, string eventName)
        {
            if (string.IsNullOrWhiteSpace(subscriptionName)) subscriptionName = AnySubscriptionName;
            if (string.IsNullOrWhiteSpace(eventName)) eventName = AnyEventName;
            var location = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var filename = $"{this.ObjectName}.{subscriptionName}.{eventName}.lock";
            return Path.Combine(location, "CPP", "WebJobs", filename);
        }

        /// <summary>
        /// Retrieves a sequence of names for subscriptions that are subscribed to receive a given
        /// <see cref="BrokeredMessage"/>.
        /// </summary>
        /// <param name="message">The target message object.</param>
        /// <returns>An <see cref="IEnumerable{T}"/></returns>
        protected internal virtual IEnumerable<string> GetMessageSubscribers(BrokeredMessage message)
        {
            ArgumentValidator.ValidateNotNull(() => message);

            var topicInfo = this.ServiceBus.NamespaceManager.GetTopic(this.ObjectName);
            var eventName = Convert.ToString(message.Properties[AzureServiceBusSubscription.EventPropertyName]);
            if (string.IsNullOrWhiteSpace(eventName)) eventName = AnyEventName;

            var local = message;
            var subscribers = _SubscriptionEventMap.AddOrUpdate(
                $"{this.ObjectName}.{eventName}",
                (key) =>
                {
                    var query = this.QueryMessageSubscribers(local);
                    return new TopicSubscriberInfo(topicInfo.SubscriptionCount, query);
                },
                (key, existing) =>
                {
                    if (topicInfo.SubscriptionCount != existing.TotalCount)
                    {
                        var query = this.QueryMessageSubscribers(local);
                        return new TopicSubscriberInfo(topicInfo.SubscriptionCount, query);
                    }
                    return existing;
                });
            return subscribers.SubscriptionNames;
        }

        /// <summary>
        /// Gets a reference to a subscription for a Windows Azure Service Bus topic.
        /// </summary>
        /// <param name="subscriptionName">The name of the subscription.</param>
        /// <param name="eventName">The name of the event that triggers the subscription.</param>
        /// <returns>An <see cref="AzureServiceBusSubscription"/> object.</returns>
        public virtual AzureServiceBusSubscription GetSubscription(string subscriptionName, string eventName)
        {
            var resolvers = new ServiceResolver[]
            {
                new DependencyResolver(typeof(AzureServiceBusTopic), this),
                new ParameterResolver("subscriptionName", subscriptionName),
                new ParameterResolver("eventName", eventName), 
            };
            return ServiceLocator.GetInstance<AzureServiceBusSubscription>(resolvers);
        }

        /// <summary>
        /// Queries the current service bus topic for a sequence of names for subscriptions that 
        /// are subscribed to a given <see cref="BrokeredMessage"/> object.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence.</returns>
        protected internal IEnumerable<string> QueryMessageSubscribers(BrokeredMessage message)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            var manager = this.ServiceBus.NamespaceManager;
            foreach (var subscription in manager.GetSubscriptions(this.ObjectName))
            {
                foreach (var rule in manager.GetRules(this.ObjectName, subscription.Name))
                {
                    var filter = rule.Filter;
                    while (filter.RequiresPreprocessing)
                    {
                        filter = filter.Preprocess();
                    }
                    if (filter.Match(message))
                    {
                        yield return subscription.Name;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Queries the current service bus topic for a sequence of names for subscriptions that 
        /// are subscribed to a given <see cref="BrokeredMessage"/> object.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence.</returns>
        protected internal async Task<IEnumerable<string>> QueryMessageSubscribersAsync(BrokeredMessage message)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            var hashset = new HashSet<string>();
            var manager = this.ServiceBus.NamespaceManager;
            foreach (var subscription in await manager.GetSubscriptionsAsync(this.ObjectName))
            {
                foreach (var rule in await manager.GetRulesAsync(this.ObjectName, subscription.Name))
                {
                    var filter = rule.Filter;
                    while (filter.RequiresPreprocessing)
                    {
                        filter = filter.Preprocess();
                    }
                    if (filter.Match(message))
                    {
                        hashset.Add(subscription.Name);
                        break;
                    }
                }
            }
            return hashset;
        }

        /// <summary>
        /// Resolves an subscription name to a machine-specific name.
        /// </summary>
        /// <param name="subscriptionName">The subscription name to resolve.</param>
        /// <param name="eventName">The name of the event associated with the subscription.</param>
        /// <returns>The resolved subscription name.</returns>
        protected internal virtual string ResolveSubscriptionName(string subscriptionName, string eventName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => subscriptionName);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);
#if USES_TARGETED_SBMESSAGES
            var lockfile = this.GetLockFilePath(subscriptionName, eventName);
            var file = Path.GetFileName(lockfile);
            var path = Path.GetDirectoryName(lockfile);
            if ((path != null) && (file != null) && Directory.GetFiles(path, file).Any())
            {
                return AzureServiceBusNameResolver.Current.Resolve(subscriptionName);
            }
#endif //USES_TARGETED_SBMESSAGES
            return subscriptionName;
        }

        /// <summary>
        /// Sends an event message to the topic.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="eventName">The name of the topic event.</param>
        /// <param name="content">The message content.</param>
        public virtual void SendEventMessage<TContent>(string eventName, TContent content)
        {
            this.SendEventMessage(eventName, content, null);
        }

        /// <summary>
        /// Sends an event message to the topic.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="eventName">The name of the topic event.</param>
        /// <param name="content">The message content.</param>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the operation.</returns>
        public virtual async Task SendEventMessageAsync<TContent>(string eventName, TContent content)
        {
            await this.SendEventMessageAsync(eventName, content, null);
        }

        /// <summary>
        /// Sends an event message to the topic.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="eventName">The name of the topic event.</param>
        /// <param name="content">The message content.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        public virtual void SendEventMessage<TContent>(string eventName, TContent content, DateTime? scheduled)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);
            var message = this.CreateBrokeredMessage(content, eventName, scheduled);
            this.SendMessage(message);
        }

        /// <summary>
        /// Sends an event message to the topic based on the attribute decorations on the content 
        /// model class.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The message content.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        /// <param name="existing">
        /// An optional reference to an existing <see cref="AzureStorageAccount"/> session to use
        /// for the model. If this value is null, then a new local session is created instead.
        /// </param>
        public static void SendEventMessage<TContent>(TContent content, DateTime? scheduled = null, AzureStorageAccount existing = null)
        {
            var account = (existing ?? AzureStorageAccount.GetInstance(content));
            try
            {
                var metadata = GetContentModelMetadata(content);
                using (var serviceBus = account.GetServiceBus())
                using (var topic = serviceBus.GetTopic(metadata.TopicName))
                {
                    topic.SendEventMessage(metadata.EventName, content, scheduled);
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
        /// <param name="eventName">The name of the topic event.</param>
        /// <param name="content">The message content.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the operation.</returns>
        public virtual async Task SendEventMessageAsync<TContent>(string eventName, TContent content, DateTime? scheduled)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);
            var message = this.CreateBrokeredMessage(content, eventName, scheduled);
            await this.SendMessageAsync(message);
        }

        /// <summary>
        /// Sends an event message to the topic based on the attribute decorations on the content 
        /// model class.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content.</typeparam>
        /// <param name="content">The message content.</param>
        /// <param name="scheduled">An optional <see cref="DateTime"/> that specifies when the message should be sent.</param>
        /// <param name="existing">
        /// An optional reference to an existing <see cref="AzureStorageAccount"/> session to use
        /// for the model. If this value is null, then a new local session is created instead.
        /// </param>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the operation.</returns>
        public static async Task SendEventMessageAsync<TContent>(TContent content, DateTime? scheduled = null, AzureStorageAccount existing = null)
        {
            var account = (existing ?? AzureStorageAccount.GetInstance(content));
            try
            {
                var metadata = GetContentModelMetadata(content);
                using (var serviceBus = account.GetServiceBus())
                using (var topic = serviceBus.GetTopic(metadata.TopicName))
                {
                    await topic.SendEventMessageAsync(metadata.EventName, content, scheduled);
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
        /// Sends a message to the topic.
        /// </summary>
        /// <param name="message">The message to send.</param>
        internal virtual void SendMessage(BrokeredMessage message)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            _topicClient.Value.Send(message);
        }

        /// <summary>
        /// Sends a message to the topic.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the operation.</returns>
        internal virtual async Task SendMessageAsync(BrokeredMessage message)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            await _topicClient.Value.SendAsync(message);
        }

        #region TopicSubscriberInfo Class Declaration

        /// <summary>
        /// Defines information about a subscriber to a topic.
        /// </summary>
        private class TopicSubscriberInfo
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TopicSubscriberInfo"/> class.
            /// </summary>
            /// <param name="totalCount">The total number of subscribers to the topic.</param>
            /// <param name="subscriptions">The list of topic subscription names.</param>
            internal TopicSubscriberInfo(int totalCount, IEnumerable<string> subscriptions)
            {
                this.TotalCount = totalCount;
                this.SubscriptionNames = new ReadOnlyCollection<string>(subscriptions.ToArray());
            }

            /// <summary>
            /// Gets the list of topic subscription names.
            /// </summary>
            internal IReadOnlyCollection<string> SubscriptionNames { get; }

            /// <summary>
            /// Gets the total number of subscribers to the topic.
            /// </summary>
            internal int TotalCount { get; }
        }

        #endregion // TopicSubscriberInfo Class Declaration
    }
}
