using System;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.DependencyInjection;
using CPP.Framework.DependencyInjection.Resolvers;
using CPP.Framework.Diagnostics;
using CPP.Framework.Threading;
using CPP.Framework.Threading.Tasks;

using JetBrains.Annotations;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Manages the lease for a single <see cref="AzureStorageBlob"/> object.
    /// </summary>
    internal class AzureStorageLeaseManager : IDisposable
    {
        private static readonly TimeSpan LeaseAcquireDuration = TimeSpan.FromSeconds(60.0);
        private static readonly TimeSpan LeaseRenewalDuration = TimeSpan.FromSeconds(30.0);
        private static readonly TimeSpan LockWaitDuration = TimeSpan.FromMilliseconds(200);

        private readonly AsyncLock _lock = new AsyncLock();
        private readonly TaskFactory _taskFactory;

        private Task<Task> _autoRenewTask;
        private CancellationTokenSource _cancellationSource;
        private long _refCount, _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageLeaseManager"/> class.
        /// </summary>
        /// <param name="owner">The <see cref="AzureStorageBlob"/> that owns the lease.</param>
        [ServiceLocatorConstructor]
        protected AzureStorageLeaseManager(AzureStorageBlob owner)
        {
            ArgumentValidator.ValidateNotNull(() => owner);
            this.LeaseOwner = owner;

            _taskFactory = new TaskFactory(
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);
            _refCount = 0;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AzureStorageLeaseManager"/> class. 
        /// </summary>
        ~AzureStorageLeaseManager() => this.Dispose(false);

        /// <summary>
        /// Gets a value indicating whether or not the lease is active and in use.
        /// </summary>
        [UsedImplicitly]
        public bool IsActive => (Interlocked.Read(ref _refCount) >= 1);

        /// <summary>
        /// Gets or sets the id of the active lease for the current execution context.
        /// </summary>
        private string LeaseID { get; set; }

        /// <summary>
        /// Gets the <see cref="AzureStorageBlob"/> that owns the lease.
        /// </summary>
        private AzureStorageBlob LeaseOwner { get; }

        /// <summary>
        /// Creates a task to automaticatlly renew the lease at a regular interval.
        /// </summary>
        /// <param name="leaseId">The id of the lease to renew.</param>
        /// <param name="cancellationToken">
        /// The <see cref="T:System.Threading.CancellationToken"/> to monitor for task
        /// cancellation requests.
        /// </param>
        /// <returns>A <see cref="Task"/> object.</returns>
        [UsedImplicitly]
        private async Task AutoRenewLeaseAsync(string leaseId, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                var conditions = AccessCondition.GenerateLeaseCondition(leaseId);
                do
                {
                    try
                    {
                        var options = this.LeaseOwner.RequestOptions.CreateOptions<BlobRequestOptions>();
                        await Task.Delay(LeaseRenewalDuration, cancellationToken);
                        await this.LeaseOwner.CloudBlob.RenewLeaseAsync(conditions, options, null, cancellationToken);
                    }
                    catch (StorageException)
                    {
                        /* ignored */
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
                while (!cancellationToken.IsCancellationRequested);
            }
        }

        /// <summary>
        /// Acquires a lease for exclusive access to the associated blob.
        /// </summary>
        /// <returns>
        /// An <see cref="AzureStorageLease"/> object that will automatically release the lock when
        /// it is disposed.
        /// </returns>
        [UsedImplicitly]
        public AzureStorageLease Acquire() => AsyncHelper.RunSync(() => this.AcquireAsync(Timeout.InfiniteTimeSpan));

        /// <summary>
        /// Acquires a lease for exclusive access to the associated blob.
        /// </summary>
        /// <param name="timeout">
        /// The maximum amount of time to wait to acquire the lock before throwing a
        /// <see cref="TimeoutException"/>, in milliseconds.
        /// </param>
        /// <returns>
        /// An <see cref="AzureStorageLease"/> object that will automatically release the lock when
        /// it is disposed.
        /// </returns>
        [UsedImplicitly]
        public AzureStorageLease Acquire(int timeout) => AsyncHelper.RunSync(() => this.AcquireAsync(TimeSpan.FromMilliseconds(timeout)));

        /// <summary>
        /// Acquires a lease for exclusive access to the associated blob.
        /// </summary>
        /// <param name="timeout">
        /// The maximum amount of time to wait to acquire the lock before throwing a
        /// <see cref="TimeoutException"/>.
        /// </param>
        /// <returns>
        /// An <see cref="AzureStorageLease"/> object that will automatically release the lock when
        /// it is disposed.
        /// </returns>
        [UsedImplicitly]
        public AzureStorageLease Acquire(TimeSpan timeout) => AsyncHelper.RunSync(() => this.AcquireAsync(timeout));

        /// <summary>
        /// Acquires a lease for exclusive access to the associated blob.
        /// </summary>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> value that is monitored for cancellation
        /// requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> object that returns a <see cref="AzureStorageLease"/> object that
        /// will automatically release the lock when it is disposed.
        /// </returns>
        [UsedImplicitly]
        public async Task<AzureStorageLease> AcquireAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.AcquireAsync(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        /// <summary>
        /// Attempts to acquire a lease for exclusive access to the associated blob.
        /// </summary>
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
        public async Task<AzureStorageLease> AcquireAsync(int timeout, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.AcquireAsync(TimeSpan.FromMilliseconds(timeout), cancellationToken);
        }

        /// <summary>
        /// Attempts to acquire a lease for exclusive access to the associated blob.
        /// </summary>
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
        public async Task<AzureStorageLease> AcquireAsync(TimeSpan timeout, CancellationToken cancellationToken = default(CancellationToken))
        {
            var firstRun = true;
            var timeoutSource = new CancellationTokenSource();
            var source = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                timeoutSource.Token);
            timeoutSource.CancelAfter(timeout);

            // we want to do a temporarily bump to the reference count in order to guard
            // against another thread unintentionally releasing the lease before we have a
            // finish creating it and incrementing the value for our own reference.
            Interlocked.Increment(ref _refCount);

            try
            {
                while (true)
                {
                    try
                    {
                        if (!firstRun)
                        {
                            await Task.Delay(LockWaitDuration, source.Token);
                        }
                        firstRun = false;

                        using (await _lock.AcquireAsync(cancellationToken))
                        {
                            if (this.LeaseID == null)
                            {
                                var options = this.LeaseOwner.RequestOptions.CreateOptions<BlobRequestOptions>();
                                var blob = this.LeaseOwner.CloudBlob;

                                if (!(await blob.ExistsAsync(options, null, source.Token)))
                                {
                                    var buffer = Encoding.UTF32.GetBytes(string.Empty);
                                    await blob.UploadFromByteArrayAsync(buffer, 0, 0, null, options, null, cancellationToken);
                                }
                                this.LeaseID = await blob.AcquireLeaseAsync(LeaseAcquireDuration, null, cancellationToken);

                                _cancellationSource = new CancellationTokenSource();
                                _autoRenewTask = _taskFactory.StartNew(() => AutoRenewLeaseAsync(this.LeaseID, _cancellationSource.Token), cancellationToken);
                                Interlocked.Increment(ref _refCount);
                            }
                        }

                        var resolvers = new ServiceResolver[]
                        {
                            new DependencyResolver<AzureStorageBlob>(this.LeaseOwner),
                        };
                        return ServiceLocator.GetInstance<AzureStorageLease>(resolvers);
                    }
                    catch (OperationCanceledException)
                    {
                        if (timeoutSource.Token.IsCancellationRequested)
                        {
                            throw new TimeoutException();
                        }
                        throw;
                    }
                    catch (StorageException ex)
                    {
                        switch (ex.RequestInformation.HttpStatusCode)
                        {
                            case ((int)HttpStatusCode.Conflict):
                            case ((int)HttpStatusCode.PreconditionFailed):
                                break;
                            default: throw;
                        }
                    }
                }
            }
            finally
            {
                // always decrement the value in order to remove the temporary bump we did
                // earlier. the net result after this call is that either the count is non-
                // zero (meaning we acquired or renewed successfully), or zero (if we were
                // unable to acquire the lease).
                Interlocked.Decrement(ref _refCount);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose() => this.Dispose(true);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>True</c> if the object is being disposed explicitly; otherwise, <c>false</c> if the
        /// object is being finalized.
        /// </param>
        protected internal void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {   
                try
                {
                    AsyncHelper.RunSync(() => this.TerminateLeaseAsync(CancellationToken.None, disposing));
                }
                catch (Exception ex)
                {
                    Journal.CreateSource(this).WriteWarning(ex);
                }
            }
            if (disposing) GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by the framework internally to release a previously acquired lease.
        /// </summary>
        [UsedImplicitly]
        internal void Release() => AsyncHelper.RunSync(() => this.ReleaseAsync());

        /// <summary>
        /// Called by the framework internally to release a previously acquired lease.
        /// </summary>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> value that is monitored for cancellation
        /// requests.
        /// </param>
        /// <returns>A <see cref="Task"/> object.</returns>
        [UsedImplicitly]
        internal async Task ReleaseAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var refcount = Interlocked.Decrement(ref _refCount);
            if (refcount == 0)
            {
                await this.TerminateLeaseAsync(cancellationToken);
            }
            else if (refcount < 0)
            {
                Journal.CreateSource(this).WriteWarning($"Release called for a lease that no longer exists ({this.LeaseOwner.ObjectName}).");
                Interlocked.Exchange(ref _refCount, 0);
            }
        }

        /// <summary>
        /// Immediately terminates the lease and resets the internal tracking data to an
        /// unallocated state.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> value that is monitored for cancellation requests.
        /// </param>
        /// <param name="disposing">
        /// <c>True</c> if the lease is being terminated explicitly; otherwise, <c>false</c> if it
        /// is being terminated because the object is being finalized.
        /// </param>
        /// <returns>A <see cref="Task"/> object.</returns>
        private async Task TerminateLeaseAsync(CancellationToken cancellationToken, bool disposing = true)
        {
            using (await _lock.AcquireAsync(cancellationToken))
            {
                var leaseId = this.LeaseID;
                if (leaseId != null)
                {
                    _cancellationSource?.Cancel();

                    if (_autoRenewTask != null) await _autoRenewTask;
                    var conditions = AccessCondition.GenerateLeaseCondition(leaseId);
                    var options = this.LeaseOwner.RequestOptions.CreateOptions<BlobRequestOptions>();
                    await this.LeaseOwner.CloudBlob.ReleaseLeaseAsync(conditions, options, null, cancellationToken);
                }
                this.LeaseID = null;
            }

            if (disposing)
            {
                _autoRenewTask?.Dispose();
                _cancellationSource?.Dispose();
            }
            _autoRenewTask = null;
            _cancellationSource = null;

            Interlocked.Exchange(ref _refCount, 0);
        }

        /// <summary>
        /// Attempts to get the id of the active lease for the associated
        /// <see cref="AzureStorageBlob"/>.
        /// </summary>
        /// <param name="leaseId">
        /// An output parameter that receives the value of the id on success.
        /// </param>
        /// <returns><c>True</c> if there is an active lease; otherwise, <c>false</c>.</returns>
        [UsedImplicitly]
        internal bool TryGetActiveLeaseId(out string leaseId)
        {
            using (_lock.Acquire())
            {
                leaseId = this.LeaseID;
                return (leaseId != null);
            }
        }
    }
}
