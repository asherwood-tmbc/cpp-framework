using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace CPP.Framework.DependencyInjection
{
    /// <summary>
    /// Abstract interface used by the service locator to configure its extension with the
    /// <see cref="UnityContainer"/>.
    /// </summary>
    internal interface IServiceLocatorConfiguration : IUnityContainerExtensionConfigurator
    {
        /// <summary>
        /// Gets the <see cref="ILifetimeContainer"/> for a given type registration.
        /// </summary>
        /// <param name="buildKey">
        ///     The <see cref="NamedTypeBuildKey"/> for the type registration.
        /// </param>
        /// <returns>A <see cref="LifetimeContainer "/> object.</returns>
        ILifetimePolicy GetLifetimePolicy(NamedTypeBuildKey buildKey);

        /// <summary>
        /// Sets the <see cref="ILifetimePolicy"/> for a given type registration.
        /// </summary>
        /// <param name="buildKey">
        /// The <see cref="NamedTypeBuildKey"/> for the type registration.
        /// </param>
        /// <param name="policy">
        /// The lifetime policy object to set.
        /// </param>
        void SetLifetimePolicy(NamedTypeBuildKey buildKey, ILifetimePolicy policy);
    }
}
