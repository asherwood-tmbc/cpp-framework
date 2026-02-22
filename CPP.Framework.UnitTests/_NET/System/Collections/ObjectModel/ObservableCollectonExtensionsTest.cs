
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace System.Collections.ObjectModel
{
    /// <summary>
    /// Unit tests for the <see cref="ObservableCollection{T}"/> class.
    ///
    /// Test GUID generated for the following:
    ///  1) NOT a new GUID object (with all zeros)
    ///  2) The formatting of GUID ToString() is hypenated 32 digits (00000000-0000-0000-0000-000000000000)
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
    // ReSharper disable once InconsistentNaming
    public class ObservableCollectonExtensionsTest
    {

        // Test of AddRange() using List<T> and ObservableCollection<T> as targets, with different
        // implementations of IList, ICollection, and IEnumerables as parameters
        #region Test AddRange()

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddObservableCollectionToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            ObservableCollection<int> valuesToAdd = new ObservableCollection<int> { 4, 5, 6 };

            actual.AddRange(valuesToAdd);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddListToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            List<int> valuesToAdd = new List<int> { 4, 5, 6 };

            actual.AddRange(valuesToAdd);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddSortedListToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            SortedList<int, int> valuesToAdd = new SortedList<int, int>
            {
                {5, 5},
                {4, 4},
                {6, 6}
            };

            actual.AddRange(valuesToAdd.Values);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddLinkedListToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            LinkedList<int> valuesToAdd = new LinkedList<int>();
            valuesToAdd.AddLast(4);
            valuesToAdd.AddLast(5);
            valuesToAdd.AddLast(6);

            actual.AddRange(valuesToAdd);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }


        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddQueueToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            Queue<int> valuesToAdd = new Queue<int>();
            valuesToAdd.Enqueue(4);
            valuesToAdd.Enqueue(5);
            valuesToAdd.Enqueue(6);

            actual.AddRange(valuesToAdd);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddStackToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            Stack<int> valuesToAdd = new Stack<int>();
            valuesToAdd.Push(6);
            valuesToAdd.Push(5);
            valuesToAdd.Push(4);

            actual.AddRange(valuesToAdd);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddDictionaryValuesToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            Dictionary<string, int> valuesToAdd = new Dictionary<string, int>()
            {
                {"four", 4},
                {"five", 5},
                {"six", 6},
            };

            actual.AddRange(valuesToAdd.Values);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddHashSetToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 1, 2, 3, 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            HashSet<int> valuesToAdd = new HashSet<int> { 4, 5, 6 };

            actual.AddRange(valuesToAdd);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        #endregion

        // Test of SetRange() using List<T> and ObservableCollection<T> as targets, with different
        // implementations of IList, ICollection, and IEnumerables as parameters
        #region Test SetRange()

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetListToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            List<int> valuesToSet = new List<int> { 4, 5, 6 };

            actual.SetRange(valuesToSet);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetObservableCollectionToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            ObservableCollection<int> valuesToSet = new ObservableCollection<int> { 4, 5, 6 };

            actual.SetRange(valuesToSet);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetSortedListToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            SortedList<int, int> valuesToSet = new SortedList<int, int>
            {
                {5, 5},
                {4, 4},
                {6, 6}
            };

            actual.SetRange(valuesToSet.Values);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetLinkedListToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            LinkedList<int> valuesToSet = new LinkedList<int>();
            valuesToSet.AddLast(4);
            valuesToSet.AddLast(5);
            valuesToSet.AddLast(6);

            actual.SetRange(valuesToSet);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetQueueToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            Queue<int> valuesToSet = new Queue<int>();
            valuesToSet.Enqueue(4);
            valuesToSet.Enqueue(5);
            valuesToSet.Enqueue(6);

            actual.SetRange(valuesToSet);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetStackToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            Stack<int> valuesToSet = new Stack<int>();
            valuesToSet.Push(6);
            valuesToSet.Push(5);
            valuesToSet.Push(4);

            actual.SetRange(valuesToSet);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetDictionaryValuesToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            Dictionary<string, int> valuesToSet = new Dictionary<string, int>()
            {
                {"four", 4},
                {"five", 5},
                {"six", 6},
            };

            actual.SetRange(valuesToSet.Values);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetHashSetToObservableCollectionTest()
        {
            ObservableCollection<int> expected = new ObservableCollection<int> { 4, 5, 6 };
            ObservableCollection<int> actual = new ObservableCollection<int> { 1, 2, 3 };
            HashSet<int> valuesToSet = new HashSet<int> { 4, 5, 6 };

            actual.SetRange(valuesToSet);

            actual.Count.Should().Be(expected.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                actual[i].Should().Be(expected[i]);
            }
        }

        #endregion

    }
}
