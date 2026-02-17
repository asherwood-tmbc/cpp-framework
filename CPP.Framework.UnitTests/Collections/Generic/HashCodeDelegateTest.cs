using System;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Collections.Generic
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class HashCodeDelegateTest
    {
        private MockItem _mockValue;
        private int _defaultHashCode;

        private class MockItem
        {
            public MockItem(Guid guidValue)
            {
                GuidValue = guidValue;
            }

            public Guid GuidValue { get; set; }

            protected bool Equals(MockItem other)
            {
                return GuidValue.Equals(other.GuidValue);
            }

            public override int GetHashCode()
            {
                return GuidValue.GetHashCode();
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            _mockValue = new MockItem(Guid.NewGuid());
            _defaultHashCode = _mockValue.GetHashCode();
        }

        /// <summary>
        ///     A test that specifies null hash code delegate, which defaults DefaultComparer.GetHashCode().
        /// </summary>
        [TestMethod]
        public void NullHashcodeDelegateTest()
        {
            var comparer = InlineEqualityComparer<MockItem>.Create(null);

            Verify.AreEqual(_defaultHashCode, comparer.GetHashCode(_mockValue));
        }

        /// <summary>
        /// A test that specifieds a hash code delegate that returns the length of a guid valuable in binary format
        /// </summary>
        [TestMethod]
        public void NonNullHashcodeDelegateTest()
        {
            var comparer = InlineEqualityComparer<MockItem>.Create(null, x => x.GuidValue.ToByteArray().Length);

            Verify.AreEqual(_mockValue.GuidValue.ToByteArray().Length, comparer.GetHashCode(_mockValue));
        }


        /// <summary>
        ///     A test that specifies null hash code delegate, which defaults DefaultComparer.GetHashCode().
        /// </summary>
        [TestMethod]
        public void NullHashcodeDelegateWithComparableDelegateTest()
        {
            var comparer = InlineEqualityComparer<MockItem>.Create((x, y) => x.GuidValue == y.GuidValue);

            Verify.AreEqual(_defaultHashCode, comparer.GetHashCode(_mockValue));
        }

        /// <summary>
        /// A test that specifieds a hash code delegate that returns the length of a guid valuable in binary format
        /// </summary>
        [TestMethod]
        public void NonNullHashcodeDelegateWithComparableDelegateTest()
        {
            var comparer = InlineEqualityComparer<MockItem>.Create((x, y) => x.GuidValue == y.GuidValue, x => x.GuidValue.ToByteArray().Length);

            Verify.AreEqual(_mockValue.GuidValue.ToByteArray().Length, comparer.GetHashCode(_mockValue));
        }
       
    }
}