using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Net.Http
{
    /// <summary>
    /// Thrown when a request fails for an HTTP service call.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class HttpServiceRequestFailedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServiceRequestFailedException"/> class.
        /// </summary>
        /// <param name="message">
        /// The error message for the failure.
        /// </param>
        protected HttpServiceRequestFailedException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServiceRequestFailedException"/> class.
        /// </summary>
        /// <param name="message">
        /// The error message for the failure.
        /// </param>
        /// <param name="innerException">
        /// The exception that caused the current exception.
        /// </param>
        protected HttpServiceRequestFailedException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServiceRequestFailedException"/> class. 
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> that holds the serialized object data.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/> that contains contextual information about the source or destination.
        /// </param>
        protected HttpServiceRequestFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
