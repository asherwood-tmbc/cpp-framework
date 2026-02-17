using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CPP.Framework.Collections.Generic.Traits;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace

namespace CPP.Framework.Collections.Generic
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class CompoundDictionaryTest2WithGuidKeys
    {
        private MockCompoundDictionary<MockItem, MockGuidKey1Trait, MockGuidKey2Trait> _dictionary;
        private MockItem[] _mockItems;
        private MockItem[] _duplicateDiffKey1Items;
        private MockItem[] _duplicateDiffKey2Items;

        /// <summary>
        ///     A Mock key trait that requires property "KeyId" of "long" type
        /// </summary>
        private class MockGuidKey1Trait : LambdaKeyTrait<Guid, MockItem>
        {
            public MockGuidKey1Trait() : base(m => m.KeyId)
            {
            }
        }

        /// <summary>
        ///     A Mock key trait that requires property "KeyId" of "long" type
        /// </summary>
        private class MockGuidKey2Trait : LambdaKeyTrait<Guid, MockItem>
        {
            public MockGuidKey2Trait()
                : base(m => m.KeyId2)
            {
            }
        }


       private class MockItem 
        {
           public MockItem(Guid keyId, Guid keyId2)
            {
                KeyId = keyId;
                KeyId2 = keyId2;
                Value = KeyId.ToString();
            }

            public Guid KeyId { get; set; }
            public Guid KeyId2 { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                return string.Format("KeyId: {0}, KeyId2: {1}, Value: {2}", KeyId, KeyId2, Value);
            }

            #region Generated Equality comparaer

            protected bool Equals(MockItem other)
            {
                return KeyId.Equals(other.KeyId) && KeyId2.Equals(other.KeyId2) && string.Equals(Value, other.Value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MockItem) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = KeyId.GetHashCode();
                    hashCode = (hashCode * 397) ^ KeyId2.GetHashCode();
                    hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                    return hashCode;
                }
            }

            private sealed class KeyIdKeyId2ValueEqualityComparer : IEqualityComparer<MockItem>
            {
                public bool Equals(MockItem x, MockItem y)
                {
                    if (ReferenceEquals(x, y)) return true;
                    if (ReferenceEquals(x, null)) return false;
                    if (ReferenceEquals(y, null)) return false;
                    if (x.GetType() != y.GetType()) return false;
                    return x.KeyId.Equals(y.KeyId) && x.KeyId2.Equals(y.KeyId2) && string.Equals(x.Value, y.Value);
                }

                public int GetHashCode(MockItem obj)
                {
                    unchecked
                    {
                        var hashCode = obj.KeyId.GetHashCode();
                        hashCode = (hashCode * 397) ^ obj.KeyId2.GetHashCode();
                        hashCode = (hashCode * 397) ^ (obj.Value != null ? obj.Value.GetHashCode() : 0);
                        return hashCode;
                    }
                }
            }

            private static readonly IEqualityComparer<MockItem> KeyIdKeyId2ValueComparerInstance = new KeyIdKeyId2ValueEqualityComparer();

            public static IEqualityComparer<MockItem> KeyIdKeyId2ValueComparer
            {
                get { return KeyIdKeyId2ValueComparerInstance; }
            }

            #endregion
        }

        private class MockCompoundDictionary<TEntity, TKeyTrait1, TKeyTrait2> :
            CompoundDictionary<TEntity, Guid, Guid, TKeyTrait1, TKeyTrait2>
            where TEntity : MockItem
            where TKeyTrait1 : KeyTrait<Guid, TEntity>
            where TKeyTrait2 : KeyTrait<Guid, TEntity>
        {
            static TEntity CreateIdentity(Guid keyId, Guid keyId2)
            {
                return (TEntity) new MockItem(keyId, keyId2);
            }

            public IDictionary<Guid, TEntity> GetItemsByKey1(Guid key1)
            {
                return this.GetDictionaryByKey1(key1);
            }

            public IDictionary<Guid, TEntity> GetItemsByKey2(Guid key2)
            {
                return this.GetDictionaryByKey2(key2);
            }

            public TEntity GetItemsByKeys(Guid key1, Guid key2)
            {
                return GetOrAdd(key1, key2, CreateIdentity);
            }
        }


        [TestInitialize]
        public void Initialize()
        {
            _dictionary = new MockCompoundDictionary<MockItem, MockGuidKey1Trait, MockGuidKey2Trait>();
            _mockItems = new MockItem[5];
            _duplicateDiffKey1Items = new MockItem[5];
            _duplicateDiffKey2Items = new MockItem[5];

            for (var i = 0; i < _mockItems.Length; i++)
            {
                _mockItems[i] = new MockItem(Guid.NewGuid(), Guid.NewGuid());
            }

            for (var i = 0; i < _mockItems.Length; i++)
            {
                _duplicateDiffKey1Items[i] = new MockItem(Guid.NewGuid(), _mockItems[i].KeyId2);
            }

            for (var i = 0; i < _mockItems.Length; i++)
            {
                _duplicateDiffKey2Items[i] = new MockItem(_mockItems[i].KeyId, Guid.NewGuid());
            }
        }

        /// <summary>
        /// A test for AddOrUpdate() and make sure items added are reflected in the totoal item count of the dictionary.
        /// </summary>
        [TestMethod]
        public void AddOrUpdate()
        {
            foreach (var item in _mockItems)
            {
                _dictionary.AddOrUpdate(item);
            }
            Verify.AreEqual(_mockItems.Length, _dictionary.Count());
        }

        /// <summary>
        /// A test for GetEnumerator() and make sure all items retrieved are equalivent to the items inserted
        /// </summary>
        [TestMethod]
        public void GetEnumerator()
        {
            foreach (var item in _mockItems)
            {
                _dictionary.AddOrUpdate(item);
            }
            Verify.AreEqual(5, _dictionary.Count());

            var j = 0;
            using (var enumerator = _dictionary.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Verify.AreEqual(_mockItems[j], enumerator.Current);
                    j++;
                }
            }
        }

        /// <summary>
        /// A test for GetItemsByKeys() and make sure all items retrieved are equalivent to the items inserted
        /// </summary>
        [TestMethod]
        public void GetItemsByKeys()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
            }
            Verify.AreEqual(5, _dictionary.Count());

            foreach (var mockItem in _mockItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(mockItem.KeyId, mockItem.KeyId2);
                Verify.AreEqual(mockItem, dictItem);
            }

            // count should not change
            Verify.AreEqual(5, _dictionary.Count());
        }

        /// <summary>
        /// A test for AddAndUpdate() functions.  The second add becomes "update" and should not affect dictionary
        /// </summary>
        [TestMethod]
        public void AddDuplicate()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
                _dictionary.AddOrUpdate(_mockItems[i]);
            }
            Verify.AreEqual(5, _dictionary.Count());

            var j = 0;
            using (var enumerator = _dictionary.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Verify.AreEqual(_mockItems[j], enumerator.Current);
                    j++;
                }
            }
        }

        /// <summary>
        /// A test for AddAndUpdate() functions
        /// </summary>
        [TestMethod]
        public void AddAndUpdateWithKey1Changed()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
            }
            Verify.AreEqual(5, _dictionary.Count());

            for (var i = 0; i < _duplicateDiffKey1Items.Length; i++)
            {
                _dictionary.AddOrUpdate(_duplicateDiffKey1Items[i]);
            }
            Verify.AreEqual(10, _dictionary.Count());

            foreach (var mockItem in _mockItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(mockItem.KeyId, mockItem.KeyId2);
                Verify.AreEqual(mockItem, dictItem);
            }

            foreach (var duplicateDiffGuidItem in _duplicateDiffKey1Items)
            {
                var dictItem = _dictionary.GetItemsByKeys(duplicateDiffGuidItem.KeyId, duplicateDiffGuidItem.KeyId2);
                Verify.AreEqual(duplicateDiffGuidItem, dictItem);
            }

            // Make sure nothing is added
            Verify.AreEqual(10, _dictionary.Count());

        }

        /// <summary>
        /// A test for AddAndUpdate() functions
        /// </summary>
        [TestMethod]
        public void AddAndUpdateWithKey2Changed()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
                _dictionary.AddOrUpdate(_duplicateDiffKey2Items[i]);
            }
            Verify.AreEqual(10, _dictionary.Count());

            foreach (var mockItem in _mockItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(mockItem.KeyId, mockItem.KeyId2);
                Verify.AreEqual(mockItem, dictItem);
            }

            foreach (var duplicateDiffIntItem in _duplicateDiffKey2Items)
            {
                var dictItem = _dictionary.GetItemsByKeys(duplicateDiffIntItem.KeyId, duplicateDiffIntItem.KeyId2);
                Verify.AreEqual(duplicateDiffIntItem, dictItem);
            }

            // Make sure nothing is added
            Verify.AreEqual(10, _dictionary.Count());
        }

        /// <summary>
        /// Test GetItemByKeys() and make sure new items are inserted upon calling GetItemByKeys()
        /// </summary>
        [TestMethod]
        public void GetAndInsertWithKey1Changed()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
            }
            Verify.AreEqual(5, _dictionary.Count());

            foreach (var mockItem in _mockItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(mockItem.KeyId, mockItem.KeyId2);
                Verify.AreEqual(mockItem, dictItem);
            }

            // These items do not exist in the dictionary (key2 is different), and should be inserted
            foreach (var duplicateDiffGuidItem in _duplicateDiffKey1Items)
            {
                var dictItem = _dictionary.GetItemsByKeys(duplicateDiffGuidItem.KeyId, duplicateDiffGuidItem.KeyId2);
                Verify.AreEqual(duplicateDiffGuidItem, dictItem);
            }

            // Make sure the different items were added 
            Verify.AreEqual(10, _dictionary.Count());

        }

        /// <summary>
        /// Test GetItemByKeys() and make sure new items are inserted upon calling GetItemByKeys()
        /// </summary>
        [TestMethod]
        public void GetItemByKeysWithKey2Changed()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
            }
            Verify.AreEqual(5, _dictionary.Count());
            
            foreach (var mockItem in _mockItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(mockItem.KeyId, mockItem.KeyId2);
                Verify.AreEqual(mockItem, dictItem);
            }

            // These items do not exist in the dictionary (key2 is different), and should be inserted
            foreach (var duplicateDiffIntItem in _duplicateDiffKey2Items)
            {
                var dictItem = _dictionary.GetItemsByKeys(duplicateDiffIntItem.KeyId, duplicateDiffIntItem.KeyId2);
                Verify.AreEqual(duplicateDiffIntItem, dictItem);
            }

            // Make sure the different items were added 
            Verify.AreEqual(10, _dictionary.Count());
        }

        /// <summary>
        /// A test to validate removal of items by using an item.
        /// </summary>
        [TestMethod]
        public void AddAndRemoveByItem()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
            }
            Verify.AreEqual(5, _dictionary.Count());

            foreach (var mockItem in _mockItems)
            {
                Verify.IsTrue(_dictionary.Remove(mockItem));
            }

            // verify empty
            Verify.AreEqual(0, _dictionary.Count());
        }

        /// <summary>
        /// A test to validate removal of items by matching both keys.
        /// </summary>
        [TestMethod]
        public void AddAndRemoveByKeys()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
            }
            Verify.AreEqual(5, _dictionary.Count());

            foreach (var mockItem in _mockItems)
            {
                Verify.IsTrue(_dictionary.Remove(mockItem.KeyId, mockItem.KeyId2));
            }

            // verify empty
            Verify.AreEqual(0, _dictionary.Count());
        }

        /// <summary>
        /// A test to validate removal of items by matching just one key. The removal is expected to fail. 
        /// </summary>
        [TestMethod]
        [Ignore]
        public void AddAndRemoveByKey1AndIncorrectKey2()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
            }
            Verify.AreEqual(5, _dictionary.Count());

            foreach (var item in _mockItems)
            {
                Verify.IsFalse(_dictionary.Remove(item.KeyId, Guid.NewGuid()));
            }

            // verify empty
            Verify.AreEqual(5, _dictionary.Count());
        }

        /// <summary>
        /// A test to validate removal of items by matching just one key.  The removal is expected to fail.  
        /// This is a redundent test.  It is preserved just in case if future expectation is changed.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void AddAndRemoveByKey1AndIncorrectKey2WithDuplicateKey1()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
                _dictionary.AddOrUpdate(_duplicateDiffKey2Items[i]);
            }
            Verify.AreEqual(10, _dictionary.Count());

            foreach (var item in _mockItems)
            {
                Verify.IsFalse(_dictionary.Remove(item.KeyId, Guid.NewGuid()));
            }

            // verify empty
            Verify.AreEqual(10, _dictionary.Count());
        }

        /// <summary>
        /// A test to validate removal of items by matching just one key.  The removal is expected to fail.  
        /// This is a redundent test.  It is preserved just in case if future expectation is changed.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void AddAndRemoveByKey1AndIncorrectKey2WithDuplicateKey2()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
                _dictionary.AddOrUpdate(_duplicateDiffKey1Items[i]);
            }
            Verify.AreEqual(10, _dictionary.Count());

            for (var i = 0; i < _mockItems.Length; i++) 
            {
                Verify.IsFalse(_dictionary.Remove(_mockItems[i].KeyId, Guid.NewGuid()));
                Verify.IsFalse(_dictionary.Remove(_duplicateDiffKey1Items[i].KeyId, Guid.NewGuid()));
            }

            // verify empty
            Verify.AreEqual(10, _dictionary.Count());
        }

        /// <summary>
        /// A test to validate removal of items by matching just one key.  The removal is expected to fail.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void AddAndRemoveByKey2AndIncorrectKey1()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
            }
            Verify.AreEqual(5, _dictionary.Count());

            foreach (var item in _mockItems)
            {
                Verify.IsFalse(_dictionary.Remove(Guid.NewGuid(), item.KeyId2));
            }

            // verify empty
            Verify.AreEqual(5, _dictionary.Count());
        }

        /// <summary>
        /// A test to validate removal of items by matching just one key.  The removal is expected to fail.  
        /// This is a redundent test.  It is preserved just in case if future expectation is changed.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void AddAndRemoveByKey2AndIncorrectKey1WithDuplicateKey2()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
                _dictionary.AddOrUpdate(_duplicateDiffKey1Items[i]);
            }
            Verify.AreEqual(10, _dictionary.Count());

            foreach (var item in _mockItems)
            {
                Verify.IsFalse(_dictionary.Remove(Guid.NewGuid(), item.KeyId2));
            }

            // verify empty
            Verify.AreEqual(10, _dictionary.Count());
        }

        /// <summary>
        /// A test to validate removal of items by matching just one key.  The removal is expected to fail.  
        /// This is a redundent test.  It is preserved just in case if future expectation is changed.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void AddAndRemoveByKey2AndIncorrectKey1WithDuplicateKey1()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
                _dictionary.AddOrUpdate(_duplicateDiffKey2Items[i]);
            }
            Verify.AreEqual(10, _dictionary.Count());

            for (var i = 0; i < _mockItems.Length; i++)
            {
                Verify.IsFalse(_dictionary.Remove(Guid.NewGuid(), _mockItems[i].KeyId2));
                Verify.IsFalse(_dictionary.Remove(Guid.NewGuid(), _duplicateDiffKey2Items[i].KeyId2));
            }

            // verify empty
            Verify.AreEqual(10, _dictionary.Count());
        }
    }
}