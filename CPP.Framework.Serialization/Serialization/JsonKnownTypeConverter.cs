using System;
using System.Runtime.Serialization;
using System.Threading;
using CPP.Framework.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CPP.Framework.Serialization
{
    /// <summary>
    /// <see cref="JsonConverter"/> that deserializes polymorphic objects using the 
    /// <see cref="KnownTypeAttribute"/> and a <see cref="JsonKnownTypeIndicatorAttribute"/>.
    /// </summary>
    /// <typeparam name="TBase"></typeparam>
    public class JsonKnownTypeConverter<TBase> : JsonConverter
    {
        private static readonly Lazy<JsonKnownTypeResolver> _TypeResolver = new Lazy<JsonKnownTypeResolver>(() => JsonKnownTypeResolver.Create(typeof(TBase)), LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>True if this instance can convert the specified object type; otherwise, false.</returns>
        public override bool CanConvert(Type objectType)
        {
            return (typeof(TBase).IsAssignableFrom(objectType));
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter"/> can write JSON.
        /// </summary>
        /// <value>True if this <see cref="T:Newtonsoft.Json.JsonConverter"/> can write JSON; otherwise, false.</value>
        public override bool CanWrite { get { return false; /* use the default converter to write */ } }

        /// <summary>
        /// Creates an instance of known type for a deserialized object.
        /// </summary>
        /// <param name="jObject">The JSON object for which to create the known type.</param>
        /// <returns>A known type object.</returns>
        protected virtual TBase CreateKnownType(JObject jObject)
        {
            var resolver = _TypeResolver.Value;
            var knownType = typeof(TBase);
            if (resolver != null)
            {
                knownType = resolver.ResolveType(jObject);
            }
            return ((TBase)ServiceLocator.GetInstance(knownType));
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var target = default(TBase);
            if (reader.TokenType != JsonToken.None)
            {
                // get the JSON object from the stream, and create a new reader for it.
                var jObject = JObject.Load(reader);
                var jObjectReader = jObject.CreateReader();
                jObjectReader.Culture = reader.Culture;
                jObjectReader.DateFormatString = reader.DateFormatString;
                jObjectReader.DateParseHandling = reader.DateParseHandling;
                jObjectReader.DateTimeZoneHandling = reader.DateTimeZoneHandling;
                jObjectReader.FloatParseHandling = reader.FloatParseHandling;
                jObjectReader.MaxDepth = reader.MaxDepth;
                jObjectReader.SupportMultipleContent = reader.SupportMultipleContent;

                // create a known type instance based on the JSON object and populate it.
                target = this.CreateKnownType(jObject);
                serializer.Populate(jObjectReader, target);
            }
            return target;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
