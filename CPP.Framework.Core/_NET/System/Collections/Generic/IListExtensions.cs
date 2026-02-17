// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// Extension methods for the <see cref="IList{T}"/> interface.
    /// </summary>
    public static class IListExtensions
    {
        /// <summary>
        /// Adds a sequence of items to the collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection.</typeparam>
        /// <param name="collection">The <see cref="IList{T}"/> to modify.</param>
        /// <param name="sequence">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of items to add.</param>
        public static void AddRange<T>(this IList<T> collection, IEnumerable<T> sequence)
        {
            if (collection is List<T> list)
            {
                list.AddRange(sequence);
            }
            else foreach (var item in sequence) collection.Add(item);
        }

        /// <summary>
        /// Assigns the sequence of items in the collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection.</typeparam>
        /// <param name="collection">The <see cref="IList{T}"/> to modify.</param>
        /// <param name="sequence">An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of items to set.</param>
        public static void SetRange<T>(this IList<T> collection, IEnumerable<T> sequence)
        {
            collection.Clear();
            collection.AddRange(sequence);
        }
    }
}
