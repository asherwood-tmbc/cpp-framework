using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace CPP.Framework.Collections
{
    /// <summary>
    /// Automatically maps class properties that return <see cref="ICollection{T}"/> compatible 
    /// types to their corresponding property names via reflection. Please note that only collection
    /// properties for reference types are mapped; collections of value types are not supported.
    /// </summary>
    /// <typeparam name="T">The type of the class to map.</typeparam>
    public sealed class CollectionPropertyMap<T> :
        ICollectionPropertyMap
        where T : class
    {
        /// <summary>
        /// The <see cref="BindingFlags"/> to use when searching for collection properties.
        /// </summary>
        private const BindingFlags SearchFlags = (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

        /// <summary>
        /// The set of properties for objects in the collection.
        /// </summary>
        // ReSharper disable once StaticFieldInGenericType
        private static readonly HashSet<CollectionProperty> _PropertySet = new HashSet<CollectionProperty>();

        /// <summary>
        /// The map of property names to their collections.
        /// </summary>
        private readonly ReadOnlyDictionary<string, IContraVariantCollection> _collectionPropertyMap;

        /// <summary>
        /// Initializes static members of the <see cref="CollectionPropertyMap{T}"/> class. 
        /// </summary>
        static CollectionPropertyMap()
        {
            // add the information for the properties to the set, which will be shared by all
            // instances of the closed generic for the current type, but not globally (which is
            // exactly what we want).
            _PropertySet.UnionWith(GetCollectionProperties(typeof(T)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionPropertyMap{T}"/> class. 
        /// </summary>
        /// <param name="objectReference">
        /// The object instance whose properties are being mapped.
        /// </param>
        public CollectionPropertyMap(T objectReference)
        {
            ArgumentValidator.ValidateNotNull(() => objectReference);
            var map = _PropertySet.ToDictionary(
                pi => pi.PropertyInfo.Name,
                pi =>
                    {
                        // create a closed generic type for the collection type, then get the value of the 
                        // collection property from the object reference, and then return it wrapped inside 
                        // a ContraVariantCollection object.
                        var typeInfo = typeof(ContraVariantCollection<>)
                            .MakeGenericType(pi.CollectionType);
                        var collection = pi.PropertyInfo.GetValue(objectReference);
                        return ((IContraVariantCollection)Activator.CreateInstance(typeInfo, collection));
                    });
            _collectionPropertyMap = new ReadOnlyDictionary<string, IContraVariantCollection>(map);
        }

        /// <summary>
        /// Gets the <see cref="IContraVariantCollection"/> for a given property name.
        /// </summary>
        /// <param name="propertyName">The name of the collection property.</param>
        /// <returns>An <see cref="IContraVariantCollection"/> instance.</returns>
        public IContraVariantCollection this[string propertyName] => _collectionPropertyMap[propertyName];

        /// <summary>
        /// Locates all of the collection properties declared by a type and its ancestors.
        /// </summary>
        /// <param name="declaringType">The type to search.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the sequence of
        /// found <see cref="CollectionProperty"/> instances for each collection property.
        /// </returns>
        private static IEnumerable<CollectionProperty> GetCollectionProperties(Type declaringType)
        {
            while (declaringType != null)
            {
                if (declaringType == typeof(object)) break;

                foreach (var propertyInfo in declaringType.GetProperties(SearchFlags))
                {
                    var returnType = propertyInfo.PropertyType;

                    // first check if the property returns an ICollection<T> value directly.
                    Type collectionType = null;
                    if ((returnType.IsGenericType) && (returnType.GetGenericTypeDefinition() == typeof(ICollection<>)))
                    {
                        // get the type of the collection items
                        collectionType = returnType.GenericTypeArguments[0];
                    }
                    else
                    {
                        // if the property doesn't return ICollection<T> directly, then verify whether
                        // or not it returns a type that implements (which will also match array types
                        // as well, just an FYI).
                        var candidate = returnType.GetInterfaces()
                            .Where(ti => (ti.IsGenericType))
                            .Where(ti => (ti.GetGenericTypeDefinition() == typeof(ICollection<>)))
                            .FirstOrDefault();
                        if (candidate != null) collectionType = candidate.GenericTypeArguments[0];
                    }
                    if (collectionType == null) continue; // property isn't a collection type
                    if (collectionType.IsValueType) continue; // property is a value collection (not supported)

                    yield return new CollectionProperty(propertyInfo, collectionType);
                }

                declaringType = (declaringType.BaseType ?? typeof(object));
            }
        }

        #region CollectionProperty Class Declaration

        /// <summary>
        /// Metadata class used to track information related to a collection property.
        /// </summary>
        private sealed class CollectionProperty : IEquatable<CollectionProperty>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CollectionProperty"/> class. 
            /// </summary>
            /// <param name="propertyInfo">
            /// The <see cref="PropertyInfo"/> for the collection property.
            /// </param>
            /// <param name="collectionType">
            /// The type for the items in the collection.
            /// </param>
            internal CollectionProperty(PropertyInfo propertyInfo, Type collectionType)
            {
                this.CollectionType = collectionType;
                this.PropertyInfo = propertyInfo;
            }

            /// <summary>
            /// Gets the type for the items in the collection.
            /// </summary>
            internal Type CollectionType { get; }

            /// <summary>
            /// Gets the <see cref="PropertyInfo"/> for the collection property.
            /// </summary>
            internal PropertyInfo PropertyInfo { get; }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="that">An object to compare with this object.</param>
            /// <returns>True if the current object is equal to the <paramref name="that"/> parameter; otherwise, false.</returns>
            public bool Equals(CollectionProperty that)
            {
                return ((that != null) && (that.PropertyInfo == this.PropertyInfo) && (that.CollectionType == this.CollectionType));
            }
        }

        #endregion // CollectionProperty Class Declaration
    }
}
