using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using CPP.Framework.Configuration;

namespace CPP.Framework.Cryptography
{
    /// <summary>
    /// Provider used to locate certificates in the current working folder.
    /// </summary>
    public class FileCertificateProvider : CertificateProvider
    {
        /// <summary>
        /// Attempts to locate an X509 certificate based on the thumbprint value.
        /// </summary>
        /// <param name="certificateThumbprint">The thumbprint for which to search.</param>
        /// <returns>An <see cref="X509Certificate2"/> instance.</returns>
        public override X509Certificate2 GetCertificate(string certificateThumbprint)
        {
            return new X509Certificate2(
                Path.Combine(Environment.CurrentDirectory, "CPPEncryption.pfx"),
                ConfigSettingProvider.Current.GetSetting("IssuerCertificatePassword"),
                X509KeyStorageFlags.MachineKeySet);
        }
    }
}
