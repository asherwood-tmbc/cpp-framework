using System;
using System.Threading.Tasks;
using CPP.Framework.WindowsAzure.Storage;
using Microsoft.ServiceBus;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    /// <summary>
    /// Abstract base class for all objects associated with a Windows Azure Service Bus.
    /// </summary>
    public abstract class AzureServiceBusObject :
        AzureStorageObject,
        IEquatable<AzureServiceBusObject>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusObject"/> class. 
        /// </summary>
        /// <param name="serviceBus">
        /// The <see cref="AzureServiceBus"/> that owns the object.
        /// </param>
        /// <param name="objectName">
        /// The name of the service bus object.
        /// </param>
        protected AzureServiceBusObject(AzureServiceBus serviceBus, string objectName)
            : base(GetStorageAccount(serviceBus), objectName)
        {
            ArgumentValidator.ValidateNotNull(() => serviceBus);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => objectName);
            this.ServiceBus = serviceBus;
        }

        /// <summary>
        /// Gets the <see cref="NamespaceManager"/> associated with the <see cref="AzureServiceBus"/>
        /// that owns the current class.
        /// </summary>
        protected NamespaceManager NamespaceManager => this.ServiceBus.NamespaceManager;

        /// <summary>
        /// Gets the <see cref="AzureServiceBus"/> object that owns the current object.
        /// </summary>
        public AzureServiceBus ServiceBus { get; private set; }

        /// <summary>
        /// Ensures that the object has been created in the service bus.
        /// </summary>
        /// <returns>True if the object was created successfully; otherwise, false if it already exists.</returns>
        internal abstract bool CreateIfNotExists();

        /// <summary>
        /// Ensures that the object has been created in the service bus.
        /// </summary>
        /// <returns>True if the object was created successfully; otherwise, false if it already exists.</returns>
        internal abstract Task<bool> CreateIfNotExistsAsync();

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that"/> parameter; otherwise, false.</returns>
        public virtual bool Equals(AzureServiceBusObject that)
        {
            if (ReferenceEquals(null, that)) return false;
            if (ReferenceEquals(this, that)) return true;

            if (base.Equals(that))
            {
                return DefaultComparer.Equals(this.ServiceBus.ConnectionString, that.ServiceBus.ConnectionString);
            }
            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that"/> parameter; otherwise, false.</returns>
        public override bool Equals(AzureStorageObject that)
        {
            return this.Equals(that as AzureServiceBusObject);
        }

        /// <summary>
        /// Gets the <see cref="AzureStorageAccount"/> associated with a service bus.
        /// </summary>
        /// <param name="serviceBus">An <see cref="AzureServiceBus"/> object.</param>
        /// <returns>An <see cref="AzureStorageAccount"/> object.</returns>
        private static AzureStorageAccount GetStorageAccount(AzureServiceBus serviceBus)
        {
            ArgumentValidator.ValidateNotNull(() => serviceBus);
            return serviceBus.Account;
        }
    }
}
