using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Represents a block blob in Windows Azure Cloud Storage.
    /// </summary>
    public class AzureStorageBlockBlob : AzureStorageBlob
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="account">The <see cref="AzureStorageAccount"/> where the object is stored.</param>
        /// <param name="objectName">The name of the storage object.</param>
        public AzureStorageBlockBlob(AzureStorageAccount account, string objectName) : base(account, objectName) { }

        /// <summary>
        /// Gets a reference to the blob associated with the storage object.
        /// </summary>
        /// <returns>A <see cref="ICloudBlob"/> reference.</returns>
        protected override ICloudBlob GetCloudBlob()
        {
            return this.GetCloudBlobContainer().GetBlockBlobReference(this.BlobName);
        }

        /// <summary>
        /// Opens a stream for writing to the blob.
        /// </summary>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public virtual Stream OpenWrite()
        {
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            var access = this.GetAccessCondition();
            return ((CloudBlockBlob)this.GetCloudBlob()).OpenWrite(access, options);
        }

        /// <summary>
        /// Initiates an asynchronous operation to open a stream for writing to the blob.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> object of type <see cref="Stream"/> that represents the asynchronous operation.</returns>
        public virtual async Task<Stream> OpenWriteAsync()
        {
            return await this.OpenWriteAsync(CancellationToken.None);
        }

        /// <summary>
        /// Initiates an asynchronous operation to open a stream for writing to the blob.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for a task to complete.</param>
        /// <returns>A <see cref="Task{TResult}"/> object of type <see cref="Stream"/> that represents the asynchronous operation.</returns>
        public virtual async Task<Stream> OpenWriteAsync(CancellationToken cancellationToken)
        {
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            var access = this.GetAccessCondition();
            return await ((CloudBlockBlob)this.GetCloudBlob()).OpenWriteAsync(access, options, null, cancellationToken);
        }

        /// <summary>
        /// Sets the contents of the blob to a string value.
        /// </summary>
        /// <param name="source">A string value that contains the new contents.</param>
        public override void UpdateFromString(string source)
        {
            var access = this.GetAccessCondition();
            ((CloudBlockBlob)this.CloudBlob).UploadText(source ?? String.Empty, accessCondition: access);
        }

        /// <summary>
        /// Sets the contents of the blob to a string value.
        /// </summary>
        /// <param name="source">A string value that contains the new contents.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override async Task UpdateFromStringAsync(string source, CancellationToken cancellationToken)
        {
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            var access = this.GetAccessCondition();
            await ((CloudBlockBlob)this.CloudBlob).UploadTextAsync(source ?? String.Empty, Encoding.UTF8, access, options, null, cancellationToken);
        }
    }
}
