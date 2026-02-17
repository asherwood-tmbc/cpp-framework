using System.Security.Claims;
using System.Security.Principal;
using CPP.Framework.DependencyInjection;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Extension methods for the 
    /// </summary>
    public static class ConfigSettingProviderExtensions
    {
        /// <summary>
        /// Gets the value of the setting that contains the thumbprint of the certificate used for
        /// encryption of data and messages (i.e. database encryption).
        /// </summary>
        /// <param name="provider">
        /// The <see cref="ConfigSettingProvider"/> that contains the settings.
        /// </param>
        /// <returns>A <see langword="string"/> value that contains the thumbprint.</returns>
        public static string GetEncryptionCertificateThumprintSetting(this ConfigSettingProvider provider)
        {
            if (provider.TryGetSetting(ConfigSettingNames.DataEncryptionThumbprint, out var value))
            {
                return value;
            }
            return provider.GetSetting(ConfigSettingNames.EncryptionThumbprint);
        }

        /// <summary>
        /// Gets the value of the setting that indicates whether or not to use the legacy algorithm
        /// to encrypt data (but only if the message length is small enough).
        /// </summary>
        /// <param name="provider">
        /// The <see cref="ConfigSettingProvider"/> that contains the settings.
        /// </param>
        /// <returns>A <see langword="bool"/> value.</returns>
        public static bool GetUseLegacyEncryptionSetting(this ConfigSettingProvider provider)
        {
            return provider.GetSetting(ConfigSettingNames.UseLegacyEncryption, bool.Parse, "true");
        }

        /// <summary>
        /// Registers default implementations of the <see cref="IPrincipal"/> and <see cref="ClaimsPrincipal"/>
        /// </summary>
        /// <param name="provider">
        /// The <see cref="ConfigSettingProvider"/> that contains the settings.
        /// </param>
        /// <returns>The calling <see cref="ConfigSettingProvider"/> (for chaining).</returns>
        public static ConfigSettingProvider UseDefaultClaimsPrincipal(this ConfigSettingProvider provider)
        {
            ServiceLocator.Register<IPrincipal>(name => ClaimsPrincipal.Current);
            ServiceLocator.Register(name => ClaimsPrincipal.Current);
            return provider;
        }
    }
}
