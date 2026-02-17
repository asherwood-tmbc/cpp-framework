using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Thrown whenever the recipient (To) list for an email message is empty;
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidMailRecipientsException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMailRecipientsException"/> class. 
        /// </summary>
        public InvalidMailRecipientsException() : this(ErrorStrings.InvalidMailRecipientList) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMailRecipientsException"/> class. 
        /// </summary>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        public InvalidMailRecipientsException(Exception innerException) : this(ErrorStrings.InvalidMailRecipientList, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMailRecipientsException"/> class. 
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
        protected InvalidMailRecipientsException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMailRecipientsException"/> class. 
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        protected InvalidMailRecipientsException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMailRecipientsException"/> class 
        /// with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected InvalidMailRecipientsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
