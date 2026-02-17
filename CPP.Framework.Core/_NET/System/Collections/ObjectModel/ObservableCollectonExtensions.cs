using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace System.Collections.ObjectModel
{
    /// <summary>
    /// Extension methods for the system <see cref="ObservableCollection{T}"/> class.
    /// </summary>
    public static class ObservableCollectonExtensions
    {
        /// <summary>
        /// Adds a sequence of items to the collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection.</typeparam>
        /// <param name="collection">The <see cref="ObservableCollection{T}"/> to modify.</param>
        /// <param name="sequence">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of items to add.</param>
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> sequence)
        {
            foreach (var item in sequence) collection.Add(item);
        }

        /// <summary>
        /// Assigns the sequence of items in the collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection.</typeparam>
        /// <param name="collection">The <see cref="ObservableCollection{T}"/> to modify.</param>
        /// <param name="sequence">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of items to set.</param>
        public static void SetRange<T>(this ObservableCollection<T> collection, IEnumerable<T> sequence)
        {
            collection.Clear();
            foreach (var item in sequence) collection.Add(item);
        }
    }
}
