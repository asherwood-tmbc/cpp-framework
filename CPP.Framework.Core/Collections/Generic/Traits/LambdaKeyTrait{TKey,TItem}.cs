using System;
using System.Collections.Generic;

namespace CPP.Framework.Collections.Generic.Traits
{
    /// <summary>
    /// Abstract base class for objects that define traits for a dictionary key using a lambda
    /// expression to select the value.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of item.</typeparam>
    public abstract class LambdaKeyTrait<TKey, TItem> : KeyTrait<TKey, TItem>
    {
        /// <summary>
        /// The delegate that is called to generate a key for an item.
        /// </summary>
        private readonly Func<TItem, TKey> _selector;

        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaKeyTrait{TKey,TItem}"/> class. 
        /// </summary>
        /// <param name="selector">
        /// A delegate that is called to generate a key for an item.
        /// </param>
        protected LambdaKeyTrait(Func<TItem, TKey> selector) : this(selector, EqualityComparer<TKey>.Default) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LambdaKeyTrait{TKey,TItem}"/> class. 
        /// </summary>
        /// <param name="selector">
        /// A delegate that is called to generate a key for an item.
        /// </param>
        /// <param name="comparer">
        /// An <see cref="IEqualityComparer{T}"/> object that should be used to compare the keys.
        /// </param>
        protected LambdaKeyTrait(Func<TItem, TKey> selector, IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            ArgumentValidator.ValidateNotNull(() => selector);
            _selector = selector;
        }

        /// <summary>
        /// Generates a key for a given item.
        /// </summary>
        /// <param name="item">The item for which generate a unique key.</param>
        /// <returns>The key value for <paramref name="item"/>.</returns>
        public override TKey GetKeyValue(TItem item) { return _selector(item); }
    }
}
