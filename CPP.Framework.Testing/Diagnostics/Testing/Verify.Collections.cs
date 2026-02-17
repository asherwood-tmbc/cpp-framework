using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Diagnostics.Testing
{
    public static partial class Verify
    {
        /// <summary>
        /// Verifies that the contents of two <see cref="List{T}"/> objects are equal.
        /// </summary>
        /// <typeparam name="TValue">The type of the list value.</typeparam>
        /// <param name="expected">The expected list value.</param>
        /// <param name="actual">The actual list value to test against.</param>
        public static void AreEqual<TValue>(HashSet<TValue> expected, HashSet<TValue> actual)
        {
            Verify.AreEqual(expected, actual, default(IEqualityComparer<TValue>));
        }

        /// <summary>
        /// Verifies that the contents of two <see cref="List{T}"/> objects are equal.
        /// </summary>
        /// <typeparam name="TValue">The type of the list value.</typeparam>
        /// <param name="expected">The expected list value.</param>
        /// <param name="actual">The actual list value to test against.</param>
        /// <param name="comparer">The compare to use when comparing list values, or null to use the default comparer for the type.</param>
        public static void AreEqual<TValue>(HashSet<TValue> expected, HashSet<TValue> actual, IEqualityComparer<TValue> comparer)
        {
            if ((expected == null) && (actual == null)) return;
            if (((expected == null) && (actual != null)) || (((expected != null) && (actual == null))))
            {
                Assert.AreEqual(expected, actual);
                return;
            }
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.IsTrue(!expected.Except(actual).Any());
        }

        /// <summary>
        /// Verifies that the contents of two <see cref="List{T}"/> objects are equal.
        /// </summary>
        /// <typeparam name="TValue">The type of the list value.</typeparam>
        /// <param name="expected">The expected list value.</param>
        /// <param name="actual">The actual list value to test against.</param>
        public static void AreEqual<TValue>(List<TValue> expected, List<TValue> actual)
        {
            Verify.AreEqual(expected, actual, default(IEqualityComparer<TValue>));
        }

        /// <summary>
        /// Verifies that the contents of two <see cref="List{T}"/> objects are equal.
        /// </summary>
        /// <typeparam name="TValue">The type of the list value.</typeparam>
        /// <param name="expected">The expected list value.</param>
        /// <param name="actual">The actual list value to test against.</param>
        /// <param name="comparer">The compare to use when comparing list values, or null to use the default comparer for the type.</param>
        public static void AreEqual<TValue>(List<TValue> expected, List<TValue> actual, IEqualityComparer<TValue> comparer)
        {
            var callback = ((comparer != null)
                ? (x, y) => (comparer.Equals(x, y))
                : (Func<TValue, TValue, bool>)null);

            Verify.AreEqual(expected, actual, callback);
        }

        /// <summary>
        /// Verifies that the contents of two <see cref="List{T}"/> objects are equal.
        /// </summary>
        /// <typeparam name="TValue">The type of the list value.</typeparam>
        /// <param name="expected">The expected list value.</param>
        /// <param name="actual">The actual list value to test against.</param>
        /// <param name="comparer">The compare to use when comparing list values, or null to use the default comparer for the type.</param>
        public static void AreEqual<TValue>(List<TValue> expected, List<TValue> actual, Func<TValue, TValue, bool> comparer)
        {
            if ((expected == null) && (actual == null)) return;
            if (((expected == null) && (actual != null)) || (((expected != null) && (actual == null))))
            {
                Assert.AreEqual(expected, actual);
                return;
            }
            Assert.AreEqual(expected.Count, actual.Count);
            for (var i = 0; i < expected.Count; i++)
            {
                if (comparer != null)
                {
                    Assert.IsTrue(comparer(expected[i], actual[i]));
                }
                else Assert.AreEqual(expected[i], actual[i]);
            }
        }
    }
}
