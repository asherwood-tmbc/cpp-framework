// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// Extension methods for the <see cref="IDictionary{TKey, TValue}"/> interface.
    /// </summary>
    public static class IDictionaryExtensions
    {
        /// <summary>
        /// Merges the contents of one collection into another.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="target">The <see cref="IDictionary{TKey,TValue}"/> object that wil receive the merged contents.</param>
        /// <param name="other">An <see cref="IEnumerable{T}"/> that can be used to iterate over the sequence of items to be merged into <paramref name="target"/>.</param>
        public static void UnionWith<TKey, TValue>(this IDictionary<TKey, TValue> target, IEnumerable<KeyValuePair<TKey, TValue>> other)
        {
            foreach (var kvp in other)
            {
                if (!target.ContainsKey(kvp.Key)) target.Add(kvp.Key, kvp.Value);
            }
        }
    }
}
