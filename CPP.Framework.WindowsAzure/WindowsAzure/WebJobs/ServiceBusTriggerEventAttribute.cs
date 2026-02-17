using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.WindowsAzure.WebJobs
{
    /// <summary>
    /// Applied to a parameter to indicate the name of an event filter to append to a subscription.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    [ExcludeFromCodeCoverage]
    public sealed class ServiceBusTriggerEventAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="eventName">The name of the event used to send and receive target messages..</param>
        public ServiceBusTriggerEventAttribute(string eventName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);
            this.EventName = eventName;
        }

        /// <summary>
        /// Gets the name of the event used to send and receive target messages.
        /// </summary>
        public string EventName { get; private set; }
    }
}
