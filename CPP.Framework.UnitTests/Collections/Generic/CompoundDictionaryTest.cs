using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CPP.Framework.Collections.Generic.Traits;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace

namespace CPP.Framework.Collections.Generic
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class CompoundDictionaryTestWithGuidKey1IntKey2
    {
        private MockCompoundDictionary<MockItem, MockGuidKeyTrait, MockIntKeyTrait> _dictionary;
        private MockItem[] _mockItems;
        private MockItem[] _duplicateDiffGuidItems;
        private MockItem[] _duplicateDiffIntItems;

        /// <summary>
        ///     A Mock key trait that requires property "KeyId" of "long" type
        /// </summary>
        private class MockGuidKeyTrait : LambdaKeyTrait<Guid, MockItem>
        {
            public MockGuidKeyTrait() : base(m => m.KeyId)
            {
            }
        }

        /// <summary>
        ///     A Mock key trait that requires property "KeyId" of "int" type
        /// </summary>
        private class MockIntKeyTrait : LambdaKeyTrait<int, MockItem>
        {
            public MockIntKeyTrait() : base(m => m.NumberId)
            {
            }
        }

        private class MockItem
        {
            public MockItem(Guid keyId, int numberId)
            {
                KeyId = keyId;
                NumberId = numberId;
                Value = KeyId.ToString();
            }

            public Guid KeyId { get; set; }
            public int NumberId { get; set; }
            public string Value { get; set; }

            public override string ToString()
            {
                return string.Format("KeyId: {0}, NumberId: {1}, Value: {2}", KeyId, NumberId, Value);
            }

            #region Generated Equality comparaer
            protected bool Equals(MockItem other)
            {
                return KeyId.Equals(other.KeyId) && NumberId == other.NumberId && string.Equals(Value, other.Value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((MockItem) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = KeyId.GetHashCode();
                    hashCode = (hashCode * 397) ^ NumberId;
                    hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                    return hashCode;
                }
            }

            private sealed class KeyIdNumberIdValueEqualityComparer : IEqualityComparer<MockItem>
            {
                public bool Equals(MockItem x, MockItem y)
                {
                    if (ReferenceEquals(x, y)) return true;
                    if (ReferenceEquals(x, null)) return false;
                    if (ReferenceEquals(y, null)) return false;
                    if (x.GetType() != y.GetType()) return false;
                    return x.KeyId.Equals(y.KeyId) && x.NumberId == y.NumberId && string.Equals(x.Value, y.Value);
                }

                public int GetHashCode(MockItem obj)
                {
                    unchecked
                    {
                        var hashCode = obj.KeyId.GetHashCode();
                        hashCode = (hashCode * 397) ^ obj.NumberId;
                        hashCode = (hashCode * 397) ^ (obj.Value != null ? obj.Value.GetHashCode() : 0);
                        return hashCode;
                    }
                }
            }

            private static readonly IEqualityComparer<MockItem> KeyIdNumberIdValueComparerInstance = new KeyIdNumberIdValueEqualityComparer();

            public static IEqualityComparer<MockItem> KeyIdNumberIdValueComparer
            {
                get { return KeyIdNumberIdValueComparerInstance; }
            }
            #endregion
        }

        private class MockCompoundDictionary<TEntity, TKeyTrait1, TKeyTrait2> :
            CompoundDictionary<TEntity, Guid, int, TKeyTrait1, TKeyTrait2>
            where TEntity : MockItem
            where TKeyTrait1 : KeyTrait<Guid, TEntity>
            where TKeyTrait2 : KeyTrait<int, TEntity>
        {
            static TEntity CreateIdentity(Guid keyId, int numberId)
            {
                return (TEntity) new MockItem(keyId, numberId);
            }

            public IDictionary<int, TEntity> GetItemsByKey1(Guid key1)
            {
                return GetDictionaryByKey1(key1);
            }

            public IDictionary<Guid, TEntity> GetItemsByKey2(int key2)
            {
                return GetDictionaryByKey2(key2);
            }

            public TEntity GetItemsByKeys(Guid key1, int key2)
            {
                return GetOrAdd(key1, key2, CreateIdentity);
            }
        }


        [TestInitialize]
        public void Initialize()
        {
            _dictionary = new MockCompoundDictionary<MockItem, MockGuidKeyTrait, MockIntKeyTrait>();
            _mockItems = new MockItem[5];
            _duplicateDiffGuidItems = new MockItem[5];
            _duplicateDiffIntItems = new MockItem[5];

            for (var i = 0; i < _mockItems.Length; i++)
            {
                _mockItems[i] = new MockItem(Guid.NewGuid(), i+1);
            }

            for (var i = 0; i < _mockItems.Length; i++)
            {
                _duplicateDiffGuidItems[i] = new MockItem(Guid.NewGuid(), _mockItems[i].NumberId);
            }

            for (var i = 0; i < _mockItems.Length; i++)
            {
                _duplicateDiffIntItems[i] = new MockItem(_mockItems[i].KeyId, (i+1)*2);
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
            _dictionary.Count().Should().Be(_mockItems.Length);
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
            _dictionary.Count().Should().Be(5);

            var j = 0;
            using (var enumerator = _dictionary.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Should().Be(_mockItems[j]);
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
            _dictionary.Count().Should().Be(5);

            foreach (var mockItem in _mockItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(mockItem.KeyId, mockItem.NumberId);
                dictItem.Should().Be(mockItem);
            }

            // count should not change
            _dictionary.Count().Should().Be(5);
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
            _dictionary.Count().Should().Be(5);

            var j = 0;
            using (var enumerator = _dictionary.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Should().Be(_mockItems[j]);
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
            _dictionary.Count().Should().Be(5);

            for (var i = 0; i < _duplicateDiffGuidItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_duplicateDiffGuidItems[i]);
            }
            _dictionary.Count().Should().Be(10);

            foreach (var mockItem in _mockItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(mockItem.KeyId, mockItem.NumberId);
                dictItem.Should().Be(mockItem);
            }

            foreach (var duplicateDiffGuidItem in _duplicateDiffGuidItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(duplicateDiffGuidItem.KeyId, duplicateDiffGuidItem.NumberId);
                dictItem.Should().Be(duplicateDiffGuidItem);
            }

            // Make sure nothing is added
            _dictionary.Count().Should().Be(10);

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
                _dictionary.AddOrUpdate(_duplicateDiffIntItems[i]);
            }
            _dictionary.Count().Should().Be(10);

            foreach (var mockItem in _mockItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(mockItem.KeyId, mockItem.NumberId);
                dictItem.Should().Be(mockItem);
            }

            foreach (var duplicateDiffIntItem in _duplicateDiffIntItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(duplicateDiffIntItem.KeyId, duplicateDiffIntItem.NumberId);
                dictItem.Should().Be(duplicateDiffIntItem);
            }

            // Make sure nothing is added
            _dictionary.Count().Should().Be(10);
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
            _dictionary.Count().Should().Be(5);

            foreach (var mockItem in _mockItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(mockItem.KeyId, mockItem.NumberId);
                dictItem.Should().Be(mockItem);
            }

            // These items do not exist in the dictionary (key2 is different), and should be inserted
            foreach (var duplicateDiffGuidItem in _duplicateDiffGuidItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(duplicateDiffGuidItem.KeyId, duplicateDiffGuidItem.NumberId);
                dictItem.Should().Be(duplicateDiffGuidItem);
            }

            // Make sure the different items were added
            _dictionary.Count().Should().Be(10);

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
            _dictionary.Count().Should().Be(5);

            foreach (var mockItem in _mockItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(mockItem.KeyId, mockItem.NumberId);
                dictItem.Should().Be(mockItem);
            }

            // These items do not exist in the dictionary, and should be inserted
            foreach (var duplicateDiffIntItem in _duplicateDiffIntItems)
            {
                var dictItem = _dictionary.GetItemsByKeys(duplicateDiffIntItem.KeyId, duplicateDiffIntItem.NumberId);
                dictItem.Should().Be(duplicateDiffIntItem);
            }

            // Make sure the different items were added
            _dictionary.Count().Should().Be(10);
        }

        /// <summary>
        ///  A test to validate removal of items by using an item.
        /// </summary>
        [TestMethod]
        public void AddAndRemoveByItem()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
            }
            _dictionary.Count().Should().Be(5);

            foreach (var mockItem in _mockItems)
            {
                _dictionary.Remove(mockItem).Should().BeTrue();
            }

            // verify empty
            _dictionary.Count().Should().Be(0);
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
            _dictionary.Count().Should().Be(5);

            foreach (var mockItem in _mockItems)
            {
                _dictionary.Remove(mockItem.KeyId, mockItem.NumberId).Should().BeTrue();
            }

            // verify empty
            _dictionary.Count().Should().Be(0);
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
            _dictionary.Count().Should().Be(5);

            foreach (var item in _mockItems)
            {
                _dictionary.Remove(item.KeyId, 0).Should().BeFalse();
            }

            // verify empty
            _dictionary.Count().Should().Be(5);
        }

        /// <summary>
        /// A test to validate removal of items by matching just one key. The removal is expected to fail.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void AddAndRemoveByKey2AndIncorrectKey1()
        {
            for (var i = 0; i < _mockItems.Length; i++)
            {
                _dictionary.AddOrUpdate(_mockItems[i]);
            }
            _dictionary.Count().Should().Be(5);

            foreach (var item in _mockItems)
            {
                _dictionary.Remove(Guid.Empty, item.NumberId).Should().BeFalse();
            }

            // verify empty
            _dictionary.Count().Should().Be(5);
        }
    }
}