using System;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
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

            comparer.Equals(_value11, _value11).Should().BeTrue();
            comparer.Equals(_value22, _value22).Should().BeTrue();

            comparer.Equals(_value11, new MockComparable(_value11.IntValue, _value11.GuidValue)).Should().BeFalse();
            comparer.Equals(_value22, new MockComparable(_value22.IntValue, _value22.GuidValue)).Should().BeFalse();

            comparer.Equals(_value11, _value22).Should().BeFalse();
            comparer.Equals(_value22, _value11).Should().BeFalse();
            comparer.Equals(_value21, _value22).Should().BeFalse();
            comparer.Equals(_value22, _value21).Should().BeFalse();
            comparer.Equals(_value11, _value12).Should().BeFalse();
            comparer.Equals(_value12, _value11).Should().BeFalse();
            comparer.Equals(_value21, _value11).Should().BeFalse();
            comparer.Equals(_value22, _value12).Should().BeFalse();
            comparer.Equals(_value11, _value21).Should().BeFalse();
            comparer.Equals(_value12, _value22).Should().BeFalse();
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

            comparer.Equals(_value11, _value11).Should().BeTrue();
            comparer.Equals(_value22, _value22).Should().BeTrue();

            comparer.Equals(_value11, new MockComparable(_value11.IntValue, _value11.GuidValue)).Should().BeTrue();
            comparer.Equals(_value22, new MockComparable(_value22.IntValue, _value22.GuidValue)).Should().BeTrue();

            comparer.Equals(_value11, _value22).Should().BeFalse();
            comparer.Equals(_value22, _value11).Should().BeFalse();
            comparer.Equals(_value21, _value22).Should().BeFalse();
            comparer.Equals(_value22, _value21).Should().BeFalse();
            comparer.Equals(_value11, _value12).Should().BeFalse();
            comparer.Equals(_value12, _value11).Should().BeFalse();

            comparer.Equals(_value21, _value11).Should().BeFalse();
            comparer.Equals(_value22, _value12).Should().BeFalse();
            comparer.Equals(_value11, _value21).Should().BeFalse();
            comparer.Equals(_value12, _value22).Should().BeFalse();
        }

        /// <summary>
        ///     A test that compares only Guid value of the mock object by supplying an inline function.
        /// </summary>
        [TestMethod]
        public void PartialValueComparerDelegateTest()
        {
            var comparer = InlineEqualityComparer<MockComparable>.Create((x, y) => x.GuidValue == y.GuidValue);

            comparer.Equals(_value11, _value11).Should().BeTrue();
            comparer.Equals(_value22, _value22).Should().BeTrue();

            comparer.Equals(_value11, new MockComparable(_value11.IntValue, _value11.GuidValue)).Should().BeTrue();
            comparer.Equals(_value22, new MockComparable(_value22.IntValue, _value22.GuidValue)).Should().BeTrue();

            comparer.Equals(_value11, _value22).Should().BeFalse();
            comparer.Equals(_value22, _value11).Should().BeFalse();
            comparer.Equals(_value21, _value22).Should().BeFalse();
            comparer.Equals(_value22, _value21).Should().BeFalse();
            comparer.Equals(_value11, _value12).Should().BeFalse();
            comparer.Equals(_value12, _value11).Should().BeFalse();

            comparer.Equals(_value21, _value11).Should().BeTrue();
            comparer.Equals(_value22, _value12).Should().BeTrue();
            comparer.Equals(_value11, _value21).Should().BeTrue();
            comparer.Equals(_value12, _value22).Should().BeTrue();
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