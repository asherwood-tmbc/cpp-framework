using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace CPP.Framework.Collections.Generic.Traits
{
    /// <summary>
    /// Abstract base class for objects that define traits for a dictionary key using a property
    /// name that is invoked at runtime using reflection.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TItem">The type of item.</typeparam>
    public abstract class PropertyKeyTrait<TKey, TItem> : KeyTrait<TKey, TItem>
    {
        /// <summary>
        /// The default <see cref="BindingFlags"/> value for searching for object properties.
        /// </summary>
        private const BindingFlags SearchFlags = (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// The map of properties to their <see cref="GetValueDelegate"/> delegates.
        /// </summary>
        private static readonly ConcurrentDictionary<string, GetValueDelegate> _PropertyDelegateMap = new ConcurrentDictionary<string, GetValueDelegate>();

        /// <summary>
        /// The <see cref="GetValueDelegate"/> for the property associated with the current trait.
        /// </summary>
        private readonly GetValueDelegate _getValueDelegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyKeyTrait{TKey,TItem}"/> class. 
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that represents the key value.
        /// </param>
        protected PropertyKeyTrait(string propertyName) : this(propertyName, EqualityComparer<TKey>.Default) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyKeyTrait{TKey,TItem}"/> class. 
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property that represents the key value.
        /// </param>
        /// <param name="comparer">
        /// An <see cref="IEqualityComparer{T}"/> object that should be used to compare the keys.
        /// </param>
        protected PropertyKeyTrait(string propertyName, IEqualityComparer<TKey> comparer)
            : base(comparer)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => propertyName);
            _getValueDelegate = _PropertyDelegateMap.GetOrAdd(
                propertyName,
                name =>
                    {
                        var propertyInfo = typeof(TItem).GetProperty(name, SearchFlags);
                        if (propertyInfo == null)
                        {
                            throw new ArgumentException(ErrorStrings.InvalidPropertyOrFieldMemberInfo, nameof(propertyName));
                        }
                        Contract.Assume(propertyInfo != null);

                        var param = Expression.Parameter(typeof(TItem), "item");
                        var property = Expression.Property(param, propertyInfo);
                        var lambda = Expression.Lambda<GetValueDelegate>(property, param);

                        return lambda.Compile();
                    });
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Defines a delegate used to retrieve a property value for an object.
        /// </summary>
        /// <param name="item">The object for which get the property.</param>
        /// <returns>The property value.</returns>
        private delegate TKey GetValueDelegate(TItem item);

        /// <summary>
        /// Gets the name of the property that represents the key value.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Generates a key for a given item.
        /// </summary>
        /// <param name="item">The item for which generate a unique key.</param>
        /// <returns>The key value for <paramref name="item"/>.</returns>
        public override TKey GetKeyValue(TItem item) { return _getValueDelegate(item); }
    }
}
