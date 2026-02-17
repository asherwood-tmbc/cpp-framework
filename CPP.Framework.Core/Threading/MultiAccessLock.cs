using System;
using System.Threading;

namespace CPP.Framework.Threading
{
    /// <summary>
    /// Synchronizes access to a shared resource, allowing read access for multiple threads, but 
    /// write access to only one thread at a time.
    /// </summary>
    public class MultiAccessLock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiAccessLock"/> class. 
        /// </summary>
        public MultiAccessLock() : this(LockRecursionPolicy.NoRecursion) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiAccessLock"/> class. 
        /// </summary>
        /// <param name="recursionPolicy">The default recursion policy for the lock.</param>
        public MultiAccessLock(LockRecursionPolicy recursionPolicy)
        {
            this.LockObject = new ReaderWriterLockSlim(recursionPolicy);
        }

        /// <summary>
        /// Gets a flag that indicates whether or not future lock requests should be ignored.
        /// </summary>
        protected virtual bool IgnoreLockRequests => false;

        /// <summary>
        /// Gets a reference to framework the <see cref="ReaderWriterLockSlim"/> object.
        /// </summary>
        protected ReaderWriterLockSlim LockObject { get; }

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
        /// <exception cref="LockRecursionException">The lock was created with <see cref="LockRecursionPolicy.NoRecursion"/> and the current thread has already obtained the lock.</exception>
        public IDisposable GetReaderAccess() => this.GetReaderAccess(Timeout.InfiniteTimeSpan);

        /// <summary>
        /// Acquires a read lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock, in milliseconds.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="LockRecursionException">The lock was created with <see cref="LockRecursionPolicy.NoRecursion"/> and the current thread has already obtained the lock.</exception>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public IDisposable GetReaderAccess(int timeout) => this.GetReaderAccess(TimeSpan.FromMilliseconds(timeout));

        /// <summary>
        /// Acquires a read lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="LockRecursionException">The lock was created with <see cref="LockRecursionPolicy.NoRecursion"/> and the current thread has already obtained the lock.</exception>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public IDisposable GetReaderAccess(TimeSpan timeout)
        {
            if (this.IgnoreLockRequests)
            {
                return new NoLockAccessToken(this);
            }
            return new ReaderAccessToken(this, timeout);
        }

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="LockRecursionException">
        ///     <para>The lock was created with <see cref="LockRecursionPolicy.NoRecursion"/> and the current thread has already obtained the lock.</para>
        ///     <para>-or-</para>
        ///     <para>The current thread initially obtained a read lock, which has not been released.</para>
        /// </exception>
        public IDisposable GetWriterAccess() => this.GetWriterAccess(Timeout.InfiniteTimeSpan);

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock, in milliseconds.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="LockRecursionException">
        ///     <para>The lock was created with <see cref="LockRecursionPolicy.NoRecursion"/> and the current thread has already obtained the lock.</para>
        ///     <para>-or-</para>
        ///     <para>The current thread initially obtained a read lock, which has not been released.</para>
        /// </exception>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public IDisposable GetWriterAccess(int timeout) => this.GetWriterAccess(TimeSpan.FromMilliseconds(timeout));

        /// <summary>
        /// Acquires a write lock for the protected resource.
        /// </summary>
        /// <param name="timeout">The amount of time to wait to acquire the lock.</param>
        /// <returns>An access token instance that releases the acquired lock automatically once the token has been disposed by a using block.</returns>
        /// <exception cref="LockRecursionException">
        ///     <para>The lock was created with <see cref="LockRecursionPolicy.NoRecursion"/> and the current thread has already obtained the lock.</para>
        ///     <para>-or-</para>
        ///     <para>The current thread initially obtained a read lock, which has not been released.</para>
        /// </exception>
        /// <exception cref="TimeoutException">The <paramref name="timeout"/> period elapsed before the lock could be acquired.</exception>
        public IDisposable GetWriterAccess(TimeSpan timeout)
        {
            if (this.IgnoreLockRequests)
            {
                return new NoLockAccessToken(this);
            }
            return new WriterAccessToken(this, timeout);
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
            /// The <see cref="MultiAccessLock"/> object that contains the lock being accessed.
            /// </param>
            protected LockAccessToken(MultiAccessLock container) { this.Container = container; }

            /// <summary>
            /// Gets the <see cref="MultiAccessLock"/> object that contains the lock being accessed.
            /// </summary>
            protected MultiAccessLock Container { get; private set; }

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
            protected abstract void Release();
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
            /// The <see cref="MultiAccessLock"/> object that contains the lock being accessed.
            /// </param>
            internal NoLockAccessToken(MultiAccessLock container) : base(container) { }

            /// <summary>
            /// Called by the base class to release the associated lock.
            /// </summary>
            protected override void Release() { }
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
            /// The <see cref="MultiAccessLock"/> object that contains the lock being accessed.
            /// </param>
            /// <param name="timeout">
            /// The maximum amount of time to wait when acquiring the lock.
            /// </param>
            /// <exception cref="TimeoutException">
            /// The <paramref name="timeout"/> period elapsed before the lock could be acquired.
            /// </exception>
            internal ReaderAccessToken(MultiAccessLock container, TimeSpan timeout)
                : base(container)
            {
                if (!this.Container.LockObject.TryEnterReadLock(timeout))
                {
                    throw new TimeoutException(ErrorStrings.AcquireLockTimedOut);
                }
            }

            /// <summary>
            /// Called by the base class to release the associated lock.
            /// </summary>
            protected override void Release() { this.Container.LockObject.ExitReadLock(); }
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
            /// The <see cref="MultiAccessLock"/> object that contains the lock being accessed.
            /// </param>
            /// <param name="timeout">
            /// The maximum amount of time to wait when acquiring the lock.
            /// </param>
            /// <exception cref="TimeoutException">
            /// The <paramref name="timeout"/> period elapsed before the lock could be acquired.
            /// </exception>
            internal WriterAccessToken(MultiAccessLock container, TimeSpan timeout)
                : base(container)
            {
                if (!this.Container.LockObject.TryEnterWriteLock(timeout))
                {
                    throw new TimeoutException(ErrorStrings.AcquireLockTimedOut);
                }
            }

            /// <summary>
            /// Called by the base class to release the associated lock.
            /// </summary>
            protected override void Release() { this.Container.LockObject.ExitWriteLock(); }
        }

        #endregion // LockAccessToken Class Declarations
    }
}
