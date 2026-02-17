using System;

namespace CPP.Framework.Cryptography
{
    /// <summary>
    /// Provides access encrypt data using an X509 certificate.
    /// </summary>
    /// <remarks>
    /// You can use the following command to generate a self-signed encryption certificate:
    /// <code>
    /// makecert -r -pe -n "CN=CPPEncryption" -b 01/01/2000 -e 01/01/2036 -eku 1.3.6.1.5.5.7.3.1 -ss my -sr CurrentUser -sky exchange -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12
    /// </code>
    /// </remarks>
    [Obsolete("Please use the CryptographyService class instead.")]
    public class CertificateEncryptionProvider : SingletonServiceBase
    {
        /// <summary>
        /// The current reference to the shared service instance for the application.
        /// </summary>
        private static readonly ServiceInstance<CertificateEncryptionProvider> _ServiceInstance = new ServiceInstance<CertificateEncryptionProvider>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateEncryptionProvider"/> class.
        /// </summary>
        protected CertificateEncryptionProvider() { }

        /// <summary>
        /// Gets the current reference to the shared service instance for the application.
        /// </summary>
        public static CertificateEncryptionProvider Current => _ServiceInstance.GetInstance();

        /// <summary>
        /// Decrypts the specified value.
        /// </summary>
        /// <param name="value">The encrypted value.</param>
        /// <returns>The decrypted value.</returns>
        [Obsolete("Please use CryptographyService.Decrypt() instead.")]
        public string DecryptValue(string value) => CryptographyService.Decrypt(value);

        /// <summary>
        /// Encrypts the specified value.
        /// </summary>
        /// <param name="value">The value to encrypt.</param>
        /// <returns>The encrypted value.</returns>
        [Obsolete("Please use CryptographyService.Encrypt() instead.")]
        public string EncryptValue(string value) => CryptographyService.Encrypt(value);
    }
}
