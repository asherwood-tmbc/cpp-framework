// ReSharper disable RedundantExplicitParamsArrayCreation
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace CPP.Framework.ComponentModel
{
    /// <summary>
    /// Dynamically accesses the value of a property at runtime.
    /// </summary>
    public sealed class DynamicPropertyAccessor
    {
        /// <summary>
        /// The map of cached <see cref="DynamicPropertyAccessor"/> instances to their associated 
        /// object types.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, DynamicPropertyAccessor> _TypeInstanceMap = new ConcurrentDictionary<Type, DynamicPropertyAccessor>();

        /// <summary>
        /// The cache of <see cref="PropertyAccessor"/> instances for each accessed property 
        /// defined by the current object type.
        /// </summary>
        private readonly ConcurrentDictionary<string, PropertyAccessor> _objectPropertyMap = new ConcurrentDictionary<string, PropertyAccessor>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPropertyAccessor"/> class. 
        /// </summary>
        /// <param name="objectType">
        /// The <see cref="Type"/> for the object whose properties are being accessed.
        /// </param>
        private DynamicPropertyAccessor(Type objectType)
        {
            ArgumentValidator.ValidateNotNull(() => objectType);
            this.ObjectType = objectType;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> for the object whose properties are being accessed.
        /// </summary>
        private Type ObjectType { get; }

        /// <summary>
        /// Gets the shared instance of a <see cref="DynamicPropertyAccessor"/> object for a given
        /// <see cref="Type"/>, creating one if necessary.
        /// </summary>
        /// <typeparam name="TObject">The type of the object.</typeparam>
        /// <returns>A <see cref="DynamicPropertyAccessor"/> value.</returns>
        public static DynamicPropertyAccessor GetInstance<TObject>() => GetInstance(typeof(TObject));

        /// <summary>
        /// Gets the shared instance of a <see cref="DynamicPropertyAccessor"/> object for a given
        /// <see cref="Type"/>, creating one if necessary.
        /// </summary>
        /// <param name="objectType">The type of the object.</param>
        /// <returns>A <see cref="DynamicPropertyAccessor"/> value.</returns>
        public static DynamicPropertyAccessor GetInstance(Type objectType)
        {
            ArgumentValidator.ValidateNotNull(() => objectType);
            return _TypeInstanceMap.GetOrAdd(objectType, ((ti) => new DynamicPropertyAccessor(ti)));
        }

        /// <summary>
        /// Gets a value that indicates whether or not a property can be read.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property can be read; otherwise, false.</returns>
        public bool CanGetValue(string propertyName)
        {
            return (GetPropertyAccessor(propertyName)?.CanGetValue ?? false);
        }

        /// <summary>
        /// Gets a value that indicates whether or not a property can be assigned.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property can be read; otherwise, false.</returns>
        public bool CanSetValue(string propertyName)
        {
            return (GetPropertyAccessor(propertyName)?.CanSetValue ?? false);
        }

        /// <summary>
        /// Gets a value that indicates whether or not a property is defined.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>True if the property can be read; otherwise, false.</returns>
        public bool IsDefined(string propertyName)
        {
            return (GetPropertyAccessor(propertyName) != null);
        }

        /// <summary>
        /// Reads the value for a property.
        /// </summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="instance">The instance of the object whose property is being read.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>The value of the property.</returns>
        /// <exception cref="InvalidCastException">
        /// <typeparamref name="TValue"/> does not match the property type.
        /// </exception>
        public TValue GetValue<TValue>(object instance, string propertyName)
        {
            return GetPropertyAccessor<TValue>(propertyName).GetValue(instance);
        }

        /// <summary>
        /// Assigned the value for a property.
        /// </summary>
        /// <typeparam name="TValue">The type of the property.</typeparam>
        /// <param name="instance">The instance of the object whose property is being read.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to assign to the property.</param>
        /// <returns>The original value of the property before the assignment.</returns>
        /// <exception cref="InvalidCastException">
        /// <typeparamref name="TValue"/> does not match the property type.
        /// </exception>
        public TValue SetValue<TValue>(object instance, string propertyName, TValue value)
        {
            return GetPropertyAccessor<TValue>(propertyName).SetValue(instance, value);
        }

        /// <summary>
        /// Gets the <see cref="PropertyAccessor"/> object for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>
        /// A <see cref="PropertyAccessor"/> object, or a null reference if 
        /// <paramref name="propertyName"/> is not defined by the class.
        /// </returns>
        private PropertyAccessor GetPropertyAccessor(string propertyName)
        {
            var accessor = _objectPropertyMap.GetOrAdd(
                propertyName,
                name =>
                {
                    const BindingFlags SearchFlags = (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    var instance = default(PropertyAccessor);
                    var propertyInfo = this.ObjectType.GetProperty(propertyName, SearchFlags);

                    if (propertyInfo != null)
                    {
                        var genericType = typeof(PropertyAccessor<>).MakeGenericType(propertyInfo.PropertyType);
                        var constructor = genericType.GetConstructor(new[] { typeof(PropertyInfo) });
                        instance = ((PropertyAccessor)constructor?.Invoke(new object[] { propertyInfo }));
                    }

                    return instance;
                });
            return accessor;
        }

        /// <summary>
        /// Gets the <see cref="PropertyAccessor{TValue}"/> object for a property.
        /// </summary>
        /// <typeparam name="TValue">The expected type of the property.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>
        /// A <see cref="PropertyAccessor{TValue}"/> object, or a null reference if 
        /// <paramref name="propertyName"/> is not defined by the class.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// <typeparamref name="TValue"/> does not match the property type.
        /// </exception>
        private PropertyAccessor<TValue> GetPropertyAccessor<TValue>(string propertyName) => ((PropertyAccessor<TValue>)this.GetPropertyAccessor(propertyName));

        #region PropertyAccessor Class Declaration

        /// <summary>
        /// Dynamically accesses the value of a property at runtime.
        /// </summary>
        private abstract class PropertyAccessor
        {
            /// <summary>
            /// Gets a value indicating whether or not the property value can be read.
            /// </summary>
            public abstract bool CanGetValue { get; }

            /// <summary>
            /// Gets a value indicating whether or not the property value can be assigned.
            /// </summary>
            public abstract bool CanSetValue { get; }

            /// <summary>
            /// Gets the name of the property.
            /// </summary>
            public abstract string Name { get; }

            /// <summary>
            /// Gets the <see cref="Type"/> of the property value.
            /// </summary>
            public abstract Type PropertyType { get; }
        }

        /// <summary>
        /// Dynamically accesses the value of a property at runtime.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        private sealed class PropertyAccessor<TValue> : PropertyAccessor
        {
            /// <summary>
            /// The dynamically compiled delegate for the property get method.
            /// </summary>
            private readonly Func<object, TValue> _getter;

            /// <summary>
            /// The dynamically compiled delegate for the property set method.
            /// </summary>
            private readonly Func<object, TValue, TValue> _setter;

            /// <summary>
            /// Initializes a new instance of the <see cref="PropertyAccessor{TValue}"/> class. 
            /// </summary>
            /// <param name="propertyInfo">
            /// The <see cref="PropertyInfo"/> for the property to access.
            /// </param>
            public PropertyAccessor(PropertyInfo propertyInfo)
            {
                ArgumentValidator.ValidateNotNull(() => propertyInfo);

                var objectType = (propertyInfo.DeclaringType);
                Contract.Assume(objectType != null);

                var instance = Expression.Parameter(typeof(object), "instance");
                var declared = Expression.Convert(instance, objectType);
                var property = Expression.Property(declared, propertyInfo);

                if (propertyInfo.CanRead)
                {
                    _getter = Expression.Lambda<Func<object, TValue>>(
                        property,
                        new[] { instance }).Compile();
                }

                if (propertyInfo.CanWrite)
                {
                    var newValue = Expression.Parameter(typeof(TValue), "newValue");
                    var oldValue = Expression.Parameter(typeof(TValue), "oldValue");

                    var condition = Expression.NotEqual(newValue, oldValue);
                    var updateVal = Expression.Assign(property, newValue);

                    _setter = Expression.Lambda<Func<object, TValue, TValue>>(
                        Expression.Block(
                            new[] { oldValue }, // variables for the block
                            Expression.Assign(oldValue, property),
                            Expression.IfThen(condition, updateVal),
                            oldValue),
                        new[] { instance, newValue }).Compile();
                }

                this.Name = propertyInfo.Name;
                this.PropertyType = propertyInfo.PropertyType;
            }

            /// <summary>
            /// Gets a value indicating whether or not the property value can be read.
            /// </summary>
            public override bool CanGetValue => (_getter != null);

            /// <summary>
            /// Gets a value indicating whether or not the property value can be assigned.
            /// </summary>
            public override bool CanSetValue => (_setter != null);

            /// <summary>
            /// Gets the name of the property.
            /// </summary>
            public override string Name { get; }

            /// <summary>
            /// Gets the <see cref="Type"/> of the property value.
            /// </summary>
            public override Type PropertyType { get; }

            /// <summary>
            /// Gets the value of the property for an instance.
            /// </summary>
            /// <param name="instance">The instance whose property to retrieve.</param>
            /// <returns>The property value.</returns>
            public TValue GetValue(object instance) => _getter(instance);

            /// <summary>
            /// Sets the value of a property for an instance.
            /// </summary>
            /// <param name="instance">The instance whose property is being set.</param>
            /// <param name="value">The value to set.</param>
            /// <returns>The original value of the property.</returns>
            public TValue SetValue(object instance, TValue value) => _setter(instance, value);
        }

        #endregion
    }
}
