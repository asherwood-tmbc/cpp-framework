using System;
using System.Linq;
using System.Threading;
using CPP.Framework.Configuration;
using CPP.Framework.DependencyInjection;
using CPP.Framework.DependencyInjection.Resolvers;
using CPP.Framework.Threading;
using CPP.Framework.WindowsAzure.ServiceBus;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Represents an connection to a Windows Azure Cloud storage account.
    /// </summary>
    public class AzureStorageAccount : IDisposable
    {
        /// <summary>
        /// The thread synchronization lock for the <see cref="_isStorageEmulatorActive"/> flag.
        /// </summary>
        private static readonly MultiAccessLock _SyncLock = new MultiAccessLock(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// A value indicating whether or not the Azure Storage Emulator is running.
        /// </summary>
        private static bool? _isStorageEmulatorActive;

        /// <summary>
        /// The connection string for the storage account.
        /// </summary>
        private readonly Lazy<string> _connectionString;

        /// <summary>
        /// The name of the storage account connection.
        /// </summary>
        private string _accountName;

        /// <summary>
        /// A value indicating whether or not the object has been disposed.
        /// </summary>
        private int _isDisposed;

        /// <summary>
        /// A value indicating whether not to redirect queue requests to the local storage emulator.
        /// </summary>
        private bool? _useDevelopmentStorage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageAccount"/> class.
        /// </summary>
        protected AzureStorageAccount()
        {
            _connectionString = new Lazy<string>(LoadConnectionString);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageAccount"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string to the storage account to use.</param>
        /// <param name="accountName">The unique name for the connection type.</param>
        public AzureStorageAccount(string connectionString, string accountName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => connectionString);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => accountName);
            this.AccountName = accountName;
            _connectionString = new Lazy<string>(() => connectionString);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageAccount"/> class.
        /// </summary>
        /// <param name="configurationKey">The configuration key of the storage connection string.</param>
        /// <param name="accountName">The unique name for the connection type.</param>
        public AzureStorageAccount(ConfigSettingKey configurationKey, string accountName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => accountName);
            this.AccountName = accountName;
            _connectionString = new Lazy<string>(() => ConfigSettingProvider.Current.GetSetting(configurationKey));
        }

        /// <summary>
        /// Gets the name of the storage account associated with the current connection.
        /// </summary>
        public string AccountName
        {
            get
            {
                var value = _accountName;
                if (value == null)
                {
                    _accountName = value = this.LoadAzureAccountName();
                }
                return value;
            }
            private set => _accountName = value;
        }

        /// <summary>
        /// Gets the connection string for the storage account.
        /// </summary>
        public string ConnectionString => _connectionString.Value;

        /// <summary>
        /// Gets or sets a value indicating whether or not to use the emulator storage 
        /// account when accessing development storage.
        /// </summary>
        public virtual bool UseDevelopmentStorage
        {
            get => (_useDevelopmentStorage ?? (_useDevelopmentStorage = IsStorageEmulatorActive()).Value);
            set => _useDevelopmentStorage = value;
        }

        /// <summary>
        /// Creates a reference to a storage object.
        /// </summary>
        /// <typeparam name="TObject">The type of object to create.</typeparam>
        /// <param name="objectName">The name of the storage object.</param>
        /// <returns>An <see cref="AzureStorageQueue"/> instance.</returns>
        protected TObject CreateStorageObject<TObject>(string objectName)
            where TObject : AzureStorageObject
        {
            var resolvers = new ServiceResolver[]
            {
                new DependencyResolver<AzureStorageAccount>(this),
                new DependencyResolver<string>(objectName), 
            };
            return ServiceLocator.GetInstance<TObject>(resolvers);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                this.Dispose(true);
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if the object is being disposed explicitly; otherwise, false if it being finalized by the runtime.</param>
        protected virtual void Dispose(bool disposing) { }

        /// <summary>
        /// Gets the default instance of an <see cref="AzureStorageAccount"/>.
        /// </summary>
        /// <returns>An <see cref="AzureStorageAccount"/> object.</returns>
        public static AzureStorageAccount GetInstance()
        {
            if (!ServiceLocator.TryGetInstance<AzureStorageAccount>(out var instance))
            {
                instance = new AzureStorageAccount();
            }
            return instance;
        }

        /// <summary>
        /// Gets an instance of an <see cref="AzureStorageAccount"/> for a given storage account 
        /// name.
        /// </summary>
        /// <param name="storageAccountName">The name of the storage account.</param>
        /// <returns>An <see cref="AzureStorageAccount"/> object.</returns>
        public static AzureStorageAccount GetInstance(string storageAccountName)
        {
            if (string.IsNullOrWhiteSpace(storageAccountName))
            {
                return AzureStorageAccount.GetInstance();
            }
            if (!ServiceLocator.TryGetInstance<AzureStorageAccount>(storageAccountName, out var instance))
            {
                return new AzureStorageAccount { AccountName = storageAccountName };
            }
            return instance;
        }

        /// <summary>
        /// Gets a reference to an <see cref="AzureStorageAccount"/> associated with a data model.
        /// </summary>
        /// <typeparam name="TModel">The type of the data model.</typeparam>
        /// <param name="model">The data model associated with the account.</param>
        /// <returns>An <see cref="AzureStorageAccount"/> object.</returns>
        protected internal static AzureStorageAccount GetInstance<TModel>(TModel model)
        {
            var attribute = typeof(TModel).GetCustomAttributes(typeof(AzureAccountNameAttribute), true)
                .OfType<AzureAccountNameAttribute>()
                .SingleOrDefault();
            return AzureStorageAccount.GetInstance(attribute?.AccountName);
        }

        /// <summary>
        /// Gets a connection to a Windows Azure Service Bus account.
        /// </summary>
        /// <returns>A <see cref="AzureServiceBus"/> object.</returns>
        public virtual AzureServiceBus GetServiceBus()
        {
            var connectionString = ConfigSettingProvider.Current.GetServiceBusConnectionString();
            return this.GetServiceBus(connectionString);
        }

        /// <summary>
        /// Gets a connection to a Windows Azure Service Bus account.
        /// </summary>
        /// <param name="configurationKey">A <see cref="ConfigSettingKey"/> of the service bus connection string.</param>
        /// <returns>A <see cref="AzureServiceBus"/> object.</returns>
        public virtual AzureServiceBus GetServiceBus(ConfigSettingKey configurationKey)
        {
            var connectionString = ConfigSettingProvider.Current.GetSetting(configurationKey);
            return this.GetServiceBus(connectionString);
        }

        /// <summary>
        /// Gets a connection to a Windows Azure Service Bus account.
        /// </summary>
        /// <param name="connectionString">The connection string for the service bus.</param>
        /// <returns>A <see cref="AzureServiceBus"/> object.</returns>
        public virtual AzureServiceBus GetServiceBus(string connectionString)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => connectionString);
            var resolvers = new ServiceResolver[]
            {
                new DependencyResolver(typeof(AzureStorageAccount), this),
                new DependencyResolver(typeof(string), connectionString), 
            };
            return ServiceLocator.GetInstance<AzureServiceBus>(resolvers);
        }

        /// <summary>
        /// Gets a reference to a <see cref="AzureStorageBlockBlob"/> object that represents a blob
        /// in storage.
        /// </summary>
        /// <param name="path">The full path to the blob, starting with the container name.</param>
        /// <returns>An <see cref="AzureStorageBlockBlob"/> instance.</returns>
        public virtual AzureStorageBlockBlob GetStorageBlockBlob(string path)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => path);
            var container = AzureStoragePath.GetContainerName(path);
            var objectName = AzureStoragePath.GetBlobFilePath(path);
            return this.GetStorageBlockBlob(container, objectName);
        }

        /// <summary>
        /// Gets a reference to a <see cref="AzureStorageBlockBlob"/> object that represents a blob
        /// in storage.
        /// </summary>
        /// <param name="containerName">The name of the container where the blob is stored.</param>
        /// <param name="blockBlobName">The name of the blob.</param>
        /// <returns>An <see cref="AzureStorageBlockBlob"/> instance.</returns>
        public virtual AzureStorageBlockBlob GetStorageBlockBlob(string containerName, string blockBlobName)
        {
            var objectName = AzureStoragePath.Combine(containerName, blockBlobName);
            return this.CreateStorageObject<AzureStorageBlockBlob>(objectName);
        }

        /// <summary>
        /// Gets a reference to a <see cref="AzureStorageQueue"/> object that represents a storage
        /// queue.
        /// </summary>
        /// <param name="queueName">The name of the queue.</param>
        /// <returns>An <see cref="AzureStorageQueue"/> instance.</returns>
        public virtual AzureStorageQueue GetStorageQueue(string queueName)
        {
            return this.CreateStorageObject<AzureStorageQueue>(queueName);
        }

        /// <summary>
        /// Gets a reference to a <see cref="AzureStorageTable{TEntity}"/> object that represents a 
        /// storage table. This method uses the value of the <see cref="AzureTableNameAttribute"/>
        /// to identify the name of the storage table, or the name of the type if the attribute has 
        /// not been applied to <typeparamref name="TEntity"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities stored in the table.</typeparam>
        /// <returns>An <see cref="AzureStorageTable{TEntity}"/> instance.</returns>
        public virtual AzureStorageTable<TEntity> GetStorageTable<TEntity>()
            where TEntity : class, ITableEntity, new()
        {
            var tableName = AzureStorageTable<TEntity>.GetTableName();
            return this.CreateStorageObject<AzureStorageTable<TEntity>>(tableName);
        }

        /// <summary>
        /// Gets a reference to a <see cref="AzureStorageTable{TEntity}"/> object that represents a 
        /// storage table.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entities stored in the table.</typeparam>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>An <see cref="AzureStorageTable{TEntity}"/> instance.</returns>
        public virtual AzureStorageTable<TEntity> GetStorageTable<TEntity>(string tableName)
            where TEntity : class, ITableEntity, new()
        {
            return this.CreateStorageObject<AzureStorageTable<TEntity>>(tableName);
        }

        /// <summary>
        /// Gets a value indicating whether or not the Azure Storage emulator is active.
        /// </summary>
        /// <returns>A boolean value.</returns>
        private static bool IsStorageEmulatorActive()
        {
            using (_SyncLock.GetReaderAccess())
            {
                if (_isStorageEmulatorActive.HasValue) return _isStorageEmulatorActive.Value;
            }
            using (_SyncLock.GetWriterAccess())
            {
                if (!_isStorageEmulatorActive.HasValue)
                {
                    try
                    {
#if DEBUG
                        var account = CloudStorageAccount.DevelopmentStorageAccount;
                        var qclient = account.CreateCloudQueueClient();
                        var myqueue = qclient.GetQueueReference("ping");

                        if (myqueue != null)
                        {
                            var options = new QueueRequestOptions
                            {
                                ServerTimeout = TimeSpan.FromMilliseconds(2500),
                                RetryPolicy = new NoRetry(),
                            };
                            myqueue.CreateIfNotExists(options);
                        }
                        _isStorageEmulatorActive = ((myqueue != null) && (myqueue.Exists()));
#else // DEBUG
                            _isStorageEmulatorActive = false;
#endif // DEBUG
                    }
                    catch (StorageException)
                    {
                        _isStorageEmulatorActive = false;
                    }
                }
                return _isStorageEmulatorActive.Value;
            }
        }

        /// <summary>
        /// Dynamically loads the name of the storage account based on the value of the 
        /// <see cref="AzureAccountNameAttribute"/> assigned to the class, if available.
        /// </summary>
        /// <returns>A string value.</returns>
        protected internal virtual string LoadAzureAccountName()
        {
            var attribute = this.GetType()
                .GetCustomAttributes(typeof(AzureAccountNameAttribute), true)
                .OfType<AzureAccountNameAttribute>()
                .SingleOrDefault();
            return (attribute?.AccountName ?? string.Empty);
        }

        /// <summary>
        /// Dynamically loads the connection string value based on the configured settings for the
        /// account.
        /// </summary>
        /// <returns>A connection string value.</returns>
        protected internal virtual string LoadConnectionString()
        {
            throw new NotImplementedException();    // TODO : Update this when we are ready to implement simplified registration.
        }

        /// <summary>
        /// Gets a reference to a <see cref="CloudStorageAccount"/> using the associated connection
        /// string value.
        /// </summary>
        /// <returns>A <see cref="CloudStorageAccount"/> object.</returns>
        protected internal virtual CloudStorageAccount OpenStorageAccount()
        {
            return CloudStorageAccount.Parse(this.ConnectionString);
        }
    }
}
