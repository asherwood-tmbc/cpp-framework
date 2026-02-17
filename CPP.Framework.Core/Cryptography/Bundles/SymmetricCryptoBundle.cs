using System;
using System.Security.Cryptography;
using System.Text;

namespace CPP.Framework.Cryptography.Bundles
{
    /// <summary>
    /// Abstract base class for bundles that encrypt data using a symmetric algorithm (e.g. AES).
    /// </summary>
    public abstract class SymmetricCryptoBundle : CryptoBundle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SymmetricCryptoBundle"/> class.
        /// </summary>
        /// <param name="version">The version number of the bundle.</param>
        /// <param name="encoding">
        /// The encoding used to interpret the bytes of the input strings. If this value is null,
        /// then the default of <see cref="Encoding.UTF8"/> will be used instead.
        /// </param>
        internal SymmetricCryptoBundle(ushort version, Encoding encoding) : base(version, encoding) { }

        /// <summary>
        /// Creates the <see cref="SymmetricAlgorithm"/> to use for encrypting and decrypting data.
        /// </summary>
        /// <returns>A <see cref="SymmetricAlgorithm"/> object.</returns>
        protected abstract SymmetricAlgorithm CreateEncryptionAlgorithm();

        /// <summary>
        /// Creates the encrypted key exchange data from the specified input data.
        /// </summary>
        /// <param name="source">The secret information to be passed in the key exchange.</param>
        /// <returns>A <see cref="byte"/> array that contains the key exchange data.</returns>
        protected internal override byte[] CreateKeyExchange(byte[] source) => LegacyBundle.Instance.CreateKeyExchange(source);

        /// <summary>
        /// Extracts the secret information from the encrypted key exchange data.
        /// </summary>
        /// <param name="source">
        /// The key exchange data within which the secret information is hidden.
        /// </param>
        /// <returns>A <see langword="byte"/> array that contains the secret information.</returns>
        protected internal override byte[] DecryptKeyExchange(byte[] source) => LegacyBundle.Instance.DecryptKeyExchange(source);

        /// <summary>
        /// Decrypts the payload of an encoded message string.
        /// </summary>
        /// <param name="payload">The payload buffer than contains the data to decrypt.</param>
        /// <param name="startAt">
        /// The zero-base position in <paramref name="payload"/> where decryption should start.
        /// </param>
        /// <returns>A <see langword="string"/> that contains the decrypted value.</returns>
        protected internal override string DecryptValue(byte[] payload, int startAt)
        {
            var message = default(string);
            if (payload != null)
            {
                // extract the version number and create the symmetric encryption algoritm.
                var sa = this.CreateEncryptionAlgorithm();

                // extract the key and iv lengths from the payload first.
                var xchlen = BitConverter.ToInt32(payload, startAt);
                startAt += sizeof(int);
                var veclen = BitConverter.ToInt32(payload, startAt);
                startAt += sizeof(int);

                // extract the encryption key.
                var xch = new byte[xchlen];
                Buffer.BlockCopy(payload, startAt, xch, 0, xch.Length);
                startAt += xch.Length;

                // extract the initialization vector.
                var vec = new byte[veclen];
                Buffer.BlockCopy(payload, startAt, vec, 0, vec.Length);
                startAt += vec.Length;

                // finally, extract the encrypted data and decrypt it.
                var key = this.DecryptKeyExchange(xch);
                vec = this.DecryptKeyExchange(vec);
                var en = sa.CreateDecryptor(key, vec);
                var buffer = en.TransformFinalBlock(payload, startAt, (payload.Length - startAt));
                message = this.Encoding.GetString(buffer);
            }
            return message;
        }

        /// <summary>
        /// Encrypts an input string into an encoded message value.
        /// </summary>
        /// <param name="source">The source value to encrypt.</param>
        /// <returns>A <see langword="string"/> that contains the encoded message.</returns>
        protected internal override string EncryptValue(byte[] source)
        {
            var message = default(string);
            if (source != null)
            {
                // get the bytes of the message and encrypt it using a symmetric algorithm.
                var sa = this.CreateEncryptionAlgorithm();
                var en = sa.CreateEncryptor();
                var encrypted = en.TransformFinalBlock(source, 0, source.Length);
                var vec = this.CreateKeyExchange(sa.IV);
                var veclen = BitConverter.GetBytes(vec.Length);

                // perform the key exchange using the public encryption key.
                var xch = this.CreateKeyExchange(sa.Key);
                var xchlen = BitConverter.GetBytes(xch.Length);

                // calculate the required capacity of the payload, and then create the buffer.
                var vers = BitConverter.GetBytes(this.Version);
                var capacity = vers.Length;
                capacity += (xch.Length + xchlen.Length);
                capacity += (vec.Length + veclen.Length);
                capacity += (encrypted.Length);

                var payload = new byte[capacity];
                var startAt = 0;

                // now pack everything into a single payload and export it as a Base64 string.
                var blocks = new[] { vers, xchlen, veclen, xch, vec, encrypted };
                foreach (var src in blocks)
                {
                    Buffer.BlockCopy(src, 0, payload, startAt, src.Length);
                    startAt += src.Length;
                }
                message = $"{CryptoBundleTokenChar}{Convert.ToBase64String(payload)}";
            }
            return message;
        }
    }
}
