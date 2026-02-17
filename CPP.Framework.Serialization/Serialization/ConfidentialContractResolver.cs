using System;
using System.Reflection;

using CPP.Framework.Data;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CPP.Framework.Serialization
{
    /// <summary>
    /// Used to resolve a <see cref="JsonContract"/> for a given type, while filtering out any
    /// fields or properties that may be marked as confidential.
    /// </summary>
    public sealed class ConfidentialContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Creates a <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.
        /// </summary>
        /// <param name="member">The member to create a <see cref="JsonProperty" /> for.</param>
        /// <param name="memberSerialization">The member's parent <see cref="MemberSerialization" />.</param>
        /// <returns>A created <see cref="JsonProperty" /> for the given <see cref="MemberInfo" />.</returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (member.HasCustomAttribute<ConfidentialAttribute>(true))
            {
                property.ShouldSerialize = ((instance) => false);
            }
            return property;
        }

        #region JsonConfidentialPropertyConverter Class Declaration

        /// <summary>
        /// Suppressing serialization for properties marked with a 
        /// <see cref="ConfidentialAttribute"/>.
        /// </summary>
        private sealed class JsonConfidentialPropertyConverter : JsonConverter
        {
            /// <summary>
            /// The base <see cref="JsonConverter"/> for the type.
            /// </summary>
            private readonly JsonConverter _converter;

            /// <summary>
            /// Initializes a new instance of the <see cref="JsonConfidentialPropertyConverter"/>
            /// class.
            /// </summary>
            /// <param name="baseConverter">The base <see cref="JsonConverter"/> for the type.</param>
            public JsonConfidentialPropertyConverter(JsonConverter baseConverter)
            {
                _converter = baseConverter;
            }

            /// <summary>
            /// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.
            /// </summary>
            /// <value><c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON; otherwise, <c>false</c>.</value>
            public override bool CanRead { get; } = true;

            /// <summary>
            /// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON.
            /// </summary>
            /// <value><c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON; otherwise, <c>false</c>.</value>
            public override bool CanWrite { get; } = false;

            /// <summary>
            /// Determines whether this instance can convert the specified object type.
            /// </summary>
            /// <param name="objectType">Type of the object.</param>
            /// <returns>
            /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
            /// </returns>
            public override bool CanConvert(Type objectType) => true;
            
            /// <summary>Reads the JSON representation of the object.</summary>
            /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
            /// <param name="objectType">Type of the object.</param>
            /// <param name="existingValue">The existing value of object being read.</param>
            /// <param name="serializer">The calling serializer.</param>
            /// <returns>The object value.</returns>
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return _converter.ReadJson(reader, objectType, existingValue, serializer);
            }

            /// <summary>Writes the JSON representation of the object.</summary>
            /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
            /// <param name="value">The value.</param>
            /// <param name="serializer">The calling serializer.</param>
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
