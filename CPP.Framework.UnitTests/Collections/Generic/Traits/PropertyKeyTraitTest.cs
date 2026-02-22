using System;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable once CheckNamespace
namespace CPP.Framework.Collections.Generic.Traits
{
    /// <summary>
    /// Test PropertyKeyTrait with different value types (int and long only, since GUID is tested in ProductKeyTraitTest) and a couple reference
    /// types (BaseType and DerivedType)
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class PropertyKeyTraitTest
    {
        #region Mock reference types for ID field

        private class BaseType { }

        private class DerivedType : BaseType { }

        /// <summary>
        /// A Mock key trait that requires property "KeyId" of "long" type
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        private class MockLongPropertyKeyTrait<TItem> : PropertyKeyTrait<long, TItem>
        {
            public MockLongPropertyKeyTrait() : base("KeyId") { }
        }

        /// <summary>
        /// A Mock key trait that requires property "KeyId" of "int" type
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        private class MockIntPropertyKeyTrait<TItem> : PropertyKeyTrait<int, TItem>
        {
            public MockIntPropertyKeyTrait() : base("KeyId") { }
        }

        /// <summary>
        /// A Mock key trait that requires property "KeyId" of "long" type
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        private class MockBaseTypePropertyKeyTrait<TItem> : PropertyKeyTrait<BaseType, TItem>
        {
            public MockBaseTypePropertyKeyTrait() : base("KeyId") { }
        }

        /// <summary>
        /// A Mock key trait that requires property "KeyId" of "int" type
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        private class MockIDerivedTypePropertyKeyTrait<TItem> : PropertyKeyTrait<DerivedType, TItem>
        {
            public MockIDerivedTypePropertyKeyTrait() : base("KeyId") { }
        }


        private class MockIntItem
        {
            public int KeyId { get; set; }
        }

        private class MockLongItem
        {
            public long KeyId { get; set; }
        }

        private class MockNoIdItem
        {
            public Guid NoKeyId { get; set; }
        }

        private class MockBaseTypeItem
        {
            public BaseType KeyId { get; set; }
        }

        private class MockDerivedTypeItem
        {
            public DerivedType KeyId { get; set; }
        }

        #endregion

        #region Test with missing ID field

        [TestMethod]
        public void CreatePropertyKeyTraitWithNoId()
        {
            Action act = () =>
            {
                var trait = new MockLongPropertyKeyTrait<MockNoIdItem>();
            };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("propertyName");
        }

        #endregion

        #region Test between long and int ID types

        [TestMethod]
        public void CreateMockLongPropertyKeyTrait()
        {
            var trait = new MockLongPropertyKeyTrait<MockLongItem>();

            trait.Should().NotBeNull();
            trait.PropertyName.Should().Be("KeyId");
            trait.Comparer.GetType().Name.Should().Be("GenericEqualityComparer`1");
            trait.Comparer.GetType().IsGenericType.Should().BeTrue();
            trait.Comparer.GetType().GenericTypeArguments[0].Should().Be(typeof(long));
        }

        [TestMethod]
        public void CreateMockLongPropertyKeyTraitWithIntItem()
        {
            Action act = () =>
            {
                var trait = new MockLongPropertyKeyTrait<MockIntItem>();
            };
            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void CreateMockIntPropertyKeyTrait()
        {
            var trait = new MockIntPropertyKeyTrait<MockIntItem>();

            trait.Should().NotBeNull();
            trait.PropertyName.Should().Be("KeyId");
            trait.Comparer.GetType().Name.Should().Be("GenericEqualityComparer`1");
            trait.Comparer.GetType().IsGenericType.Should().BeTrue();
            trait.Comparer.GetType().GenericTypeArguments[0].Should().Be(typeof(int));
        }

        [TestMethod]
        public void CreateMockIntPropertyKeyTraitWithLongtem()
        {
            Action act = () =>
            {
                var trait = new MockIntPropertyKeyTrait<MockLongItem>();
            };
            act.Should().Throw<ArgumentException>();
        }

        #endregion

        [TestMethod]
        public void CreateMockBaseTypePropertyKeyTrait()
        {
            var trait = new MockBaseTypePropertyKeyTrait<MockBaseTypeItem>();

            trait.Should().NotBeNull();
            trait.PropertyName.Should().Be("KeyId");
            trait.Comparer.GetType().Name.Should().Be("ObjectEqualityComparer`1");
            trait.Comparer.GetType().IsGenericType.Should().BeTrue();
            trait.Comparer.GetType().GenericTypeArguments[0].Should().Be(typeof(BaseType));
        }

        /// <summary>
        /// Inheritance of ID Type is ok.  Type check is loose
        /// </summary>
        [TestMethod]
        public void CreateMockBaseTypePropertyKeyTraitWithDerivedTypeItem()
        {
            var trait = new MockBaseTypePropertyKeyTrait<MockDerivedTypeItem>();
            trait.Should().NotBeNull();
            trait.PropertyName.Should().Be("KeyId");
            trait.Comparer.GetType().Name.Should().Be("ObjectEqualityComparer`1");
            trait.Comparer.GetType().IsGenericType.Should().BeTrue();
            trait.Comparer.GetType().GenericTypeArguments[0].Should().Be(typeof(BaseType));
        }

        [TestMethod]
        public void CreateMockDerivedTypePropertyKeyTrait()
        {
            var trait = new MockIDerivedTypePropertyKeyTrait<MockDerivedTypeItem>();

            trait.Should().NotBeNull();
            trait.PropertyName.Should().Be("KeyId");
            trait.Comparer.GetType().Name.Should().Be("ObjectEqualityComparer`1");
            trait.Comparer.GetType().IsGenericType.Should().BeTrue();
            trait.Comparer.GetType().GenericTypeArguments[0].Should().Be(typeof(DerivedType));
        }

        [TestMethod]
        public void CreateMockDerivedTypePropertyKeyTraitWithBaseTypeItem()
        {
            Action act = () =>
            {
                var trait = new MockIDerivedTypePropertyKeyTrait<MockBaseTypeItem>();
            };
            act.Should().Throw<ArgumentException>();
        }


    }
}
