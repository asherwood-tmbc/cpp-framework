using System;
using System.Threading;

using CPP.Framework.Threading;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Provides a container that manages a service instance with a singleton lifetime for the
    /// entire application (i.e. a "basic" singleton).
    /// </summary>
    /// <typeparam name="TProvider">The type of the service provider class.</typeparam>
    internal class CodeServiceBasicSingletonContainer<TProvider> : CodeServiceContainer<TProvider> where TProvider : ICodeService
    {
        /// <summary>
        /// The current reference to the instance of the service.
        /// </summary>
        private ContainerReference _reference;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeServiceBasicSingletonContainer{TProvider}"/> 
        /// class.
        /// </summary>
        /// <param name="activator">
        /// The <see cref="CodeServiceActivator{TProvider}"/> object that is used to create new 
        /// instances of the service.
        /// service.
        /// </param>
        /// <param name="recursionPolicy">
        /// The <see cref="LockRecursionPolicy"/> for the service instance access lock.
        /// </param>
        internal CodeServiceBasicSingletonContainer(CodeServiceActivator<TProvider> activator, LockRecursionPolicy recursionPolicy)
            : base(activator)
        {
            this.SyncLock = new MultiAccessLock(recursionPolicy);
        }

        /// <summary>
        /// Gets the <see cref="MultiAccessLock"/> used to synchronize access to the instance.
        /// </summary>
        private MultiAccessLock SyncLock { get; }

        /// <summary>
        /// Gets a reference to an instance for a service interface.
        /// </summary>
        /// <param name="serviceType">The type of the service interface.</param>
        /// <param name="name">The name of the type of service being created.</param>
        /// <returns>The instance of the service.</returns>
        internal override object GetInstance(Type serviceType, string name)
        {
            if (string.IsNullOrWhiteSpace(name)) name = string.Empty;
            using (this.SyncLock.GetReaderAccess())
            {
                if (_reference != null) return _reference.GetServiceInstance();
            }
            using (this.SyncLock.GetWriterAccess())
            {
                if (_reference == null)
                {
                    _reference = new ContainerReference(this.Activator, serviceType, name);
                }
                return _reference.GetServiceInstance();
            }
        }

        /// <summary>
        /// Releases a named service instance being managed by the current container.
        /// </summary>
        /// <param name="name">The name of the service instance to release.</param>
        internal override void Release(string name) => this.ReleaseAll();

        /// <summary>
        /// Releases all service instances being managed by the current container.
        /// </summary>
        internal override void ReleaseAll()
        {
            using (this.SyncLock.GetWriterAccess())
            {
                _reference?.Dispose();
                _reference = null;
            }
        }
    }
}
