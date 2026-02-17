using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Abstract base exception for all service registration-related errors.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class ServiceRegistrationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        protected ServiceRegistrationException(string message) : base(message, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error. </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception, or a null reference
        /// (<see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
        /// </param>
        protected ServiceRegistrationException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationException" /> class 
        /// with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized object data about the 
        /// exception being thrown. 
        /// </param>
        /// <param name="context">
        /// The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains 
        /// contextual information about the source or destination.
        /// </param>
        protected ServiceRegistrationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
