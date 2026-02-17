using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Diagnostics.Testing
{
    [ExcludeFromCodeCoverage]
    public static partial class Verify
    {
        /// <summary>
        /// Asserts that a sequence of key-value pairs are all contained within a dictionary. In 
        /// order for a key-value pair to match, the key must be present in the dictionary, and the
        /// values must be equal.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="actual">The <see cref="IDictionary{TKey,TValue}"/> to analyze.</param>
        /// <param name="expected">An <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/> objects to locate within </param>
        public static void IsSubsetOf<TKey, TValue>(IDictionary<TKey, TValue> actual, IEnumerable<KeyValuePair<TKey, TValue>> expected)
        {
            Verify.IsSubsetOf(actual, expected, (x, y) => Object.Equals(x, y));
        }

        /// <summary>
        /// Asserts that a sequence of key-value pairs are all contained within a dictionary. In 
        /// order for a key-value pair to match, the key must be present in the dictionary, and
        /// <paramref name="comparer"/> must return true for the values.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="actual">The <see cref="IDictionary{TKey,TValue}"/> to analyze.</param>
        /// <param name="expected">An <see cref="IEnumerable{T}"/> of <see cref="KeyValuePair{TKey,TValue}"/> objects to locate within </param>
        /// <param name="comparer">A delegate to call to test for equality for each matching key-value pair.</param>
        public static void IsSubsetOf<TKey, TValue>(IDictionary<TKey, TValue> actual, IEnumerable<KeyValuePair<TKey, TValue>> expected, Func<TValue, TValue, bool> comparer)
        {
            foreach (var kvp in expected)
            {
                Assert.IsTrue(actual.ContainsKey(kvp.Key));
                Assert.IsTrue(comparer(actual[kvp.Key], kvp.Value));
            }
        }
    }
}
