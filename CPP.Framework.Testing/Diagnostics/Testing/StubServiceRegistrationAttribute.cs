using System;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.DependencyInjection;

namespace CPP.Framework.Diagnostics.Testing
{
    /// <summary>
    /// Applied to a class to define service registration information when registering a service 
    /// stub with the <see cref="ServiceLocator"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [ExcludeFromCodeCoverage]
    public class StubServiceRegistrationAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="interfaceInfo">The service interface to use for registration.</param>
        public StubServiceRegistrationAttribute(Type interfaceInfo) : this(interfaceInfo, null) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="interfaceType">The service interface to use for registration.</param>
        /// <param name="registrationName">The name to use for the service registration.</param>
        public StubServiceRegistrationAttribute(Type interfaceType, string registrationName)
        {
            ArgumentValidator.ValidateNotNull(() => interfaceType);
            this.InterfaceType = interfaceType;
            this.RegistrationName = (registrationName ?? String.Empty).Trim();
        }

        /// <summary>
        /// Gets the service interface to use for registration.
        /// </summary>
        public Type InterfaceType { get; private set; }

        /// <summary>
        /// Gets the name to use for the service registration.
        /// </summary>
        public string RegistrationName { get; private set; }
    }
}
