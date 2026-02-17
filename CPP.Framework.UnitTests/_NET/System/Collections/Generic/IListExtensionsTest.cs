
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// Unit tests for the <see cref="IListExtensions"/> class.
    /// 
    /// Test GUID generated for the following:
    ///  1) NOT a new GUID object (with all zeros)
    ///  2) The formatting of GUID ToString() is hypenated 32 digits (00000000-0000-0000-0000-000000000000)
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
    // ReSharper disable once InconsistentNaming
    public class IListExtensionsTest
    {

        // Test of AddRange() using List<T> and ObservableCollection<T> as targets, with different
        // implementations of IList, ICollection, and IEnumerables as parameters
        #region Test AddRange()

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddListToListTest()
        {
            List<int> expected = new List<int> {1, 2, 3, 4, 5, 6};
            List<int> actual = new List<int> {1, 2, 3};
            List<int> valuesToAdd = new List<int> {4, 5, 6};

            actual.AddRange(valuesToAdd);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddObjectListToObjectListTest()
        {
            List<object> expected = new List<object> { 1, 2, 3, "4", "5", "6" };
            List<object> actual = new List<object> { 1, 2, 3 };
            List<object> valuesToSet = new List<object> { "4", "5", "6" };

            actual.AddRange(valuesToSet);

            Verify.AreEqual(expected.Count, actual.Count);
            
            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddObservableCollectionToListTest()
        {
            List<int> expected = new List<int> {1, 2, 3, 4, 5, 6};
            List<int> actual = new List<int> {1, 2, 3};
            ObservableCollection<int> valuesToAdd = new ObservableCollection<int> {4, 5, 6};

            actual.AddRange(valuesToAdd);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }
        
        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddSortedListToListTest()
        {
            List<int> expected = new List<int> {1, 2, 3, 4, 5, 6};
            List<int> actual = new List<int> {1, 2, 3};
            SortedList<int, int> valuesToAdd = new SortedList<int, int>
            {
                {5, 5},
                {4, 4},
                {6, 6}
            };

            actual.AddRange(valuesToAdd.Values);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddLinkedListToListTest()
        {
            List<int> expected = new List<int> {1, 2, 3, 4, 5, 6};
            List<int> actual = new List<int> {1, 2, 3};
            LinkedList<int> valuesToAdd = new LinkedList<int>();
            valuesToAdd.AddLast(4);
            valuesToAdd.AddLast(5);
            valuesToAdd.AddLast(6);

            actual.AddRange(valuesToAdd);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddQueueToListTest()
        {
            List<int> expected = new List<int> {1, 2, 3, 4, 5, 6};
            List<int> actual = new List<int> {1, 2, 3};
            Queue<int> valuesToAdd = new Queue<int>();
            valuesToAdd.Enqueue(4);
            valuesToAdd.Enqueue(5);
            valuesToAdd.Enqueue(6);

            actual.AddRange(valuesToAdd);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddStackToListTest()
        {
            List<int> expected = new List<int> {1, 2, 3, 4, 5, 6};
            List<int> actual = new List<int> {1, 2, 3};
            Stack<int> valuesToAdd = new Stack<int>();
            valuesToAdd.Push(6);
            valuesToAdd.Push(5);
            valuesToAdd.Push(4);

            actual.AddRange(valuesToAdd);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddDictionaryValuesToListTest()
        {
            List<int> expected = new List<int> {1, 2, 3, 4, 5, 6};
            List<int> actual = new List<int> {1, 2, 3};
            Dictionary<string, int> valuesToAdd = new Dictionary<string, int>()
            {
                {"four", 4},
                {"five", 5},
                {"six", 6},
            };

            actual.AddRange(valuesToAdd.Values);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void AddHashSetToListTest()
        {
            List<int> expected = new List<int> {1, 2, 3, 4, 5, 6};
            List<int> actual = new List<int> {1, 2, 3};
            HashSet<int> valuesToAdd = new HashSet<int> {4, 5, 6};

            actual.AddRange(valuesToAdd);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        #endregion

        // Test of SetRange() using List<T> and ObservableCollection<T> as targets, with different
        // implementations of IList, ICollection, and IEnumerables as parameters
        #region Test SetRange()

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetListToListTest()
        {
            List<int> expected = new List<int> { 4, 5, 6 };
            List<int> actual = new List<int> { 1, 2, 3 };
            List<int> valuesToSet = new List<int> { 4, 5, 6 };

            actual.SetRange(valuesToSet);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetObservableCollectionToListTest()
        {
            List<int> expected = new List<int> { 4, 5, 6 };
            List<int> actual = new List<int> { 1, 2, 3 };
            ObservableCollection<int> valuesToSet = new ObservableCollection<int> { 4, 5, 6 };

            actual.SetRange(valuesToSet);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetSortedListToListTest()
        {
            List<int> expected = new List<int> { 4, 5, 6 };
            List<int> actual = new List<int> { 1, 2, 3 };
            SortedList<int, int> valuesToSet = new SortedList<int, int>
            {
                {5, 5},
                {4, 4},
                {6, 6}
            };

            actual.SetRange(valuesToSet.Values);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetLinkedListToListTest()
        {
            List<int> expected = new List<int> { 4, 5, 6 };
            List<int> actual = new List<int> { 1, 2, 3 };
            LinkedList<int> valuesToSet = new LinkedList<int>();
            valuesToSet.AddLast(4);
            valuesToSet.AddLast(5);
            valuesToSet.AddLast(6);

            actual.SetRange(valuesToSet);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetQueueToListTest()
        {
            List<int> expected = new List<int> { 4, 5, 6 };
            List<int> actual = new List<int> { 1, 2, 3 };
            Queue<int> valuesToSet = new Queue<int>();
            valuesToSet.Enqueue(4);
            valuesToSet.Enqueue(5);
            valuesToSet.Enqueue(6);

            actual.SetRange(valuesToSet);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetStackToListTest()
        {
            List<int> expected = new List<int> { 4, 5, 6 };
            List<int> actual = new List<int> { 1, 2, 3 };
            Stack<int> valuesToSet = new Stack<int>();
            valuesToSet.Push(6);
            valuesToSet.Push(5);
            valuesToSet.Push(4);

            actual.SetRange(valuesToSet);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetDictionaryValuesToListTest()
        {
            List<int> expected = new List<int> { 4, 5, 6 };
            List<int> actual = new List<int> { 1, 2, 3 };
            Dictionary<string, int> valuesToSet = new Dictionary<string, int>()
            {
                {"four", 4},
                {"five", 5},
                {"six", 6},
            };

            actual.SetRange(valuesToSet.Values);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void SetHashSetToListTest()
        {
            List<int> expected = new List<int> { 4, 5, 6 };
            List<int> actual = new List<int> { 1, 2, 3 };
            HashSet<int> valuesToSet = new HashSet<int> { 4, 5, 6 };

            actual.SetRange(valuesToSet);

            Verify.AreEqual(expected.Count, actual.Count);

            for (var i = 0; i < expected.Count; i++)
            {
                Verify.AreEqual(expected[i], actual[i]);
            }
        }

        #endregion

    }
}