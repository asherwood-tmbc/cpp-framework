using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Thrown when a <see cref="MailTransportProvider"/> is unable to send an email message to the
    /// mail server.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MailTransportFailureException : MessagingException
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        public MailTransportFailureException() : this(ErrorStrings.MailTransportFailure) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        public MailTransportFailureException(Exception innerException) : this(ErrorStrings.MailTransportFailure, innerException) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
        protected MailTransportFailureException(string message) : base(message) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        protected MailTransportFailureException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected MailTransportFailureException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
