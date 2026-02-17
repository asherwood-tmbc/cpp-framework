using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.DependencyInjection.Resolvers
{
    /// <summary>
    /// Instructs the <see cref="ServiceLocator"/> to override the resolved value for a type when
    /// resolving dependencies for a requested service instance.
    /// </summary>
    /// <typeparam name="TService">The type of the service interface, which can also be a class.</typeparam>
    [ExcludeFromCodeCoverage]
    public class DependencyResolver<TService> : DependencyResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyResolver{TService}"/> class. 
        /// </summary>
        /// <param name="resolvesTo">
        /// The object instance to return when <see cref="DependencyResolver.TargetType"/> is being resolved.
        /// </param>
        public DependencyResolver(TService resolvesTo) : base(typeof(TService), resolvesTo) { }
    }
}
