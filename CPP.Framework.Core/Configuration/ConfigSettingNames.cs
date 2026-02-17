using System;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Defines the name of the settings used by the current library.
    /// </summary>
    internal static class ConfigSettingNames
    {
        /// <summary>
        /// Name = DataEncryptionCertificateThumbprintId
        /// </summary>
        [Obsolete("Please use EncryptionThumbprint instead.")]
        internal const string DataEncryptionThumbprint = "DataEncryptionCertificateThumbprintId";

        /// <summary>
        /// Name = EncryptionCertificate
        /// </summary>
        internal const string EncryptionThumbprint = "EncryptionCertificate";

        /// <summary>
        /// Name = UseLegacyEncryption
        /// </summary>
        internal const string UseLegacyEncryption = "UseLegacyEncryption";
    }
}
