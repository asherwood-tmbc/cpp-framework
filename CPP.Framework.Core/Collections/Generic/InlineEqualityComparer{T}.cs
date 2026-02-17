using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Collections.Generic
{
    /// <summary>
    /// Determines whether the specified objects are equal.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>True if the specified objects are equal; otherwise, false.</returns>
    public delegate bool EqualityDelegate<in T>(T x, T y);

    /// <summary>
    /// Returns a hash code for the specified object.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    /// <param name="obj">The object for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    /// <exception cref="ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
    public delegate int HashCodeDelegate<in T>(T obj);

    /// <summary>
    /// Represents an <see cref="IEqualityComparer{T}"/> that was created using an inline Lambda
    /// expression.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    [ExcludeFromCodeCoverage]
    public class InlineEqualityComparer<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// The default comparer for the type.
        /// </summary>
        private static readonly IEqualityComparer<T> DefaultComparer = EqualityComparer<T>.Default;

        /// <summary>
        /// The comparison delegate to use when checking for equality.
        /// </summary>
        private readonly EqualityDelegate<T> _comparer;

        /// <summary>
        /// The hash delegate to use when generating a hash code for an object.
        /// </summary>
        private readonly HashCodeDelegate<T> _hashFunc;
 
        /// <summary>
        /// Initializes a new instance of the <see cref="InlineEqualityComparer{T}"/> class. 
        /// </summary>
        /// <param name="comparer">
        /// The comparison delegate to use when checking for equality.
        /// </param>
        /// <param name="hashFunc">
        /// An optional hash delegate to use when generating a hash code for an object. If this 
        /// value is null, then the default hash function is used.
        /// </param>
        protected InlineEqualityComparer(EqualityDelegate<T> comparer, HashCodeDelegate<T> hashFunc)
        {
            _comparer = (comparer ?? DefaultComparer.Equals);
            _hashFunc = (hashFunc ?? DefaultComparer.GetHashCode);
        }

        /// <summary>
        /// Creates an equality comparer based on an inline expression.
        /// </summary>
        /// <param name="comparer">The comparison delegate to use when checking for equality.</param>
        /// <returns>An <see cref="IEqualityComparer{T}"/> object.</returns>
        public static IEqualityComparer<T> Create(EqualityDelegate<T> comparer) => Create(comparer, null);

        /// <summary>
        /// Creates an equality comparer based on an inline expression.
        /// </summary>
        /// <param name="comparer">The comparison delegate to use when checking for equality.</param>
        /// <param name="hashFunc">An optional hash delegate to use when generating a hash code for an object. If this value is null, then the default hash function is used.</param>
        /// <returns>An <see cref="IEqualityComparer{T}"/> object.</returns>
        public static IEqualityComparer<T> Create(EqualityDelegate<T> comparer, HashCodeDelegate<T> hashFunc)
        {
            return new InlineEqualityComparer<T>(comparer, hashFunc);
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>True if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(T x, T y)
        {
            if (ReferenceEquals(x, y)) return true;
            if ((!ReferenceEquals(null, x)) && (ReferenceEquals(null, y))) return false;
            if ((ReferenceEquals(null, x)) && (!ReferenceEquals(null, y))) return false;
            return this._comparer(x, y);
        }

        /// <summary>
        /// Returns a hash code for the specified object.
        /// </summary>
        /// <param name="obj">The object for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
        public int GetHashCode(T obj)
        {
            if (ReferenceEquals(null, obj))
            {
                throw new ArgumentNullException(nameof(obj));
            }
            return this._hashFunc(obj);
        }
    }
}
