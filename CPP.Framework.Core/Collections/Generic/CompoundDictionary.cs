using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using CPP.Framework.Collections.Generic.Traits;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Threading;

namespace CPP.Framework.Collections.Generic
{
    /// <summary>
    /// Represents a collection of items that can be mapped using two different key values.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <typeparam name="TKey1">The type of the first key.</typeparam>
    /// <typeparam name="TKey2">The type of the second key.</typeparam>
    /// <typeparam name="TKeyTrait1">The type of the traits object for <typeparamref name="TKey1"/>.</typeparam>
    /// <typeparam name="TKeyTrait2">The type of the traits object for <typeparamref name="TKey2"/>.</typeparam>
    public abstract class CompoundDictionary<TItem, TKey1, TKey2, TKeyTrait1, TKeyTrait2> :
        IEnumerable<TItem>
        where TKeyTrait1 : KeyTrait<TKey1, TItem>
        where TKeyTrait2 : KeyTrait<TKey2, TItem>
    {
        /// <summary>
        /// An empty dictionary indexed by the primary key type.
        /// </summary>
        private static readonly ReadOnlyDictionary<TKey1, TItem> _EmptyDictionaryByKey1 = new ReadOnlyDictionary<TKey1, TItem>(new Dictionary<TKey1, TItem>());

        /// <summary>
        /// An empty dictionary indexed by the secondary key type.
        /// </summary>
        private static readonly ReadOnlyDictionary<TKey2, TItem> _EmptyDictionaryByKey2 = new ReadOnlyDictionary<TKey2, TItem>(new Dictionary<TKey2, TItem>());

        /// <summary>
        /// The <see cref="MapType{T1,T2}"/> of primary key values to secondary key values.
        /// </summary>
        private readonly MapType<TKey1, TKey2> _itemKeyMap1;

        /// <summary>
        /// The <see cref="MapType{T1,T2}"/> of secondary key values to primary key values.
        /// </summary>
        private readonly MapType<TKey2, TKey1> _itemKeyMap2;

        /// <summary>
        /// The value selector for the primary key.
        /// </summary>
        private readonly TKeyTrait1 _keySelector1;

        /// <summary>
        /// The value selector for the secondary key.
        /// </summary>
        private readonly TKeyTrait2 _keySelector2;

        /// <summary>
        /// The <see cref="MultiAccessLock"/> used to synchronize access to the object across
        /// multiple threads.
        /// </summary>
        private readonly MultiAccessLock _syncLock = new MultiAccessLock(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Initializes a new instance of the <see cref="CompoundDictionary{TItem,TKey1,TKey2,TKeyTrait1,TKeyTrait2}"/> class. 
        /// </summary>
        protected CompoundDictionary()
        {
            _keySelector1 = ServiceLocator.GetInstance<TKeyTrait1>();
            _keySelector2 = ServiceLocator.GetInstance<TKeyTrait2>();
            _itemKeyMap1 = new MapType<TKey1, TKey2>(_keySelector1.Comparer);
            _itemKeyMap2 = new MapType<TKey2, TKey1>(_keySelector2.Comparer);
        }

        /// <summary>
        /// Adds an item to the map if it does not exists, or updates the existing value if it
        /// does exist.
        /// </summary>
        /// <param name="item">The item to add to the map.</param>
        public virtual void AddOrUpdate(TItem item)
        {
            using (_syncLock.GetWriterAccess())
            {
                AddOrUpdate(item, _itemKeyMap1, _keySelector1, _keySelector2);
                AddOrUpdate(item, _itemKeyMap2, _keySelector2, _keySelector1);
            }
        }

        /// <summary>
        /// Adds or updates an item in a <see cref="MapType{T1,T2}"/> object.
        /// </summary>
        /// <typeparam name="T1">The type of the primary key.</typeparam>
        /// <typeparam name="T2">The type of the secondary key.</typeparam>
        /// <param name="item">The item to add to the map.</param>
        /// <param name="target">The target <see cref="MapType{T1,T2}"/> object.</param>
        /// <param name="keySelector1">The <see cref="KeyTrait{TKey,TItem}"/> for the primary key.</param>
        /// <param name="keySelector2">The <see cref="KeyTrait{TKey,TItem}"/> for the secondary key.</param>
        private static void AddOrUpdate<T1, T2>(TItem item, MapType<T1, T2> target, KeyTrait<T1, TItem> keySelector1, KeyTrait<T2, TItem> keySelector2)
        {
            var key1 = keySelector1.GetKeyValue(item);
            var key2 = keySelector2.GetKeyValue(item);

            if (!target.TryGetValue(key1, out var secondary))
            {
                target[key1] = secondary = new Dictionary<T2, TItem>(keySelector2.Comparer);
            }
            secondary[key2] = item;
        }

        /// <summary>
        /// Adds a range of items to the map if they do not exist, or updates the existing value if 
        /// any of them do exist.
        /// </summary>
        /// <param name="sequence">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of items to add.</param>
        public virtual void AddOrUpdateRange(IEnumerable<TItem> sequence)
        {
            using (_syncLock.GetWriterAccess())
            {
                foreach (var item in sequence) this.AddOrUpdate(item);
            }
        }

        /// <summary>
        /// Removes all of the items from the map.
        /// </summary>
        public virtual void Clear()
        {
            using (_syncLock.GetWriterAccess())
            {
                _itemKeyMap1.Clear();
                _itemKeyMap2.Clear();
            }
        }

        /// <summary>
        /// Gets the list of values associated with a primary key value as a dictionary of 
        /// secondary keys mapped to the value.
        /// </summary>
        /// <param name="key">The value of the primary key.</param>
        /// <returns>A <see cref="IDictionary{TKey,TValue}"/> object.</returns>
        protected virtual IDictionary<TKey2, TItem> GetDictionaryByKey1(TKey1 key)
        {
            using (_syncLock.GetReaderAccess())
            {
                if (_itemKeyMap1.TryGetValue(key, out var secondary))
                {
                    return new ReadOnlyDictionary<TKey2, TItem>(secondary);
                }
                return _EmptyDictionaryByKey2;
            }
        }

        /// <summary>
        /// Gets the list of values associated with a secondary key value as a dictionary of 
        /// primary keys mapped to the value.
        /// </summary>
        /// <param name="key">The value of the secondary key.</param>
        /// <returns>A <see cref="IDictionary{TKey,TValue}"/> object.</returns>
        protected virtual IDictionary<TKey1, TItem> GetDictionaryByKey2(TKey2 key)
        {
            using (_syncLock.GetReaderAccess())
            {
                if (_itemKeyMap2.TryGetValue(key, out var secondary))
                {
                    return new ReadOnlyDictionary<TKey1, TItem>(secondary);
                }
                return _EmptyDictionaryByKey1;
            }
        }

        /// <summary>
        /// Gets an item from the dictionary, adding it if it does not already exist.
        /// </summary>
        /// <param name="key1">The primary value of the compound key.</param>
        /// <param name="key2">The secondary value of the compound key.</param>
        /// <param name="factory">A callback delegate that is used to generate the value if the item isn't found.</param>
        /// <returns>The object in the dictionary that is associated with the compound key.</returns>
        protected virtual TItem GetOrAdd(TKey1 key1, TKey2 key2, Func<TKey1, TKey2, TItem> factory)
        {
            ArgumentValidator.ValidateNotNull(() => factory);
            using (_syncLock.GetWriterAccess())
            {
                if (!this.TryGetValue(key1, key2, out var item))
                {
                    item = factory(key1, key2);
                    this.AddOrUpdate(item);
                }
                return item;
            }
        }

        /// <summary>
        /// Gets the list of values associated with a primary key value.
        /// </summary>
        /// <param name="key">The value of the primary key.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence.</returns>
        protected virtual IEnumerable<TItem> GetValuesByKey1(TKey1 key)
        {
            using (_syncLock.GetReaderAccess())
            {
                return GetMappedValues(key, _itemKeyMap1).ToList();
            }
        }

        /// <summary>
        /// Gets the list of values associated with a secondary key value.
        /// </summary>
        /// <param name="key">The value of the secondary key.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence.</returns>
        protected virtual IEnumerable<TItem> GetValuesByKey2(TKey2 key)
        {
            using (_syncLock.GetReaderAccess())
            {
                return GetMappedValues(key, _itemKeyMap2).ToList();
            }
        }

        /// <summary>
        /// Gets the values associated with a single key value.
        /// </summary>
        /// <typeparam name="T1">The type of the primary key.</typeparam>
        /// <typeparam name="T2">The type of the secondary key.</typeparam>
        /// <param name="key">The key value to search for.</param>
        /// <param name="target">The target <see cref="MapType{T1,T2}"/> that contains the values for <paramref name="key"/>.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence.</returns>
        private static IEnumerable<TItem> GetMappedValues<T1, T2>(T1 key, MapType<T1, T2> target)
        {
            if (!target.TryGetValue(key, out var secondary))
            {
                return Enumerable.Empty<TItem>();
            }
            return secondary.Select(x => x.Value);
        }

        /// <summary>
        /// Removes an item from the map.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was removed; otherwise, false.</returns>
        public virtual bool Remove(TItem item)
        {
            return this.Remove(_keySelector1.GetKeyValue(item), _keySelector2.GetKeyValue(item));
        }

        /// <summary>
        /// Removes an item from the map.
        /// </summary>
        /// <param name="key1">The primary key for the item.</param>
        /// <param name="key2">The secondary key for the item.</param>
        /// <returns>True if the item was removed; otherwise, false.</returns>
        public virtual bool Remove(TKey1 key1, TKey2 key2)
        {
            using (_syncLock.GetWriterAccess())
            {
                return (_itemKeyMap1.Remove(key1) || _itemKeyMap2.Remove(key2));
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TItem> GetEnumerator()
        {
            using (_syncLock.GetReaderAccess())
            {
                return _itemKeyMap1
                    .SelectMany(x => x.Value.Select(kvp => kvp.Value))
                    .ToList().GetEnumerator();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

        /// <summary>
        /// Attempts to read a value from the dictionary for the given compound keys.
        /// </summary>
        /// <param name="key1">The primary value of the compound key.</param>
        /// <param name="key2">The secondary value of the compound key.</param>
        /// <param name="item">An output parameter that receives the value on success.</param>
        /// <returns>True if the item was found in the dictionary; otherwise, false.</returns>
        protected virtual bool TryGetValue(TKey1 key1, TKey2 key2, out TItem item)
        {
            item = default(TItem);
            using (_syncLock.GetReaderAccess())
            {
                if (_itemKeyMap1.TryGetValue(key1, out var secondary))
                {
                    return secondary.TryGetValue(key2, out item);
                }
                return false;
            }
        }

        #region MapType Class Declaration

        /// <summary>
        /// Maps a primary key value with a dictionary indexed by a secondary key value.
        /// </summary>
        /// <typeparam name="T1">
        /// The type of the primary key.
        /// </typeparam>
        /// <typeparam name="T2">
        /// The type of the secondary key.
        /// </typeparam>
        private sealed class MapType<T1, T2> : Dictionary<T1, Dictionary<T2, TItem>>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MapType{T1,T2}"/> class. 
            /// </summary>
            /// <param name="comparer">
            /// The default <see cref="IEqualityComparer{T}"/> for the primary key.
            /// </param>
            internal MapType(IEqualityComparer<T1> comparer) : base(comparer) { }
        }

        #endregion // MapType Class Declaration
    }
}
