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
    public class EqualityDelegateTest
    {
        private MockComparable _value11;
        private MockComparable _value12;
        private MockComparable _value21;
        private MockComparable _value22;

        [TestInitialize]
        public void Initialize()
        {
            _value11 = new MockComparable(1, Guid.NewGuid());
            _value22 = new MockComparable(2, Guid.NewGuid());
            _value12 = new MockComparable(_value11.IntValue, _value22.GuidValue);
            _value21 = new MockComparable(_value22.IntValue, _value11.GuidValue);
        }

        /// <summary>
        ///     A test that compares objects with default comparer.
        /// </summary>
        [TestMethod]
        public void DefaultComparerTest()
        {
            var comparer = InlineEqualityComparer<MockComparable>.Create(null);

            Verify.IsTrue(comparer.Equals(_value11, _value11));
            Verify.IsTrue(comparer.Equals(_value22, _value22));

            Verify.IsFalse(comparer.Equals(_value11, new MockComparable(_value11.IntValue, _value11.GuidValue)));
            Verify.IsFalse(comparer.Equals(_value22, new MockComparable(_value22.IntValue, _value22.GuidValue)));

            Verify.IsFalse(comparer.Equals(_value11, _value22));
            Verify.IsFalse(comparer.Equals(_value22, _value11));
            Verify.IsFalse(comparer.Equals(_value21, _value22));
            Verify.IsFalse(comparer.Equals(_value22, _value21));
            Verify.IsFalse(comparer.Equals(_value11, _value12));
            Verify.IsFalse(comparer.Equals(_value12, _value11));
            Verify.IsFalse(comparer.Equals(_value21, _value11));
            Verify.IsFalse(comparer.Equals(_value22, _value12));
            Verify.IsFalse(comparer.Equals(_value11, _value21));
            Verify.IsFalse(comparer.Equals(_value12, _value22));
        }

        /// <summary>
        ///     A test that compares the value of the mock objects by supplying an inline function.
        /// </summary>
        [TestMethod]
        public void ValueComparerDelegateTest()
        {
            var comparer =
                InlineEqualityComparer<MockComparable>.Create(
                    (x, y) => x.GuidValue == y.GuidValue && x.IntValue == y.IntValue);

            Verify.IsTrue(comparer.Equals(_value11, _value11));
            Verify.IsTrue(comparer.Equals(_value22, _value22));

            Verify.IsTrue(comparer.Equals(_value11, new MockComparable(_value11.IntValue, _value11.GuidValue)));
            Verify.IsTrue(comparer.Equals(_value22, new MockComparable(_value22.IntValue, _value22.GuidValue)));

            Verify.IsFalse(comparer.Equals(_value11, _value22));
            Verify.IsFalse(comparer.Equals(_value22, _value11));
            Verify.IsFalse(comparer.Equals(_value21, _value22));
            Verify.IsFalse(comparer.Equals(_value22, _value21));
            Verify.IsFalse(comparer.Equals(_value11, _value12));
            Verify.IsFalse(comparer.Equals(_value12, _value11));

            Verify.IsFalse(comparer.Equals(_value21, _value11));
            Verify.IsFalse(comparer.Equals(_value22, _value12));
            Verify.IsFalse(comparer.Equals(_value11, _value21));
            Verify.IsFalse(comparer.Equals(_value12, _value22));
        }

        /// <summary>
        ///     A test that compares only Guid value of the mock object by supplying an inline function.
        /// </summary>
        [TestMethod]
        public void PartialValueComparerDelegateTest()
        {
            var comparer = InlineEqualityComparer<MockComparable>.Create((x, y) => x.GuidValue == y.GuidValue);

            Verify.IsTrue(comparer.Equals(_value11, _value11));
            Verify.IsTrue(comparer.Equals(_value22, _value22));

            Verify.IsTrue(comparer.Equals(_value11, new MockComparable(_value11.IntValue, _value11.GuidValue)));
            Verify.IsTrue(comparer.Equals(_value22, new MockComparable(_value22.IntValue, _value22.GuidValue)));

            Verify.IsFalse(comparer.Equals(_value11, _value22));
            Verify.IsFalse(comparer.Equals(_value22, _value11));
            Verify.IsFalse(comparer.Equals(_value21, _value22));
            Verify.IsFalse(comparer.Equals(_value22, _value21));
            Verify.IsFalse(comparer.Equals(_value11, _value12));
            Verify.IsFalse(comparer.Equals(_value12, _value11));

            Verify.IsTrue(comparer.Equals(_value21, _value11));
            Verify.IsTrue(comparer.Equals(_value22, _value12));
            Verify.IsTrue(comparer.Equals(_value11, _value21));
            Verify.IsTrue(comparer.Equals(_value12, _value22));
        }

        private class MockComparable
        {
            public MockComparable(int intValue, Guid guidValue)
            {
                IntValue = intValue;
                GuidValue = guidValue;
            }

            public int IntValue { get; set; }
            public Guid GuidValue { get; set; }
        }
    }
}