using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable CheckNamespace
namespace System.Linq
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Reviewed")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local", Justification = "Reviewed")]
    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Reviewed")]
    public class IEnumerableExtensionsTest
    {
        [TestMethod]
        public void DistinctWithCustomSelector()
        {
            var expect = CreateMockObjects(5).ToArray();
            var random = expect.Select(obj => new MockObject(obj.GuidValue)).ToArray();
            var source = new List<MockObject>();

            for (var i = 0; i < expect.Length; i++)
            {
                source.Add(expect[i]);
                source.Add(random[i]);
            }
            var actual = source.Distinct(obj => obj.GuidValue).ToArray();

            actual.Length.Should().Be(expect.Length);
            expect.SequenceEqual(actual).Should().BeTrue();
            random.Where((obj, idx) => ReferenceEquals(obj, actual[idx])).Any().Should().BeFalse();
        }

        [TestMethod]
        public void WhereAnyOfWithMultipleValuesAsEnumerable()
        {
            var expect = new HashSet<MockObject>(CreateMockObjects(3));
            var random = new HashSet<MockObject>(CreateMockObjects(7));
            var source = new HashSet<MockObject>();

            var filter = expect.Select(obj => obj.GuidValue);
            source.UnionWith(expect);
            source.UnionWith(random);
            var result = IEnumerableExtensions.WhereAnyOf(source, obj => obj.GuidValue, filter);
            var actual = result.ToArray();

            // this assert verifies that the result was constructed to use delayed execution, and
            // has not already enumerated the sequence, which is very important for chaining Linq
            // calls. therfore, if this assert fails, it's a pretty good indication that callers'
            // performance numbers are going to suffer if they use this extension method.
            result.Should().BeAssignableTo<IEnumerable<MockObject>>();

            actual.Length.Should().Be(expect.Count);
            expect.SetEquals(actual).Should().BeTrue();
            source.IsProperSupersetOf(actual).Should().BeTrue();
            random.Overlaps(actual).Should().BeFalse();
        }

        [TestMethod]
        public void WhereAnyOfWithMultipleValuesAsQueryable()
        {
            var expect = new HashSet<MockObject>(CreateMockObjects(3));
            var random = new HashSet<MockObject>(CreateMockObjects(7));
            var source = new HashSet<MockObject>();

            var filter = expect.Select(obj => obj.GuidValue);
            source.UnionWith(expect);
            source.UnionWith(random);
            var result = IEnumerableExtensions.WhereAnyOf(source.AsQueryable(), obj => obj.GuidValue, filter);
            var actual = result.ToArray();

            result.Should().BeAssignableTo<IQueryable<MockObject>>();
            actual.Length.Should().Be(expect.Count);
            expect.SetEquals(actual).Should().BeTrue();
            source.IsProperSupersetOf(actual).Should().BeTrue();
            random.Overlaps(actual).Should().BeFalse();
        }

        #region Internal Helper Functions

        private static IEnumerable<MockObject> CreateMockObjects(int count)
        {
            for (var i = 0; i < count; i++) yield return new MockObject();
        }

        #endregion // Internal Helper Functions

        #region SampleObject Class Declaration

        private class MockObject : IEquatable<MockObject>
        {
            public MockObject() : this(Guid.NewGuid()) { }

            public MockObject(Guid guidValue)
            {
                GuidValue = guidValue;
            }

            public Guid GuidValue { get; }

            public bool Equals(MockObject other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return this.GuidValue.Equals(other.GuidValue);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MockObject)obj);
            }

            public override int GetHashCode() => this.GuidValue.GetHashCode();

            public static bool operator ==(MockObject left, MockObject right) => Equals(left, right);

            public static bool operator !=(MockObject left, MockObject right) => !Equals(left, right);
        }

        #endregion // SampleObject Class Declaration
    }
}