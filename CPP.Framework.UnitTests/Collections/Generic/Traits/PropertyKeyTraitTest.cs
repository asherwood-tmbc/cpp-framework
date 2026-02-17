using System;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.Diagnostics.Testing;
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
        [ExpectedArgumentException("propertyName")]
        public void CreatePropertyKeyTraitWithNoId()
        {
            var trait = new MockLongPropertyKeyTrait<MockNoIdItem>();
        }

        #endregion

        #region Test between long and int ID types

        [TestMethod]
        public void CreateMockLongPropertyKeyTrait()
        {
            var trait = new MockLongPropertyKeyTrait<MockLongItem>();

            Verify.IsNotNull(trait);
            Verify.AreEqual("KeyId", trait.PropertyName);
            Verify.AreEqual("GenericEqualityComparer`1", trait.Comparer.GetType().Name);
            Verify.IsTrue( trait.Comparer.GetType().IsGenericType);
            Verify.AreEqual(typeof(long), trait.Comparer.GetType().GenericTypeArguments[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateMockLongPropertyKeyTraitWithIntItem()
        {
            var trait = new MockLongPropertyKeyTrait<MockIntItem>();
        }
        
        [TestMethod]
        public void CreateMockIntPropertyKeyTrait()
        {
            var trait = new MockIntPropertyKeyTrait<MockIntItem>();

            Verify.IsNotNull(trait);
            Verify.AreEqual("KeyId", trait.PropertyName);
            Verify.AreEqual("GenericEqualityComparer`1", trait.Comparer.GetType().Name);
            Verify.IsTrue(trait.Comparer.GetType().IsGenericType);
            Verify.AreEqual(typeof(int), trait.Comparer.GetType().GenericTypeArguments[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateMockIntPropertyKeyTraitWithLongtem()
        {
            var trait = new MockIntPropertyKeyTrait<MockLongItem>();
        }

        #endregion

        [TestMethod]
        public void CreateMockBaseTypePropertyKeyTrait()
        {
            var trait = new MockBaseTypePropertyKeyTrait<MockBaseTypeItem>();

            Verify.IsNotNull(trait);
            Verify.AreEqual("KeyId", trait.PropertyName);
            Verify.AreEqual("ObjectEqualityComparer`1", trait.Comparer.GetType().Name);
            Verify.IsTrue(trait.Comparer.GetType().IsGenericType);
            Verify.AreEqual(typeof(BaseType), trait.Comparer.GetType().GenericTypeArguments[0]);
        }

        /// <summary>
        /// Inheritance of ID Type is ok.  Type check is loose
        /// </summary>
        [TestMethod]
        public void CreateMockBaseTypePropertyKeyTraitWithDerivedTypeItem()
        {
            var trait = new MockBaseTypePropertyKeyTrait<MockDerivedTypeItem>();
            Verify.IsNotNull(trait);
            Verify.AreEqual("KeyId", trait.PropertyName);
            Verify.AreEqual("ObjectEqualityComparer`1", trait.Comparer.GetType().Name);
            Verify.IsTrue(trait.Comparer.GetType().IsGenericType);
            Verify.AreEqual(typeof(BaseType), trait.Comparer.GetType().GenericTypeArguments[0]);
        }

        [TestMethod]
        public void CreateMockDerivedTypePropertyKeyTrait()
        {
            var trait = new MockIDerivedTypePropertyKeyTrait<MockDerivedTypeItem>();

            Verify.IsNotNull(trait);
            Verify.AreEqual("KeyId", trait.PropertyName);
            Verify.AreEqual("ObjectEqualityComparer`1", trait.Comparer.GetType().Name);
            Verify.IsTrue(trait.Comparer.GetType().IsGenericType);
            Verify.AreEqual(typeof(DerivedType), trait.Comparer.GetType().GenericTypeArguments[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateMockDerivedTypePropertyKeyTraitWithBaseTypeItem()
        {
            var trait = new MockIDerivedTypePropertyKeyTrait<MockBaseTypeItem>();
        }


    }
}
