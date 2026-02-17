using System.Security.Cryptography;

namespace CPP.Framework.Cryptography
{
    /// <summary>
    /// Model used to provide the encryption keys for a certificate.
    /// </summary>
    internal class EncryptionKeys
    {
        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        internal RSACryptoServiceProvider PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the private key.
        /// </summary>
        internal RSACryptoServiceProvider PrivateKey { get; set; }
    }
}
