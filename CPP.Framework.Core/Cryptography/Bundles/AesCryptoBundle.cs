using System.Security.Cryptography;
using System.Text;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Services;

namespace CPP.Framework.Cryptography.Bundles
{
    /// <summary>
    /// Defines a <see cref="CryptoBundle"/> using AES-256 symmetric encryption for the data, and
    /// RSA for the key exchange based on a certificate value.
    /// </summary>
    [AutoRegisterService]
    public sealed class AesCryptoBundle : SymmetricCryptoBundle
    {
        /// <summary>
        /// The unique version number of the current bundle.
        /// </summary>
        public const ushort BundleVersion = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="AesCryptoBundle"/> class.
        /// </summary>
        [ServiceLocatorConstructor]
        internal AesCryptoBundle() : base(BundleVersion, null) { }

        /// <summary>
        /// Gets the current instance of the bundle for the application.
        /// </summary>
        internal static AesCryptoBundle Instance => CodeServiceProvider.GetService<AesCryptoBundle>();

        /// <summary>
        /// Creates the <see cref="SymmetricAlgorithm"/> to use for encrypting and decrypting data.
        /// </summary>
        /// <returns>A <see cref="SymmetricAlgorithm"/> object.</returns>
        protected override SymmetricAlgorithm CreateEncryptionAlgorithm()
        {
            //// WARNING : Never change the value of KeySize or BlockSize, as this can break the
            //// decryption of messages that may be out in the wild. If there is a need to use a
            //// different cipher strength, then you will need to define a new CryptoBundle with
            //// a unique (but separate) version number.

            var algorithm = new AesCryptoServiceProvider
            {
                KeySize = 256,
                BlockSize = 128,
            };
            return algorithm;
        }
    }
}
