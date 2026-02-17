using System;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.Threading.Tasks;

using JetBrains.Annotations;

namespace CPP.Framework.Threading
{
    /// <summary>
    /// Provides an simple exclusive lock (similar to a <see cref="Mutex"/>) that can be used
    /// inside of methods that have been marked with the <see langword="async" /> keyword.
    /// </summary>
    public sealed class AsyncLock : MarshalByRefObject
    {
        private const string ActiveLockSlotName = nameof(AsyncLock) + "#Active";
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private long _refCount;

        /// <summary>
        /// Acquires exclusive access to the lock.
        /// </summary>
        /// <returns>
        /// An <see cref="IDisposable"/> object that will automatically release the lock when
        /// disposed.
        /// </returns>
        [UsedImplicitly]
        public IDisposable Acquire() => this.Acquire(Timeout.InfiniteTimeSpan);

        /// <summary>
        /// Acquires exclusive access to the lock.
        /// </summary>
        /// <param name="timeout">
        /// The amount of time to wait to acquiring the lock before throwing an exception (in
        /// milliseconds), or <see cref="Timeout.Infinite"/> to wait indefinitely.
        /// </param>
        /// <returns>
        /// An <see cref="IDisposable"/> object that will automatically release the lock when
        /// disposed.
        /// </returns>
        [UsedImplicitly]
        public IDisposable Acquire(int timeout) => this.Acquire(TimeSpan.FromMilliseconds(timeout));

        /// <summary>
        /// Acquires exclusive access to the lock.
        /// </summary>
        /// <param name="timeout">
        /// The amount of time to wait to acquiring the lock before throwing an exception, or
        /// <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
        /// </param>
        /// <returns>
        /// An <see cref="IDisposable"/> object that will automatically release the lock when
        /// disposed.
        /// </returns>
        [UsedImplicitly]
        public IDisposable Acquire(TimeSpan timeout) => AsyncHelper.RunSync(() => this.AcquireAsync(timeout));

        /// <summary>
        /// Acquires exclusive access to the lock.
        /// </summary>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> the is monitored for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> object that returns an <see cref="IDisposable"/> object that will
        /// automatically release the lock when disposed.
        /// </returns>
        [UsedImplicitly]
        public async Task<IDisposable> AcquireAsync(CancellationToken cancellationToken = default(CancellationToken)) => await this.AcquireAsync(Timeout.InfiniteTimeSpan, cancellationToken);

        /// <summary>
        /// Acquires exclusive access to the lock.
        /// </summary>
        /// <param name="timeout">
        /// The amount of time to wait to acquiring the lock before throwing an exception (in
        /// milliseconds), or <see cref="Timeout.Infinite"/> to wait indefinitely.
        /// </param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> the is monitored for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> object that returns an <see cref="IDisposable"/> object that will
        /// automatically release the lock when disposed.
        /// </returns>
        [UsedImplicitly]
        public async Task<IDisposable> AcquireAsync(int timeout, CancellationToken cancellationToken = default(CancellationToken)) => await this.AcquireAsync(TimeSpan.FromMilliseconds(timeout), cancellationToken);

        /// <summary>
        /// Acquires exclusive access to the lock.
        /// </summary>
        /// <param name="timeout">
        /// The amount of time to wait to acquiring the lock before throwing an exception, or
        /// <see cref="Timeout.InfiniteTimeSpan"/> to wait indefinitely.
        /// </param>
        /// <param name="cancellationToken">
        /// An optional <see cref="CancellationToken"/> the is monitored for cancellation requests.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> object that returns an <see cref="IDisposable"/> object that will
        /// automatically release the lock when disposed.
        /// </returns>
        [UsedImplicitly]
        public async Task<IDisposable> AcquireAsync(TimeSpan timeout, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (CallContext.LogicalGetData(ActiveLockSlotName) != this)
            {
                if (!(await _lock.WaitAsync(timeout, cancellationToken)))
                {
                    throw new TimeoutException();
                }
                CallContext.LogicalSetData(ActiveLockSlotName, this);
            }
            Interlocked.Increment(ref _refCount);

            return new LockAccessToken(this);
        }

        /// <summary>
        /// Releases a previously acquired lock.
        /// </summary>
        [UsedImplicitly]
        private void Release()
        {
            if (Interlocked.Decrement(ref _refCount) == 0)
            {
                var existing = CallContext.LogicalGetData(ActiveLockSlotName);
                if (existing == this) existing = null;
                CallContext.LogicalSetData(ActiveLockSlotName, existing);
                _lock.Release();
            }
        }

        #region LockAccessToken Class Declaration

        /// <summary>
        /// Helper class used to automatically release locks when disposed.
        /// </summary>
        private sealed class LockAccessToken : IDisposable
        {
            private AsyncLock _parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="LockAccessToken"/> class.
            /// </summary>
            /// <param name="parent">The parent lock to release.</param>
            internal LockAccessToken(AsyncLock parent) => _parent = parent;

            /// <inheritdoc />
            void IDisposable.Dispose() => Interlocked.Exchange(ref _parent, null)?.Release();
        }

        #endregion // LockAccessToken Class Declaration
    }
}
