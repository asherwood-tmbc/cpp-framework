using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace CPP.Framework.Collections.Generic
{
    /// <summary>
    /// Represents a collection of keys and values that supports a limited suite of set operations.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the collection.</typeparam>
    /// <typeparam name="TValue">The type of the values in the collection.</typeparam>
    public class SetDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        /// <summary>
        /// The default <see cref="BindingFlags"/> value for searching for object constructors.
        /// </summary>
        private const BindingFlags SearchFlags = (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// A delegate that is used to call the <see cref="ISerializable"/> constructor for the
        /// dictionary class.
        /// </summary>
        private static readonly CreateDictionaryDelegate CreateDictionaryFromStream;

        /// <summary>
        /// The dictionary that stores the underlying values.
        /// </summary>
        private readonly Dictionary<TKey, TValue> _dictionary;

        /// <summary>
        /// The <see cref="HashSet{T}"/> of keys that is used for set operations.
        /// </summary>
        private readonly HashSet<TKey> _hashSet;

        /// <summary>
        /// Initializes static members of the <see cref="SetDictionary{TKey,TValue}"/> class.
        /// </summary>
        static SetDictionary()
        {
            var ctorParamsTypes = new[] { typeof(SerializationInfo), typeof(StreamingContext) };
            var constructorInfo = typeof(Dictionary<TKey, TValue>).GetConstructor(SearchFlags, null, ctorParamsTypes, null);
            Contract.Assert(constructorInfo != null);

            var parameters = new[]
            {
                Expression.Parameter(typeof(SerializationInfo), "info"),
                Expression.Parameter(typeof(StreamingContext), "context"),
            };
            var create = Expression.New(constructorInfo, parameters.OfType<Expression>());
            var lambda = Expression.Lambda<CreateDictionaryDelegate>(create, parameters);

            CreateDictionaryFromStream = lambda.Compile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDictionary{TKey,TValue}"/> class. 
        /// </summary>
        public SetDictionary() : this(0, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDictionary{TKey,TValue}"/> class. 
        /// </summary>
        /// <param name="capacity">
        /// The initial number of elements that the collection can contain.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is less than 0.
        /// </exception>
        public SetDictionary(int capacity) : this(capacity, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDictionary{TKey,TValue}"/> class. 
        /// </summary>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys, or 
        /// null to use the default.
        /// </param>
        public SetDictionary(IEqualityComparer<TKey> comparer) : this(0, comparer) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDictionary{TKey,TValue}"/> class. 
        /// </summary>
        /// <param name="capacity">
        /// The initial number of elements that the collection can contain.
        /// </param>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys, or
        /// null to use the default.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="capacity"/> is less than 0.
        /// </exception>
        public SetDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _dictionary = new Dictionary<TKey, TValue>(capacity, comparer);
            _hashSet = new HashSet<TKey>(comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDictionary{TKey,TValue}"/> class. 
        /// </summary>
        /// <param name="dictionary">
        /// The <see cref="IDictionary{TKey,TValue}"/> whose elements are copied to the new 
        /// collection.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dictionary"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="dictionary"/> contains one or more duplicate keys.
        /// </exception>
        public SetDictionary(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDictionary{TKey,TValue}"/> class. 
        /// </summary>
        /// <param name="dictionary">
        /// The <see cref="IDictionary{TKey,TValue}"/> whose elements are copied to the new 
        /// collection.
        /// </param>
        /// <param name="comparer">
        /// The <see cref="IEqualityComparer{T}"/> implementation to use when comparing keys, or 
        /// null to use the default.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dictionary"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="dictionary"/> contains one or more duplicate keys.
        /// </exception>
        public SetDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : this((dictionary?.Count ?? 0), comparer)
        {
            ArgumentValidator.ValidateNotNull(() => dictionary);
            Contract.Assume(dictionary != null);
            _dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
            _hashSet = new HashSet<TKey>(_dictionary.Keys, comparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDictionary{TKey,TValue}"/> class. 
        /// </summary>
        /// <param name="info">
        /// A <see cref="SerializationInfo"/> object containing the information required to 
        /// deserialize the object.
        /// </param>
        /// <param name="context">
        /// A <see cref="StreamingContext"/> structure containing the source and destination of the 
        /// serialized stream associated with the object.
        /// </param>
        protected SetDictionary(SerializationInfo info, StreamingContext context)
        {
            _dictionary = CreateDictionaryFromStream(info, context);
            _hashSet = new HashSet<TKey>(_dictionary.Keys);
        }

        /// <summary>
        /// Delegate used to initialize a new instance of the <see cref="Dictionary{TKey,TValue}" /> 
        /// class with serialized data.</summary>
        /// <param name="info">
        /// A <see cref="SerializationInfo" /> object containing the information required to 
        /// deserialize the <see cref="Dictionary{TKey,TValue}" />.
        /// </param>
        /// <param name="context">
        /// A <see cref="StreamingContext" /> structure containing the source and destination of 
        /// the serialized stream associated with the <see cref="Dictionary{TKey,TValue}" />.
        /// </param>
        /// <returns>A new <see cref="Dictionary{TKey,TValue}"/> instance.</returns>
        private delegate Dictionary<TKey, TValue> CreateDictionaryDelegate(SerializationInfo info, StreamingContext context);

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        /// <summary>
        /// Gets a collection containing the keys of the collection.
        /// </summary>
        public ICollection<TKey> Keys => _hashSet;

        /// <summary>
        /// Gets a collection containing the values in the collection.
        /// </summary>
        public ICollection<TValue> Values => _dictionary.Values;

        /// <summary>
        /// Gets or sets the element with the specified key.
        /// </summary>
        /// <param name="key">The key of the element to get or set.</param>
        /// <returns>The indexed value.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">The property is retrieved and <paramref name="key"/> is not found.</exception>
        /// <exception cref="NotSupportedException">The property is set and the collection is read-only.</exception>
        public TValue this[TKey key]
        {
            get => _dictionary[key];
            set { if (_hashSet.Add(key)) _dictionary[key] = value; }
        }

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The object to add to the collection.</param>
        /// <exception cref="NotSupportedException">The collection is read-only.</exception>
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) { this.Add(item.Key, item.Value); }

        /// <summary>
        /// Adds an element with the provided key and value to the collection.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the collection.</exception>
        /// <exception cref="NotSupportedException">The collection is read-only.</exception>
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) { this.Add(key, value); }

        /// <summary>
        /// Adds an element with the provided key and value to the collection.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        /// <returns>True if the value was added to the dictionary; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">An element with the same key already exists in the collection.</exception>
        /// <exception cref="NotSupportedException">The collection is read-only.</exception>
        public bool Add(TKey key, TValue value)
        {
            if (_hashSet.Add(key))
            {
                _dictionary[key] = value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">The collection is read-only. </exception>
        public void Clear()
        {
            _dictionary.Clear();
            _hashSet.Clear();
        }

        /// <summary>
        /// Determines whether the collection contains an element with the specified key.
        /// </summary>
        /// <returns>True if the collection contains an element with the key; otherwise, false.</returns>
        /// <param name="key">The key to locate in the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey(TKey key) { return _hashSet.Contains(key); }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>True if <paramref name="item"/> is found in the collection; otherwise, false.</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item) { return _dictionary.Contains(item); }

        /// <summary>
        /// Copies the elements of the collection to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied 
        /// from collection. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which 
        /// copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source collection is greater than the available space 
        /// from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)_dictionary).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets all the values in the collection that do not match a given sequence of keys.
        /// </summary>
        /// <param name="other">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of keys.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of returned values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public IEnumerable<TValue> ExceptWith(IEnumerable<TKey> other)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            ArgumentValidator.ValidateNotNull(() => other);
            return this.GetAllValues(_hashSet.Except(other, _hashSet.Comparer));
        }

        /// <summary>
        /// Gets all the values in the collection that match a given sequence of keys.
        /// </summary>
        /// <param name="other">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of keys.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of returned values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public IEnumerable<TValue> IntersectWith(IEnumerable<TKey> other)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            ArgumentValidator.ValidateNotNull(() => other);
            return this.GetAllValues(_hashSet.Intersect(other, _hashSet.Comparer));
        }

        /// <summary>
        /// Gets a sequence of values in the collection using a sequence of keys.
        /// </summary>
        /// <param name="sequence">An <see cref="IEnumerable{T}"/> that can be used to iterate over the sequence of keys.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of returned values.</returns>
        protected IEnumerable<TValue> GetAllValues(IEnumerable<TKey> sequence)
        {
            foreach (var key in sequence) yield return _dictionary[key];
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> object that can be used to iterate through the collection.</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return _dictionary.GetEnumerator(); }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return ((IEnumerable)_dictionary).GetEnumerator(); }

        /// <summary>
        /// Determines whether a sequence of keys is a subset of the keys in the collection.
        /// </summary>
        /// <param name="other">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of keys.</param>
        /// <returns>True if <paramref name="other"/> is a subset of the keys in the collection; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsSubsetOf(IEnumerable<TKey> other)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            ArgumentValidator.ValidateNotNull(() => other);
            return _hashSet.IsSubsetOf(other);
        }

        /// <summary>
        /// Determines whether a sequence of keys is a superset of the keys in the collection.
        /// </summary>
        /// <param name="other">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of keys.</param>
        /// <returns>True if <paramref name="other"/> is a superset of the keys in the collection; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsSupersetOf(IEnumerable<TKey> other)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            ArgumentValidator.ValidateNotNull(() => other);
            return _hashSet.IsSupersetOf(other);
        }

        /// <summary>
        /// Determines whether a sequence of keys is a proper (strict) subset of the keys in the 
        /// collection.
        /// </summary>
        /// <param name="other">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of keys.</param>
        /// <returns>True if <paramref name="other"/> is a proper subset of the keys in the collection; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsProperSubsetOf(IEnumerable<TKey> other)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            ArgumentValidator.ValidateNotNull(() => other);
            return _hashSet.IsProperSubsetOf(other);
        }

        /// <summary>
        /// Determines whether a sequence of keys is a proper (strict) superset of the keys in the
        /// collection.
        /// </summary>
        /// <param name="other">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of keys.</param>
        /// <returns>True if <paramref name="other"/> is a proper superset of the keys in the collection; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool IsProperSupersetOf(IEnumerable<TKey> other)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            ArgumentValidator.ValidateNotNull(() => other);
            return _hashSet.IsProperSupersetOf(other);
        }

        /// <summary>
        /// Determines whether a sequence of keys overlap with the keys in the collection.
        /// </summary>
        /// <returns>True if at least one of the keys in <paramref name="other"/> exist in the collection; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        /// <param name="other">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of keys.</param>
        public bool Overlaps(IEnumerable<TKey> other)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            ArgumentValidator.ValidateNotNull(() => other);
            return _hashSet.Overlaps(other);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <param name="item">The object to remove from the collection.</param>
        /// <returns>True if <paramref name="item"/> was successfully removed from the collection; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original collection.</returns>
        /// <exception cref="NotSupportedException">The collection is read-only.</exception>
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) { return _dictionary.Remove(item.Key); }

        /// <summary>
        /// Removes the element with the specified key from the collection.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>True if the element is successfully removed; otherwise, false. This method also returns false if <paramref name="key"/> was not found in the original collection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="NotSupportedException">The collection is read-only.</exception>
        public bool Remove(TKey key)
        {
            if (_hashSet.Remove(key))
            {
                _dictionary.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether a sequence of keys exactly matches the keys in the collection.
        /// </summary>
        /// <param name="other">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of keys.</param>
        /// <returns>True if <paramref name="other"/> matches the keys in the collection; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
        public bool SetEquals(IEnumerable<TKey> other)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            ArgumentValidator.ValidateNotNull(() => other);
            return _hashSet.SetEquals(other);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <returns>
        /// True if the object that implements collection contains an element with the specified 
        /// key; otherwise, false.
        /// </returns>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if the key is 
        /// found; otherwise, the default value for the type of the <paramref name="value"/> 
        /// parameter. This parameter is passed uninitialized.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);
    }
}
