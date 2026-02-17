using System;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Abstract base interface for objects used to activate instances of a service implementation.
    /// </summary>
    internal abstract class CodeServiceActivator
    {
        /// <summary>
        /// Gets a value indicating whether or not the service implementation supports creating
        /// named instances (i.e. different instance per registration name).
        /// </summary>
        internal abstract bool SupportsNamedRegistrations { get; }

        /// <summary>
        /// Gets a value indicating whether or not the service implementation supports singleton
        /// instances (i.e. one instance per application or named registration).
        /// </summary>
        internal abstract bool SupportsSingletonInstances { get; }

        /// <summary>
        /// Creates an instance of the service.
        /// </summary>
        /// <param name="serviceType">
        /// The type of the service interface being requested.
        /// </param>
        /// <param name="registrationName">
        ///     The registered name of the service instance to create, or a null/empty string for the
        ///     default unnamed instance.
        /// </param>
        /// <returns>An instance of the service.</returns>
        internal abstract object CreateInstance(Type serviceType, string registrationName);
    }
}
