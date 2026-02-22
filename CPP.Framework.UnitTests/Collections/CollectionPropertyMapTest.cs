using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Collections
{
    [TestClass]
    public class CollectionPropertyMapTest
    {
        [ExcludeFromCodeCoverage]
        [TestMethod]
        public void CreateCollectionPropertyMapValueTypeCollectionObjects()
        {
            var testObject = new ValueTypeCollectionObject();
            var actual = new CollectionPropertyMap<ValueTypeCollectionObject>(testObject);

            var ignoredPropertyNames = new[]
            {
                "IntList", "ObjectStack", "ObjectQueue", "IntHashSet", "ObjectObservableCollection", "ObjectDictionary"
            };

            foreach (var ignoredPropertyName in ignoredPropertyNames)
                try
                {
                    var ignored = actual[ignoredPropertyName];
                }
                catch (Exception ex)
                {
                    ex.Should().BeOfType<KeyNotFoundException>();
                }
        }

        [ExcludeFromCodeCoverage]
        [TestMethod]
        public void CreateCollectionPropertyMapReferenceTypeCollectionObject()
        {
            var testObject = new ReferenceTypeCollectionObject();
            var actual = new CollectionPropertyMap<ReferenceTypeCollectionObject>(testObject);

            var expectedPropertyTypes = new Dictionary<string, Type>
            {
                {"ObjectList", typeof(object)},
                {"ObjectHashSet", typeof(object)}
            };

            Console.WriteLine(actual);

            foreach (var propertyName in expectedPropertyTypes.Keys)
            {
                actual[propertyName].Should().NotBeNull();
                actual[propertyName].GetType().GetGenericArguments()[0].Should().Be(expectedPropertyTypes[propertyName]);
            }
        }

        [ExcludeFromCodeCoverage]
        [TestMethod]
        [ExpectedException(typeof(TargetInvocationException))]
        public void CreateCollectionPropertyUninitializedCollectionObject()
        {
            var testObject = new UninitializedCollectionObject();
            var actual = new CollectionPropertyMap<UninitializedCollectionObject>(testObject); // should throw exception
        }

        [ExcludeFromCodeCoverage]
        [TestMethod]
        public void CreateCollectionPropertyMapComplextObject()
        {
            var testObject = new ComplexObject();
            var actual = new CollectionPropertyMap<ComplexObject>(testObject);

            var expectedPropertyTypes = new Dictionary<string, Type>
            {
                {"ObjectList", typeof(object)},
                {"InnerObject", typeof(SimpleListObject)}
            };

            foreach (var propertyName in expectedPropertyTypes.Keys)
            {
                actual[propertyName].Should().NotBeNull();
                actual[propertyName].GetType().GetGenericArguments()[0].Should().Be(expectedPropertyTypes[propertyName]);
            }
        }

        #region Inner class used for unit test
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class ValueTypeCollectionObject
        {
            public ValueTypeCollectionObject()
            {
                IntList = new List<int>();
                ObjectStack = new Stack<object>();
                ObjectQueue = new Queue<object>();
                IntHashSet = new HashSet<int>();
                ObjectObservableCollection = new ObservableCollection<object>();
                ObjectDictionary = new Dictionary<object, object>();
            }

            public List<int> IntList { get; set; }
            public Stack<object> ObjectStack { get; set; }
            public Queue<object> ObjectQueue { get; set; }
            public HashSet<int> IntHashSet { get; set; }
            public ObservableCollection<object> ObjectObservableCollection { get; set; }
            public Dictionary<object, object> ObjectDictionary { get; set; }
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class ReferenceTypeCollectionObject
        {
            public ReferenceTypeCollectionObject()
            {
                ObjectList = new List<object>();
                ObjectHashSet = new HashSet<object>();
            }

            public List<object> ObjectList { get; set; }
            public HashSet<object> ObjectHashSet { get; set; }
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class SimpleListObject
        {
            public SimpleListObject()
            {
                ObjectList = new List<object>();
            }

            public List<object> ObjectList { get; set; }
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class UninitializedCollectionObject
        {
            // This should throw an exception when mapped
            public List<object> ObjectList { get; set; }
        }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
        private class ComplexObject
        {
            public ComplexObject()
            {
                ObjectList = new List<object>();
                InnerObject = new List<SimpleListObject>();
            }

            public List<object> ObjectList { get; set; }
            public List<SimpleListObject> InnerObject { get; set; }
        }
        #endregion
    }
}