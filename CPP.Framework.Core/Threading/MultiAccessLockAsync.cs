using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace CPP.Framework.Threading
{
    /// <summary>
    /// Synchronizes access to a shared resource, allowing read access for multiple threads, but 
    /// write access to only one thread at a time.
    /// </summary>
    public class MultiAccessLockAsync
    {
        /// <summary>
        /// The map of outstanding tokens to their id's.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, LockAccessToken> _activeTokenMap = new ConcurrentDictionary<Guid, LockAccessToken>();

        /// <summary>
        /// The <see cref="MultiAccessLock"/> used to synchronize access to the object across
        /// multiple threads.
        /// </summary>
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// The count currently active readers.
        /// </summary>
        private long _activeReaderCount;

        /// <summary>
        /// Gets a flag that indicates whether or not future lock requests should be ignored.
        /// </summary>
        protected virtual bool IgnoreLockRequests => false;

        /// <summary>
        /// Called by a <see cref="LockAccessToken"/> object just prior to releasing its associated 
        /// lock.
        /// </summary>
        /// <param name="accessToken">The access token for the lock.</param>
        protected virtual void BeforeRelease(LockAccessToken accessToken) { }

        /// <summary>
        /// Acquires a read lock for the protected resource.
        /// </summary>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        public IDisposable GetReaderAccess() => this.GetReaderAccess(Timeout.InfiniteTimeSpan);

        /// <summary>
        /// Acquires a read lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock, in milliseconds.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public IDisposable GetReaderAccess(int timeout) => this.GetReaderAccess(TimeSpan.FromMilliseconds(timeout));

        /// <summary>
        /// Acquires a read lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public IDisposable GetReaderAccess(TimeSpan timeout)
        {
            var token = default(LockAccessToken);

            if (!this.IgnoreLockRequests)
            {
                if (_syncLock.Wait(timeout))
                {
                    Interlocked.Increment(ref _activeReaderCount);
                    _syncLock.Release();
                    token = new ReaderAccessToken(this);
                }
                else throw new TimeoutException();
            }
            else
            {
                token = new NoLockAccessToken(this);
            }
            _activeTokenMap.TryAdd(token.InstanceId, token);

            return token;
        }

        /// <summary>
        /// Acquires a read lock for the protected resource.
        /// </summary>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="LockRecursionException">The lock was created with <see cref="LockRecursionPolicy.NoRecursion"/> and the current thread has already obtained the lock.</exception>
        public async Task<IDisposable> GetReaderAccessAsync()
        {
            return await this.GetReaderAccessAsync(Timeout.InfiniteTimeSpan, CancellationToken.None);
        }

        /// <summary>
        /// Acquires a read lock for the protected resource.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        public async Task<IDisposable> GetReaderAccessAsync(CancellationToken cancellationToken)
        {
            return await this.GetReaderAccessAsync(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        /// <summary>
        /// Acquires a read lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock, in milliseconds.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public async Task<IDisposable> GetReaderAccessAsync(int timeout)
        {
            return await this.GetReaderAccessAsync(TimeSpan.FromMilliseconds(timeout), CancellationToken.None);
        }

        /// <summary>
        /// Acquires a read lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock, in milliseconds.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public async Task<IDisposable> GetReaderAccessAsync(int timeout, CancellationToken cancellationToken)
        {
            return await this.GetReaderAccessAsync(TimeSpan.FromMilliseconds(timeout), cancellationToken);
        }

        /// <summary>
        /// Acquires a read lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public async Task<IDisposable> GetReaderAccessAsync(TimeSpan timeout)
        {
            return await this.GetReaderAccessAsync(timeout, CancellationToken.None);
        }

        /// <summary>
        /// Acquires a read lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public async Task<IDisposable> GetReaderAccessAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var token = default(LockAccessToken);

            if (!this.IgnoreLockRequests)
            {
                if (await _syncLock.WaitAsync(timeout, cancellationToken))
                {
                    Interlocked.Increment(ref _activeReaderCount);
                    _syncLock.Release();
                    token = new ReaderAccessToken(this);
                }
                else throw new TimeoutException();
            }
            else
            {
                token = new NoLockAccessToken(this);
            }
            _activeTokenMap.TryAdd(token.InstanceId, token);
            
            return token;
        }

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        public IDisposable GetWriterAccess()
        {
            return this.GetWriterAccess(Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock, in milliseconds.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public IDisposable GetWriterAccess(int timeout)
        {
            return this.GetWriterAccess(TimeSpan.FromMilliseconds(timeout));
        }

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public IDisposable GetWriterAccess(TimeSpan timeout)
        {
            var token = default(LockAccessToken);

            if (!this.IgnoreLockRequests)
            {
                if (_syncLock.Wait(timeout))
                {
                    while (Interlocked.Read(ref _activeReaderCount) != 0)
                    {
                        Thread.Sleep(100);
                    }
                    token = new WriterAccessToken(this);
                }
                else throw new TimeoutException();
            }
            else
            {
                token = new NoLockAccessToken(this);
            }
            _activeTokenMap.TryAdd(token.InstanceId, token);

            return token;
        }

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        public async Task<IDisposable> GetWriterAccessAsync()
        {
            return await this.GetWriterAccessAsync(Timeout.InfiniteTimeSpan, CancellationToken.None);
        }

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        public async Task<IDisposable> GetWriterAccessAsync(CancellationToken cancellationToken)
        {
            return await this.GetWriterAccessAsync(Timeout.InfiniteTimeSpan, cancellationToken);
        }

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock, in milliseconds.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public async Task<IDisposable> GetWriterAccessAsync(int timeout)
        {
            return await this.GetWriterAccessAsync(TimeSpan.FromMilliseconds(timeout), CancellationToken.None);
        }

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock, in milliseconds.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public async Task<IDisposable> GetWriterAccessAsync(int timeout, CancellationToken cancellationToken)
        {
            return await this.GetWriterAccessAsync(TimeSpan.FromMilliseconds(timeout), cancellationToken);
        }

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public async Task<IDisposable> GetWriterAccessAsync(TimeSpan timeout)
        {
            return await this.GetWriterAccessAsync(timeout, CancellationToken.None);
        }

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> object to observe for cancellation requests.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public async Task<IDisposable> GetWriterAccessAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            var token = default(LockAccessToken);

            if (!this.IgnoreLockRequests)
            {
                if (await _syncLock.WaitAsync(timeout, cancellationToken))
                {
                    while (Interlocked.Read(ref _activeReaderCount) != 0)
                    {
                        await Task.Delay(100, cancellationToken);
                    }
                    token = new WriterAccessToken(this);
                }
                else throw new TimeoutException();
            }
            else
            {
                token = new NoLockAccessToken(this);
            }
            _activeTokenMap.TryAdd(token.InstanceId, token);

            return token;
        }

        /// <summary>
        /// Releases a previously acquired lock.
        /// </summary>
        /// <param name="instanceId">The id of the <see cref="LockAccessToken"/> object associated with the lock.</param>
        protected void Release(Guid instanceId)
        {
            if (_activeTokenMap.TryRemove(instanceId, out var token))
            {
                if (token is ReaderAccessToken)
                {
                    Interlocked.Decrement(ref _activeReaderCount);
                }
                else if (token is WriterAccessToken)
                {
                    _syncLock.Release();
                }
            }
        }

        #region LockAccessToken Class Declarations

        /// <summary>
        /// Abstract base class for all lock access tokens.
        /// </summary>
        protected internal abstract class LockAccessToken : IDisposable
        {
            /// <summary>
            /// The flag the indicates whether or not the current class has been disposed.
            /// </summary>
            private int _disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="LockAccessToken"/> class. 
            /// </summary>
            /// <param name="container">
            /// The <see cref="MultiAccessLockAsync"/> object that contains the lock being accessed.
            /// </param>
            protected LockAccessToken(MultiAccessLockAsync container)
            {
                this.Container = container;
                this.InstanceId = GuidGeneratorService.Current.NewGuid(this);
            }

            /// <summary>
            /// Gets the <see cref="MultiAccessLockAsync"/> object that contains the lock being accessed.
            /// </summary>
            protected MultiAccessLockAsync Container { get; }

            /// <summary>
            /// Gets the unique id for the current lock instance.
            /// </summary>
            public Guid InstanceId { get; }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting 
            /// unmanaged resources.
            /// </summary>
            void IDisposable.Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                {
                    this.Container.BeforeRelease(this);
                    this.Release();
                }
            }

            /// <summary>
            /// Called by the base class to release the associated lock.
            /// </summary>
            protected virtual void Release() { this.Container.Release(this.InstanceId); }
        }

        /// <summary>
        /// Represents a lock access token that does not perform any actual locking.
        /// </summary>
        protected internal sealed class NoLockAccessToken : LockAccessToken
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="NoLockAccessToken"/> class. 
            /// </summary>
            /// <param name="container">
            /// The <see cref="MultiAccessLockAsync"/> object that contains the lock being accessed.
            /// </param>
            internal NoLockAccessToken(MultiAccessLockAsync container) : base(container) { }

            /// <summary>
            /// Called by the base class to release the associated lock.
            /// </summary>
            protected override void Release() { this.Container.Release(this.InstanceId); }
        }

        /// <summary>
        /// Represents a read access lock token.
        /// </summary>
        protected internal sealed class ReaderAccessToken : LockAccessToken
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ReaderAccessToken"/> class. 
            /// </summary>
            /// <param name="container">
            /// The <see cref="MultiAccessLockAsync"/> object that contains the lock being accessed.
            /// </param>
            internal ReaderAccessToken(MultiAccessLockAsync container) : base(container) { }
        }

        /// <summary>
        /// Represents a write lock access token.
        /// </summary>
        protected internal sealed class WriterAccessToken : LockAccessToken
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WriterAccessToken"/> class. 
            /// </summary>
            /// <param name="container">
            /// The <see cref="MultiAccessLockAsync"/> object that contains the lock being accessed.
            /// </param>
            internal WriterAccessToken(MultiAccessLockAsync container) : base(container) { }
        }

        #endregion // LockAccessToken Class Declarations
    }
}
