using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CPP.Framework.Collections.Generic
{
    /// <summary>
    /// <see cref="IEqualityComparer{T}"/> implementation that forces comparisons based on reference
    /// equality (i.e. identity), as opposed to equality based on value or a custom implementation
    /// of the <see cref="IEquatable{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">The type of the object being compared.</typeparam>
    [ExcludeFromCodeCoverage]
    public sealed class IdentityEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="IdentityEqualityComparer{T}"/> class from 
        /// being created.
        /// </summary>
        private IdentityEqualityComparer() { }

        /// <summary>
        /// Gets the default comparer for the type.
        /// </summary>
        public static IdentityEqualityComparer<T> Default { get; } = new IdentityEqualityComparer<T>();
 
        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object of type <typeparamref name="T"/> to compare.</param>
        /// <param name="y">The second object of type <typeparamref name="T"/> to compare.</param>
        /// <returns>True if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(T x, T y)
        {
            if ((x != null) && (y == null)) return false;
            if ((x == null) && (y != null)) return false;
            return object.ReferenceEquals(x, y);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The object for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(T obj)
        {
            ArgumentValidator.ValidateNotNull(() => obj);
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
