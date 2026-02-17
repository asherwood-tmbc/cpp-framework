using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.DependencyInjection
{
    /// <summary>
    /// Thrown when a named configuration cannot be located in the configuration file.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InjectionConfigNotFoundException : InvalidInjectionConfigException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InjectionConfigNotFoundException"/> class. 
        /// </summary>
        /// <param name="configuration">
        /// The name of the configuration that generated the exception.
        /// </param>
        public InjectionConfigNotFoundException(string configuration) : base(configuration) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectionConfigNotFoundException"/> class. 
        /// </summary>
        /// <param name="configuration">The name of the configuration that generated the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        public InjectionConfigNotFoundException(string configuration, Exception innerException) : base(configuration, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectionConfigNotFoundException"/> class. 
        /// </summary>
        /// <param name="configuration">The name of the configuration that generated the exception.</param>
        /// <param name="message">The message text for the exception.</param>
        public InjectionConfigNotFoundException(string configuration, string message) : base(configuration, message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectionConfigNotFoundException"/> class. 
        /// </summary>
        /// <param name="configuration">The name of the configuration that generated the exception.</param>
        /// <param name="message">The message text for the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        public InjectionConfigNotFoundException(string configuration, string message, Exception innerException) : base(configuration, message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectionConfigNotFoundException"/> class 
        /// with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected InjectionConfigNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
