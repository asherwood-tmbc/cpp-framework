using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using CPP.Framework.DependencyInjection;
using CPP.Framework.Threading;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Provides a container that manages a service instance with a singleton lifetime for every
    /// name instance.
    /// </summary>
    /// <typeparam name="TProvider">The type of the service provider class.</typeparam>
    internal class CodeServiceNamedSingletonContainer<TProvider> : CodeServiceContainer<TProvider> where TProvider : ICodeService
    {
        /// <summary>
        /// The current instances of the service, mapped by name.
        /// </summary>
        private readonly Dictionary<string, ContainerReference> _instances = new Dictionary<string, ContainerReference>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeServiceNamedSingletonContainer{TProvider}"/> 
        /// class.
        /// </summary>
        /// <param name="activator">
        /// The <see cref="CodeServiceActivator{TProvider}"/> object that is used to create new 
        /// instances of the service.
        /// </param>
        /// <param name="recursionPolicy">
        /// The <see cref="LockRecursionPolicy"/> for the service instance access lock.
        /// </param>
        internal CodeServiceNamedSingletonContainer(CodeServiceActivator<TProvider> activator, LockRecursionPolicy recursionPolicy)
            : base(activator)
        {
            ServiceLocator.Unloaded += (sender, args) =>
                {
                    this.ReleaseAll();
                };
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
                if (_instances.TryGetValue(name, out var service)) return service.GetServiceInstance();
            }
            using (this.SyncLock.GetWriterAccess())
            {
                if (!_instances.TryGetValue(name, out var service))
                {
                    _instances[name] = service = new ContainerReference(this.Activator, serviceType, name);
                }
                return service.GetServiceInstance();
            }
        }

        /// <summary>
        /// Releases a named service instance being managed by the current container.
        /// </summary>
        /// <param name="name">The name of the service instance to release.</param>
        internal override void Release(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) name = string.Empty;
            using (this.SyncLock.GetWriterAccess())
            {
                if (_instances.TryGetValue(name, out var reference))
                {
                    this.ReleaseOne(reference);
                }
            }
        }

        /// <summary>
        /// Releases all service instances being managed by the current container.
        /// </summary>
        internal override void ReleaseAll()
        {
            using (this.SyncLock.GetWriterAccess())
            {
                // we cannot do this cleanup in a traditional for/each loop, because ReleaseOne
                // removes all references to the service instance, thereby changing the entries
                // in the collection, which isn't allowed while it is being enumerated.
                while (_instances.Count >= 1)
                {
                    var entry = _instances.First();
                    if (entry.Value != null)
                    {
                        this.ReleaseOne(entry.Value);
                        _instances.Remove(entry.Key);
                    }
                }
                _instances.Clear();
            }
        }

        /// <summary>
        /// Releases a single service instance, and removes all references to it from the named
        /// instance map.
        /// </summary>
        /// <param name="reference">
        /// The <see cref="CodeServiceContainer.ContainerReference"/> of the service instance to
        /// remove.
        /// </param>
        private void ReleaseOne(ContainerReference reference) => this.ReleaseOne(reference?.GetServiceInstance(false));

        /// <summary>
        /// Releases a single service instance, and removes all references to it from the named
        /// instance map.
        /// </summary>
        /// <param name="service">The service instance to release.</param>
        private void ReleaseOne(object service)
        {
            // cleanup the instance, then remove all references to it from the named 
            // instance map.
            if (service == null) return;
            var referenceList = _instances
                .Where((entry) =>
                    {
                        var candidate = entry.Value.GetServiceInstance(false);
                        if (candidate == null) return false;
                        return (ReferenceEquals(candidate, service));
                    })
                .ToList();
            foreach (var entry in referenceList)
            {
                entry.Value.Dispose();
                _instances.Remove(entry.Key);
            }
        }
    }
}
