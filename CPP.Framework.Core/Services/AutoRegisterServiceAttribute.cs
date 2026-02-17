using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using JetBrains.Annotations;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Applied to a base service class interface to indicate that it should be automatically
    /// registered on first access by the <see cref="CodeServiceProvider"/> if a registration for
    /// it does not already exist.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [ExcludeFromCodeCoverage]
    public class AutoRegisterServiceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoRegisterServiceAttribute"/> class.
        /// </summary>
        [UsedImplicitly]
        public AutoRegisterServiceAttribute() : this(null, LockRecursionPolicy.NoRecursion) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoRegisterServiceAttribute"/> class.
        /// </summary>
        /// <param name="recursionPolicy">
        /// The <see cref="LockRecursionPolicy"/> to use for the service instance access lock.
        /// </param>
        [UsedImplicitly]
        public AutoRegisterServiceAttribute(LockRecursionPolicy recursionPolicy) : this(null, recursionPolicy) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoRegisterServiceAttribute"/> class.
        /// </summary>
        /// <param name="providerType">
        /// The provider type to register for the service interface, or null to use the same type
        /// for both the interface and the implementation.
        /// </param>
        [UsedImplicitly]
        public AutoRegisterServiceAttribute(Type providerType) : this(providerType, LockRecursionPolicy.NoRecursion) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoRegisterServiceAttribute"/> class.
        /// </summary>
        /// <param name="providerType">
        /// The provider type to register for the service interface, or null to use the same type
        /// for both the interface and the implementation.
        /// </param>
        /// <param name="recursionPolicy">
        /// The <see cref="LockRecursionPolicy"/> to use for the service instance access lock.
        /// </param>
        [UsedImplicitly]
        public AutoRegisterServiceAttribute(Type providerType, LockRecursionPolicy recursionPolicy)
        {
            this.ProviderType = providerType;
            this.RecursionPolicy = recursionPolicy;
        }

        /// <summary>
        /// Gets the implementation type to register for the service interface, or null to use the
        /// same type for both the interface and the implementation.
        /// </summary>
        public Type ProviderType { get; }

        /// <summary>
        /// Gets the <see cref="LockRecursionPolicy"/> to use for the service instance access lock.
        /// </summary>
        public LockRecursionPolicy RecursionPolicy { get; }
    }
}
