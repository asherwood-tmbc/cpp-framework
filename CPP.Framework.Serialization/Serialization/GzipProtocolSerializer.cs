using ProtoBuf;
using System.IO;
using System.IO.Compression;

namespace CPP.Framework.Serialization
{
    /// <summary>
    /// Allows serializing data to a binary stream that has been compressed using a GZip format.
    /// </summary>
    public class GzipProtocolSerializer : ProtocolSerializer
    {
        /// <summary>
        /// Deflates (serializes) an object and writes it to a binary stream.
        /// </summary>
        /// <typeparam name="TValue">The type of the oject.</typeparam>
        /// <param name="value">The value to deflate.</param>
        /// <param name="stream">A <see cref="Stream"/> object that represents the binary stream.</param>
        /// <returns>A <see cref="byte"/> array that represents the contents written to the stream.</returns>
        public override byte[] Deflate<TValue>(TValue value, Stream stream)
        {
            byte[] contents = null;
            if (!ReferenceEquals(null, value))
            {
                using (var buffer = new MemoryStream())
                {
                    using (var gzip = new GZipStream(buffer, CompressionLevel.Optimal, true))
                    {
                        Serializer.Serialize(gzip, value);
                    }
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
        /// <param name="stream">A <see cref="Stream"/> object that represents the binary stream.</param>
        /// <returns>A <typeparamref name="TValue"/> object.</returns>
        public override TValue Inflate<TValue>(Stream stream)
        {
            using (var gzip = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                if (stream.Position >= stream.Length)
                {
                    return default(TValue);
                }
                return Serializer.Deserialize<TValue>(gzip);
            }
        }
    }
}
