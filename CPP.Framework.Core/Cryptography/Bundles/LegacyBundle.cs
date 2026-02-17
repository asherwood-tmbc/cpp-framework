using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using CPP.Framework.Configuration;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Services;

namespace CPP.Framework.Cryptography.Bundles
{
    /// <summary>
    /// Defines a cryptograph bundle that uses the legacy encryption algorithm, which encrypts the
    /// data directly using the RSA public and private keys of the encryption certificate.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         New code should not use this algorithm for the most part because it can only handle
    ///         strings of relatively short lengths, and is dependent on the size of the public key,
    ///         which can be calculated using the following formula:
    ///     </para>
    ///     <code>
    ///         max = ((KeySize - 384) / 8) + 37
    ///     </code>
    ///     <para>
    ///         This means that with a standard key size of 1024 bits, the maximum size of a buffer
    ///         that can be encrypted before an exception is thrown is 117 bytes (245 at 2048, etc).
    ///         In addition, the maximum character length can be even lower than this number, since
    ///         each character can take up anywhere from 1 to 4 bytes, due to the UTF-8 encoding.
    ///     </para>
    /// </remarks>
    [AutoRegisterService]
    public sealed class LegacyBundle : AsymmetricCryptoBundle
    {
        /// <summary>
        /// The unique version number of the current bundle.
        /// </summary>
        public const ushort BundleVersion = CryptoBundle.LegacyVersion;

        private X509Certificate2 _certificate;
        private RSACryptoServiceProvider _decryptKey, _encryptKey;
        private bool _enabled;
        private int _maxSourceBufferSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="LegacyBundle"/> class.
        /// </summary>
        [ServiceLocatorConstructor]
        internal LegacyBundle() : base(BundleVersion, null) { }

        /// <summary>
        /// Gets the current instance of the bundle for the application.
        /// </summary>
        internal static LegacyBundle Instance => CodeServiceProvider.GetService<LegacyBundle>();

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
        internal override bool CanEncrypt(string input, ref byte[] source, ref Encoding encoding)
        {
            if (_enabled)
            {
                if ((source == null) || (!this.Encoding.Equals(encoding)))
                {
                    source = ((input == null) ? default(byte[]) : this.Encoding.GetBytes(input));
                    encoding = this.Encoding;
                }
                return ((source?.Length ?? 0) <= _maxSourceBufferSize);
            }
            return false;
        }

        /// <summary>
        /// Creates the algorithm for decrypting values using the private key.
        /// </summary>
        /// <returns>An <see cref="AsymmetricAlgorithm"/> object.</returns>
        protected override AsymmetricAlgorithm CreateDecryptionAlgorithm() => _decryptKey;

        /// <summary>
        /// Creates the algorithm for encrypting values using the public key.
        /// </summary>
        /// <returns>An <see cref="AsymmetricAlgorithm"/> object.</returns>
        protected override AsymmetricAlgorithm CreateEncryptionAlgorithm() => _encryptKey;

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
            if ((payload != null) && (payload.Length != 0))
            {
                var rsa = _decryptKey;
                var buffer = rsa.Decrypt(payload, true);
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
                var rsa = _encryptKey;
                var buffer = rsa.Encrypt(source, true);
                message = Convert.ToBase64String(buffer);
            }
            return message;
        }

        /// <summary>
        /// Called by the application framework to perform any initialization tasks when the 
        /// instance is being created.
        /// </summary>
        protected internal override void StartupInstance()
        {
            // get the encryption and decryption keys from the cert based on the thumbprint, and
            // determine whether or not our bundle is available for use (or has been disabled).
            var thumbprint = ConfigSettingProvider.Current.GetEncryptionCertificateThumprintSetting();
            _certificate = CertificateProvider.Current.GetCertificate(thumbprint);
            _enabled = ConfigSettingProvider.Current.GetUseLegacyEncryptionSetting();

            // cache these values for use later to speed things up.
            _decryptKey = (RSACryptoServiceProvider)_certificate.PrivateKey;
            _encryptKey = (RSACryptoServiceProvider)_certificate.PublicKey.Key;

            // calculate the maximum allowed input buffer size bases on the size of the of the key.
            _maxSourceBufferSize = ((_encryptKey.KeySize - 384) / 8) + 6;

            // always call the base class.
            base.StartupInstance();
        }
    }
}
