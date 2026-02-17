using System;

using CPP.Framework.DependencyInjection;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Provides a container used to manages a service instance with an scope lifetime (i.e. single
    /// use).
    /// </summary>
    /// <typeparam name="TProvider">The type of the service provider class.</typeparam>
    internal class CodeServicePerUseLifetimeContainer<TProvider> : CodeServiceContainer<TProvider> where TProvider : ICodeService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeServicePerUseLifetimeContainer{TProvider}"/> 
        /// class.
        /// </summary>
        /// <param name="activator">
        /// The <see cref="CodeServiceActivator{TProvider}"/> object that is used to create new 
        /// instances of the service.
        /// </param>
        internal CodeServicePerUseLifetimeContainer(CodeServiceActivator<TProvider> activator) : base(activator) { }

        /// <summary>
        /// Gets a reference to an instance for a service interface.
        /// </summary>
        /// <param name="serviceType">The type of the service interface.</param>
        /// <param name="name">The name of the type of service being created.</param>
        /// <returns>The instance of the service.</returns>
        internal override object GetInstance(Type serviceType, string name) => this.Activator.CreateInstance(serviceType, name);

        /// <summary>
        /// Releases a named service instance being managed by the current container.
        /// </summary>
        /// <param name="name">The name of the service instance to release.</param>
        internal override void Release(string name) { }

        /// <summary>
        /// Releases all service instances being managed by the current container.
        /// </summary>
        internal override void ReleaseAll() { }
    }
}
