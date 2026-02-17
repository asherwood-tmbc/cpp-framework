using Microsoft.Practices.Unity;

namespace CPP.Framework.DependencyInjection.Resolvers
{
    /// <summary>
    /// This is a specialized dependency resolver that allows the <see cref="IUnityContainer"/> for
    /// the  <see cref="ServiceLocator"/> to be injected into a constructor when a service instance
    /// is being resolved.
    /// </summary>
    public sealed class UnityContainerResolver : ServiceResolver
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="UnityContainerResolver"/> class from
        /// being created.
        /// </summary>
        private UnityContainerResolver() { }

        /// <summary>
        /// Gets the current instance of the <see cref="UnityContainerResolver"/> for the
        /// application.
        /// </summary>
        public static UnityContainerResolver Instance { get; } = new UnityContainerResolver();

        /// <inheritdoc />
        internal override ResolverOverride CreateOverride()
        {
            return new DependencyOverride(typeof(IUnityContainer), ServiceLocator.Container);
        }
    }
}
