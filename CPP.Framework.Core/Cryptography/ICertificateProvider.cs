using System.Security.Cryptography.X509Certificates;

namespace CPP.Framework.Cryptography
{
    /// <summary>
    /// Abstract interface for the certificate provider service.
    /// </summary>
    public interface ICertificateProvider
    {
        /// <summary>
        /// Attempts to locate an X509 certificate based on the thumbprint value.
        /// </summary>
        /// <param name="certificateThumbprint">The thumbprint for which to search.</param>
        /// <returns>An <see cref="X509Certificate2"/> instance.</returns>
        X509Certificate2 GetCertificate(string certificateThumbprint);
    }
}