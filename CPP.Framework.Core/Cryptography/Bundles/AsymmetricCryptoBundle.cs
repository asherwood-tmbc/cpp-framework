using System;
using System.Security.Cryptography;
using System.Text;

namespace CPP.Framework.Cryptography.Bundles
{
    /// <summary>
    /// Abstract base class for bundles that encrypt data using an asymmetric algorithm (e.g. RSA).
    /// </summary>
    public abstract class AsymmetricCryptoBundle : CryptoBundle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsymmetricCryptoBundle"/> class.
        /// </summary>
        /// <param name="version">The version number of the bundle.</param>
        /// <param name="encoding">
        /// The encoding used to interpret the bytes of the input strings. If this value is null,
        /// then the default of <see cref="Encoding.UTF8"/> will be used instead.
        /// </param>
        internal AsymmetricCryptoBundle(ushort version, Encoding encoding) : base(version, encoding) { }

        /// <summary>
        /// Determines whether or not message data can be encrypted by the current bundle,
        /// </summary>
        /// <param name="input">The input string being encrypted.</param>
        /// <param name="source">
        /// A <see langword="byte"/> array that represents the encoded bytes from the characters in
        /// <paramref name="input"/> on input, and can be null. On output, this parameter is always
        /// updated to match encoded bytes of the character encoding used by the current bundle.
        /// </param>
        /// <param name="encoding">
        /// The <see cref="Encoding"/> used to generate <paramref name="source"/> on input, or null
        /// if <paramref name="source"/> is null. On output, this parameter is always updated with
        /// the encoding associated to the current bundle.
        /// </param>
        /// <returns><c>True</c> if the bundle can encrypt the data; otherwise, <c>false</c>.</returns>
        internal override bool CanEncrypt(string input, ref byte[] source, ref Encoding encoding) => false;

        /// <summary>
        /// Creates the algorithm for decrypting values using the private key.
        /// </summary>
        /// <returns>An <see cref="AsymmetricAlgorithm"/> object.</returns>
        protected abstract AsymmetricAlgorithm CreateDecryptionAlgorithm();

        /// <summary>
        /// Creates the algorithm for encrypting values using the public key.
        /// </summary>
        /// <returns>An <see cref="AsymmetricAlgorithm"/> object.</returns>
        protected abstract AsymmetricAlgorithm CreateEncryptionAlgorithm();

        /// <summary>
        /// Creates the encrypted key exchange data from the specified input data.
        /// </summary>
        /// <param name="source">The secret information to be passed in the key exchange.</param>
        /// <returns>A <see cref="byte"/> array that contains the key exchange data.</returns>
        protected internal override byte[] CreateKeyExchange(byte[] source)
        {
            var key = this.CreateEncryptionAlgorithm();
            var fmt = new RSAOAEPKeyExchangeFormatter(key);
            return fmt.CreateKeyExchange(source);
        }

        /// <summary>
        /// Extracts the secret information from the encrypted key exchange data.
        /// </summary>
        /// <param name="source">
        /// The key exchange data within which the secret information is hidden.
        /// </param>
        /// <returns>A <see langword="byte"/> array that contains the secret information.</returns>
        protected internal override byte[] DecryptKeyExchange(byte[] source)
        {
            var key = this.CreateDecryptionAlgorithm();
            var fmt = new RSAOAEPKeyExchangeDeformatter(key);
            return fmt.DecryptKeyExchange(source);
        }

        /// <summary>
        /// Decrypts the payload of an encoded message string.
        /// </summary>
        /// <param name="payload">The payload buffer than contains the data to decrypt.</param>
        /// <param name="startAt">
        /// The zero-base position in <paramref name="payload"/> where decryption should start.
        /// </param>
        /// <returns>A <see langword="string"/> that contains the decrypted value.</returns>
        /// <exception cref="NotSupportedException">
        /// The encryption algorithm associated to the bundle does not support generic data
        /// encryption.
        /// </exception>
        protected internal override string DecryptValue(byte[] payload, int startAt)
        {
            // by default, asymmetric algorithms (e.g. RSA) can only be used for key exchanges, and
            // not unbounded data encryption due to key size limitations (plus they are much slower
            // than symmetric algorithms anyway).
            throw new NotSupportedException();
        }

        /// <summary>
        /// Encrypts an input string into an encoded message value.
        /// </summary>
        /// <param name="source">The source value to encrypt.</param>
        /// <returns>A <see langword="string"/> that contains the encoded message.</returns>
        /// <exception cref="NotSupportedException">
        /// The encryption algorithm associated to the bundle does not support generic data
        /// encryption.
        /// </exception>
        protected internal override string EncryptValue(byte[] source)
        {
            // by default, asymmetric algorithms (e.g. RSA) can only be used for key exchanges, and
            // not unbounded data encryption due to key size limitations (plus they are much slower
            // than symmetric algorithms anyway).
            throw new NotSupportedException();
        }
    }
}
