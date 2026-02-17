using System;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    /// <summary>
    /// Helper class used to store the metadata for a service bus message property.
    /// </summary>
    /// <typeparam name="TContent">The type of the message content model.</typeparam>
    internal sealed class MessagePropertyMetadata<TContent>
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="getter">
        /// A delegate that can be used to get the value of the property for a given message
        /// content model.
        /// </param>
        /// <param name="format">An optional format string used to format the property value.</param>
        /// <param name="lowercase"><b>True</b> to lowercase the value after it is formatted; otherwise, <b>false</b>.</param>
        private MessagePropertyMetadata(string name, Func<TContent, object> getter, string format, bool lowercase)
        {
            this.Format = format;
            this.Getter = getter;
            this.LowerCase = lowercase;
            this.Name = name;
        }

        /// <summary>
        /// Gets the format string used to format the property value. If this value is null, then
        /// the default format string for the type should be used.
        /// </summary>
        public string Format { get; }

        /// <summary>
        /// Gets a delegate that can be used to get the value of the property for a given message
        /// content model.
        /// </summary>
        public Func<TContent, object> Getter { get; }

        /// <summary>
        /// Gets or sets a value that indicates whether the resulting value should be made lower
        /// case or not after it is formatted using <see cref="Format"/>. Please note that this
        /// property has no effect if <see cref="Format"/> is not specified.
        /// </summary>
        public bool LowerCase { get; set; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get; }

        internal static MessagePropertyMetadata<TContent> Create(string name, Func<TContent, object> getter, string format, bool lowercase)
        {
            return new MessagePropertyMetadata<TContent>(name, getter, format, lowercase);
        }
    }
}
