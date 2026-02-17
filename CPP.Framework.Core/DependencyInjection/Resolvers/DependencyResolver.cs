using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Practices.Unity;

namespace CPP.Framework.DependencyInjection.Resolvers
{
    /// <summary>
    /// Instructs the <see cref="ServiceLocator"/> to override the resolved value for a type when
    /// resolving dependencies for a requested service instance.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DependencyResolver : ServiceResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyResolver"/> class. 
        /// </summary>
        /// <param name="targetType">
        /// The target type to intercept during dependency resolution.
        /// </param>
        /// <param name="resolvesTo">
        /// The object instance to return when <see cref="TargetType"/> is being resolved.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="targetType"/> is a null reference.
        /// </exception>
        public DependencyResolver(Type targetType, object resolvesTo)
        {
            ArgumentValidator.ValidateNotNull(() => targetType);
            this.TargetType = targetType;
            this.ResolvesTo = resolvesTo;
        }

        /// <summary>
        /// Gets the target type to intercept during dependency resolution.
        /// </summary>
        public Type TargetType { get; }

        /// <summary>
        /// Gets the object instance to return when <see cref="TargetType"/> is being resolved.
        /// </summary>
        public object ResolvesTo { get; }

        /// <summary>
        /// Converts the current instance to a <see cref="ResolverOverride"/> object for the
        /// <see cref="ServiceLocator"/> class to use when resolving registered object instances.
        /// </summary>
        /// <returns>A <see cref="ResolverOverride"/> instance.</returns>
        internal override ResolverOverride CreateOverride()
        {
            return new DependencyOverride(this.TargetType, this.ResolvesTo);
        }
    }
}
