using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Collections.Generic
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class SetDictionaryTest
    {
        private MockKey[] _mockKeys;
        private MockKey[] _mockKeys2;
        private MockItem[] _mockItems;
        private MockItem[] _mockItems2;

        private MockKey[] _mockKeysSubset;
        private MockKey[] _mockKeys2Subset;

        private MockKey[] _mockKeysSuperset;
        private MockKey[] _mockKeys2Superset;

        private class MockKey
        {
            public MockKey(Guid guidValue)
            {
                KeyValue = guidValue;
            }

            public Guid KeyValue { get; set; }
        }

        private class MockItem
        {
            public MockItem(Guid guidValue)
            {
                GuidValue = guidValue;
            }

            public Guid GuidValue { get; set; }
        }

        private class MockKeyComparer : IEqualityComparer<MockKey>
        {
            public bool Equals(MockKey x, MockKey y)
            {
                return x.KeyValue == y.KeyValue;
            }

            public int GetHashCode(MockKey obj)
            {
                if (obj == null || obj.KeyValue == null)
                {
                    throw new ArgumentNullException();
                }

                return obj.KeyValue.GetHashCode();
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            _mockKeys = new MockKey[5];
            _mockKeys2 = new MockKey[5];
            _mockItems = new MockItem[5];
            _mockItems2 = new MockItem[5];

            _mockKeysSubset = new MockKey[3];
            _mockKeys2Subset = new MockKey[3];
            
            _mockKeysSuperset = new MockKey[8];
            _mockKeys2Superset = new MockKey[8];

            for (int i = 0; i < _mockItems.Length; i++)
            {
                var guid = Guid.NewGuid();
                _mockKeys[i] = new MockKey(guid);
                _mockKeys2[i] = new MockKey(guid);
                _mockItems[i] = new MockItem(guid);
                _mockItems2[i] = new MockItem(guid);

                _mockKeysSuperset[i] = _mockKeys[i];
                _mockKeys2Superset[i] = _mockKeys2[i];

                if (i < 3)
                {
                    _mockKeysSubset[i] = _mockKeys[i];
                    _mockKeys2Subset[i] = _mockKeys2[i];
                }
            }

            for (int i = 5; i < 8; i++)
            {
                var guid = Guid.NewGuid(); 
                _mockKeysSuperset[i] = new MockKey(guid);
                _mockKeys2Superset[i] = new MockKey(guid);
            }
        }


        [TestMethod]
        [ExpectedException(typeof(System.Collections.Generic.KeyNotFoundException))]
        public void GetWithInvalidIndex()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            var actual = dict[new MockKey(Guid.NewGuid())];
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetWithNullIndex()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            var actual = dict[null];
        }

        [TestMethod]
        public void ClearTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                dict.Add(mockKey, item);
            }

            Verify.AreEqual(5, dict.Count);

            dict.Clear();

            Verify.AreEqual(0, dict.Count);
        }

        #region Constructor options tests

        [TestMethod]
        public void SetDictionaryWithDefaultEqualityComparerAndReferenceTypeKeyTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and newKeys are not equal, even though they have the same KeyValue
                dict.Add(mockKey, item);
                dict.Add(mockKey2, item2);
            }

            Verify.AreEqual(10, dict.Count);

            // _mockKeys and _mockKeys2 are considered to be completely different set, and therefore the 
            // dictionary should have 10 items, even though _mockKey[i] and _mockKey2[i] contains the same Guid.
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                Verify.AreEqual(_mockItems[i], dict[_mockKeys[i]]);
                Verify.AreNotEqual(_mockItems[i], dict[_mockKeys2[i]]);
                Verify.AreEqual(_mockItems2[i], dict[_mockKeys2[i]]);
                Verify.AreNotEqual(dict[_mockKeys[i]], dict[_mockKeys2[i]]);
            }
        }

        [TestMethod]
        public void SetDictionaryWithGuidKeyValueEqualityComparerAndReferenceTypeKeyTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>(new MockKeyComparer());

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and newKeys are considered equal, since the comparer only cares about the guid value
                // set of item2 should not be added to the dictionary
                dict.Add(mockKey, item);
                dict.Add(mockKey2, item2);
            }

            Verify.AreEqual(5, dict.Count);

            // Second set of keys are considered the same as the first, therefore, the insertion of mockItems2 never happened,
            // and _mockKey2[i] resolves to _mockKey[i], and the dictionary has only 5 entries.
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                Verify.AreEqual(_mockItems[i], dict[_mockKeys[i]]);
                Verify.AreEqual(_mockItems[i], dict[_mockKeys2[i]]);
                Verify.AreNotEqual(_mockItems2[i], dict[_mockKeys2[i]]);
                Verify.AreEqual(dict[_mockKeys[i]], dict[_mockKeys2[i]]);
            }
        }

        [TestMethod]
        public void SetDictionaryWithValueTypeTypeKeyTest()
        {
            var dict = new SetDictionary<Guid, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and newKeys are considered equal, since the comparer only cares about the guid value
                // set of item2 should not be added to the dictionary
                dict.Add(mockKey.KeyValue, item);
                dict.Add(mockKey2.KeyValue, item2);
            }

            Verify.AreEqual(5, dict.Count);

            // Second set of keys are considered the same as the first, therefore, the insertion of mockItems2 never happened,
            // and _mockKey2[i] resolves to _mockKey[i], and the dictionary has only 5 entries.
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                Verify.AreEqual(_mockItems[i], dict[_mockKeys[i].KeyValue]);
                Verify.AreEqual(_mockItems[i], dict[_mockKeys2[i].KeyValue]);
                Verify.AreNotEqual(_mockItems2[i], dict[_mockKeys2[i].KeyValue]);
                Verify.AreEqual(dict[_mockKeys[i].KeyValue], dict[_mockKeys2[i].KeyValue]);
            }
        }

        #endregion

        #region Values and Keys

        /// <summary>
        /// Test Values to make sure it returns all values stored
        /// </summary>
        [TestMethod]
        public void ValuesTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not equal, even though they have the same KeyValue
                dict.Add(mockKey, item);
            }

            var actual = dict.Values;

            Verify.AreEqual(5, dict.Count);
            Verify.AreEqual(actual.Count, dict.Count);

            Verify.IsTrue(actual.SequenceEqual(_mockItems));
        }


        /// <summary>
        /// Test IntersectWith() on an empty dictionary.  No exception is expected from using non-existing keys
        /// Test Keys() to make sure it returns all values stored
        /// </summary>
        [TestMethod]
        public void KeysTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not equal, even though they have the same KeyValue
                dict.Add(mockKey, item);
            }

            var actual = dict.Keys;

            Verify.AreEqual(5, dict.Count);
            Verify.AreEqual(actual.Count, dict.Count);

            Verify.IsTrue(actual.SequenceEqual(_mockKeys));
        }

        #endregion

        #region ExceptWith()

        /// <summary>
        /// Test IntersectWith() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        [ExpectedArgumentNullException("other")]
        public void ExceptWithNullParameterTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and newKeys are not equal, even though they have the same KeyValue
                dict.Add(mockKey, item);
                dict.Add(mockKey2, item2);
            }

            Verify.AreEqual(10, dict.Count);

            // throws exception
            var actual = dict.ExceptWith(null);
        }

        /// <summary>
        /// Test ExceptWith() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void ExceptWithEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            Verify.AreEqual(0, dict.Count);

            var actual = dict.ExceptWith(_mockKeys2);

            Verify.IsNotNull(actual);
            Verify.AreEqual(0, actual.Count());
        }

        /// <summary>
        /// Test ExceptWith() with reference type MockKey and default comparer.  _mockKeys[i] and _mockKeys2[i] has 
        /// different references even though their GuidKey field values are the same, and therefore, they are treated as 
        /// different keys for the dictionary.  The ExceptWith() call should only remove the 2nd set of items (item2).
        /// </summary>
        [TestMethod]
        public void ExceptWithDefaultEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and newKeys are not equal, even though they have the same KeyValue
                dict.Add(mockKey, item);
                dict.Add(mockKey2, item2);
            }
            
            Verify.AreEqual(10, dict.Count);

            var actual = dict.ExceptWith(_mockKeys2);

            Verify.IsNotNull(actual);
            Verify.AreEqual(5, actual.Count());

            Verify.IsTrue(actual.SequenceEqual(_mockItems));
            Verify.IsFalse(actual.SequenceEqual(_mockItems2));

