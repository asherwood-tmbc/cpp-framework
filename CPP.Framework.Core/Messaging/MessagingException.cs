using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Thrown whenever an error occurs related to application messaging.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class MessagingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingException"/> class. 
        /// </summary>
        /// <param name="message">
        /// The message text for the exception.
        /// </param>
        protected MessagingException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingException"/> class. 
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        protected MessagingException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingException"/> class with 
        /// serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected MessagingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
