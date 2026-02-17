using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Thrown when the address of the mail server in the configuration is missing or invalid.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidMailServerException : MessagingException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMailServerException"/> class. 
        /// </summary>
        public InvalidMailServerException() : this(ErrorStrings.InvalidMailServerAddress) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMailServerException"/> class. 
        /// </summary>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        public InvalidMailServerException(Exception innerException) : this(ErrorStrings.InvalidMailServerAddress, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMailServerException"/> class. 
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
        protected InvalidMailServerException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMailServerException"/> class. 
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        protected InvalidMailServerException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMailServerException"/> class 
        /// with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected InvalidMailServerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
