using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using CPP.Framework.UnitTests.Testing;
using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace System.Linq
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Reviewed")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local", Justification = "Reviewed")]
    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Reviewed")]
    public class IQueryableExtensionsTest
    {
        [TestMethod]
        public void WhereAnyOfWithMultipleValues()
        {
            var expect = new HashSet<MockObject>(CreateMockObjects(3));
            var random = new HashSet<MockObject>(CreateMockObjects(7));
            var source = new HashSet<MockObject>();

            var filter = expect.Select(obj => obj.GuidValue);
            source.UnionWith(expect);
            source.UnionWith(random);
            var actual = IQueryableExtensions.WhereAnyOf(source.AsQueryable(), obj => obj.GuidValue, filter).ToArray();

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

        #region MockObject Class Declaration

        private sealed class MockObject : IEquatable<MockObject>
        {
            public MockObject() : this(Guid.NewGuid()) { }

            public MockObject(Guid guidValue)
            {
                this.GuidValue = guidValue;
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
                return obj is MockObject o && Equals(o);
            }

            public override int GetHashCode() => this.GuidValue.GetHashCode();

            public static bool operator ==(MockObject left, MockObject right) => Equals(left, right);

            public static bool operator !=(MockObject left, MockObject right) => !Equals(left, right);
        }

        #endregion // MockObject Class Declaration
    }
}
