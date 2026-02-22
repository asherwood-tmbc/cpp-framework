using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Threading
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MultiAccessLockTests
    {
        #region NoLockAccessImpl Class Declaration

        private sealed class NoLockAccessImpl : MultiAccessLock
        {
            /// <summary>
            /// Gets a flag that indicates whether or not future lock requests should be ignored.
            /// </summary>
            protected override bool IgnoreLockRequests { get { return true; } }
        }

        #endregion // NoLockAccessImpl Class Declaration

        #region InfiniteLockThread Class Declaration

        private abstract class InfiniteLockThread
        {
            private readonly ManualResetEventSlim _StartupEvent = new ManualResetEventSlim(false);
            private readonly Thread _WorkerThread;

            protected InfiniteLockThread()
            {
                _WorkerThread = new Thread(arg =>
                {
                    var target = (arg as MultiAccessLock);
                    try
                    {
                        using (this.TakeLock(target))
                        {
                            _StartupEvent.Set();
                            Thread.Sleep(Timeout.InfiniteTimeSpan);
                        }
                    }
                    catch (ThreadAbortException) { }
                });
            }
            public void Abort() { _WorkerThread.Abort(); }
            public void Start(MultiAccessLock target)
            {
                _WorkerThread.Start(target);
                _StartupEvent.Wait();
            }
            protected abstract IDisposable TakeLock(MultiAccessLock target);
        }
        private sealed class InfiniteReaderThread : InfiniteLockThread
        {
            protected override IDisposable TakeLock(MultiAccessLock target) { return target.GetReaderAccess(); }
        }
        private sealed class InfiniteWriterThread : InfiniteLockThread
        {
            protected override IDisposable TakeLock(MultiAccessLock target) { return target.GetWriterAccess(); }
        }

        #endregion // InfiniteLockThread Class Declaration

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void GetReaderAccess()
        {
            var target = new MultiAccessLock();
            using (var token = target.GetReaderAccess())
            {
                token.Should().BeOfType<MultiAccessLock.ReaderAccessToken>();
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void GetReaderAccessThenGetWriterAccess()
        {
            var target = new MultiAccessLock();
            using (var token = target.GetReaderAccess())
            {
                token.Should().BeOfType<MultiAccessLock.ReaderAccessToken>();
            }
            using (var token = target.GetWriterAccess())
            {
                token.Should().BeOfType<MultiAccessLock.WriterAccessToken>();
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void GetReaderAccessWithNoLock()
        {
            var target = new NoLockAccessImpl();
            using (var token = target.GetReaderAccess())
            {
                token.Should().BeOfType<MultiAccessLock.NoLockAccessToken>();
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void GetReaderAccessWithRecurseReader()
        {
            var target = new MultiAccessLock(LockRecursionPolicy.SupportsRecursion);
            using (target.GetReaderAccess())
            using (var token = target.GetReaderAccess())
            {
                token.Should().BeOfType<MultiAccessLock.ReaderAccessToken>();
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [ExpectedException(typeof(LockRecursionException))]
        public void GetReaderAccessWithRecurseReaderAndNoRecursion()
        {
            var target = new MultiAccessLock(LockRecursionPolicy.NoRecursion);
            using (target.GetReaderAccess())
            using (target.GetReaderAccess()) { }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [ExpectedException(typeof(LockRecursionException))]
        public void GetReaderAccessWithRecurseWriter()
        {
            var target = new MultiAccessLock(LockRecursionPolicy.SupportsRecursion);
            using (target.GetReaderAccess())
            using (target.GetWriterAccess()) { }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void GetReaderAccessWithThreadReaderLockAndTimeout()
        {
            var thread = new InfiniteReaderThread();
            try
            {
                var target = new MultiAccessLock();
                thread.Start(target);
                using (target.GetReaderAccess(0)) { }
            }
            finally { thread.Abort(); }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [ExpectedException(typeof(TimeoutException))]
        public void GetReaderAccessWithThreadWriterLockAndTimeout()
        {
            var thread = new InfiniteWriterThread();
            try
            {
                var target = new MultiAccessLock();
                thread.Start(target);
                using (target.GetReaderAccess(0)) { }
            }
            finally { thread.Abort(); }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void GetWriterAccess()
        {
            var target = new MultiAccessLock();
            using (var token = target.GetWriterAccess())
            {
                token.Should().BeOfType<MultiAccessLock.WriterAccessToken>();
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void GetWriterAccessWithNoLock()
        {
            var target = new NoLockAccessImpl();
            using (var token = target.GetWriterAccess())
            {
                token.Should().BeOfType<MultiAccessLock.NoLockAccessToken>();
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void GetWriterAccessWithRecurseReader()
        {
            var target = new MultiAccessLock(LockRecursionPolicy.SupportsRecursion);
            using (target.GetWriterAccess())
            using (var token = target.GetReaderAccess())
            {
                token.Should().BeOfType<MultiAccessLock.ReaderAccessToken>();
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [ExpectedException(typeof(LockRecursionException))]
        public void GetWriterAccessWithRecurseReaderAndNoRecursion()
        {
            var target = new MultiAccessLock(LockRecursionPolicy.NoRecursion);
            using (target.GetWriterAccess())
            using (target.GetReaderAccess()) { }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void GetWriterAccessWithRecurseWriter()
        {
            var target = new MultiAccessLock(LockRecursionPolicy.SupportsRecursion);
            using (target.GetWriterAccess())
            using (var token = target.GetWriterAccess())
            {
                token.Should().BeOfType<MultiAccessLock.WriterAccessToken>();
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [ExpectedException(typeof(LockRecursionException))]
        public void GetWriterAccessWithRecurseWriterAndNoRecursion()
        {
            var target = new MultiAccessLock(LockRecursionPolicy.NoRecursion);
            using (target.GetWriterAccess())
            using (target.GetWriterAccess()) { }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [ExpectedException(typeof(TimeoutException))]
        public void GetWriterAccessWithThreadReaderLockAndTimeout()
        {
            var thread = new InfiniteReaderThread();
            try
            {
                var target = new MultiAccessLock();
                thread.Start(target);
                using (target.GetWriterAccess(0)) { }
            }
            finally { thread.Abort(); }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [ExpectedException(typeof(TimeoutException))]
        public void GetWriterAccessWithThreadWriterLockAndTimeout()
        {
            var thread = new InfiniteWriterThread();
            try
            {
                var target = new MultiAccessLock();
                thread.Start(target);
                using (target.GetWriterAccess(0)) { }
            }
            finally { thread.Abort(); }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        public void GetWriterAccessWithTimeout()
        {
            var target = new MultiAccessLock();
            using (var token = target.GetWriterAccess(0))
            {
                token.Should().BeOfType<MultiAccessLock.WriterAccessToken>();
            }
        }

        #region Test Class Helper Methods

        #endregion // Test Class Helper Methods
    }
}
