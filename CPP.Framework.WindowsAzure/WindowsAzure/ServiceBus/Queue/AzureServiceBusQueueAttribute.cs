using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.WindowsAzure.ServiceBus.Queue
{
    /// <summary>
    /// Applied to a class to mark it as a message model for an Azure Service Bus queue.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [ExcludeFromCodeCoverage]
    public sealed class AzureServiceBusQueueAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusQueueAttribute"/> class.
        /// </summary>
        /// <param name="queueName">The name of the service bus queue.</param>
        public AzureServiceBusQueueAttribute(string queueName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => queueName);
            this.QueueName = queueName;
        }

        /// <summary>
        /// Gets the name of the service bus queue.
        /// </summary>
        public string QueueName { get; }
    }
}
