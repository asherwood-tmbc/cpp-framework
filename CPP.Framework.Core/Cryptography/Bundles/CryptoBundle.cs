using System;
using System.Text;

using CPP.Framework.Services;

namespace CPP.Framework.Cryptography.Bundles
{
    /// <summary>
    /// Abstract base class that is used to bundle a collection of algorithms that can be used to
    /// provide encryption services for string values. Please note that neither this class nor any
    /// of the derived classes can be extended outside of the core framework library, which was
    /// done to guarantee stability and compatibility with the <see cref="CryptographyService"/>.
    /// However, these classes have been declared public so that can be consumed directly if the
    /// need arises.
    /// </summary>
    public abstract class CryptoBundle : CodeServiceSingleton
    {
        /// <summary>
        /// A special version value that is reserved for the legacy (i.e. certificate encryption)
        /// crypto bundle.
        /// </summary>
        public const ushort LegacyVersion = ushort.MinValue;

        /// <summary>
        /// A special version number that is reserved for the latest crypto bundle.
        /// </summary>
        public const ushort LatestVersion = ushort.MaxValue;

        /// <summary>
        /// The character token used to indicate that an encrypted Base-64 encoded string is using
        /// a <see cref="CryptoBundle"/>, instead of the legacy certificate encryption.
        /// </summary>
        protected internal const char CryptoBundleTokenChar = '#';

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoBundle"/> class.
        /// </summary>
        /// <param name="version">The version number of the bundle.</param>
        /// <param name="encoding">
        /// The encoding used to interpret the bytes of the input strings. If this value is null,
        /// then the default of <see cref="System.Text.Encoding.UTF8"/> will be used instead.
        /// </param>
        internal CryptoBundle(ushort version, Encoding encoding)
        {
            if (version == LatestVersion)
            {
                var message = string.Format(ErrorStrings.ReservedCryptoBundleVersion, version, nameof(LatestVersion));
                throw new ArgumentException(message, nameof(version));
            }
            if ((version == LegacyVersion) && (this.GetType() != typeof(LegacyBundle)))
            {
                var message = string.Format(ErrorStrings.ReservedCryptoBundleVersion, version, nameof(LegacyVersion));
                throw new ArgumentException(message, nameof(version));
            }
            this.Version = version;
            this.Encoding = (encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// Gets the <see cref="Encoding"/> used to interpret the bytes of the messages.
        /// </summary>
        internal Encoding Encoding { get; }

        /// <summary>
        /// Gets the version number of the current bundle.
        /// </summary>
        internal ushort Version { get; }

        /// <summary>
        /// Determines whether or not message data can be decrypted by the current bundle,
        /// </summary>
        /// <param name="version">The version number of the bundle that created the message.</param>
        /// <param name="payload">A <see langword="byte"/> array that contains the data to decrypt.</param>
        /// <returns><c>True</c> if the bundle can decrypt the data; otherwise, <c>false</c>.</returns>
        protected bool CanDecrypt(ushort version, byte[] payload)
        {
            // by default, any bundle can be used to process an empty payload (it will just produce
            // a null string on decryption).
            return ((version == this.Version) || (payload == null));
        }

        /// <summary>
        /// Determines whether or not message data can be encrypted by the current bundle,
        /// </summary>
        /// <param name="input">The input string being encrypted.</param>
        /// <param name="source">
        /// An output parameter that receives the bytes for the characters in
        /// <paramref name="input"/> based on the bundle's character encoding.
        /// </param>
        /// <returns><c>True</c> if the bundle can encrypt the data; otherwise, <c>false</c>.</returns>
        protected bool CanEncrypt(string input, out byte[] source)
        {
            var encoding = default(Encoding);
            source = default(byte[]);
            return this.CanEncrypt(input, ref source, ref encoding);
        }

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
        internal virtual bool CanEncrypt(string input, ref byte[] source, ref Encoding encoding)
        {
            if ((source == null) || (!this.Encoding.Equals(encoding)))
            {
                source = ((input == null) ? default(byte[]) : this.Encoding.GetBytes(input));
                encoding = this.Encoding;
            }
            return true; // default is to encrypt anything, and only override to check additional requirements
        }

        /// <summary>
        /// Creates the encrypted key exchange data from the specified input data.
        /// </summary>
        /// <param name="source">The secret information to be passed in the key exchange.</param>
        /// <returns>A <see cref="byte"/> array that contains the key exchange data.</returns>
        protected internal abstract byte[] CreateKeyExchange(byte[] source);

        /// <summary>
        /// Extracts the secret information from the encrypted key exchange data.
        /// </summary>
        /// <param name="source">
        /// The key exchange data within which the secret information is hidden.
        /// </param>
        /// <returns>A <see langword="byte"/> array that contains the secret information.</returns>
        protected internal abstract byte[] DecryptKeyExchange(byte[] source);

        /// <summary>
        /// Extracts the encrypted payload from an encoded message string.
        /// </summary>
        /// <param name="message">The message string to decode.</param>
        /// <param name="startAt">
        /// An output parameter that receives the position in the returned payload buffer where the
        /// decryption process can start.
        /// </param>
        /// <param name="version">
        /// An output parameter that receives the version of the returned payload.
        /// </param>
        /// <returns>A <see cref="byte"/> array that contains the encrypted payload.</returns>
        internal static byte[] DecodeMessage(string message, out int startAt, out ushort version)
        {
            // initialize all the output values first to get that out of the way.
            startAt = version = 0;

            // determine if we need to validate the version, and remove the crypto bundle token
            // character if needed (since it will cause the decoding algorithm to fail in all cases).
            var hasVersion = false;
            if (CryptoBundle.IsCryptoBundleMessage(message))
            {
                hasVersion = true;
                message = message?.Substring(1);
            }

            // decode the message string, and extract the version number, if available.
            var payload = ((message == null) ? default(byte[]) : Convert.FromBase64String(message));
            if ((payload != null) && (hasVersion))
            {
                version = BitConverter.ToUInt16(payload, startAt);
                startAt += sizeof(ushort);
            }
            else version = LegacyBundle.BundleVersion;

            // return the extracted payload to the caller.
            return payload;
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
        protected internal abstract string DecryptValue(byte[] payload, int startAt);

        /// <summary>
        /// Decrypts the data embedded in an encoded message value.
        /// </summary>
        /// <param name="message">An encoded message that contains the data to decrypt.</param>
        /// <returns>A <see langword="string"/> that contains the decrypted data.</returns>
        public string DecryptValue(string message)
        {
            var payload = DecodeMessage(message, out var startAt, out var version);
            var bundle = this;  // assume that we are the ones who will be decrypting
            if (!this.CanDecrypt(version, payload))
            {
                if ((this.Version == version) || (!CryptographyService.Instance.TryGetCryptoBundle(version, out bundle)))
                {
                    var error = string.Format(ErrorStrings.InvalidCryptoMessagePayload, this.Version);
                    throw new ArgumentException(error, nameof(message));
                }
            }
            return bundle.DecryptValue(payload, startAt);
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
        protected internal abstract string EncryptValue(byte[] source);

        /// <summary>
        /// Encrypts an input string into an encoded message value.
        /// </summary>
        /// <param name="input">The input string to encrypt.</param>
        /// <returns>A <see langword="string"/> that contains the encoded message.</returns>
        public string EncryptValue(string input)
        {
            if (!this.TryDecryptValue(input, out var message))
            {
                var error = string.Format(ErrorStrings.InvalidCryptoMessagePayload, this.Version);
                throw new ArgumentException(error, nameof(input));
            }
            return message;
        }

        /// <summary>
        /// Determines whether or not an encrypted Base-64 encoded message was generated by a
        /// <see cref="CryptoBundle"/> object that was not <see cref="LegacyBundle"/>.
        /// </summary>
        /// <param name="message">The input message to validate.</param>
        /// <returns>
        /// <c>True</c> if <paramref name="message"/> was generated from a <see cref="CryptoBundle"/>;
        /// otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCryptoBundleMessage(string message)
        {
            return (((message?.Length ?? 0) >= 1) && (message?[0] == CryptoBundleTokenChar));
        }

        /// <summary>
        /// Attempts to decrypt the data embedded in an encoded message value.
        /// </summary>
        /// <param name="message">An encoded message that contains the data to decrypt.</param>
        /// <param name="value">
        /// An output parameters that receives the decrypted data on success.
        /// </param>
        /// <returns>
        /// <c>True</c> if the data was decrypted successfully; otherwise, <c>false</c> if the data
        /// could not be decrypted (usually because it was generated by a different bundle).
        /// </returns>
        public bool TryDecryptValue(string message, out string value)
        {
            // decode the message contents and delegate to the overload so that we can delegate
            // to another bundle if needed.
            var payload = DecodeMessage(message, out var startAt, out var version);
            return this.TryDecryptValue(version, payload, startAt, out value);
        }

        /// <summary>
        /// Attempts to decrypt the data embedded in an encoded message value.
        /// </summary>
        /// <param name="version">The version number of the bundle that created the message.</param>
        /// <param name="payload">The payload buffer than contains the data to decrypt.</param>
        /// <param name="startAt">
        /// The zero-base position in <paramref name="payload"/> where decryption should start.
        /// </param>
        /// <param name="value">
        /// An output parameters that receives the decrypted data on success.
        /// </param>
        /// <returns>
        /// <c>True</c> if the data was decrypted successfully; otherwise, <c>false</c> if the data
        /// could not be decrypted (usually because it was generated by a different bundle).
        /// </returns>
        protected internal virtual bool TryDecryptValue(ushort version, byte[] payload, int startAt, out string value)
        {
            value = default(string);

            // determine if we can decrypt the data, and if so, do it.
            if (this.CanDecrypt(version, payload))
            {
                value = this.DecryptValue(payload, startAt);
                return true;
            }

            // otherwise, we may have been handed a message that was generated by a different
            // bundle, so make a best case attempt to forward the call on behalf of the caller.
            if (version != this.Version)
            {
                if (CryptographyService.Instance.TryGetCryptoBundle(version, out var bundle))
                {
                    return bundle.TryDecryptValue(version, payload, startAt, out value);
                }
            }
            return false;   // at this point, we just plain flat out don't know how to decrypt the data.
        }

        /// <summary>
        /// Attempts to encrypt an input string into an encoded message value.
        /// </summary>
        /// <param name="input">The input string to encrypt.</param>
        /// <param name="message">
        /// An output parameter that receives an encoded message with the encrypted data embedded
        /// in it on success.
        /// </param>
        /// <returns>
        /// <c>True</c> if the encryption succeeded; otherwise, <c>false</c> if the current bundle
        /// cannot process the data (usually due to data size or restrictions from the algorithm).
        /// </returns>
        public bool TryEncryptValue(string input, out string message)
        {
            message = default(string);
            if (this.CanEncrypt(input, out var source))
            {
                message = this.EncryptValue(source);
                return true;
            }
            return false;
        }
    }
}
