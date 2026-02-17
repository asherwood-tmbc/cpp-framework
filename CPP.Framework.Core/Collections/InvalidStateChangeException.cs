using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Collections
{
    /// <summary>
    /// Thrown when an attempt is made to transition to a state that is invalid for the existing
    /// state.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidStateChangeException : InvalidOperationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateChangeException"/> class. 
        /// </summary>
        /// <param name="message">
        /// A message that describes the exception.
        /// </param>
        public InvalidStateChangeException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateChangeException"/> class. 
        /// </summary>
        /// <param name="message">A message that describes the exception.</param>
        /// <param name="innerException">The exception that generated the current exception.</param>
        public InvalidStateChangeException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateChangeException"/> class. with
        /// serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected InvalidStateChangeException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Creates a new instance of the <see cref="InvalidStateChangeException"/> class.
        /// </summary>
        /// <typeparam name="T">The type of the state value.</typeparam>
        /// <param name="current">The current state value.</param>
        /// <param name="proposed">The invalid proposed state value.</param>
        /// <returns>An <see cref="InvalidStateChangeException"/> object.</returns>
        public static InvalidStateChangeException Create<T>(T current, T proposed) => new InvalidStateChangeException<T>(current, proposed);
    }
}
