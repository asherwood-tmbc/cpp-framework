using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable once CheckNamespace
namespace CPP.Framework.Collections.Generic.Traits
{
    /// <summary>
    /// Test PropertyKeyTrait with different value types (int and long only, since GUID is tested in ProductKeyTraitTest) and a couple reference
    /// types (LambdaBaseType and LambdaDerivedType)
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public class LambdaKeyTraitTest
    {
        #region Mock reference types for ID field

        private class LambdaBaseType
        {
            public LambdaBaseType()
            {
                KeyId = 12345;
            }

            public int KeyId { get; set; }

            public sealed class KeyIdEqualityComparer : IEqualityComparer<LambdaBaseType>
            {
                public bool Equals(LambdaBaseType x, LambdaBaseType y)
                {
                    return x.KeyId == y.KeyId;
                }

                public int GetHashCode(LambdaBaseType obj)
                {
                    return obj.KeyId;
                }
            }

            public static readonly IEqualityComparer<LambdaBaseType> KeyIdComparerInstance = new KeyIdEqualityComparer();

            public static IEqualityComparer<LambdaBaseType> KeyIdComparer
            {
                get { return KeyIdComparerInstance; }
            }
        }

        private class LambdaDerivedType : LambdaBaseType
        {
            public LambdaDerivedType()
            {
                KeyId = 24680;
            }
        }

        /// <summary>
        /// A Mock key trait that requires property "KeyId" of "long" type
        /// </summary>
        private class MockLongLambdaKeyTrait : LambdaKeyTrait<long, MockLongIdModel>
        {
            public MockLongLambdaKeyTrait() : base(m => m.KeyId) { }
        }

        /// <summary>
        /// A Mock key trait that requires property "KeyId" of "int" type
        /// </summary>
        private class MockIntLambdaKeyTrait : LambdaKeyTrait<int, MockIntIdModel>
        {
            public MockIntLambdaKeyTrait() : base(m => m.KeyId) { }
        }

        /// <summary>
        /// A Mock key trait that requires property "KeyId" of "Base" type
        /// </summary>
        private class MockBaseTypeLambdaKeyTrait : LambdaKeyTrait<LambdaBaseType, MockBaseTypeIdModel>
        {
            public MockBaseTypeLambdaKeyTrait() : base(m => m.KeyId, LambdaBaseType.KeyIdComparer) { }
        }

        /// <summary>
        /// A Mock key trait that requires property "KeyId" of "Derived" type
        /// </summary>
        private class MockDerivedTypeLambdaKeyTrait : LambdaKeyTrait<LambdaDerivedType, MockDerivedTypeIdModel>
        {
            public MockDerivedTypeLambdaKeyTrait() : base(m => m.KeyId, LambdaBaseType.KeyIdComparer) { }
        }

        /// <summary>
        /// A Mock key trait that requires property "DifferentKeyId" of "GUID" type
        /// </summary>
        private class MockIGuidTypeLambdaKeyTrait : LambdaKeyTrait<Guid, MockGuidIdModel>
        {
            public MockIGuidTypeLambdaKeyTrait() : base(m => m.DifferentKeyId) { }
        }
        
        /// <summary>
        /// A Mock key trait that supplies a null for lambda expression
        /// </summary>
        private class MockINullLambdaKeyTrait : LambdaKeyTrait<Guid, MockGuidIdModel>
        {
            public MockINullLambdaKeyTrait() : base(null) { }
        }

        private class MockIntIdModel
        {
            public MockIntIdModel()
            {
                KeyId = Int32.MaxValue;
            }

            public int KeyId { get; set; }
        }

        private class MockLongIdModel
        {
            public MockLongIdModel()
            {
                KeyId = Int64.MaxValue;
            }

            public long KeyId { get; set; }
        }

        private class MockGuidIdModel
        {
            public MockGuidIdModel()
            {
                DifferentKeyId = Guid.Empty;
            }

            public Guid DifferentKeyId { get; set; }
        }

        private class MockBaseTypeIdModel
        {
            public MockBaseTypeIdModel()
            {
                KeyId = new LambdaBaseType();
            }

            public LambdaBaseType KeyId { get; set; }
        }

        private class MockDerivedTypeIdModel
        {
            public MockDerivedTypeIdModel()
            {
                KeyId = new LambdaDerivedType();
            }

            public LambdaDerivedType KeyId { get; set; }
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNullLambdaKeyTraiy()
        {
            var trait = new MockINullLambdaKeyTrait();
        }

        [TestMethod]
        public void CreateIntLambdaKeyTrait()
        {
            var trait = new MockIntLambdaKeyTrait();
            var item = new MockIntIdModel();

            Verify.IsNotNull(trait);
            Verify.AreEqual("GenericEqualityComparer`1", trait.Comparer.GetType().Name);
            Verify.IsTrue(trait.Comparer.GetType().IsGenericType);
            Verify.AreEqual(typeof(int), trait.Comparer.GetType().GenericTypeArguments[0]);

            var keyValue = trait.GetKeyValue(item);
            Verify.AreEqual(Int32.MaxValue, keyValue);
        }

        [TestMethod]
        public void CreateLongLambdaKeyTrait()
        {
            var trait = new MockLongLambdaKeyTrait();
            var item = new MockLongIdModel();

            Verify.IsNotNull(trait);
            Verify.AreEqual("GenericEqualityComparer`1", trait.Comparer.GetType().Name);
            Verify.IsTrue(trait.Comparer.GetType().IsGenericType);
            Verify.AreEqual(typeof(long), trait.Comparer.GetType().GenericTypeArguments[0]);

            var keyValue = trait.GetKeyValue(item);
            Verify.AreEqual(Int64.MaxValue, keyValue);
        }

        [TestMethod]
        public void CreateGuidLambdaKeyTrait()
        {
            var trait = new MockIGuidTypeLambdaKeyTrait();
            var item = new MockGuidIdModel();

            Verify.IsNotNull(trait);
            Verify.AreEqual("GenericEqualityComparer`1", trait.Comparer.GetType().Name);
            Verify.IsTrue(trait.Comparer.GetType().IsGenericType);
            Verify.AreEqual(typeof(Guid), trait.Comparer.GetType().GenericTypeArguments[0]);

            var keyValue = trait.GetKeyValue(item);
            Verify.AreEqual(Guid.Empty, keyValue);
        }

        [TestMethod]
        public void CreateBaseTypeLambdaKeyTrait()
        {
            var trait = new MockBaseTypeLambdaKeyTrait();
            var item = new MockBaseTypeIdModel();

            Verify.IsNotNull(trait);
            Verify.AreEqual("KeyIdEqualityComparer", trait.Comparer.GetType().Name);
            Verify.IsFalse(trait.Comparer.GetType().IsGenericType);

            var keyValue = trait.GetKeyValue(item);
            Verify.AreEqual(item.KeyId, keyValue);
        }

        [TestMethod]
        public void CreateDerivedTypeLambdaKeyTrait()
        {
            var trait = new MockDerivedTypeLambdaKeyTrait();
            var item = new MockDerivedTypeIdModel();

            Verify.IsNotNull(trait);
            Verify.AreEqual("KeyIdEqualityComparer", trait.Comparer.GetType().Name);
            Verify.IsFalse(trait.Comparer.GetType().IsGenericType);
           
            var keyValue = trait.GetKeyValue(item);
            Verify.AreEqual(item.KeyId, keyValue);
        }
    }
}
