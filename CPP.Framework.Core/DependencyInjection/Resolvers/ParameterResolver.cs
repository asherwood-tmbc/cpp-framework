using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Practices.Unity;

namespace CPP.Framework.DependencyInjection.Resolvers
{
    /// <summary>
    /// Instructs the <see cref="ServiceLocator"/> to override the value of a named constructor
    /// parameter when resolving dependencies for a requested service instance.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ParameterResolver : ServiceResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterResolver"/> class. 
        /// </summary>
        /// <param name="targetName">
        /// The target parameter name to intercept during dependency resolution.
        /// </param>
        /// <param name="resolvesTo">
        /// The object instance to return when <paramref name="targetName"/> is being resolved.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="targetName"/> is a null reference.
        /// </exception>
        public ParameterResolver(string targetName, object resolvesTo)
        {
            ArgumentValidator.ValidateNotNull(() => targetName);
            this.TargetName = targetName;
            this.ResolvesTo = resolvesTo;
        }

        /// <summary>
        /// Gets the target parameter name to intercept during dependency resolution.
        /// </summary>
        public string TargetName { get; }

        /// <summary>
        /// Gets the object instance to return when <see cref="TargetName"/> is being resolved.
        /// </summary>
        public object ResolvesTo { get; }

        /// <summary>
        /// Converts the current instance to a <see cref="ResolverOverride"/> object for the
        /// <see cref="ServiceLocator"/> class to use when resolving registered object instances.
        /// </summary>
        /// <returns>A <see cref="ResolverOverride"/> instance.</returns>
        internal override ResolverOverride CreateOverride()
        {
            return new ParameterOverride(this.TargetName, this.ResolvesTo);
        }
    }
}
