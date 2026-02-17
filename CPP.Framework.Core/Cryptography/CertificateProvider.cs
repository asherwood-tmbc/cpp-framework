using System.Security.Cryptography.X509Certificates;
using CPP.Framework.DependencyInjection;

namespace CPP.Framework.Cryptography
{
    /// <summary>
    /// Base provider class used to locate encryption certificates stored on the local machine.
    /// </summary>
    public abstract class CertificateProvider : SingletonServiceBase, ICertificateProvider
    {
        /// <summary>
        /// The reference to the shared instance of the service for the application.
        /// </summary>
        private static readonly ServiceInstance<CertificateProvider> _ServiceInstance = new ServiceInstance<CertificateProvider>(CreateServiceInstance);

        /// <summary>
        /// Gets the reference to the shared instance of the service for the application.
        /// </summary>
        public static CertificateProvider Current => _ServiceInstance.GetInstance();

        /// <summary>
        /// Creates a new instance of the service.
        /// </summary>
        /// <returns>
        /// A <see cref="CertificateProvider"/> value.
        /// </returns>
        private static CertificateProvider CreateServiceInstance()
        {
            if (!ServiceLocator.TryGetInstance(out CertificateProvider provider))
            {
                provider = new MachineCertificateProvider();
            }
            return provider;
        }

        /// <summary>
        /// Attempts to locate an X509 certificate based on the thumbprint value.
        /// </summary>
        /// <param name="certificateThumbprint">The thumbprint for which to search.</param>
        /// <returns>An <see cref="X509Certificate2"/> instance.</returns>
        public abstract X509Certificate2 GetCertificate(string certificateThumbprint);

        /// <summary>
        /// Attempts to locate an X509 certificate from a specific store and location base on the
        /// thumbprint value.
        /// </summary>
        /// <param name="name">The name of the certificate store to search.</param>
        /// <param name="location">The location within the store to search.</param>
        /// <param name="certificateThumbprint">The thumbprint for which to search.</param>
        /// <returns>An <see cref="X509Certificate2"/> instance.</returns>
        protected static X509Certificate2 GetCertificateFromCertificateStore(StoreName name, StoreLocation location, string certificateThumbprint)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => certificateThumbprint);

            var store = new X509Store(name, location);
            store.Open(OpenFlags.ReadOnly);

            try
            {
                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, certificateThumbprint.Trim(), false);
                if (certificates.Count == 0)
                {
                    throw new CertificateNotFoundException(name, location, certificateThumbprint);
                }
                return certificates[0];
            }
            finally
            {
                store.Close();
            }
        }
    }
}
