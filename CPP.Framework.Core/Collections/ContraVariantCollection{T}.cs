using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Collections
{
    /// <summary>
    /// Wraps a generic collection object that implements <see cref="ICollection{T}"/> so that its
    /// elements can be treated as contravariant.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the collection.</typeparam>
    [ExcludeFromCodeCoverage]
    public sealed class ContraVariantCollection<T> : IContraVariantCollection where T : class
    {
        /// <summary>
        /// The typed collection being wrapped.
        /// </summary>
        private readonly ICollection<T> _innerCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContraVariantCollection{T}"/> class. 
        /// </summary>
        /// <param name="collection">
        /// The typed collection to wrap.
        /// </param>
        public ContraVariantCollection(ICollection<T> collection)
        {
            ArgumentValidator.ValidateNotNull(() => collection);
            _innerCollection = collection;
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        /// <returns>The number of elements contained in the collection.</returns>
        public int Count => _innerCollection.Count;

        /// <summary>
        /// Gets a value indicating whether the collection is read-only.
        /// </summary>
        /// <returns>True if the collection is read-only; otherwise, false.</returns>
        public bool IsReadOnly => _innerCollection.IsReadOnly;

        /// <summary>
        /// Adds an item to the collection.
        /// </summary>
        /// <param name="item">The object to add to the collection.</param>
        /// <exception cref="NotSupportedException">The collection is read-only.</exception>
        public void Add(object item) { _innerCollection.Add((T)item); }

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        /// <exception cref="NotSupportedException">The collection is read-only. </exception>
        public void Clear() { _innerCollection.Clear(); }

        /// <summary>
        /// Determines whether the collection contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the collection.</param>
        /// <returns>True if <paramref name="item"/> is found in the collection; otherwise, false.</returns>
        public bool Contains(object item) { return _innerCollection.Contains((T)item); }

        /// <summary>
        /// Copies the elements of the collection to an array, starting at a particular index.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from collection. The array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source collection is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(object[] array, int arrayIndex)
        {
            ArgumentValidator.ValidateNotNull(() => array);
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            foreach (var item in this)
            {
                try
                {
                    array[arrayIndex++] = item;
                }
                catch (ArgumentOutOfRangeException)
                {
                    throw new ArgumentException();
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.</returns>
        public IEnumerator<object> GetEnumerator() { return _innerCollection.GetEnumerator(); }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

        /// <summary>
        /// Removes the first occurrence of a specific object from the collection.
        /// </summary>
        /// <returns>True if <paramref name="item"/> was successfully removed from the collection; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original collection.</returns>
        /// <param name="item">The object to remove from the collection.</param><exception cref="T:System.NotSupportedException">The collection is read-only.</exception>
        public bool Remove(object item) { return _innerCollection.Remove((T)item); }
    }
}
