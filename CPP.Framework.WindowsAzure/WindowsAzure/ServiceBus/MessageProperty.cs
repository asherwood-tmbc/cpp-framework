using Microsoft.ServiceBus.Messaging;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    /// <summary>
    /// Represents a property for a <see cref="BrokeredMessage"/> object that was read from a
    /// message content model.
    /// </summary>
    internal sealed class MessageProperty
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageProperty"/> class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">The value of the property.</param>
        internal MessageProperty(string name, object value)
        {
            ArgumentValidator.ValidateNotNull(() => name);
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets the property name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the property value.
        /// </summary>
        public object Value { get; }
    }
}
