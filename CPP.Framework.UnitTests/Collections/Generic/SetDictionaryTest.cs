using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            dict.Count.Should().Be(5);

            dict.Clear();

            dict.Count.Should().Be(0);
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

            dict.Count.Should().Be(10);

            // _mockKeys and _mockKeys2 are considered to be completely different set, and therefore the
            // dictionary should have 10 items, even though _mockKey[i] and _mockKey2[i] contains the same Guid.
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                dict[_mockKeys[i]].Should().Be(_mockItems[i]);
                dict[_mockKeys2[i]].Should().NotBe(_mockItems[i]);
                dict[_mockKeys2[i]].Should().Be(_mockItems2[i]);
                dict[_mockKeys2[i]].Should().NotBe(dict[_mockKeys[i]]);
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

            dict.Count.Should().Be(5);

            // Second set of keys are considered the same as the first, therefore, the insertion of mockItems2 never happened,
            // and _mockKey2[i] resolves to _mockKey[i], and the dictionary has only 5 entries.
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                dict[_mockKeys[i]].Should().Be(_mockItems[i]);
                dict[_mockKeys2[i]].Should().Be(_mockItems[i]);
                dict[_mockKeys2[i]].Should().NotBe(_mockItems2[i]);
                dict[_mockKeys2[i]].Should().Be(dict[_mockKeys[i]]);
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

            dict.Count.Should().Be(5);

            // Second set of keys are considered the same as the first, therefore, the insertion of mockItems2 never happened,
            // and _mockKey2[i] resolves to _mockKey[i], and the dictionary has only 5 entries.
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                dict[_mockKeys[i].KeyValue].Should().Be(_mockItems[i]);
                dict[_mockKeys2[i].KeyValue].Should().Be(_mockItems[i]);
                dict[_mockKeys2[i].KeyValue].Should().NotBe(_mockItems2[i]);
                dict[_mockKeys2[i].KeyValue].Should().Be(dict[_mockKeys[i].KeyValue]);
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

            dict.Count.Should().Be(5);
            actual.Count.Should().Be(dict.Count);

            actual.SequenceEqual(_mockItems).Should().BeTrue();
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

            dict.Count.Should().Be(5);
            actual.Count.Should().Be(dict.Count);

            actual.SequenceEqual(_mockKeys).Should().BeTrue();
        }

        #endregion

        #region ExceptWith()

        /// <summary>
        /// Test IntersectWith() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
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

            dict.Count.Should().Be(10);

            // throws exception
            Action act = () => { var actual = dict.ExceptWith(null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("other");
        }

        /// <summary>
        /// Test ExceptWith() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void ExceptWithEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            dict.Count.Should().Be(0);

            var actual = dict.ExceptWith(_mockKeys2);

            actual.Should().NotBeNull();
            actual.Count().Should().Be(0);
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

            dict.Count.Should().Be(10);

            var actual = dict.ExceptWith(_mockKeys2);

            actual.Should().NotBeNull();
            actual.Count().Should().Be(5);

            actual.SequenceEqual(_mockItems).Should().BeTrue();
            actual.SequenceEqual(_mockItems2).Should().BeFalse();

//            using (var e = actual.GetEnumerator()) {
//                while (e.MoveNext())
//                {
//                    _mockItems.Contains(e.Current).Should().BeTrue();
//                    _mockItems2.Contains(e.Current).Should().BeFalse();
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

            dict.Count.Should().Be(5);

            var actual = dict.ExceptWith(_mockKeys2);

            actual.Should().NotBeNull();
            actual.Count().Should().Be(0);
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

            dict.Count.Should().Be(5);

            var actual = dict.ExceptWith(_mockKeys2.Select(x=>x.KeyValue));

            actual.Should().NotBeNull();
            actual.Count().Should().Be(0);
        }

        #endregion

        #region IntersectWith()

        /// <summary>
        /// Test IntersectWith() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void IntersectWithNullParameterTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            // throws exception
            Action act = () => { var actual = dict.IntersectWith(null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("other");
        }

        /// <summary>
        /// Test IntersectWith() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void IntersectWithEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            dict.Count.Should().Be(0);

            var actual = dict.IntersectWith(_mockKeys2);

            actual.Should().NotBeNull();
            actual.Count().Should().Be(0);
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

            dict.Count.Should().Be(10);

            var actual = dict.IntersectWith(_mockKeys2);

            actual.Should().NotBeNull();
            actual.Count().Should().Be(5);

            using (var e = actual.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    _mockItems.Contains(e.Current).Should().BeFalse();
                    _mockItems2.Contains(e.Current).Should().BeTrue();
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

            dict.Count.Should().Be(5);

            var actual = dict.IntersectWith(_mockKeys2);

            actual.Should().NotBeNull();
            actual.Count().Should().Be(5);

            using (var e = actual.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    _mockItems.Contains(e.Current).Should().BeTrue();
                    _mockItems2.Contains(e.Current).Should().BeFalse();
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

            dict.Count.Should().Be(5);

            var actual = dict.IntersectWith(_mockKeys2.Select(x => x.KeyValue));

            actual.Should().NotBeNull();
            actual.Count().Should().Be(5);

            using (var e = actual.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    _mockItems.Contains(e.Current).Should().BeTrue();
                    _mockItems2.Contains(e.Current).Should().BeFalse();
                }
            }
        }

        #endregion

        #region IsSubsetOf()

        /// <summary>
        /// Test IsSubsetOf() with null parameter.  ArgumentmentNullException is expected
        /// </summary>
        [TestMethod]
        public void IsSubsetOfNullParameterTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            // throws exception
            Action act = () => { var actual = dict.IsSubsetOf(null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("other");
        }

        /// <summary>
        /// Test IsSubsetOf() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void IsSubsetOfEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            dict.Count.Should().Be(0);

            dict.IsSubsetOf(_mockKeys2).Should().BeTrue();
            dict.IsSubsetOf(new SetDictionary<MockKey, MockItem>().Keys).Should().BeTrue();
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

            dict.IsSubsetOf(_mockKeys).Should().BeTrue();
            dict.IsSubsetOf(_mockKeysSubset).Should().BeFalse();
            dict.IsSubsetOf(_mockKeysSuperset).Should().BeTrue();

            dict.IsSubsetOf(_mockKeys2).Should().BeFalse();
            dict.IsSubsetOf(_mockKeys2Subset).Should().BeFalse();
            dict.IsSubsetOf(_mockKeys2Superset).Should().BeFalse();
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

            dict.IsSubsetOf(_mockKeys).Should().BeTrue();
            dict.IsSubsetOf(_mockKeysSubset).Should().BeFalse();
            dict.IsSubsetOf(_mockKeysSuperset).Should().BeTrue();

            dict.IsSubsetOf(_mockKeys2).Should().BeTrue();
            dict.IsSubsetOf(_mockKeys2Subset).Should().BeFalse();
            dict.IsSubsetOf(_mockKeys2Superset).Should().BeTrue();
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

            dict.IsSubsetOf(_mockKeys.Select(x => x.KeyValue)).Should().BeTrue();
            dict.IsSubsetOf(_mockKeysSubset.Select(x => x.KeyValue)).Should().BeFalse();
            dict.IsSubsetOf(_mockKeysSuperset.Select(x => x.KeyValue)).Should().BeTrue();

            dict.IsSubsetOf(_mockKeys2.Select(x => x.KeyValue)).Should().BeTrue();
            dict.IsSubsetOf(_mockKeys2Subset.Select(x => x.KeyValue)).Should().BeFalse();
            dict.IsSubsetOf(_mockKeys2Superset.Select(x => x.KeyValue)).Should().BeTrue();

        }

        #endregion

        #region IsSupersetOf()

        /// <summary>
        /// Test IsSupersetOf() with null parameter.  ArgumentmentNullException is expected
        /// </summary>
        [TestMethod]
        public void IsSupersetOfNullParameterTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            // throws exception
            Action act = () => { var actual = dict.IsSupersetOf(null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("other");
        }

        /// <summary>
        /// Test IsSupersetOf() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void IsSupersetOfEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            dict.Count.Should().Be(0);

            dict.IsSupersetOf(_mockKeys2).Should().BeFalse();
            dict.IsSupersetOf(new SetDictionary<MockKey, MockItem>().Keys).Should().BeTrue();
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

            dict.IsSupersetOf(_mockKeys).Should().BeTrue();
            dict.IsSupersetOf(_mockKeysSubset).Should().BeTrue();
            dict.IsSupersetOf(_mockKeysSuperset).Should().BeFalse();

            dict.IsSupersetOf(_mockKeys2).Should().BeFalse();
            dict.IsSupersetOf(_mockKeys2Subset).Should().BeFalse();
            dict.IsSupersetOf(_mockKeys2Superset).Should().BeFalse();
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

            dict.IsSupersetOf(_mockKeys).Should().BeTrue();
            dict.IsSupersetOf(_mockKeysSubset).Should().BeTrue();
            dict.IsSupersetOf(_mockKeysSuperset).Should().BeFalse();

            dict.IsSupersetOf(_mockKeys2).Should().BeTrue();
            dict.IsSupersetOf(_mockKeys2Subset).Should().BeTrue();
            dict.IsSupersetOf(_mockKeys2Superset).Should().BeFalse();
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

            dict.IsSupersetOf(_mockKeys.Select(x => x.KeyValue)).Should().BeTrue();
            dict.IsSupersetOf(_mockKeysSubset.Select(x => x.KeyValue)).Should().BeTrue();
            dict.IsSupersetOf(_mockKeysSuperset.Select(x => x.KeyValue)).Should().BeFalse();

            dict.IsSupersetOf(_mockKeys2.Select(x => x.KeyValue)).Should().BeTrue();
            dict.IsSupersetOf(_mockKeys2Subset.Select(x => x.KeyValue)).Should().BeTrue();
            dict.IsSupersetOf(_mockKeys2Superset.Select(x => x.KeyValue)).Should().BeFalse();

        }

        #endregion

        #region IsProperSubsetOf()

        /// <summary>
        /// Test IsProperSubsetOf() with null parameter.  ArgumentmentNullException is expected
        /// </summary>
        [TestMethod]
        public void IsProperSubsetOfNullParameterTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            // throws exception
            Action act = () => { var actual = dict.IsProperSubsetOf(null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("other");
        }

        /// <summary>
        /// Test IsProperSubsetOf() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void IsProperSubsetOfEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            dict.Count.Should().Be(0);

            dict.IsProperSubsetOf(_mockKeys2).Should().BeTrue();
            dict.IsProperSubsetOf(new SetDictionary<MockKey, MockItem>().Keys).Should().BeFalse();
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

            dict.IsProperSubsetOf(_mockKeys).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeysSubset).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeysSuperset).Should().BeTrue();

            dict.IsProperSubsetOf(_mockKeys2).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeys2Subset).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeys2Superset).Should().BeFalse();
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

            dict.IsProperSubsetOf(_mockKeys).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeysSubset).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeysSuperset).Should().BeTrue();

            dict.IsProperSubsetOf(_mockKeys2).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeys2Subset).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeys2Superset).Should().BeTrue();
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

            dict.IsProperSubsetOf(_mockKeys.Select(x => x.KeyValue)).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeysSubset.Select(x => x.KeyValue)).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeysSuperset.Select(x => x.KeyValue)).Should().BeTrue();

            dict.IsProperSubsetOf(_mockKeys2.Select(x => x.KeyValue)).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeys2Subset.Select(x => x.KeyValue)).Should().BeFalse();
            dict.IsProperSubsetOf(_mockKeys2Superset.Select(x => x.KeyValue)).Should().BeTrue();

        }

        #endregion

        #region IsProperSupersetOf()

        /// <summary>
        /// Test IsProperSupersetOf() with null parameter.  ArgumentmentNullException is expected
        /// </summary>
        [TestMethod]
        public void IsProperSupersetOfNullParameterTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            // throws exception
            Action act = () => { var actual = dict.IsProperSupersetOf(null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("other");
        }

        /// <summary>
        /// Test IsProperSupersetOf() on an empty dictionary.  No exception is expected from using non-existing keys
        /// </summary>
        [TestMethod]
        public void IsProperSupersetOfEmptyDictionaryTest()
        {
            var dict = new SetDictionary<MockKey, MockItem>();

            dict.Count.Should().Be(0);

            dict.IsProperSupersetOf(_mockKeys2).Should().BeFalse();
            dict.IsProperSupersetOf(new SetDictionary<MockKey, MockItem>().Keys).Should().BeFalse();
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

            dict.IsProperSupersetOf(_mockKeys).Should().BeFalse();
            dict.IsProperSupersetOf(_mockKeysSubset).Should().BeTrue();
            dict.IsProperSupersetOf(_mockKeysSuperset).Should().BeFalse();

            dict.IsProperSupersetOf(_mockKeys2).Should().BeFalse();
            dict.IsProperSupersetOf(_mockKeys2Subset).Should().BeFalse();
            dict.IsProperSupersetOf(_mockKeys2Superset).Should().BeFalse();
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

            dict.IsProperSupersetOf(_mockKeys).Should().BeFalse();
            dict.IsProperSupersetOf(_mockKeysSubset).Should().BeTrue();
            dict.IsProperSupersetOf(_mockKeysSuperset).Should().BeFalse();

            dict.IsProperSupersetOf(_mockKeys2).Should().BeFalse();
            dict.IsProperSupersetOf(_mockKeys2Subset).Should().BeTrue();
            dict.IsProperSupersetOf(_mockKeys2Superset).Should().BeFalse();
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

            dict.IsProperSupersetOf(_mockKeys.Select(x => x.KeyValue)).Should().BeFalse();
            dict.IsProperSupersetOf(_mockKeysSubset.Select(x => x.KeyValue)).Should().BeTrue();
            dict.IsProperSupersetOf(_mockKeysSuperset.Select(x => x.KeyValue)).Should().BeFalse();

            dict.IsProperSupersetOf(_mockKeys2.Select(x => x.KeyValue)).Should().BeFalse();
            dict.IsProperSupersetOf(_mockKeys2Subset.Select(x => x.KeyValue)).Should().BeTrue();
            dict.IsProperSupersetOf(_mockKeys2Superset.Select(x => x.KeyValue)).Should().BeFalse();

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

            dict.Count.Should().Be(5);

            // _mockKeys and _mockKeys2 are considered to be completely different with default equality comparer
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                dict.ContainsKey(_mockKeys[i]).Should().BeTrue();
                dict.ContainsKey(_mockKeys2[i]).Should().BeFalse();
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

            dict.Count.Should().Be(5);

            // _mockKeys and _mockKeys2 are considered to be completely different with default equality comparer
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                dict.ContainsKey(_mockKeys[i]).Should().BeTrue();
                dict.ContainsKey(_mockKeys2[i]).Should().BeTrue();
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

            dict.Count.Should().Be(10);

            // _mockKeys and _mockKeys2 are considered to be completely different set, and therefore the
            // dictionary should have 10 items, even though _mockKey[i] and _mockKey2[i] contains the same Guid.
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys[i], _mockItems[i])).Should().BeTrue();
                dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys2[i], _mockItems2[i])).Should().BeTrue();
                dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys[i], _mockItems2[i])).Should().BeFalse();
                dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys2[i], _mockItems[i])).Should().BeFalse();
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

            dict.Count.Should().Be(5);

            // Second set of keys are considered the same as the first, therefore, the insertion of mockItems2 never happened,
            // and _mockKey2[i] resolves to _mockKey[i], and the dictionary has only 5 entries.
            for (var i = 0; i < _mockKeys.Length; i++)
            {
                dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys[i], _mockItems[i])).Should().BeTrue();
                dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys2[i], _mockItems2[i])).Should().BeFalse();
                dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys[i], _mockItems2[i])).Should().BeFalse();
                dict.Contains(new KeyValuePair<MockKey, MockItem>(_mockKeys2[i], _mockItems[i])).Should().BeTrue();
            }
        }

        #endregion
    }
}
