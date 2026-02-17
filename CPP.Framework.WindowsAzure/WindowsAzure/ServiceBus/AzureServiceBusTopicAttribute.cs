using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    /// <summary>
    /// Applied to a class to mark it as a message model for an Azure Service Bus topic.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [ExcludeFromCodeCoverage]
    public sealed class AzureServiceBusTopicAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusTopicAttribute"/> class.
        /// </summary>
        /// <param name="topicName">
        /// The name of the target service bus topic for the message.
        /// </param>
        public AzureServiceBusTopicAttribute(string topicName)
            : this(topicName, AzureServiceBusTopic.AnyEventName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureServiceBusTopicAttribute"/> class.
        /// </summary>
        /// <param name="topicName">
        /// The name of the target service bus topic for the message.
        /// </param>
        /// <param name="eventName">
        /// The name of the target event for the service bus message.
        /// </param>
        public AzureServiceBusTopicAttribute(string topicName, string eventName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => topicName);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);

            this.EventName = (eventName ?? AzureServiceBusTopic.AnyEventName);
            this.TopicName = (topicName);
        }
        
        /// <summary>
        /// Gets the name of the target event for the service bus message.
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Gets the name of the target service bus topic for the message.
        /// </summary>
        public string TopicName { get; }
    }
}
