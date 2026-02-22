using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace CPP.Framework.Collections.Generic.Traits
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class KeyTraitTest
    {
        private class TestKeyTrait<TKey, TItem> : KeyTrait<TKey, TItem>
        {
            public TestKeyTrait(IEqualityComparer<TKey> comparer) : base(comparer)
            {
            }

            public override TKey GetKeyValue(TItem item)
            {
                throw new System.NotImplementedException();
            }
        }

        private class TestKeyTraitItem
        {
            public string ItemValue = "Item: " + DateTime.Now;


        }

        private sealed class GuidEqualityComparer : IEqualityComparer<Guid>
        {
            public bool Equals(Guid x, Guid y)
            {
                if (x.GetType() != y.GetType()) return false;
                return Guid.Equals(x, y);
            }

            public int GetHashCode(Guid obj)
            {
                return (obj != Guid.Empty ? obj.GetHashCode() : 0);
            }
        }

        private static readonly IEqualityComparer<Guid> GuidComparerInstance = new GuidEqualityComparer();

        public static IEqualityComparer<Guid> GuidComparer
        {
            get { return GuidComparerInstance; }
        }


        [TestMethod]
        public void ContructorWithComparerTest()
        {
            var expected = typeof(GuidEqualityComparer);
            var testKeyTrait = new TestKeyTrait<Guid, TestKeyTraitItem>(new GuidEqualityComparer());
            var actual = testKeyTrait.Comparer;

            actual.Should().BeOfType<GuidEqualityComparer>();
        }
    }
}
