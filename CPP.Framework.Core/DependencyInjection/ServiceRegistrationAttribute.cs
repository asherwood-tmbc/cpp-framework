using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CPP.Framework.Threading;

namespace CPP.Framework.DependencyInjection
{
    /// <summary>
    /// Applied to a class to indicate the registration information needed to register the type
    /// with the <see cref="ServiceLocator"/> class during automatic assembly registration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ServiceRegistrationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationAttribute"/> class.
        /// </summary>
        /// <param name="interfaceType">The type of the service interface to register.</param>
        public ServiceRegistrationAttribute(Type interfaceType)
        {
            ArgumentValidator.ValidateNotNull(() => interfaceType);
            this.InterfaceType = interfaceType;
            this.Name = default(string);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRegistrationAttribute"/> class.
        /// </summary>
        /// <param name="interfaceType">The type of the service interface to register.</param>
        /// <param name="registrationName">
        /// The unique name to used to identify the service registration when getting an instance
        /// of the service from the <see cref="ServiceLocator"/>.
        /// </param>
        public ServiceRegistrationAttribute(Type interfaceType, string registrationName)
        {
            ArgumentValidator.ValidateNotNull(() => interfaceType);
            ArgumentValidator.ValidateNotNullOrEmpty(() => registrationName);
            this.InterfaceType = interfaceType;
            this.Name = registrationName;
        }

        /// <summary>
        /// Gets the type of the service interface to register.
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// Gets the unique name to used to identify the service registration when getting an
        /// instance of the service from the <see cref="ServiceLocator"/>.
        /// </summary>
        public string Name { get; }
    }
}
