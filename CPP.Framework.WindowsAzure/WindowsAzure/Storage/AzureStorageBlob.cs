using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.Data.Entities;
using CPP.Framework.DependencyInjection;
using CPP.Framework.DependencyInjection.Resolvers;

using JetBrains.Annotations;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Abstract base class for all classes that represent a Windows Azure Cloud Storage blob.
    /// </summary>
    public abstract class AzureStorageBlob : AzureStorageObject
    {
        private const string XRefMetadataPrefix = "XREFID_";

        private readonly string _blobName;
        private readonly Lazy<ICloudBlob> _cloubBlob; 
        private readonly string _containerName;
        private readonly Lazy<AzureStorageLeaseManager> _leaseManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageBlob"/> class.
        /// </summary>
        /// <param name="account">The <see cref="AzureStorageAccount"/> where the object is stored.</param>
        /// <param name="objectName">The name of the storage object.</param>
        protected AzureStorageBlob(AzureStorageAccount account, string objectName) : base(account, objectName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => objectName);
            
            _containerName = AzureStoragePath.GetContainerName(objectName);
            _blobName = AzureStoragePath.GetBlobFilePath(objectName);

            if (!AzureStoragePath.IsContainerNameValid(_containerName))
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => objectName, ErrorStrings.InvalidAzureContainerName, _containerName);
            }
            if (string.IsNullOrWhiteSpace(_blobName) || AzureStoragePath.HasInvalidPathChars(_blobName))
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => objectName, ErrorStrings.InvalidAzureBlobLocation, _blobName);
            }

            _leaseManager = new Lazy<AzureStorageLeaseManager>(
                () =>
                    {
                        var resolvers = new ServiceResolver[]
                        {
                            new DependencyResolver<AzureStorageBlob>(this),
                        };
                        return ServiceLocator.GetInstance<AzureStorageLeaseManager>(resolvers);
                    },
                LazyThreadSafetyMode.PublicationOnly);
            _cloubBlob = new Lazy<ICloudBlob>(() => this.GetCloudBlob());
        }

        /// <summary>
        /// Gets the name of the blob in the storage container.
        /// </summary>
        public virtual string BlobName => _blobName;

        /// <summary>
        /// Gets a reference to the blob in cloud storage.
        /// </summary>
        protected internal ICloudBlob CloudBlob => _cloubBlob.Value;

        /// <summary>
        /// Gets the name of the container where the blob is stored.
        /// </summary>
        public virtual string ContainerName => _containerName;

        /// <summary>
        /// Gets the reference to the <see cref="AzureStorageLeaseManager"/> object for the current
        /// blob.
        /// </summary>
        internal AzureStorageLeaseManager LeaseManager => _leaseManager.Value;

        /// <summary>
        /// Acquires an exclusive lease for the current blob from the server where the it is stored.
        /// </summary>
        /// <returns>An <see cref="AzureStorageLease"/> object.</returns>
        /// <exception cref="TimeoutException">The timeout period elapsed before the lease could be acquired.</exception>
        [Obsolete("Please use the AzureStorageLease.Acquire method instead.", true)]
        public AzureStorageLease AcquireLease() { return this.AcquireLease(Timeout.InfiniteTimeSpan); }

        /// <summary>
        /// Acquires an exclusive lease for the current blob from the server where the it is stored.
        /// </summary>
        /// <param name="timeout">The amount of time to wait (in milliseconds) when trying to acquire the lease before throwing an exception.</param>
        /// <returns>An <see cref="AzureStorageLease"/> object.</returns>
        /// <exception cref="TimeoutException">The timeout period elapsed before the lease could be acquired.</exception>
        [UsedImplicitly]
        [Obsolete("Please use the AzureStorageLease.Acquire method instead.", true)]
        public AzureStorageLease AcquireLease(int timeout) { return this.AcquireLease(TimeSpan.FromMilliseconds(timeout)); }

        /// <summary>
        /// Acquires an exclusive lease for the current blob from the server where the it is stored.
        /// </summary>
        /// <param name="timeout">The amount of time to wait when trying to acquire the lease before throwing an exception.</param>
        /// <returns>An <see cref="AzureStorageLease"/> object.</returns>
        /// <exception cref="TimeoutException">The timeout period elapsed before the lease could be acquired.</exception>
        [Obsolete("Please use the AzureStorageLease.Acquire method instead.", true)]
        public AzureStorageLease AcquireLease(TimeSpan timeout) => this.LeaseManager.Acquire(timeout);

        /// <summary>
        /// Attaches a reference to the current blob indicating that it is the dependency of an
        /// external entity.
        /// </summary>
        /// <param name="referenceId">The unique id of the reference to attach.</param>
        /// <param name="type">
        /// An optional string value that provides a display-friendly name that indicates the type
        /// of reference identified by <paramref name="referenceId"/>. If this value is a null or an emtpy
        /// string, then the id is used for the value instead.
        /// </param>
        /// <returns>
        /// An <see langword="int" /> value that contains the count of references to the blob after
        /// adding the reference.
        /// </returns>
        public virtual long AttachExternalReference(Guid referenceId, string type = null)
        {
            using (AzureStorageLease.Acquire(this))
            {
                var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
                var access = this.GetAccessCondition();
                this.CloudBlob.FetchAttributes(access, options);

                var key = this.GenerateExternalReferenceKey(referenceId);
                var val = (string.IsNullOrWhiteSpace(type) ? $"{referenceId:N}" : type);

                if (!this.CloudBlob.Metadata.ContainsKey(key))
                {
                    this.CloudBlob.Metadata[key] = val;
                }
                this.CloudBlob.SetMetadata(access, options);

                return this.GetExternalReferenceCount(false);
            }
        }

        /// <summary>
        /// Attaches an entity object as an external reference to the current blob.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity object.</param>
        /// <param name="idProvider">Extracts an ID from the entity.</param>
        /// <returns>
        /// An <see langword="int" /> value that contains the count of references to the blob after
        /// adding the reference.
        /// </returns>
        [UsedImplicitly]
        public virtual long AttachExternalReference<TEntity>(TEntity entity, Func<TEntity,Guid> idProvider)
            where TEntity : class
        {
            Guid id = idProvider(entity);
            ArgumentValidator.ValidateNotNull(() => entity);
            if (Guid.Empty.Equals(id))
            {
                throw new ArgumentException(ErrorStrings.InvalidAzureBlobEntityReference, nameof(entity));
            }
            return this.AttachExternalReference(id, typeof(TEntity).Name);
        }

        /// <summary>
        /// Creates the blob file in storage if it doesn't already exist.
        /// </summary>
        /// <param name="options">The <see cref="BlobRequestOptions"/> to use for the call, or null to use the default options.</param>
        protected virtual void CreateIfNotExists(BlobRequestOptions options)
        {
            options = (options ?? this.RequestOptions.CreateOptions<BlobRequestOptions>());
            if (!this.CloudBlob.Exists(options))
            {
                this.UpdateFromString(string.Empty);
            }
        }

        /// <summary>
        /// Creates the blob file in storage if it doesn't already exist.
        /// </summary>
        /// <param name="options">The <see cref="BlobRequestOptions"/> to use for the call, or null to use the default options.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        protected virtual async Task CreateIfNotExistsAsync(BlobRequestOptions options, CancellationToken cancellationToken)
        {
            options = (options ?? this.RequestOptions.CreateOptions<BlobRequestOptions>());
            if (!(await this.CloudBlob.ExistsAsync(options, null, cancellationToken)))
            {
                await this.UpdateFromStringAsync(string.Empty, cancellationToken);
            }
        }

        /// <summary>
        /// Creates a new lease for the current object.
        /// </summary>
        /// <param name="duration">A <see cref="TimeSpan"/> object that specifies the duration of the lease.</param>
        /// <returns>A <see cref="String"/> that contains the unique id of the lease.</returns>
        [UsedImplicitly]
        internal virtual string CreateLease(TimeSpan duration)
        {
            if (duration != Timeout.InfiniteTimeSpan)
            {
                if ((duration.TotalSeconds < 15) || (duration.TotalSeconds > 60))
                {
                    throw ArgumentValidator.CreateArgumentExceptionFor(() => duration, ErrorStrings.InvalidAzureLeaseDuration);
                }
            }
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            this.CreateIfNotExists(options);
            return this.CloudBlob.AcquireLease(duration, null, options: options);
        }

        /// <summary>
        /// Deletes the blob from the storage container.
        /// </summary>
        /// <returns>True if the blob exists and was deleted; otherwise, false.</returns>
        public override bool Delete()
        {
            if (!this.Exists()) return false;
            using (AzureStorageLease.Acquire(this))
            {
                if (this.GetExternalReferenceCount() >= 1)
                {
                    // if the object still has any external references attached, then we will need
                    // to block deleting the blob from storage until they have all been removed.
                    throw new AzureStorageReferenceException();
                }
                try
                {
                    var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
                    var access = this.GetAccessCondition();
                    this.CloudBlob.Delete(accessCondition: access, options: options);
                    return true;
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                    {
                        if (ex.RequestInformation.ExtendedErrorInformation == null || ex.RequestInformation.ExtendedErrorInformation.ErrorCode == BlobErrorCodeStrings.BlobNotFound)
                        {
                            return false;
                        }
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Deletes the blob from the storage container.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public override async Task<bool> DeleteAsync(CancellationToken cancellationToken)
        {
            if (!(await this.ExistsAsync(cancellationToken))) return false;
            using (await AzureStorageLease.AcquireAsync(this, cancellationToken))
            {
                if (this.GetExternalReferenceCount() >= 1)
                {
                    // if the object still has any external references attached, then we will need
                    // to block deleting the blob from storage until they have all been removed.
                    throw new AzureStorageReferenceException();
                }
                var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
                var access = this.GetAccessCondition();
                return await this.CloudBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.None, access, options, null, cancellationToken);
            }
        }

        /// <summary>
        /// Attaches a reference to the current blob indicating that it is the dependency of an
        /// external entity.
        /// </summary>
        /// <param name="referenceId">The unique id of the reference to attach.</param>
        /// <param name="type">
        /// An optional string value that provides a display-friendly name that indicates the type
        /// of reference identified by <paramref name="referenceId"/>. If this value is a null or an emtpy
        /// string, then the id is used for the value instead.
        /// </param>
        /// <returns>
        /// An <see langword="int" /> value that contains the count of references to the blob after
        /// adding the reference.
        /// </returns>
        public virtual long DetachExternalReference(Guid referenceId, string type = null)
        {
            using (AzureStorageLease.Acquire(this))
            {
                var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
                var access = this.GetAccessCondition();
                this.CloudBlob.FetchAttributes(access, options);

                var rid = $"{referenceId:N}";
                var key = $"{XRefMetadataPrefix}{rid.ToUpperInvariant()}";
                this.CloudBlob.Metadata.Remove(key);
                this.CloudBlob.SetMetadata(access, options);

                return this.GetExternalReferenceCount(false);
            }
        }

        /// <summary>
        /// Attaches an entity object as an external reference to the current blob.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entity">The entity object.</param>
        /// <param name="idProvider">Extracts an ID from the entity.</param>
        /// <returns>
        /// An <see langword="int" /> value that contains the count of references to the blob after
        /// adding the reference.
        /// </returns>
        [UsedImplicitly]
        public virtual long DetachExternalReference<TEntity>(TEntity entity, Func<TEntity, Guid> idProvider)
            where TEntity : class
        {
            Guid id = idProvider(entity);
            ArgumentValidator.ValidateNotNull(() => entity);
            if (Guid.Empty.Equals(id))
            {
                throw new ArgumentException(ErrorStrings.InvalidAzureBlobEntityReference, nameof(entity));
            }
            return this.DetachExternalReference(id, typeof(TEntity).Name);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if the object is being disposed explicitly; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            this.LeaseManager.Dispose(disposing);
            base.Dispose(disposing);
        }

        /// <summary>
        /// Downloads the contents of the blob to a stream.
        /// </summary>
        /// <param name="target">A <see cref="Stream"/> object that receives the blob contents.</param>
        public virtual void DownloadToStream(Stream target)
        {
            ArgumentValidator.ValidateNotNull(() => target);
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            this.CloudBlob.DownloadToStream(target, options: options);
        }

        /// <summary>
        /// Downloads the contents of the blob to a stream.
        /// </summary>
        /// <param name="target">A <see cref="Stream"/> object that receives the blob contents.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        [UsedImplicitly]
        public virtual async Task DownloadToStreamAsync(Stream target)
        {
            await this.DownloadToStreamAsync(target, CancellationToken.None);
        }

        /// <summary>
        /// Downloads the contents of the blob to a stream.
        /// </summary>
        /// <param name="target">A <see cref="Stream"/> object that receives the blob contents.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task DownloadToStreamAsync(Stream target, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => target);
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            await this.CloudBlob.DownloadToStreamAsync(target, null, options, null, cancellationToken);
        }

        /// <summary>
        /// Checks whether or not the blob exists in cloud storage.
        /// </summary>
        /// <returns>True if the blob exists; otherwise, false.</returns>
        public virtual bool Exists()
        {
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            return this.CloudBlob.Exists(options);
        }

        /// <summary>
        /// Checks whether or not the blob exists in cloud storage.
        /// </summary>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        [UsedImplicitly]
        public virtual async Task<bool> ExistsAsync()
        {
            return await this.ExistsAsync(CancellationToken.None);
        }

        /// <summary>
        /// Checks whether or not the blob exists in cloud storage.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task<bool> ExistsAsync(CancellationToken cancellationToken)
        {
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            return await this.CloudBlob.ExistsAsync(options, null, cancellationToken);
        }

        /// <summary>
        /// Generates a key value for an external reference.
        /// </summary>
        /// <param name="referenceId">The id of the external reference.</param>
        /// <returns>A string value that contains the key.</returns>
        protected internal virtual string GenerateExternalReferenceKey(Guid referenceId)
        {
            var rid = $"{referenceId:N}";
            return $"{XRefMetadataPrefix}{rid.ToUpperInvariant()}";
        }

        /// <summary>
        /// Gets the current <see cref="AccessCondition"/> value for requests against the current 
        /// blob.
        /// </summary>
        /// <returns>An <see cref="AccessCondition"/> object.</returns>
        protected AccessCondition GetAccessCondition()
        {
            if (this.LeaseManager.TryGetActiveLeaseId(out var leaseId))
            {
                return AccessCondition.GenerateLeaseCondition(leaseId);
            }
            return null;
        }

        /// <summary>
        /// Called by the base class to open a reference to the associated blob.
        /// </summary>
        /// <returns>An <see cref="ICloudBlob"/> instance.</returns>
        protected abstract ICloudBlob GetCloudBlob();

        /// <summary>
        /// Gets a reference to the current <see cref="CloudBlobContainer"/> for the blob.
        /// </summary>
        /// <returns>A <see cref="CloudBlobContainer"/> object.</returns>
        protected virtual CloudBlobContainer GetCloudBlobContainer()
        {
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            var account = this.Account.OpenStorageAccount();
            var bclient = account.CreateCloudBlobClient();
            var container = bclient.GetContainerReference(this.ContainerName);
            container.CreateIfNotExists(options);
            return container;
        }

        /// <summary>
        /// Gets a reference to the current <see cref="CloudBlobContainer"/> for the blob.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        [UsedImplicitly]
        protected virtual async Task<CloudBlobContainer> GetCloudBlobContainerAsync(CancellationToken cancellationToken)
        {
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            var account = this.Account.OpenStorageAccount();
            var bclient = account.CreateCloudBlobClient();
            var container = bclient.GetContainerReference(this.ContainerName);
            await container.CreateIfNotExistsAsync(options, null, cancellationToken);
            return container;
        }

        /// <summary>
        /// Gets the MD5 checksum of the blob contents.
        /// </summary>
        /// <returns>
        /// A <see langword="string"/> value that contains the Base-64 encoded MD5 value, or null
        /// if the blob does not exist in cloud storage.
        /// </returns>
        [UsedImplicitly]
        public string GetContentMD5() => this.GetContentMD5(false);

        /// <summary>
        /// Gets the MD5 checksum of the blob contents.
        /// </summary>
        /// <param name="refresh">
        /// <c>True</c> to force refreshing the value from the service; otherwise, <c>false</c> to
        /// use the locally cached value.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> value that contains the Base-64 encoded MD5 value, or null
        /// if the blob does not exist in cloud storage.
        /// </returns>
        public virtual string GetContentMD5(bool refresh)
        {
            if (this.Exists())
            {
                if ((this.CloudBlob.Properties.Length == -1) || refresh)
                {
                    var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
                    var access = this.GetAccessCondition();
                    this.CloudBlob.FetchAttributes(access, options);
                }
                return this.CloudBlob.Properties.ContentMD5;
            }
            return default(string);
        }

        /// <summary>
        /// Gets the MD5 checksum of the blob contents.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> object that returns a <see langword="string"/> value on completion
        /// with the either the Base-64 encoded MD5 value, or null if the blob does not exist in
        /// cloud storage.
        /// </returns>
        [UsedImplicitly]
        public async Task<string> GetContentMD5Async() => await this.GetContentMD5Async(false, CancellationToken.None);

        /// <summary>
        /// Gets the MD5 checksum of the blob contents.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to monitor for task cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> object that returns a <see langword="string"/> value on completion
        /// with the either the Base-64 encoded MD5 value, or null if the blob does not exist in
        /// cloud storage.
        /// </returns>
        [UsedImplicitly]
        public async Task<string> GetContentMD5Async(CancellationToken cancellationToken) => await this.GetContentMD5Async(false, cancellationToken);

        /// <summary>
        /// Gets the MD5 checksum of the blob contents.
        /// </summary>
        /// <param name="refresh">
        /// <c>True</c> to force refreshing the value from the service; otherwise, <c>false</c> to
        /// use the locally cached value.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> value that contains the Base-64 encoded MD5 value, or null
        /// if the blob does not exist in cloud storage.
        /// </returns>
        [UsedImplicitly]
        public async Task<string> GetContentMD5Async(bool refresh) => await this.GetContentMD5Async(refresh, CancellationToken.None);

        /// <summary>
        /// Gets the MD5 checksum of the blob contents.
        /// </summary>
        /// <param name="refresh">
        /// <c>True</c> to force refreshing the value from the service; otherwise, <c>false</c> to
        /// use the locally cached value.
        /// </param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to monitor for task cancellation requests.
        /// </param>
        /// <returns>
        /// A <see langword="string"/> value that contains the Base-64 encoded MD5 value, or null
        /// if the blob does not exist in cloud storage.
        /// </returns>
        public virtual async Task<string> GetContentMD5Async(bool refresh, CancellationToken cancellationToken)
        {
            if (await this.ExistsAsync(cancellationToken))
            {
                if ((this.CloudBlob.Properties.Length == -1) || refresh)
                {
                    var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
                    var access = this.GetAccessCondition();
                    await this.CloudBlob.FetchAttributesAsync(access, options, null, cancellationToken);
                }
                return this.CloudBlob.Properties.ContentMD5;
            }
            return default(string);
        }

        /// <summary>
        /// Gets the count of external references to the current blob.
        /// </summary>
        /// <returns>The number of external references to the blob.</returns>
        protected internal int GetExternalReferenceCount() => this.GetExternalReferenceCount(true);

        /// <summary>
        /// Gets the count of external references to the current blob.
        /// </summary>
        /// <param name="refresh">
        /// <c>True</c> to force a refresh of the attribute data for the blob from the server
        /// before calculating the reference count; otherwise, <c>false</c>.
        /// </param>
        /// <returns>The number of external references to the blob.</returns>
        protected internal virtual int GetExternalReferenceCount(bool refresh)
        {
            using (AzureStorageLease.Acquire(this))
            {
                if ((this.CloudBlob.Properties.Length == -1) || refresh)
                {
                    var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
                    var access = this.GetAccessCondition();
                    this.CloudBlob.FetchAttributes(access, options);
                }
                return this.CloudBlob.Metadata.Keys.Count(s => s.StartsWith("XREFID_"));
            }
        }

        /// <summary>
        /// Gets the length of the blob, in bytes.
        /// </summary>
        /// <returns>The current length of the blob, in bytes, or -1 if the blob does not exist.</returns>
        public virtual long GetLength()
        {
            var length = -1L;
            if (this.Exists())
            {
                var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
                var access = this.GetAccessCondition();
                var target = this.GetCloudBlob();
                target.FetchAttributes(access, options);
                length = target.Properties.Length;
            }
            return length;
        }

        /// <summary>
        /// Gets the length of the blob, in bytes.
        /// </summary>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        [UsedImplicitly]
        public virtual async Task<long> GetLengthAsync()
        {
            return await this.GetLengthAsync(CancellationToken.None);
        }

        /// <summary>
        /// Gets the length of the blob, in bytes.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task<long> GetLengthAsync(CancellationToken cancellationToken)
        {
            long length = -1;   // assume failure.
            if (await this.ExistsAsync(cancellationToken))
            {
                var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
                var access = this.GetAccessCondition();
                var target = this.GetCloudBlob();
                await target.FetchAttributesAsync(access, options, null, cancellationToken);
                length = target.Properties.Length;
            }
            return length;
        }

        /// <summary>
        /// Accesses the contents of the blob as a read-only stream.
        /// </summary>
        /// <returns>A <see cref="Stream"/> instance.</returns>
        public virtual Stream OpenRead()
        {
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            return this.CloudBlob.OpenRead(options: options);
        }

        /// <summary>
        /// Accesses the contents of the blob as a read-only stream.
        /// </summary>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        [UsedImplicitly]
        public virtual async Task<Stream> OpenReadAsync()
        {
            return await this.OpenReadAsync(CancellationToken.None);
        }

        /// <summary>
        /// Accesses the contents of the blob as a read-only stream.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            return await this.CloudBlob.OpenReadAsync(null, options, null, cancellationToken);
        }

        /// <summary>
        /// Uploads the contents of a stream to the blob.
        /// </summary>
        /// <param name="source">A <see cref="Stream"/> object that contains the new contents.</param>
        public virtual void UpdateFromStream(Stream source)
        {
            ArgumentValidator.ValidateNotNull(() => source);
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            var access = this.GetAccessCondition();
            this.CreateIfNotExists(options);
            this.CloudBlob.UploadFromStream(source, options: options, accessCondition: access);
        }

        /// <summary>
        /// Uploads the contents of a stream to the blob.
        /// </summary>
        /// <param name="source">The <see cref="Stream"/> that contains the data to write.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        [UsedImplicitly]
        public virtual async Task UpdateFromStreamAsync(Stream source)
        {
            await this.UpdateFromStreamAsync(source, CancellationToken.None);
        }

        /// <summary>
        /// Uploads the contents of a stream to the blob.
        /// </summary>
        /// <param name="source">A <see cref="Stream"/> object that contains the new contents.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task UpdateFromStreamAsync(Stream source, CancellationToken cancellationToken)
        {
            ArgumentValidator.ValidateNotNull(() => source);
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            var access = this.GetAccessCondition();
            await this.CreateIfNotExistsAsync(options, cancellationToken);
            await this.CloudBlob.UploadFromStreamAsync(source, access, options, null, cancellationToken);
        }

        /// <summary>
        /// Sets the contents of the blob to a string value.
        /// </summary>
        /// <param name="source">A string value that contains the new contents.</param>
        public virtual void UpdateFromString(string source)
        {
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            var contents = Encoding.UTF8.GetBytes(source);
            var access = this.GetAccessCondition();
            this.CreateIfNotExists(options);
            this.CloudBlob.UploadFromByteArray(contents, 0, contents.Length, options: options, accessCondition: access);
        }

        /// <summary>
        /// Sets the contents of the blob to a string value.
        /// </summary>
        /// <param name="source">A string value that contains the contents to write.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        [UsedImplicitly]
        public virtual async Task UpdateFromStringAsync(string source)
        {
            await this.UpdateFromStringAsync(source, CancellationToken.None);
        }

        /// <summary>
        /// Sets the contents of the blob to a string value.
        /// </summary>
        /// <param name="source">A string value that contains the new contents.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task UpdateFromStringAsync(string source, CancellationToken cancellationToken)
        {
            var options = this.RequestOptions.CreateOptions<BlobRequestOptions>();
            var contents = Encoding.UTF8.GetBytes(source);
            var access = this.GetAccessCondition();
            await this.CreateIfNotExistsAsync(options, cancellationToken);
            await this.CloudBlob.UploadFromByteArrayAsync(contents, 0, contents.Length, access, options, null, cancellationToken);
        }
    }
}
