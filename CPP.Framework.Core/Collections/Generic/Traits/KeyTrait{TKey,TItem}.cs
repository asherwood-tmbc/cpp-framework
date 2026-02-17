using System.Collections.Generic;

namespace CPP.Framework.Collections.Generic.Traits
{
    /// <summary>
    /// Abstract base class for objects that define traits for a dictionary key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of item.</typeparam>
    public abstract class KeyTrait<TKey, TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyTrait{TKey,TItem}"/> class. 
        /// </summary>
        /// <param name="comparer">
        /// An <see cref="IEqualityComparer{T}"/> object that should be used to compare the keys.
        /// </param>
        protected KeyTrait(IEqualityComparer<TKey> comparer)
        {
            ArgumentValidator.ValidateNotNull(() => comparer);
            this.Comparer = comparer;
        }

        /// <summary>
        /// Gets the default <see cref="IEqualityComparer{T}"/> for the key.
        /// </summary>
        public IEqualityComparer<TKey> Comparer { get; }

        /// <summary>
        /// Generates a key for a given item.
        /// </summary>
        /// <param name="item">The item for which generate a unique key.</param>
        /// <returns>The key value for <paramref name="item"/>.</returns>
        public abstract TKey GetKeyValue(TItem item);
    }
}
