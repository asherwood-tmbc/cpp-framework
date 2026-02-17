using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using ProtoBuf;

namespace CPP.Framework.Serialization
{
    /// <summary>
    /// Allows serializing data to a binary stream using a protocol buffer.
    /// </summary>
    public class ProtocolSerializer : SingletonServiceBase
    {
        private static readonly ServiceInstance<ProtocolSerializer> _ServiceInstance = new ServiceInstance<ProtocolSerializer>();

        /// <summary>
        /// Gets the default <see cref="ProtocolSerializer"/> implementation for the application.
        /// </summary>
        public static ProtocolSerializer Default { get { return _ServiceInstance.GetInstance(); } }

        /// <summary>
        /// Deflates (serializes) an object and writes it to a binary stream.
        /// </summary>
        /// <typeparam name="TValue">The type of the oject.</typeparam>
        /// <param name="value">The value to deflate.</param>
        /// <returns>A <see cref="byte"/> array that represents the contents of the binary stream.</returns>
        public virtual byte[] Deflate<TValue>(TValue value)
        {
            using (var stream = new MemoryStream())
            {
                return this.Deflate(value, stream);
            }
        }

        /// <summary>
        /// Deflates (serializes) an object and writes it to a binary stream.
        /// </summary>
        /// <typeparam name="TValue">The type of the oject.</typeparam>
        /// <param name="value">The value to deflate.</param>
        /// <param name="stream">A <see cref="Stream"/> object that represents the binary stream.</param>
        /// <returns>A <see cref="byte"/> array that represents the contents written to the stream.</returns>
        public virtual byte[] Deflate<TValue>(TValue value, Stream stream)
        {
            byte[] contents = null;
            if (!ReferenceEquals(null, value))
            {
                using (var buffer = new MemoryStream())
                {
                    Serializer.Serialize(buffer, value);
                    contents = buffer.ToArray();
                }
            }
            else contents = new byte[0];

            stream.Write(contents, 0, contents.Length);
            return contents;
        }

        /// <summary>
        /// Inflates (deserializes) an object from a binary stream.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="value">A byte array that represents the contents of the binary stream.</param>
        /// <returns>A <typeparamref name="TValue"/> object.</returns>
        public virtual TValue Inflate<TValue>(byte[] value)
        {
            using (var stream = new MemoryStream(value ?? new byte[0]))
            {
                return this.Inflate<TValue>(stream);
            }
        }

        /// <summary>
        /// Inflates (deserializes) an object from a binary stream.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="stream">A <see cref="Stream"/> object that represents the binary stream.</param>
        /// <returns>A <typeparamref name="TValue"/> object.</returns>
        public virtual TValue Inflate<TValue>(Stream stream)
        {
            if (stream.Position >= stream.Length)
            {
                return default(TValue);
            }
            return Serializer.Deserialize<TValue>(stream);
        }
    }
}
