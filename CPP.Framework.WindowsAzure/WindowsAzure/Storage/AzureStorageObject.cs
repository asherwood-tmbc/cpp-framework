using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CPP.Framework.Threading;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Abstract base class for all objects that are stored in the Windows Azure Cloud.
    /// </summary>
    public abstract class AzureStorageObject :
        IDisposable,
        IEquatable<AzureStorageObject>
    {
        /// <summary>
        /// The default string comparer for the class.
        /// </summary>
        protected static readonly IEqualityComparer<string> DefaultComparer = StringComparer.OrdinalIgnoreCase;

        private static AzureRequestOptions _DefaultRequestOptions = AzureRequestOptions.Default;
        private static readonly MultiAccessLock _SyncLock = new MultiAccessLock(LockRecursionPolicy.NoRecursion);

        private int _IsDisposed;
        private AzureRequestOptions _RequestOptions;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="account">The <see cref="AzureStorageAccount"/> where the object is stored.</param>
        /// <param name="objectName">The name of the storage object.</param>
        protected AzureStorageObject(AzureStorageAccount account, string objectName)
        {
            ArgumentValidator.ValidateNotNull(() => account);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => objectName);
            this.Account = account;
            this.ObjectName = objectName;
            _RequestOptions = AzureStorageObject.DefaultRequestOptions;
        }

        /// <summary>
        /// Gets the <see cref="AzureStorageAccount"/> where the object is stored.
        /// </summary>
        public AzureStorageAccount Account { get; private set; }

        /// <summary>
        /// Gets or sets the default <see cref="AzureRequestOptions"/> object that is 
        /// assigned to newly created Azure Storage Objects on any thread.
        /// </summary>
        public static AzureRequestOptions DefaultRequestOptions
        {
            get
            {
                using (_SyncLock.GetReaderAccess()) return _DefaultRequestOptions;
            }
            set
            {
                using (_SyncLock.GetWriterAccess())
                {
                    if (value == null)
                    {
                        value = new AzureRequestOptions();
                    }
                    _DefaultRequestOptions = value;
                }
            }
        }

        /// <summary>
        /// Gets the name of the storage object.
        /// </summary>
        public string ObjectName { get; private set; }

        /// <summary>
        /// Gets or sets the options for requests made to the server related to the current object.
        /// </summary>
        public AzureRequestOptions RequestOptions
        {
            get { return _RequestOptions; }
            set { this._RequestOptions = (value ?? new AzureRequestOptions()); }
        }

        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <returns>True if the object exists and was deleted; otherwise, false.</returns>
        public abstract bool Delete();

        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public virtual async Task<bool> DeleteAsync()
        {
            return await this.DeleteAsync(CancellationToken.None);
        }

        /// <summary>
        /// Deletes the object from Windows Azure Cloud Storage.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observer for cancellation requests.</param>
        /// <returns>A <see cref="Task"/> object that represents the asynchronous operation.</returns>
        public abstract Task<bool> DeleteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (Interlocked.Exchange(ref _IsDisposed, 1) == 0)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">True if the object is being disposed explicitly; otherwise, false.</param>
        protected virtual void Dispose(bool disposing) { }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="that">An object to compare with this object.</param>
        /// <returns>True if the current object is equal to the <paramref name="that"/> parameter; otherwise, false.</returns>
        public virtual bool Equals(AzureStorageObject that)
        {
            if (ReferenceEquals(null, that)) return false;
            if (ReferenceEquals(this, that)) return true;

            if (DefaultComparer.Equals(this.ObjectName, that.ObjectName))
            {
                return DefaultComparer.Equals(this.Account.AccountName, that.Account.AccountName);
            }
            return false;
        }
    }
}
