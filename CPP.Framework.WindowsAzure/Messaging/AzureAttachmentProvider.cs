using System;
using System.IO;
using System.Linq;
using CPP.Framework.DependencyInjection;
using CPP.Framework.WindowsAzure.Storage;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Default <see cref="MailAttachmentProvider"/> implementation for attachments that are stored
    /// in Azure Storage.
    /// </summary>
    public class AzureAttachmentProvider : MailAttachmentProvider
    {
        private readonly AzureStorageAccount _StorageAccount;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="accountName">The name of the <see cref="AzureStorageAccount"/> to use.</param>
        public AzureAttachmentProvider(string accountName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => accountName);
            _StorageAccount = ServiceLocator.GetInstance<AzureStorageAccount>(accountName);
        }

        /// <summary>
        /// Retrieves the content of an attachment as a stream.
        /// </summary>
        /// <param name="attachment">The attachment for which to get the content.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public override Stream GetContentStream(IMailMessageAttachment attachment)
        {
            ArgumentValidator.ValidateNotNull(() => attachment);
            var containerName = AzureStoragePath.GetContainerName(attachment.Location);
            var blockBlobName = AzureStoragePath.GetBlobFilePath(attachment.Location);
            if (String.IsNullOrWhiteSpace(blockBlobName))
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => attachment, ErrorStrings.InvalidAzureBlobLocation, attachment.Location);
            }
            var blob = _StorageAccount.GetStorageBlockBlob(containerName, blockBlobName);
            return blob.OpenRead();
        }

        /// <summary>
        /// Called by the base class to perform application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        protected override void OnDispose()
        {
            var disposable = (_StorageAccount as IDisposable);
            if (disposable != null)
            {
                disposable.Dispose();
            }
            base.OnDispose();
        }
    }
}
