using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.DependencyInjection;
using CPP.Framework.Threading.Tasks;

using JetBrains.Annotations;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Represents an exclusive lease for an <see cref="AzureStorageBlob"/> instance.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class AzureStorageLease : IDisposable
    {
        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageLease"/> class. 
        /// </summary>
        /// <param name="targetBlob">
        /// The <see cref="AzureStorageBlob"/> object that is the target of the lease.
        /// </param>
        [ServiceLocatorConstructor]
        internal AzureStorageLease(AzureStorageBlob targetBlob)
        {
            ArgumentValidator.ValidateNotNull(() => targetBlob);
            this.TargetBlob = targetBlob;
        }

        /// <summary>
        /// Gets the <see cref="AzureStorageBlob"/> object that is the target of the lease.
        /// </summary>
        public AzureStorageBlob TargetBlob { get; private set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                this.TargetBlob?.LeaseManager.Release();
                this.TargetBlob = null;
            }
        }


        /// <summary>
        /// Acquires a lease for exclusive access to the associated blob.
        /// </summary>
        /// <param name="blob">The blob for which to acquire the lease.</param>
        /// <returns>
        /// An <see cref="AzureStorageLease"/> object that will automatically release the lock when
        /// it is disposed.
        /// </returns>
        [UsedImplicitly]
        public static AzureStorageLease Acquire(AzureStorageBlob blob) => AsyncHelper.RunSync(() => AcquireAsync(blob, Timeout.InfiniteTimeSpan));

        /// <summary>
        /// Acquires a lease for exclusive access to the associated blob.
        /// </summary>
        /// <param name="blob">The blob for which to acquire the lease.</param>
        /// <param name="timeout">
        /// The maximum amount of time to wait to acquire the lock before throwing a
        /// <see cref="TimeoutException"/>, in milliseconds.
        /// </param>
        /// <returns>
        /// An <see cref="AzureStorageLease"/> object that will automatically release the lock when
        /// it is disposed.
        /// </returns>
        [UsedImplicitly]
        public static AzureStorageLease Acquire(AzureStorageBlob blob, int timeout) => AsyncHelper.RunSync(() => AcquireAsync(blob, TimeSpan.FromMilliseconds(timeout)));

        /// <summary>
        /// Acquires a lease for exclusive access to the associated blob.
        /// </summary>
        /// <param name="blob">The blob for which to acquire the lease.</param>
        /// <param name="timeout">
        /// The maximum amount of time to wait to acquire the lock before throwing a
        /// <see cref="TimeoutException"/>.
        /// </param>
        /// <returns>
        /// An <see cref="AzureStorageLease"/> object that will automatically release the lock when
        /// it is disposed.
        /// </returns>
        [UsedImplicitly]
        public static AzureStorageLease Acquire(AzureStorageBlob blob, TimeSpan timeout) => AsyncHelper.RunSync(() => AcquireAsync(blob, timeout));

        /// <summary>
        /// Acquires a lease for exclusive access to the associated blob.
        /// </summary>
        /// <param name="blob">The blob for which to acquire the lease.</param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> value that is monitored for cancellation
        /// requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> object that returns a <see cref="AzureStorageLease"/> object that
        /// will automatically release the lock when it is disposed.
        /// </returns>
        [UsedImplicitly]
        public static async Task<AzureStorageLease> AcquireAsync(AzureStorageBlob blob, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await AcquireAsync(blob, Timeout.InfiniteTimeSpan, cancellationToken);
        }

        /// <summary>
        /// Attempts to acquire a lease for exclusive access to the associated blob.
        /// </summary>
        /// <param name="blob">The blob for which to acquire the lease.</param>
        /// <param name="timeout">
        /// The maximum amount of time to wait to acquire the lock before throwing a
        /// <see cref="TimeoutException"/>, in milliseconds.
        /// </param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> value that is monitored for cancellation
        /// requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> object that returns a <see cref="AzureStorageLease"/> object that
        /// will automatically release the lock when it is disposed.
        /// </returns>
        [UsedImplicitly]
        public static async Task<AzureStorageLease> AcquireAsync(AzureStorageBlob blob, int timeout, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await AcquireAsync(blob, TimeSpan.FromMilliseconds(timeout), cancellationToken);
        }

        /// <summary>
        /// Attempts to acquire a lease for exclusive access to the associated blob.
        /// </summary>
        /// <param name="blob">The blob for which to acquire the lease.</param>
        /// <param name="timeout">
        /// The maximum amount of time to wait to acquire the lock before throwing a
        /// <see cref="TimeoutException"/>.
        /// </param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> value that is monitored for cancellation
        /// requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> object that returns a <see cref="AzureStorageLease"/> object that
        /// will automatically release the lock when it is disposed.
        /// </returns>
        [UsedImplicitly]
        public static async Task<AzureStorageLease> AcquireAsync(AzureStorageBlob blob, TimeSpan timeout, CancellationToken cancellationToken = default(CancellationToken))
        {
            ArgumentValidator.ValidateNotNull(() => blob);
            return await blob.LeaseManager.AcquireAsync(timeout, cancellationToken);
        }
    }
}
