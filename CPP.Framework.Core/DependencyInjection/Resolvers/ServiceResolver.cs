using System.Diagnostics.CodeAnalysis;
using Microsoft.Practices.Unity;

namespace CPP.Framework.DependencyInjection.Resolvers
{
    /// <summary>
    /// Abstract base class for all objects that help resolve service locator references.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class ServiceResolver
    {
        /// <summary>
        /// Converts the current instance to a <see cref="ResolverOverride"/> object for the
        /// <see cref="ServiceLocator"/> class to use when resolving registered object instances.
        /// </summary>
        /// <returns>A <see cref="ResolverOverride"/> instance.</returns>
        internal abstract ResolverOverride CreateOverride();
    }
}
