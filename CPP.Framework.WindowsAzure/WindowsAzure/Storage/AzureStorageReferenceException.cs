using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

using JetBrains.Annotations;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Thrown when an operation against an Azure Storage object couldn't be completed successfully
    /// due to one or more external references (e.g like deleting a blob).
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AzureStorageReferenceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageReferenceException"/> class.
        /// </summary>
        public AzureStorageReferenceException() : base(ErrorStrings.AzureStorageObjectHasExternalReferences) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageReferenceException"/> class.
        /// </summary>
        /// <param name="innerException">
        /// The <see cref="Exception"/> that triggered the current exception.
        /// </param>
        public AzureStorageReferenceException(Exception innerException) : base(ErrorStrings.AzureStorageObjectHasExternalReferences, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageReferenceException"/> class.
        /// </summary>
        /// <param name="message">The error message for the exception.</param>
        protected AzureStorageReferenceException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageReferenceException"/> class.
        /// </summary>
        /// <param name="message">The error message for the exception.</param>
        /// <param name="innerException">
        /// The <see cref="Exception"/> that triggered the current exception.
        /// </param>
        protected AzureStorageReferenceException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageReferenceException"/> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        [UsedImplicitly]
        protected AzureStorageReferenceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
