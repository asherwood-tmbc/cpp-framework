using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace CPP.Framework.Collections
{
    /// <summary>
    /// Thrown when an attempt is made to transition to a state that is invalid for the existing
    /// state.
    /// </summary>
    /// <typeparam name="T">The type of the state value.</typeparam>
    [ExcludeFromCodeCoverage]
    internal sealed class InvalidStateChangeException<T> : InvalidStateChangeException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateChangeException{T}"/> class. 
        /// </summary>
        /// <param name="current">
        /// The current state value.
        /// </param>
        /// <param name="proposed">
        /// The invalid proposed state value.
        /// </param>
        internal InvalidStateChangeException(T current, T proposed) : this(current, proposed, null, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateChangeException{T}"/> class. 
        /// </summary>
        /// <param name="current">The current state value.</param>
        /// <param name="proposed">The invalid proposed state value.</param>
        /// <param name="message">A message that describes the exception, or null to use the default message.</param>
        internal InvalidStateChangeException(T current, T proposed, string message) : this(current, proposed, message, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateChangeException{T}"/> class. 
        /// </summary>
        /// <param name="current">The current state value.</param>
        /// <param name="proposed">The invalid proposed state value.</param>
        /// <param name="innerException">The exception that generated the current exception.</param>
        internal InvalidStateChangeException(T current, T proposed, Exception innerException) : this(current, proposed, null, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateChangeException{T}"/> class. 
        /// </summary>
        /// <param name="current">The current state value.</param>
        /// <param name="proposed">The invalid proposed state value.</param>
        /// <param name="message">A message that describes the exception, or null to use the default message.</param>
        /// <param name="innerException">The exception that generated the current exception.</param>
        internal InvalidStateChangeException(T current, T proposed, string message, Exception innerException)
            : base(message ?? FormatMessage(current, proposed), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStateChangeException{T}"/> class. 
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        [UsedImplicitly]
        private InvalidStateChangeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Formats the error message string for the exception.
        /// </summary>
        /// <param name="current">The current state value.</param>
        /// <param name="proposed">The invalid proposed state value.</param>
        /// <returns>The formatted error string.</returns>
        private static string FormatMessage(T current, T proposed)
        {
            return string.Format(ErrorStrings.InvalidStateChange, current, proposed);
        }
    }
}
