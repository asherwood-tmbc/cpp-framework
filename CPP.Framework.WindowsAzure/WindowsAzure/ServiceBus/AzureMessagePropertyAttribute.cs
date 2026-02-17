using System;

using Microsoft.ServiceBus.Messaging;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    /// <summary>
    /// Applied to a message content model to specify custom properties that should be added to the
    /// property collection of the <see cref="BrokeredMessage"/> when the message is sent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true)]
    public sealed class AzureMessagePropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureMessagePropertyAttribute"/>
        /// class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="value">
        /// The value of the property. This value cannot be null, however if this value matches the
        /// name of a property on the model within a set of curly braces ({PropertyName}) or square
        /// brackets ([PropertyName]), then the serializer will use the value of named property for
        /// the message when it is configured. However, if the model property returns null, then it
        /// will not be added.
        /// </param>
        public AzureMessagePropertyAttribute(string name, string value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => value);
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the format string to use when formatting the value of a property. This
        /// value is used as the format string in a call to <see cref="string.Format(string,object)"/>,
        /// with the property value being used as the single format argument. Therefore, the string
        /// should only contain at the "{0}" style format specifier (for the property value).
        /// </summary>
        public string FormatString { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the resulting value should be made lower
        /// case or not after it is formatted using <see cref="FormatString"/>. Please note that
        /// this property has no effect if <see cref="FormatString"/> is not specified.
        /// </summary>
        public bool LowerCase { get; set; }
        
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public string Value { get; }
    }
}
