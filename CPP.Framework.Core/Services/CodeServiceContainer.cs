using System;

using CPP.Framework.DependencyInjection;

using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Provides a container to manage the lifetime of a service instance.
    /// </summary>
    internal abstract class CodeServiceContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeServiceContainer"/> class.
        /// </summary>
        /// <param name="activator">
        /// The <see cref="CodeServiceActivator"/> object that is used to create new instance of the 
        /// service.
        /// </param>
        protected CodeServiceContainer(CodeServiceActivator activator)
        {
            ArgumentValidator.ValidateNotNull(() => activator);
            this.Activator = activator;
        }

        /// <summary>
        /// Gets the <see cref="CodeServiceActivator"/> object that is used to create new instance
        /// of the service.
        /// </summary>
        protected CodeServiceActivator Activator { get; }

        /// <summary>
        /// Gets a reference to an instance for a service interface.
        /// </summary>
        /// <param name="serviceType">The type of the service interface.</param>
        /// <param name="name">The name of the type of service being created.</param>
        /// <returns>The instance of the service.</returns>
        internal abstract object GetInstance(Type serviceType, string name);

        /// <summary>
        /// Releases a named service instance being managed by the current container.
        /// </summary>
        /// <param name="name">The name of the service instance to release.</param>
        internal abstract void Release(string name);

        /// <summary>
        /// Releases all service instances being managed by the current container.
        /// </summary>
        internal abstract void ReleaseAll();

        #region ContainerReference Class Declaration

        /// <summary>
        /// Represents a reference to the service in the <see cref="ServiceLocator"/> container.
        /// </summary>
        internal sealed class ContainerReference : IDisposable
        {
            private readonly CodeServiceActivator _activator;
            private readonly NamedTypeBuildKey _buildKey;
            private object _instance;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContainerReference"/> class.
            /// </summary>
            /// <param name="activator">
            /// The <see cref="CodeServiceActivator"/> for the service instance.
            /// </param>
            /// <param name="serviceType">The type of the service interface.</param>
            /// <param name="registrationName">The name of the type of service being created.</param>
            internal ContainerReference(CodeServiceActivator activator, Type serviceType, string registrationName)
            {
                _activator = activator;
                _buildKey = new NamedTypeBuildKey(serviceType, registrationName);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (_instance != null)
                {
                    var configuration = ServiceLocator.Container.Configure<IServiceLocatorConfiguration>();
                    configuration.GetLifetimePolicy(_buildKey)?.RemoveValue();
                }
                _instance = null;
            }

            /// <summary>
            /// Gets a reference to the instance of the service. If the instance does not already,
            /// then a new one will be created.
            /// </summary>
            /// <returns>The service instance.</returns>
            internal object GetServiceInstance() => this.GetServiceInstance(true);

            /// <summary>
            /// Gets a reference to the instance of the service.
            /// </summary>
            /// <param name="create">
            /// <b>True</b> to create the instance if it doesn't exist; otherwise, <b>false</b>.
            /// </param>
            /// <returns>The service instance.</returns>
            internal object GetServiceInstance(bool create)
            {
                if (create && (_instance == null))
                {
                    _instance = _activator.CreateInstance(_buildKey.Type, _buildKey.Name);
                }
                return _instance;
            }
        }

        #endregion
    }
}
