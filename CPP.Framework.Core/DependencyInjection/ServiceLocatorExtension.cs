using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace CPP.Framework.DependencyInjection
{
    /// <summary>
    /// Unity Extension class used to install our custom constructor selection policy.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class ServiceLocatorExtension : UnityContainerExtension, IServiceLocatorConfiguration
    {
        /// <summary>
        /// Initialize the container with this extension's functionality.
        /// </summary>
        /// <remarks>
        /// When overridden in a derived class, this method will modify the given
        /// <see cref="ExtensionContext" /> by adding strategies, policies, etc. to install it's
        /// functions into the container.
        /// </remarks>
        protected override void Initialize()
        {
            this.Context.Strategies.Add(new CodeServiceSingletonTypeStrategy(this), UnityBuildStage.TypeMapping);
            this.Context.Policies.SetDefault<IConstructorSelectorPolicy>(new ConstructorSelectorPolicy());
        }

        /// <summary>
        /// Gets the <see cref="ILifetimeContainer"/> for a given type registration.
        /// </summary>
        /// <param name="buildKey">
        ///     The <see cref="NamedTypeBuildKey"/> for the type registration.
        /// </param>
        /// <returns>A <see cref="ILifetimeContainer"/> object.</returns>
        public ILifetimePolicy GetLifetimePolicy(NamedTypeBuildKey buildKey)
        {
            return this.Context.Policies.GetNoDefault<ILifetimePolicy>(buildKey, false);
        }

        /// <summary>
        /// Sets the <see cref="ILifetimePolicy"/> for a given type registration.
        /// </summary>
        /// <param name="buildKey">
        /// The <see cref="NamedTypeBuildKey"/> for the type registration.
        /// </param>
        /// <param name="policy">
        /// The lifetime policy object to set.
        /// </param>
        public void SetLifetimePolicy(NamedTypeBuildKey buildKey, ILifetimePolicy policy)
        {
            var existing = this.GetLifetimePolicy(buildKey);
            if (existing != policy)
            {
                (existing as IDisposable)?.Dispose();
            }
            this.Context.Policies.Set(policy, buildKey);
        }
    }
}
