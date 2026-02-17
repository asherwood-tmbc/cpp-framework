using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.Diagnostics.Testing;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class IDictionaryExtensionsTest
    {
        [TestMethod]
        public void UnionWithCommonAndUniqueKeys()
        {
            const int CommonItemCount = 5, UniqueItemCount = 2;

            var source = new[]
            {
                new Dictionary<Guid, Guid>(),
                new Dictionary<Guid, Guid>(),
            };
            var expect = default(IEnumerable<KeyValuePair<Guid, Guid>>);
            var common = new HashSet<Guid>(InsertCommonValues(CommonItemCount, source));
            var unique = new HashSet<Guid>(InsertUniqueValues(UniqueItemCount, source));
            
            var actual = new Dictionary<Guid, Guid>();
            foreach (var dict in source) actual.UnionWith(dict);
            var total = (common.Count + unique.Count);

            Verify.AreEqual(actual.Count, total);
            expect = source.First().Where(kvp => (common.Contains(kvp.Key)));
            Verify.IsSubsetOf(actual, expect, (x, y) => (x == y));
            expect = source.Skip(1).SelectMany(dict => dict).Where(kvp => (common.Contains(kvp.Key)));
            Verify.IsSubsetOf(actual, expect, (x, y) => (x != y));
            expect = source.SelectMany(dict => dict).Where(kvp => (unique.Contains(kvp.Key)));
            Verify.IsSubsetOf(actual, expect, (x, y) => (x == y));
        }

        [TestMethod]
        public void UnionWithCommonKeysOnly()
        {
            const int CommonItemCount = 5;

            var source = new[]
            {
                new Dictionary<Guid, Guid>(),
                new Dictionary<Guid, Guid>(),
            };
            var expect = default(IEnumerable<KeyValuePair<Guid, Guid>>);
            var common = new HashSet<Guid>(InsertCommonValues(CommonItemCount, source));

            var actual = new Dictionary<Guid, Guid>();
            foreach (var dict in source) actual.UnionWith(dict);
            var total = (common.Count);

            Verify.AreEqual(actual.Count, total);
            expect = source.First().Where(kvp => (common.Contains(kvp.Key)));
            Verify.IsSubsetOf(actual, expect, (x, y) => (x == y));
            expect = source.Skip(1).SelectMany(dict => dict).Where(kvp => (common.Contains(kvp.Key)));
            Verify.IsSubsetOf(actual, expect, (x, y) => (x != y));
        }

        [TestMethod]
        public void UnionWithUniqueKeysOnly()
        {
            const int UniqueItemCount = 5;

            var source = new[]
            {
                new Dictionary<Guid, Guid>(),
                new Dictionary<Guid, Guid>(),
            };
            var expect = default(IEnumerable<KeyValuePair<Guid, Guid>>);
            var unique = new HashSet<Guid>(InsertUniqueValues(UniqueItemCount, source));

            var actual = new Dictionary<Guid, Guid>();
            foreach (var dict in source) actual.UnionWith(dict);
            var total = (unique.Count);

            Verify.AreEqual(actual.Count, total);
            expect = source.SelectMany(dict => dict).Where(kvp => (unique.Contains(kvp.Key)));
            Verify.IsSubsetOf(actual, expect, (x, y) => (x == y));
        }

        #region Private Helper Functions

        private static IEnumerable<Guid> InsertCommonValues(int count, params Dictionary<Guid, Guid>[] expected)
        {
            for (var i = 0; i < count; i++)
            {
                var key = Guid.NewGuid();
                foreach (var dict in expected) dict[key] = Guid.NewGuid();
                yield return key;
            }
        }

        private static IEnumerable<Guid> InsertUniqueValues(int count, params Dictionary<Guid, Guid>[] expected)
        {
            for (var i=0; i < count; i++)
            {
                foreach (var dict in expected)
                {
                    var key = Guid.NewGuid();
                    dict[key] = Guid.NewGuid();
                    yield return key;
                }
            }
        }

        #endregion // Private Helper Functions
    }
}
