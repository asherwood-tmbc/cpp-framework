using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using CPP.Framework.Cryptography;
using CPP.Framework.Data;

namespace CPP.Framework.WindowsAzure.Storage.Entities
{
    /// <summary>
    /// Manages serialized property data for a Windows Azure entity.
    /// </summary>
    public class AzureTableEntitySerializer<TModel> where TModel : class
    {
        private const string CHUNK_NAME_FORMAT = "{0}_C{1:X2}";
        private const int MAX_CHUNK_SIZE = (32 * 1024); // 64K - See https://msdn.microsoft.com/en-us/library/dd179338.aspx

        // ReSharper disable StaticFieldInGenericType
        private static readonly ConcurrentDictionary<string, Delegate> _CompiledGetterMap = new ConcurrentDictionary<string, Delegate>();
        private static readonly ConcurrentDictionary<string, Delegate> _CompiledSetterMap = new ConcurrentDictionary<string, Delegate>();
        private static readonly CultureInfo EnglishCultureInfo = CultureInfo.GetCultureInfo(1033);
        // ReSharper restore StaticFieldInGenericType

        private readonly IDictionary<string, EntityProperty> _Properties;
        private readonly TModel _TargetModel;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="model">The model being serialized or deserialized.</param>
        internal AzureTableEntitySerializer(TModel model) : this(model, new Dictionary<string, EntityProperty>()) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="model">The entity associated with the model whose properties are being serialized or deserialized.</param>
        /// <param name="properties">An optional dictionary that contains the existing property values.</param>
        internal AzureTableEntitySerializer(TModel model, IDictionary<string, EntityProperty> properties) : this(model, properties, false) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="model">The entity associated with the model whose properties are being serialized or deserialized.</param>
        /// <param name="properties">An optional dictionary that contains the existing property values.</param>
        /// <param name="attach">True to attach to <paramref name="properties"/> directly; otherwise, false to create a separate dictionary and copy the contents of <paramref name="properties"/>.</param>
        internal AzureTableEntitySerializer(TModel model, IDictionary<string, EntityProperty> properties, bool attach)
        {
            ArgumentValidator.ValidateNotNull(() => model);
            ArgumentValidator.ValidateNotNull(() => properties);
            _Properties = (attach ? properties : new Dictionary<string, EntityProperty>(properties));
            _TargetModel = model;
        }

        /// <summary>
        /// Breaks up a string property value into multiple chunks.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="buffer">The string value to chunk.</param>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> object that contains the chunks.</returns>
        private void ChunkPropertyData(string propertyName, string buffer)
        {
            if (buffer == null)
            {
                // ReSharper disable once ExpressionIsAlwaysNull
                _Properties[propertyName] = new EntityProperty(buffer);
            }
            else
            {
                int start = 0, index = 0, length = buffer.Length;
                var chunk = propertyName;
                for (; start < length; start += MAX_CHUNK_SIZE, index++)
                {
                    var count = Math.Min(MAX_CHUNK_SIZE, (length - start));
                    _Properties[chunk] = new EntityProperty(buffer.Substring(start, count));
                    chunk = String.Format(CHUNK_NAME_FORMAT, propertyName, index);
                }
            }
        }

        /// <summary>
        /// Converts an object value into a compressed string.
        /// </summary>
        /// <typeparam name="TObject">The type of the object to deflate.</typeparam>
        /// <param name="source">The object to deflate.</param>
        /// <returns>A compressed string that represents <paramref name="source"/>.</returns>
        public static string DeflateObject<TObject>(TObject source)
        {
            return DeflateObject(source, typeof(TObject));
        }

