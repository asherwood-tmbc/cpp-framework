using System.Security.Cryptography.X509Certificates;

namespace CPP.Framework.Cryptography
{
    /// <summary>
    /// Provider used to locate certificates in the current user certificate store.
    /// </summary>
    public class CurrentUserCertificateProvider : CertificateProvider
    {
        /// <summary>
        /// Attempts to locate an X509 certificate based on the thumbprint value.
        /// </summary>
        /// <param name="certificateThumbprint">The thumbprint for which to search.</param>
        /// <returns>An <see cref="X509Certificate2"/> instance.</returns>
        public override X509Certificate2 GetCertificate(string certificateThumbprint)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => certificateThumbprint);
            return GetCertificateFromCertificateStore(StoreName.My, StoreLocation.CurrentUser, certificateThumbprint.Trim());
        }
    }
}
