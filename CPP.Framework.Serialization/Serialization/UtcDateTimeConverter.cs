using System;
using System.Globalization;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CPP.Framework.Serialization
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> value to and from the ISO 8601 format, always ensuring 
    /// the timezone for the value is UTC.
    /// </summary>
    public class UtcDateTimeConverter : IsoDateTimeConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UtcDateTimeConverter"/> class.
        /// </summary>
        public UtcDateTimeConverter()
        {
            this.DateTimeStyles |= (DateTimeStyles.RoundtripKind | DateTimeStyles.AdjustToUniversal);
            this.DateTimeFormat = "yyyy-MM-ddTHH:mm:ssK";
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var type = GetUnderlyingType(objectType);
            if (reader.TokenType == JsonToken.Date)
            {
                // we have to do a slight tweak in this case, because by default the implementation 
                // of ReadJson for IsoDateTimeConverter uses the DateTime property when changing a 
                // DateTimeOffset to a DateTime value, which leaves the DateTimeKind as Unspecified.
                if ((type == typeof(DateTime)) && (reader.Value is DateTimeOffset offset))
                {
                    return offset.UtcDateTime;
                }
            }
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }

        /// <summary>
        /// Gets the underlying type of a nullable value type.
        /// </summary>
        /// <param name="objectType">The type of the object.</param>
        /// <returns>
        /// The underlying type of <paramref name="objectType"/>, or the value of 
        /// <paramref name="objectType"/> if it is not a nullable value type.
        /// </returns>
        private static Type GetUnderlyingType(Type objectType)
        {
            if (objectType.IsGenericType && (objectType.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                objectType = Nullable.GetUnderlyingType(objectType);
            }
            return objectType;
        }
    }
}
