using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CPP.Framework.WindowsAzure.ServiceBus.Queue
{
    /// <summary>
    /// Applied to a model class to indicate the default event name to use when sending instances 
    /// of the object through a messaging queue (e.g. a service bus topic queue).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [ExcludeFromCodeCoverage]
    public sealed class DefaultEventNameAttribute : Attribute
    {
        /// <summary>
        /// The default instance of the attribute.
        /// </summary>
        private static readonly DefaultEventNameAttribute AnyEventNameAttribute = new DefaultEventNameAttribute(AzureServiceBusTopic.AnyEventName);

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultEventNameAttribute"/> class.
        /// </summary>
        /// <param name="eventName">The name of the event.</param>
        public DefaultEventNameAttribute(string eventName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => eventName);
            this.EventName = eventName;
        }

        /// <summary>
        /// Gets the default event name for the message model.
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Gets the default event name for a given model type.
        /// </summary>
        /// <param name="type">The type of the model.</param>
        /// <returns>A string that contains the event name.</returns>
        public static string GetDefaultEventName(Type type)
        {
            var attribute = type.GetCustomAttributes(typeof(DefaultEventNameAttribute), true)
                .OfType<DefaultEventNameAttribute>()
                .DefaultIfEmpty(AnyEventNameAttribute).First();
            return attribute.EventName;
        }
    }
}
