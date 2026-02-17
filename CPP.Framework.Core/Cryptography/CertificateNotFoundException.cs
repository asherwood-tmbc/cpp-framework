using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace CPP.Framework.Cryptography
{
    /// <summary>
    /// Exception thrown to indicate that there was an error locating a certificate.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CertificateNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateNotFoundException"/> class. 
        /// </summary>
        /// <param name="name">
        /// The <see cref="System.Security.Cryptography.X509Certificates.StoreName"/> used for the search.
        /// </param>
        /// <param name="location">
        /// The <see cref="System.Security.Cryptography.X509Certificates.StoreLocation"/> used for the search.
        /// </param>
        /// <param name="certificateThumbprint">
        /// The thumbprint value used for the search.
        /// </param>
        public CertificateNotFoundException(StoreName name, StoreLocation location, string certificateThumbprint) : base(FormatMessage(name, location, certificateThumbprint))
        {
            this.StoreName = name;
            this.StoreLocation = location;
            this.CertificateThumbprint = certificateThumbprint;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateNotFoundException"/> class. 
        /// </summary>
        /// <param name="name">The <see cref="System.Security.Cryptography.X509Certificates.StoreName"/> used for the search.</param>
        /// <param name="location">The <see cref="System.Security.Cryptography.X509Certificates.StoreLocation"/> used for the search.</param>
        /// <param name="certificateThumbprint">The thumbprint value used for the search.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        public CertificateNotFoundException(StoreName name, StoreLocation location, string certificateThumbprint, Exception innerException)
            : base(FormatMessage(name, location, certificateThumbprint), innerException)
        {
            this.StoreName = name;
            this.StoreLocation = location;
            this.CertificateThumbprint = certificateThumbprint;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CertificateNotFoundException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected CertificateNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            info.LoadProperty(this, (x => x.StoreName));
            info.LoadProperty(this, (x => x.StoreLocation));
            info.LoadProperty(this, (x => x.CertificateThumbprint));
        }

        /// <summary>
        /// Gets the <see cref="System.Security.Cryptography.X509Certificates.StoreName"/> used for the search.
        /// </summary>
        public StoreName StoreName { get; }

        /// <summary>
        /// Gets the <see cref="System.Security.Cryptography.X509Certificates.StoreLocation"/> used for the search.
        /// </summary>
        public StoreLocation StoreLocation { get; }

        /// <summary>
        /// Gets the thumbprint value used for the search.
        /// </summary>
        public string CertificateThumbprint { get; }

        /// <inheritdoc />
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SaveProperty(this, (x => x.StoreName));
            info.SaveProperty(this, (x => x.StoreLocation));
            info.SaveProperty(this, (x => x.CertificateThumbprint));
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Formats the error message for the exception.
        /// </summary>
        /// <param name="name">
        /// The name of the certificate store.
        /// </param>
        /// <param name="location">
        /// The name of the location within the certificate store.
        /// </param>
        /// <param name="certificateThumbprint">
        /// The thumbprint from the search request.
        /// </param>
        /// <returns>
        /// The formatted error message.
        /// </returns>
        private static string FormatMessage(StoreName name, StoreLocation location, string certificateThumbprint)
        {
            return string.Format(ErrorStrings.UnknownCertificateThumbprint, name, location, certificateThumbprint);
        }
    }
}