        /// <summary>
        /// Converts an object value into a compressed string.
        /// </summary>
        /// <param name="source">The object to deflate.</param>
        /// <param name="typeInfo">The type of the object to deflate.</param>
        /// <returns>A compressed string that represents <paramref name="source"/>.</returns>
        public static string DeflateObject(object source, Type typeInfo)
        {
            if (source == null) return null;
            using (var stream = new MemoryStream())
            {
                using (var writer = new GZipStream(stream, CompressionLevel.Optimal, true))
                {
                    var json = JsonConvert.SerializeObject(source);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    writer.Write(bytes, 0, bytes.Length);
                }
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        /// <summary>
        /// Verifies whether or not a property definition matches an expression.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="propertyInfo">The property definition to verify.</param>
        /// <param name="expression">An expression that evaluates to a property to match against <paramref name="propertyInfo"/>.</param>
        /// <returns>True if <paramref name="propertyInfo"/> matches the expression; otherwise, false.</returns>
        public static bool Equals<TValue>(PropertyInfo propertyInfo, Expression<Func<TModel, TValue>> expression)
        {
            return (propertyInfo == expression.GetMemberInfo());
        }

        /// <summary>
        /// Gets the compiled version of a property expression as a delegate.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="expression">The expression to compile or get.</param>
        /// <returns>A <see cref="Func{T1,TResult}"/> delegate that represents the compiled <paramref name="expression"/>.</returns>
        private static Func<TObject, TValue> GetCompiledGetter<TObject, TValue>(Expression<Func<TObject, TValue>> expression)
        {
            var qualifiedName = String.Format("{0}.{1}", typeof(TObject).Name, expression.GetMemberName());
            var compiled = (_CompiledGetterMap.GetOrAdd(qualifiedName, (name =>
            {
                return expression.Compile();
            })) as Func<TObject, TValue>);
            return compiled;
        }

        /// <summary>
        /// Gets the compiled version of a property expression as a delegate.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> for the property to compile.</param>
        /// <returns>A <see cref="Func{T1,TResult}"/> delegate that represents the compiled <paramref name="propertyInfo"/>.</returns>
        private static Func<TObject, TValue> GetCompiledGetter<TObject, TValue>(PropertyInfo propertyInfo)
        {
            var qualifiedName = String.Format("{0}.{1}", typeof(TObject).Name, propertyInfo.Name);
            var compiled = (_CompiledGetterMap.GetOrAdd(qualifiedName, (name =>
            {
                var parameter = Expression.Parameter(typeof(TObject), "model");
                var property = Expression.Property(parameter, propertyInfo);
                return Expression.Lambda<Func<TObject, TValue>>(property, parameter).Compile();
            })) as Func<TObject, TValue>);
            return compiled;
        }

        /// <summary>
        /// Gets the compiled version of a property expression as a delegate.
        /// </summary>
        /// <typeparam name="TObject">The type of the class that defines the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="expression">The expression to compile or get.</param>
        /// <returns>An <see cref="Action{T1,T2}"/> delegate that represents the compiled <paramref name="expression"/>.</returns>
        private static Action<TObject, TValue> GetCompiledSetter<TObject, TValue>(Expression<Func<TObject, TValue>> expression)
        {
            var propertyInfo = ((PropertyInfo)expression.GetMemberInfo());
            return GetCompiledSetter<TObject, TValue>(propertyInfo);
        }

        /// <summary>
        /// Gets the compiled version of a property expression as a delegate.
        /// </summary>
        /// <typeparam name="TObject">The type of the class that defines the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> for the property to compile.</param>
        /// <returns>An <see cref="Action{T1,T2}"/> delegate that represents the compiled <paramref name="propertyInfo"/>.</returns>
        private static Action<TObject, TValue> GetCompiledSetter<TObject, TValue>(PropertyInfo propertyInfo)
        {
            var qualifiedName = String.Format("{0}.{1}", typeof(TObject).Name, propertyInfo.Name);
            var compiled = (_CompiledSetterMap.GetOrAdd(qualifiedName, (name =>
            {
                var model = Expression.Parameter(typeof(TObject), "model");
                var value = Expression.Parameter(typeof(TValue), "value");
                var property = Expression.Property(model, propertyInfo);

                var setter = Expression.Lambda<Action<TObject, TValue>>(
                    Expression.Assign(property, value), model, value);
                return setter.Compile();
            })) as Action<TObject, TValue>);
            return compiled;
        }

        /// <summary>
        /// Gets a serializer that can be used to save or load properties for a custom model.
        /// </summary>
        /// <typeparam name="TCustom">The type of the child model.</typeparam>
        /// <param name="model">The child model to serialize.</param>
        /// <returns>An <see cref="AzureTableEntitySerializer{TModel}"/> instance.</returns>
        public AzureTableEntitySerializer<TCustom> GetCustomSerializer<TCustom>(TCustom model) where TCustom : class, new()
        {
            ArgumentValidator.ValidateNotNull(() => model);
            return new AzureTableEntitySerializer<TCustom>(model, _Properties, true);
        }

        /// <summary>
        /// Converts a compressed string into an object value.
        /// </summary>
        /// <typeparam name="TObject">The type of the list items.</typeparam>
        /// <param name="source">The compressed string to decode.</param>
        /// <returns>An object value that is represented by <paramref name="source"/>.</returns>
        public static TObject InflateObject<TObject>(string source) where TObject : new()
        {
            var value = InflateObject(source, typeof(TObject));
            return ((value == null) ? default(TObject) : ((TObject)value));
        }

        /// <summary>
        /// Converts a compressed string into an object value.
        /// </summary>
        /// <param name="source">The compressed string to decode.</param>
        /// <param name="typeInfo">The type of the object value.</param>
        /// <returns>An object value that is represented by <paramref name="source"/>.</returns>
        public static object InflateObject(string source, Type typeInfo)
        {
            if (String.IsNullOrWhiteSpace(source)) return null;
            using (var output = new MemoryStream())
            {
                var bytes = Convert.FromBase64String(source);

                using (var stream = new MemoryStream(bytes))
                using (var reader = new GZipStream(stream, CompressionMode.Decompress))
                {
                    reader.CopyTo(output);
                }

                var json = Encoding.UTF8.GetString(output.ToArray());
                return JsonConvert.DeserializeObject(json, typeInfo);
            }
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject, TValue>(TObject target, Expression<Func<TObject, TValue>> expression) where TValue : class, new()
        {
            return this.LoadObject(target, expression, InflateObject<TValue>);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, bool?>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property))
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.BooleanValue);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, bool>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property) && property.BooleanValue.HasValue)
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.BooleanValue.Value);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, byte[]>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property) && (property.BinaryValue != null))
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.BinaryValue);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, DateTime?>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property))
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.DateTime);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, DateTime>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property) && property.DateTime.HasValue)
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.DateTime.Value);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, DateTimeOffset?>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property))
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.DateTimeOffsetValue);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, DateTimeOffset>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property) && property.DateTimeOffsetValue.HasValue)
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.DateTimeOffsetValue.Value);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, double?>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property))
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.DoubleValue);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, double>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property) && property.DoubleValue.HasValue)
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.DoubleValue.Value);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, Guid?>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property))
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.GuidValue);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, Guid>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property) && property.GuidValue.HasValue)
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.GuidValue.Value);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, Int32?>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property))
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.Int32Value);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, Int32>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property) && property.Int32Value.HasValue)
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.Int32Value.Value);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, Int64?>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property))
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.Int64Value);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, Int64>> expression)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property) && property.Int64Value.HasValue)
            {
                var setter = GetCompiledSetter(expression);
                setter(target, property.Int64Value.Value);
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, string>> expression)
        {
            return this.Load(target, expression, null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <param name="decodeAction">A delegate to use to decode the value of the string, or null to just return the value without decoding it.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, Expression<Func<TObject, string>> expression, Func<string, string> decodeAction)
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property) && (property.StringValue != null))
            {
                decodeAction = (decodeAction ?? (input => input));
                var setter = GetCompiledSetter(expression);
                var buffer = this.MergePropertyData(propertyName);
                setter(target, decodeAction(buffer));
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="target">The source object that contains the property value.</param>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> for the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool Load<TObject>(TObject target, PropertyInfo propertyInfo)
        {
            var typeInfo = propertyInfo.PropertyType;
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyInfo.Name, out property))
            {
                if (typeInfo == typeof(bool))
                {
                    if (!property.BooleanValue.HasValue) return false;
                    var setter = GetCompiledSetter<TObject, bool>(propertyInfo);
                    setter(target, property.BooleanValue.Value);
                }
                else if (typeInfo == typeof(bool?))
                {
                    var setter = GetCompiledSetter<TObject, bool?>(propertyInfo);
                    setter(target, property.BooleanValue);
                }
                else if (typeInfo == typeof(byte[]))
                {
                    if (property.BinaryValue == null) return false;
                    var setter = GetCompiledSetter<TObject, byte[]>(propertyInfo);
                    setter(target, property.BinaryValue);
                }
                else if (typeInfo == typeof(DateTime))
                {
                    if (!property.DateTime.HasValue) return false;
                    var setter = GetCompiledSetter<TObject, DateTime>(propertyInfo);
                    setter(target, property.DateTime.Value);
                }
                else if (typeInfo == typeof(DateTime?))
                {
                    var setter = GetCompiledSetter<TObject, DateTime?>(propertyInfo);
                    setter(target, property.DateTime);
                }
                else if (typeInfo == typeof(DateTimeOffset))
                {
                    if (!property.DateTimeOffsetValue.HasValue) return false;
                    var setter = GetCompiledSetter<TObject, DateTimeOffset>(propertyInfo);
                    setter(target, property.DateTimeOffsetValue.Value);
                }
                else if (typeInfo == typeof(DateTimeOffset?))
                {
                    var setter = GetCompiledSetter<TObject, DateTimeOffset?>(propertyInfo);
                    setter(target, property.DateTimeOffsetValue);
                }
                else if (typeInfo == typeof(double))
                {
                    if (!property.DoubleValue.HasValue) return false;
                    var setter = GetCompiledSetter<TObject, double>(propertyInfo);
                    setter(target, property.DoubleValue.Value);
                }
                else if (typeInfo == typeof(double?))
                {
                    var setter = GetCompiledSetter<TObject, double?>(propertyInfo);
                    setter(target, property.DoubleValue);
                }
                else if (typeInfo == typeof(Guid))
                {
                    if (!property.GuidValue.HasValue) return false;
                    var setter = GetCompiledSetter<TObject, Guid>(propertyInfo);
                    setter(target, property.GuidValue.Value);
                }
                else if (typeInfo == typeof(Guid?))
                {
                    var setter = GetCompiledSetter<TObject, Guid?>(propertyInfo);
                    setter(target, property.GuidValue);
                }
                else if (typeInfo == typeof(Int32))
                {
                    if (!property.Int32Value.HasValue) return false;
                    var setter = GetCompiledSetter<TObject, Int32>(propertyInfo);
                    setter(target, property.Int32Value.Value);
                }
                else if (typeInfo == typeof(Int32?))
                {
                    var setter = GetCompiledSetter<TObject, Int32?>(propertyInfo);
                    setter(target, property.Int32Value);
                }
                else if (typeInfo == typeof(Int64))
                {
                    if (!property.Int64Value.HasValue) return false;
                    var setter = GetCompiledSetter<TObject, Int64>(propertyInfo);
                    setter(target, property.Int64Value.Value);
                }
                else if (typeInfo == typeof(Int32?))
                {
                    var setter = GetCompiledSetter<TObject, Int64?>(propertyInfo);
                    setter(target, property.Int64Value);
                }
                else
                {
                    var buffer = this.MergePropertyData(propertyInfo.Name, property);

                    if (propertyInfo.GetCustomAttributes<EncryptedPropertyAttribute>().Any())
                    {
                        var certificateEncryption = CertificateEncryptionProvider.Current;
                        buffer = certificateEncryption.DecryptValue(buffer);
                    }

                    if (String.IsNullOrWhiteSpace(buffer)) return false;

                    if (typeInfo == typeof(string))
                    {
                        var setter = GetCompiledSetter<TObject, string>(propertyInfo);
                        setter(target, buffer);
                    }
                    else
                    {
                        var value = InflateObject(buffer, typeInfo);
                        propertyInfo.SetValue(target, value);
                    }
                }
                return true;
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, bool?>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, bool>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, byte[]>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, DateTime?>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, DateTime>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, DateTimeOffset?>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, DateTimeOffset>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, double?>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, double>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, Guid?>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, Guid>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, Int32?>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, Int32>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, Int64?>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, Int64>> expression)
        {
            return this.Load(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, string>> expression)
        {
            return this.Load(_TargetModel, expression, null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <param name="decodeAction">A delegate to use to decode the value of the string, or null to just return the value without decoding it.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool Load(Expression<Func<TModel, string>> expression, Func<string, string> decodeAction)
        {
            return this.Load(_TargetModel, expression, decodeAction);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool LoadEnum<TObject, TValue>(TObject target, Expression<Func<TObject, TValue>> expression)
            where TValue : struct, IConvertible, IFormattable, IComparable
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property) && (property.StringValue != null))
            {
                var setter = GetCompiledSetter(expression);
                var buffer = this.MergePropertyData(propertyName);
                setter(target, ((TValue)Enum.Parse(typeof(TValue), buffer)));
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool LoadEnum<TValue>(Expression<Func<TModel, TValue>> expression)
            where TValue : struct, IConvertible, IFormattable, IComparable
        {
            return this.LoadEnum(_TargetModel, expression);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="target">The target object that defines the property to load.</param>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <param name="deserializeAction">A delegate to use to deserialize the string value for <paramref name="expression"/>, or null to use the default deserializer.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        internal bool LoadObject<TObject, TValue>(TObject target, Expression<Func<TObject, TValue>> expression, Func<string, TValue> deserializeAction) where TValue : class, new()
        {
            var propertyName = expression.GetMemberName();
            EntityProperty property = null;
            if (_Properties.TryGetValue(propertyName, out property) && (property.StringValue != null))
            {
                deserializeAction = (deserializeAction ?? (input => InflateObject<TValue>(input)));
                var setter = GetCompiledSetter(expression);
                var buffer = this.MergePropertyData(propertyName);
                setter(target, deserializeAction(buffer));
            }
            return (property != null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool LoadObject<TValue>(Expression<Func<TModel, TValue>> expression) where TValue : class, new()
        {
            return this.LoadObject(_TargetModel, expression, null);
        }

        /// <summary>
        /// Loads the value for a property from the serializer.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="expression">An expression that evaluates to the property to deserialize.</param>
        /// <param name="deserializeAction">A delegate to use to deserialize the string value for <paramref name="expression"/>, or null to use the default JSON deserializer.</param>
        /// <returns>True if the property value was loaded; otherwise, false if a value for the property does not exist in the serializer.</returns>
        public bool LoadObject<TValue>(Expression<Func<TModel, TValue>> expression, Func<string, TValue> deserializeAction) where TValue : class, new()
        {
            return this.LoadObject(_TargetModel, expression, deserializeAction);
        }

        /// <summary>
        /// Merges the chunks for a string property into a single value.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="property">The first chunk in the property, if available.</param>
        /// <returns>A <see cref="string"/> that contains the merged value.</returns>
        private string MergePropertyData(string propertyName, EntityProperty property = null)
        {
            var chunk = propertyName;
            var index = 0;
            var buffer = new StringBuilder();

            do
            {
                if (property == null) continue;
                if (property.StringValue == null) continue;
                buffer.Append(property.StringValue);
                chunk = String.Format(CHUNK_NAME_FORMAT, propertyName, (index)++);
            } while (_Properties.TryGetValue(chunk, out property));

            return buffer.ToString();
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject, TValue>(TObject source, Expression<Func<TObject, TValue>> expression, bool overwrite = true)
            where TValue : class, new()
        {
            this.SaveObject(source, expression, DeflateObject, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, bool?>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, bool>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, byte[]>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, DateTime?>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, DateTime>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, DateTimeOffset?>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, DateTimeOffset>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, double?>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, double>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, Guid?>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, Guid>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, int?>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, int>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, long?>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, long>> expression, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var entityProperty = new EntityProperty(getter(source));
                _Properties[propertyName] = entityProperty;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, string>> expression, bool overwrite = true)
        {
            this.Save(source, expression, null, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="encodeAction">A delegate to call to encode the data, or null to leave the data unencoded.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, Expression<Func<TObject, string>> expression, Func<string, string> encodeAction, bool overwrite = true)
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                encodeAction = (encodeAction ?? (input => input));
                var value = GetCompiledGetter(expression)(source);
                ChunkPropertyData(propertyName, encodeAction(value));
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> for the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void Save<TObject>(TObject source, PropertyInfo propertyInfo, bool overwrite = true)
        {
            if (overwrite || (!_Properties.ContainsKey(propertyInfo.Name)))
            {
                var typeInfo = propertyInfo.PropertyType;
                EntityProperty property = null;
                if (typeInfo == typeof(bool))
                {
                    var getter = GetCompiledGetter<TObject, bool>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(bool?))
                {
                    var getter = GetCompiledGetter<TObject, bool?>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(byte[]))
                {
                    var getter = GetCompiledGetter<TObject, byte[]>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(DateTime))
                {
                    var getter = GetCompiledGetter<TObject, DateTime>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(DateTime?))
                {
                    var getter = GetCompiledGetter<TObject, DateTime?>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(DateTimeOffset))
                {
                    var getter = GetCompiledGetter<TObject, DateTimeOffset>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(DateTimeOffset?))
                {
                    var getter = GetCompiledGetter<TObject, DateTimeOffset?>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(double))
                {
                    var getter = GetCompiledGetter<TObject, double>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(double?))
                {
                    var getter = GetCompiledGetter<TObject, double?>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(Guid))
                {
                    var getter = GetCompiledGetter<TObject, Guid>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(Guid?))
                {
                    var getter = GetCompiledGetter<TObject, Guid?>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(Int32))
                {
                    var getter = GetCompiledGetter<TObject, Int32>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(Int32?))
                {
                    var getter = GetCompiledGetter<TObject, Int32?>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(Int64))
                {
                    var getter = GetCompiledGetter<TObject, Int64>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else if (typeInfo == typeof(Int64?))
                {
                    var getter = GetCompiledGetter<TObject, Int64?>(propertyInfo);
                    property = new EntityProperty(getter(source));
                }
                else
                {
                    string buffer = null;
                    if (typeInfo == typeof(string))
                    {
                        var getter = GetCompiledGetter<TObject, string>(propertyInfo);
                        buffer = getter(source);
                    }
                    else
                    {
                        var value = propertyInfo.GetPropertyValue(source);
                        buffer = DeflateObject(value, typeInfo);
                    }

                    if (buffer != null && propertyInfo.GetCustomAttributes<EncryptedPropertyAttribute>().Any())
                    {
                        var certificateEncryption = CertificateEncryptionProvider.Current;
                        buffer = certificateEncryption.EncryptValue(buffer);
                    }

                    ChunkPropertyData(propertyInfo.Name, buffer);
                }
                if (property != null) _Properties[propertyInfo.Name] = property;
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void Save(Expression<Func<TModel, bool?>> expression, bool overwrite = true)
        {
            this.Save(_TargetModel, expression, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void Save(Expression<Func<TModel, byte[]>> expression, bool overwrite = true)
        {
            this.Save(_TargetModel, expression, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void Save(Expression<Func<TModel, DateTime?>> expression, bool overwrite = true)
        {
            this.Save(_TargetModel, expression, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void Save(Expression<Func<TModel, DateTimeOffset?>> expression, bool overwrite = true)
        {
            this.Save(_TargetModel, expression, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void Save(Expression<Func<TModel, double?>> expression, bool overwrite = true)
        {
            this.Save(_TargetModel, expression, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void Save(Expression<Func<TModel, Guid?>> expression, bool overwrite = true)
        {
            this.Save(_TargetModel, expression, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void Save(Expression<Func<TModel, int?>> expression, bool overwrite = true)
        {
            this.Save(_TargetModel, expression, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void Save(Expression<Func<TModel, long?>> expression, bool overwrite = true)
        {
            this.Save(_TargetModel, expression, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void Save(Expression<Func<TModel, string>> expression, bool overwrite = true)
        {
            this.Save(_TargetModel, expression, null, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="encodeAction">A delegate to call to encode the data, or null to leave the data unencoded.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void Save(Expression<Func<TModel, string>> expression, Func<string, string> encodeAction, bool overwrite = true)
        {
            this.Save(_TargetModel, expression, encodeAction, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void SaveEnum<TObject, TValue>(TObject source, Expression<Func<TObject, TValue>> expression, bool overwrite = true)
            where TValue : struct, IConvertible, IFormattable, IComparable
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                var getter = GetCompiledGetter(expression);
                var value = getter(source);
                ChunkPropertyData(propertyName, value.ToString(EnglishCultureInfo));
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void SaveEnum<TValue>(Expression<Func<TModel, TValue>> expression, bool overwrite = true)
            where TValue : struct, IConvertible, IFormattable, IComparable
        {
            this.SaveEnum(_TargetModel, expression, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TObject">The type of the object that defines the property.</typeparam>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="source">The source object that contains the property value.</param>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="serializeAction">A delegate that is used to serialize the value of <paramref name="expression"/> to a string, or null to use the default JSON serializer.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        internal void SaveObject<TObject, TValue>(TObject source, Expression<Func<TObject, TValue>> expression, Func<TValue, string> serializeAction, bool overwrite = true)
            where TValue : class
        {
            var propertyName = expression.GetMemberName();
            if (overwrite || (!_Properties.ContainsKey(propertyName)))
            {
                serializeAction = (serializeAction ?? (input => DeflateObject(input)));
                var getter = GetCompiledGetter(expression);
                var value = getter(source);
                ChunkPropertyData(propertyName, serializeAction(value));
            }
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void SaveObject<TValue>(Expression<Func<TModel, TValue>> expression, bool overwrite = true)
            where TValue : class
        {
            this.SaveObject(_TargetModel, expression, null, overwrite);
        }

        /// <summary>
        /// Saves the value for a property in the serializer.
        /// </summary>
        /// <typeparam name="TValue">The type of the property value.</typeparam>
        /// <param name="expression">An expression that evaluates to the property to serialize.</param>
        /// <param name="serializeAction">A delegate that is used to serialize the value of <paramref name="expression"/> to a string, or null to use the default JSON serializer.</param>
        /// <param name="overwrite">True to overwrite a value is it already exists in the property set; otherwise, false.</param>
        public void SaveObject<TValue>(Expression<Func<TModel, TValue>> expression, Func<TValue, string> serializeAction, bool overwrite = true)
            where TValue : class
        {
            this.SaveObject(_TargetModel, expression, serializeAction, overwrite);
        }

        /// <summary>
        /// Converts the serializer to a dictionary of property names to 
        /// <see cref="EntityProperty"/> objects.
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> object.</returns>
        public Dictionary<string, EntityProperty> ToDictionary() { return new Dictionary<string, EntityProperty>(_Properties); }
    }
}
