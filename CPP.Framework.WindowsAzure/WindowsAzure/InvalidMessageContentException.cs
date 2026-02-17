using System;
using System.Runtime.Serialization;

namespace CPP.Framework.WindowsAzure
{
    /// <summary>
    /// Thrown when the contents of a queue message are missing or invalid.
    /// </summary>
    public class InvalidMessageContentException : Exception
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        public InvalidMessageContentException() : this(ErrorStrings.InvalidAzureQueueMessage) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        public InvalidMessageContentException(Exception innerException) : this(ErrorStrings.InvalidAzureQueueMessage, innerException) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
        protected InvalidMessageContentException(string message) : base(message) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        protected InvalidMessageContentException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected InvalidMessageContentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
