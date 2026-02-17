using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Thrown when the provider class for a service interface is not correctly defined.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidServiceRegistrationException : ServiceRegistrationException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidServiceRegistrationException"/>
        /// class.
        /// </summary>
        /// <param name="serviceType">
        /// The <see cref="Type"/> of the service interface requested.
        /// </param>
        public InvalidServiceRegistrationException(Type serviceType) : base(FormatMessage(serviceType)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidServiceRegistrationException"/>
        /// class.
        /// </summary>
        /// <param name="serviceType">
        /// The <see cref="Type"/> of the service interface requested.
        /// </param>
        /// <param name="innerException">
        /// The <see cref="Exception"/> that triggered the current exception.
        /// </param>
        public InvalidServiceRegistrationException(Type serviceType, Exception innerException) : base(FormatMessage(serviceType), innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidServiceRegistrationException" /> 
        /// class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized object data about the 
        /// exception being thrown. 
        /// </param>
        /// <param name="context">
        /// The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains 
        /// contextual information about the source or destination.
        /// </param>
        protected InvalidServiceRegistrationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Formats the error string for the exception.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service interface requested.</param>
        /// <returns>A string that contains the exception message.</returns>
        private static string FormatMessage(Type serviceType)
        {
            return string.Format(ErrorStrings.CreateServiceActivatorFailed, (serviceType?.FullName ?? "<unknown>"));
        }
    }
}
