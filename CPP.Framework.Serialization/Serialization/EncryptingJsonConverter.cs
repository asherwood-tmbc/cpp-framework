using System;
using Newtonsoft.Json;
using CPP.Framework.Cryptography;

namespace CPP.Framework.Serialization
{
    /// <summary>
    /// <see cref="JsonConverter"/> implementation that allows certain property values to be 
    /// automatically encrypted in the JSON string, and decrypted during materialization.
    /// </summary>
    public class EncryptingJsonConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter"/> to write to.</param><param name="value">The value.</param><param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var stringValue = (string)value;
            if (string.IsNullOrEmpty(stringValue))
            {
                writer.WriteNull();
                return;
            }
            writer.WriteValue(CertificateEncryptionProvider.Current.EncryptValue(stringValue));
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
            var value = reader.Value as string;
            if (string.IsNullOrEmpty(value))
            {
                return reader.Value;
            }
            try
            {
                return CertificateEncryptionProvider.Current.DecryptValue(value);
            }
            catch (Exception) { return String.Empty; }
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>True if this instance can convert the specified object type; otherwise, false.</returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

    }
}
