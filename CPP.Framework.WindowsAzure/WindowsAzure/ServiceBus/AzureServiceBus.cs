using System;
using System.IO;
using System.Text;
using System.Threading;
using CPP.Framework.DependencyInjection;
using CPP.Framework.DependencyInjection.Resolvers;
using CPP.Framework.Diagnostics;
using CPP.Framework.WindowsAzure.ServiceBus.Queue;
using CPP.Framework.WindowsAzure.Storage;
using CPP.Framework.WindowsAzure.WebJobs;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    /// <summary>
    /// Manages a connection to the Windows Azure Service Bus.
    /// </summary>
    public class AzureServiceBus : IDisposable
    {
        internal const string JsonContentType = "application/json";

        private readonly string _ConnectionString;
        private readonly Lazy<MessagingFactory> _MessagingFactory; 
        private readonly Lazy<NamespaceManager> _NamespaceManager;
        private int _IsDisposed;
        private AzureServiceBusNameResolver _Resolver;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="account">The <see cref="AzureStorageAccount"/> where the object is stored.</param>
        /// <param name="connectionString">The service bus connection string.</param>
        public AzureServiceBus(AzureStorageAccount account, string connectionString)
        {
            ArgumentValidator.ValidateNotNull(() => account);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => connectionString);
            this.Account = account;
            _ConnectionString = connectionString;
            _MessagingFactory = new Lazy<MessagingFactory>(() => MessagingFactory.CreateFromConnectionString(connectionString));
            _NamespaceManager = new Lazy<NamespaceManager>(() => NamespaceManager.CreateFromConnectionString(connectionString));
        }

        /// <summary>
        /// Gets the <see cref="AzureStorageAccount"/> used to store service bus messages.
        /// </summary>
        public AzureStorageAccount Account { get; private set; }

        /// <summary>
        /// Gets the connection string for the service bus.
        /// </summary>
        public string ConnectionString { get { return _ConnectionString; } }

        /// <summary>
        /// Gets the <see cref="MessagingFactory"/> associated with the current service bus
        /// connection.
        /// </summary>
        internal MessagingFactory MessagingFactory { get { return _MessagingFactory.Value; } }

        /// <summary>
        /// Gets or sets the <see cref="AzureServiceBusNameResolver"/> object that used to resolve
        /// subscription names at runtime.
        /// </summary>
        public virtual AzureServiceBusNameResolver NameResolver
        {
            get { return (_Resolver ?? (_Resolver = AzureServiceBusNameResolver.Current)); }
            set { _Resolver = (value ?? AzureServiceBusNameResolver.Current); }
        }

        /// <summary>
        /// Gets the <see cref="NamespaceManager"/> associated with the current service bus 
        /// connection.
        /// </summary>
        internal NamespaceManager NamespaceManager { get { return _NamespaceManager.Value; } }

        /// <summary>
        /// Creates a reference to a web job host that is associated with the current service bus
        /// connection.
        /// </summary>
        /// <returns>An <see cref="AzureWebJob"/> object.</returns>
        public virtual AzureWebJob CreateWebJob()
        {
            Journal.WriteDebug("Creating New WebJob Host");
            var resolvers = new ServiceResolver[]
            {
                new DependencyResolver(typeof(AzureServiceBus), this),
            };
            return ServiceLocator.GetInstance<AzureWebJob>(resolvers);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (Interlocked.Exchange(ref _IsDisposed, 1) == 0)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if the object is being disposed explicitly; otherwise, false if it is being disposed by the finalizer.</param>
        protected virtual void Dispose(bool disposing) { }

        /// <summary>
        /// Deserializes the body of a <see cref="BrokeredMessage"/> instance as a specific type.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="message">A <see cref="BrokeredMessage"/> instance.</param>
        /// <param name="contentType">The content type of the data stored in <paramref name="message"/>, or null to automatically detect the type.</param>
        /// <returns>The deserialized message contents as the requested type.</returns>
        internal static TModel GetMessageBody<TModel>(BrokeredMessage message, string contentType = null)
        {
            var model = default(TModel);
            if (null != message)
            {
                contentType = (contentType ?? message.ContentType);
                if (String.Equals(contentType, JsonContentType, StringComparison.OrdinalIgnoreCase))
                {
                    // read the message contents from the brokered message.
                    byte[] buffer;
                    using (var source = message.GetBody<Stream>())
                    using (var stream = new MemoryStream())
                    {
                        source.CopyTo(stream);
                        buffer = stream.ToArray();
                    }
                    var serialized = Encoding.UTF8.GetString(buffer);
                    model = JsonConvert.DeserializeObject<TModel>(serialized);
                }
                else model = message.GetBody<TModel>();
            }
            return model;
        }

        /// <summary>
        /// Gets a queue from the service bus.
        /// </summary>
        /// <param name="queueName">The name of the queue to create.</param>
        /// <returns>An <see cref="AzureServiceBusQueue"/> object.</returns>
        public virtual AzureServiceBusQueue GetQueue(string queueName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => queueName);
            var resolvers = new ServiceResolver[]
            {
                new DependencyResolver(typeof(AzureServiceBus), this),
                new DependencyResolver(typeof(string), queueName),
            };
            return ServiceLocator.GetInstance<AzureServiceBusQueue>(resolvers);
        }

        /// <summary>
        /// Gets a topic from the service bus.
        /// </summary>
        /// <param name="topicName">The name of the topic to create.</param>
        /// <returns>A <see cref="AzureServiceBusTopic"/> object.</returns>
        public virtual AzureServiceBusTopic GetTopic(string topicName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => topicName);
            var resolvers = new ServiceResolver[]
            {
                new DependencyResolver(typeof(AzureServiceBus), this),
                new DependencyResolver(typeof(string), topicName), 
            };
            return ServiceLocator.GetInstance<AzureServiceBusTopic>(resolvers);
        }
    }
}