//            using (var e = actual.GetEnumerator()) {
//                while (e.MoveNext())
//                {
//                    Verify.IsTrue(_mockItems.Contains(e.Current));
//                    Verify.IsFalse(_mockItems2.Contains(e.Current));
//                }
//            }
        }

        /// <summary>
        /// Test ExceptWith() with reference type MockKey.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions, and since MockKeyComparer is used, only the Guid value matters.  The 
        /// dictionary should be empty after ExceptWith() call.
        /// </summary>
        [TestMethod]
        public void ExceptWithKeyGuidValueEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>(new MockKeyComparer());

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and newKeys are considered equal, since the comparer only cares about the guid value
                // set of item2 should not be added to the dictionary
                dict.Add(mockKey, item);
                dict.Add(mockKey2, item2);
            }

            Verify.AreEqual(5, dict.Count);

            var actual = dict.ExceptWith(_mockKeys2);

            Verify.IsNotNull(actual);
            Verify.AreEqual(0, actual.Count());
        }

        /// <summary>
        /// Test ExceptWith() with value type (Guid) keys.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions, and dictionary should be empty after ExceptWith() call.
        /// </summary>
        [TestMethod]
        public void ExceptWithValueTypeDefaultComparerTest()
        {
            var dict = new SetDictionary<Guid, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and mockKey2 contains the same GUID values.
                // set of item2 should not be added to the dictionary
                dict.Add(mockKey.KeyValue, item);
                dict.Add(mockKey2.KeyValue, item2);
            }

            Verify.AreEqual(5, dict.Count);

            var actual = dict.ExceptWith(_mockKeys2.Select(x=>x.KeyValue));

            Verify.IsNotNull(actual);
            Verify.AreEqual(0, actual.Count());
        }

        #endregion

        #region IntersectWith()

        /// <summary>
        /// Test IntersectWith() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        [ExpectedArgumentNullException("other")]
        public void IntersectWithNullParameterTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();
         
            // throws exception
            var actual = dict.IntersectWith(null);
        }

        /// <summary>
        /// Test IntersectWith() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void IntersectWithEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            Verify.AreEqual(0, dict.Count);

            var actual = dict.IntersectWith(_mockKeys2);

            Verify.IsNotNull(actual);
            Verify.AreEqual(0, actual.Count());
        }

        /// <summary>
        /// Test IntersectWith() with reference type MockKey and default comparer.  _mockKeys[i] and _mockKeys2[i] has 
        /// different references even though their GuidKey field values are the same, and therefore, they are treated as 
        /// different keys for the dictionary.  The IntersectWith() call should only remove the 2nd set of items (item2).
        /// </summary>
        [TestMethod]
        public void IntersectWithDefaultEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey, item);
                dict.Add(mockKey2, item2);
            }

            Verify.AreEqual(10, dict.Count);

            var actual = dict.IntersectWith(_mockKeys2);

            Verify.IsNotNull(actual);
            Verify.AreEqual(5, actual.Count());

            using (var e = actual.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    Verify.IsFalse(_mockItems.Contains(e.Current));
                    Verify.IsTrue(_mockItems2.Contains(e.Current));
                }
            }
        }

        /// <summary>
        /// Test IntersectWith() with reference type MockKey.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions, and since MockKeyComparer is used, only the Guid value matters.  The 
        /// dictionary should be empty after IntersectWith() call.
        /// </summary>
        [TestMethod]
        public void IntersectWithKeyGuidValueEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>(new MockKeyComparer());

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and newKeys are considered equal, since the comparer only cares about the guid value
                // set of item2 should not be added to the dictionary
                dict.Add(mockKey, item);
                dict.Add(mockKey2, item2);
            }

            Verify.AreEqual(5, dict.Count);

            var actual = dict.IntersectWith(_mockKeys2);

            Verify.IsNotNull(actual);
            Verify.AreEqual(5, actual.Count());

            using (var e = actual.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    Verify.IsTrue(_mockItems.Contains(e.Current));
                    Verify.IsFalse(_mockItems2.Contains(e.Current));
                }
            }
        }

        /// <summary>
        /// Test IntersectWith() with value type (Guid) keys.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions, and dictionary should be empty after IntersectWith() call.
        /// </summary>
        [TestMethod]
        public void IntersectWithValueTypeDefaultComparerTest()
        {
            var dict = new SetDictionary<Guid, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and mockKey2 contains the same GUID values.
                // set of item2 should not be added to the dictionary
                dict.Add(mockKey.KeyValue, item);
                dict.Add(mockKey2.KeyValue, item2);
            }

            Verify.AreEqual(5, dict.Count);

            var actual = dict.IntersectWith(_mockKeys2.Select(x => x.KeyValue));

            Verify.IsNotNull(actual);
            Verify.AreEqual(5, actual.Count());

            using (var e = actual.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    Verify.IsTrue(_mockItems.Contains(e.Current));
                    Verify.IsFalse(_mockItems2.Contains(e.Current));
                }
            }
        }

        #endregion

        #region IsSubsetOf()

        /// <summary>
        /// Test IsSubsetOf() with null parameter.  ArgumentmentNullException is expected
        /// </summary>
        [TestMethod]
        [ExpectedArgumentNullException("other")]
        public void IsSubsetOfNullParameterTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            // throws exception
            var actual = dict.IsSubsetOf(null);
        }

        /// <summary>
        /// Test IsSubsetOf() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void IsSubsetOfEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            Verify.AreEqual(0, dict.Count);

            Verify.IsTrue(dict.IsSubsetOf(_mockKeys2));
            Verify.IsTrue(dict.IsSubsetOf(new SetDictionary<MockKey, MockItem>().Keys));
        }

        /// <summary>
        /// Test IsSubsetOf() with reference type MockKey and default comparer.  _mockKeys[i] and _mockKeys2[i] has 
        /// different references even though their GuidKey field values are the same, and therefore, they are treated as 
        /// different keys for the dictionary.  The IsSubsetOf() call should match the superset of _mockKeys (i.e. _mockKeysSuperset)
        /// </summary>
        [TestMethod]
        public void IsSubsetOfDefaultEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey, item);
            }

            Verify.IsTrue(dict.IsSubsetOf(_mockKeys));
            Verify.IsFalse(dict.IsSubsetOf(_mockKeysSubset));
            Verify.IsTrue(dict.IsSubsetOf(_mockKeysSuperset));

            Verify.IsFalse(dict.IsSubsetOf(_mockKeys2));
            Verify.IsFalse(dict.IsSubsetOf(_mockKeys2Subset));
            Verify.IsFalse(dict.IsSubsetOf(_mockKeys2Superset));
        }

        /// <summary>
        /// Test IsSubsetOf() with reference type MockKey.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions, and since MockKeyComparer is used, only the Guid value matters.  The 
        /// IsSubsetOf() call should match the superset of _mockKeys by Guid KeyValue (i.e. _mockKeysSuperset, _mockKeys2
        /// and _mockKeys2Superset)
        /// </summary>
        [TestMethod]
        public void IsSubsetOfKeyGuidValueEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>(new MockKeyComparer());

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey, item);
            }

            Verify.IsTrue(dict.IsSubsetOf(_mockKeys));
            Verify.IsFalse(dict.IsSubsetOf(_mockKeysSubset));
            Verify.IsTrue(dict.IsSubsetOf(_mockKeysSuperset));

            Verify.IsTrue(dict.IsSubsetOf(_mockKeys2));
            Verify.IsFalse(dict.IsSubsetOf(_mockKeys2Subset));
            Verify.IsTrue(dict.IsSubsetOf(_mockKeys2Superset));
        }

        /// <summary>
        /// Test IsSubsetOf() with value type (Guid) keys.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions.
        /// </summary>
        [TestMethod]
        public void IsSubsetOfValueTypeDefaultComparerTest()
        {
            var dict = new SetDictionary<Guid, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey.KeyValue, item);
            }

            Verify.IsTrue(dict.IsSubsetOf(_mockKeys.Select(x => x.KeyValue)));
            Verify.IsFalse(dict.IsSubsetOf(_mockKeysSubset.Select(x => x.KeyValue)));
            Verify.IsTrue(dict.IsSubsetOf(_mockKeysSuperset.Select(x => x.KeyValue)));

            Verify.IsTrue(dict.IsSubsetOf(_mockKeys2.Select(x => x.KeyValue)));
            Verify.IsFalse(dict.IsSubsetOf(_mockKeys2Subset.Select(x => x.KeyValue)));
            Verify.IsTrue(dict.IsSubsetOf(_mockKeys2Superset.Select(x => x.KeyValue)));

        }

        #endregion

        #region IsSupersetOf()

        /// <summary>
        /// Test IsSupersetOf() with null parameter.  ArgumentmentNullException is expected
        /// </summary>
        [TestMethod]
        [ExpectedArgumentNullException("other")]
        public void IsSupersetOfNullParameterTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            // throws exception
            var actual = dict.IsSupersetOf(null);
        }

        /// <summary>
        /// Test IsSupersetOf() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void IsSupersetOfEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            Verify.AreEqual(0, dict.Count);

            Verify.IsFalse(dict.IsSupersetOf(_mockKeys2));
            Verify.IsTrue(dict.IsSupersetOf(new SetDictionary<MockKey, MockItem>().Keys));
        }

        /// <summary>
        /// Test IsSupersetOf() with reference type MockKey and default comparer.  _mockKeys[i] and _mockKeys2[i] has 
        /// different references even though their GuidKey field values are the same, and therefore, they are treated as 
        /// different keys for the dictionary.  The IsSupersetOf() call should match the superset of _mockKeys (i.e. _mockKeysSuperset)
        /// </summary>
        [TestMethod]
        public void IsSupersetOfDefaultEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey, item);
            }

            Verify.IsTrue(dict.IsSupersetOf(_mockKeys));
            Verify.IsTrue(dict.IsSupersetOf(_mockKeysSubset));
            Verify.IsFalse(dict.IsSupersetOf(_mockKeysSuperset));

            Verify.IsFalse(dict.IsSupersetOf(_mockKeys2));
            Verify.IsFalse(dict.IsSupersetOf(_mockKeys2Subset));
            Verify.IsFalse(dict.IsSupersetOf(_mockKeys2Superset));
        }

        /// <summary>
        /// Test IsSupersetOf() with reference type MockKey.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions, and since MockKeyComparer is used, only the Guid value matters.  The 
        /// IsSupersetOf() call should match the superset of _mockKeys by Guid KeyValue (i.e. _mockKeysSuperset, _mockKeys2
        /// and _mockKeys2Superset)
        /// </summary>
        [TestMethod]
        public void IsSupersetOfKeyGuidValueEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>(new MockKeyComparer());

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey, item);
            }

            Verify.IsTrue(dict.IsSupersetOf(_mockKeys));
            Verify.IsTrue(dict.IsSupersetOf(_mockKeysSubset));
            Verify.IsFalse(dict.IsSupersetOf(_mockKeysSuperset));

            Verify.IsTrue(dict.IsSupersetOf(_mockKeys2));
            Verify.IsTrue(dict.IsSupersetOf(_mockKeys2Subset));
            Verify.IsFalse(dict.IsSupersetOf(_mockKeys2Superset));
        }

        /// <summary>
        /// Test IsSupersetOf() with value type (Guid) keys.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions.
        /// </summary>
        [TestMethod]
        public void IsSupersetOfValueTypeDefaultComparerTest()
        {
            var dict = new SetDictionary<Guid, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey.KeyValue, item);
            }

            Verify.IsTrue(dict.IsSupersetOf(_mockKeys.Select(x => x.KeyValue)));
            Verify.IsTrue(dict.IsSupersetOf(_mockKeysSubset.Select(x => x.KeyValue)));
            Verify.IsFalse(dict.IsSupersetOf(_mockKeysSuperset.Select(x => x.KeyValue)));

            Verify.IsTrue(dict.IsSupersetOf(_mockKeys2.Select(x => x.KeyValue)));
            Verify.IsTrue(dict.IsSupersetOf(_mockKeys2Subset.Select(x => x.KeyValue)));
            Verify.IsFalse(dict.IsSupersetOf(_mockKeys2Superset.Select(x => x.KeyValue)));

        }

        #endregion

        #region IsProperSubsetOf()

        /// <summary>
        /// Test IsProperSubsetOf() with null parameter.  ArgumentmentNullException is expected
        /// </summary>
        [TestMethod]
        [ExpectedArgumentNullException("other")]
        public void IsProperSubsetOfNullParameterTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            // throws exception
            var actual = dict.IsProperSubsetOf(null);
        }

        /// <summary>
        /// Test IsProperSubsetOf() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void IsProperSubsetOfEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            Verify.AreEqual(0, dict.Count);

            Verify.IsTrue(dict.IsProperSubsetOf(_mockKeys2));
            Verify.IsFalse(dict.IsProperSubsetOf(new SetDictionary<MockKey, MockItem>().Keys));
        }

        /// <summary>
        /// Test IsProperSubsetOf() with reference type MockKey and default comparer.  _mockKeys[i] and _mockKeys2[i] has 
        /// different references even though their GuidKey field values are the same, and therefore, they are treated as 
        /// different keys for the dictionary.  The IsProperSubsetOf() call should match the superset of _mockKeys (i.e. _mockKeysSuperset)
        /// </summary>
        [TestMethod]
        public void IsProperSubsetOfDefaultEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey, item);
            }

            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeys));
            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeysSubset));
            Verify.IsTrue(dict.IsProperSubsetOf(_mockKeysSuperset));

            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeys2));
            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeys2Subset));
            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeys2Superset));
        }

        /// <summary>
        /// Test IsProperSubsetOf() with reference type MockKey.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions, and since MockKeyComparer is used, only the Guid value matters.  The 
        /// IsProperSubsetOf() call should match the superset of _mockKeys by Guid KeyValue (i.e. _mockKeysSuperset, _mockKeys2
        /// and _mockKeys2Superset)
        /// </summary>
        [TestMethod]
        public void IsProperSubsetOfKeyGuidValueEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>(new MockKeyComparer());

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey, item);
            }

            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeys));
            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeysSubset));
            Verify.IsTrue(dict.IsProperSubsetOf(_mockKeysSuperset));

            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeys2));
            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeys2Subset));
            Verify.IsTrue(dict.IsProperSubsetOf(_mockKeys2Superset));
        }

        /// <summary>
        /// Test IsProperSubsetOf() with value type (Guid) keys.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions.
        /// </summary>
        [TestMethod]
        public void IsProperSubsetOfValueTypeDefaultComparerTest()
        {
            var dict = new SetDictionary<Guid, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey.KeyValue, item);
            }

            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeys.Select(x => x.KeyValue)));
            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeysSubset.Select(x => x.KeyValue)));
            Verify.IsTrue(dict.IsProperSubsetOf(_mockKeysSuperset.Select(x => x.KeyValue)));

            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeys2.Select(x => x.KeyValue)));
            Verify.IsFalse(dict.IsProperSubsetOf(_mockKeys2Subset.Select(x => x.KeyValue)));
            Verify.IsTrue(dict.IsProperSubsetOf(_mockKeys2Superset.Select(x => x.KeyValue)));

        }

        #endregion

        #region IsProperSupersetOf()

        /// <summary>
        /// Test IsProperSupersetOf() with null parameter.  ArgumentmentNullException is expected
        /// </summary>
        [TestMethod]
        [ExpectedArgumentNullException("other")]
        public void IsProperSupersetOfNullParameterTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            // throws exception
            var actual = dict.IsProperSupersetOf(null);
        }

        /// <summary>
        /// Test IsProperSupersetOf() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void IsProperSupersetOfEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            Verify.AreEqual(0, dict.Count);

            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeys2));
            Verify.IsFalse(dict.IsProperSupersetOf(new SetDictionary<MockKey, MockItem>().Keys));
        }

        /// <summary>
        /// Test IsProperSupersetOf() with reference type MockKey and default comparer.  _mockKeys[i] and _mockKeys2[i] has 
        /// different references even though their GuidKey field values are the same, and therefore, they are treated as 
        /// different keys for the dictionary.  The IsProperSupersetOf() call should match the superset of _mockKeys (i.e. _mockKeysSuperset)
        /// </summary>
        [TestMethod]
        public void IsProperSupersetOfDefaultEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey, item);
            }

            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeys));
            Verify.IsTrue(dict.IsProperSupersetOf(_mockKeysSubset));
            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeysSuperset));

            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeys2));
            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeys2Subset));
            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeys2Superset));
        }

        /// <summary>
        /// Test IsProperSupersetOf() with reference type MockKey.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions, and since MockKeyComparer is used, only the Guid value matters.  The 
        /// IsProperSupersetOf() call should match the superset of _mockKeys by Guid KeyValue (i.e. _mockKeysSuperset, _mockKeys2
        /// and _mockKeys2Superset)
        /// </summary>
        [TestMethod]
        public void IsProperSupersetOfKeyGuidValueEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>(new MockKeyComparer());

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey, item);
            }

            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeys));
            Verify.IsTrue(dict.IsProperSupersetOf(_mockKeysSubset));
            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeysSuperset));

            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeys2));
            Verify.IsTrue(dict.IsProperSupersetOf(_mockKeys2Subset));
            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeys2Superset));
        }

        /// <summary>
        /// Test IsProperSupersetOf() with value type (Guid) keys.  Guids for _mockKeys[i] and _mockKeys2[i] are the same
        /// at the corresponding positions.
        /// </summary>
        [TestMethod]
        public void IsProperSupersetOfValueTypeDefaultComparerTest()
        {
            var dict = new SetDictionary<Guid, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                // mockKey and newKeys are not the same reference, even though they have the same KeyValue
                dict.Add(mockKey.KeyValue, item);
            }

            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeys.Select(x => x.KeyValue)));
            Verify.IsTrue(dict.IsProperSupersetOf(_mockKeysSubset.Select(x => x.KeyValue)));
            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeysSuperset.Select(x => x.KeyValue)));

            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeys2.Select(x => x.KeyValue)));
            Verify.IsTrue(dict.IsProperSupersetOf(_mockKeys2Subset.Select(x => x.KeyValue)));
            Verify.IsFalse(dict.IsProperSupersetOf(_mockKeys2Superset.Select(x => x.KeyValue)));

        }

        #endregion

        #region ContainsKey() Tests

        [TestMethod]
        public void ContainsKeyWithDefaultEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                dict.Add(mockKey, item);
            }

            Verify.AreEqual(5, dict.Count);

            // _mockKeys and _mockKeys2 are considered to be completely different with default equality comparer
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                Verify.IsTrue(dict.ContainsKey(_mockKeys[i]));
                Verify.IsFalse(dict.ContainsKey(_mockKeys2[i]));
            }
        }

        [TestMethod]
        public void ContainsKeyWithGuidKeyValueEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>(new MockKeyComparer());

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var item = _mockItems[i];

                dict.Add(mockKey, item);
            }

            Verify.AreEqual(5, dict.Count);

            // _mockKeys and _mockKeys2 are considered to be completely different with default equality comparer
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                Verify.IsTrue(dict.ContainsKey(_mockKeys[i]));
                Verify.IsTrue(dict.ContainsKey(_mockKeys2[i]));
            }
        }

        #endregion

        #region Contains() Tests

        [TestMethod]
        public void ContainsValueWithDefaultEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and newKeys are not equal, even though they have the same KeyValue
                dict.Add(mockKey, item);
                dict.Add(mockKey2, item2);
            }

            Verify.AreEqual(10, dict.Count);

            // _mockKeys and _mockKeys2 are considered to be completely different set, and therefore the 
            // dictionary should have 10 items, even though _mockKey[i] and _mockKey2[i] contains the same Guid.
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                Verify.IsTrue(dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys[i], _mockItems[i])));
                Verify.IsTrue(dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys2[i], _mockItems2[i])));
                Verify.IsFalse(dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys[i], _mockItems2[i])));
                Verify.IsFalse(dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys2[i], _mockItems[i])));
            }
        }

        [TestMethod]
        public void ContainsValueWithGuidKeyValueEqualityComparerTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>(new MockKeyComparer());

            for (var i = 0; i < _mockKeys.Length; i++)
            {
                var mockKey = _mockKeys[i];
                var mockKey2 = _mockKeys2[i];
                var item = _mockItems[i];
                var item2 = _mockItems2[i];

                // mockKey and newKeys are considered equal, since the comparer only cares about the guid value
                // set of item2 should not be added to the dictionary
                dict.Add(mockKey, item);
                dict.Add(mockKey2, item2);
            }

            Verify.AreEqual(5, dict.Count);

            // Second set of keys are considered the same as the first, therefore, the insertion of mockItems2 never happened,
            // and _mockKey2[i] resolves to _mockKey[i], and the dictionary has only 5 entries.
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                Verify.IsTrue(dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys[i], _mockItems[i])));
                Verify.IsFalse(dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys2[i], _mockItems2[i])));
                Verify.IsFalse(dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys[i], _mockItems2[i])));
                Verify.IsTrue(dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys2[i], _mockItems[i])));
            }
        }

        #endregion
    }
}
