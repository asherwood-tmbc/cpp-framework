using System;
using System.Threading;
using System.Threading.Tasks;
using CPP.Framework.DependencyInjection;
using CPP.Framework.DependencyInjection.Resolvers;
using CPP.Framework.Services;

using JetBrains.Annotations;

namespace CPP.Framework.Threading.Tasks
{
    /// <summary>
    /// Manages a group of one or more resources whose operations can be cancelled using a
    /// <see cref="CancellationToken"/> object.
    /// </summary>
    [AutoRegisterService]
    // ReSharper disable once InheritdocConsiderUsage
    public class CancellableResourceManager : CodeService, IDisposable
    {
        /// <summary>
        /// The <see cref="ManualResetEvent"/> used to signal that the resources have entered a
        /// cancelled state.
        /// </summary>
        private readonly ManualResetEvent _cancelWaitEvent = new ManualResetEvent(false);

        /// <summary>
        /// The <see cref="TaskCompletionSource{TResult}"/> used to cancel asynchronous tasks.
        /// </summary>
        private readonly TaskCompletionSource<bool> _completionSource = new TaskCompletionSource<bool>();

        /// <summary>
        /// The external <see cref="CancellationTokenSource"/> used to cancel synchronous tasks.
        /// </summary>
        private readonly CancellationTokenSource _tokenSource;

        /// <summary>
        /// Initializes a new instance of the <see cref="CancellableResourceManager"/> class. 
        /// </summary>
        /// <param name="tokenSource">
        /// The external <see cref="CancellationTokenSource"/> used to cancel synchronous tasks.
        /// </param>
        /// <param name="disposeOnCancel">
        /// <c>True</c> to dispose of the resources being managed when a cancellation is requested;
        /// otherwise; <c>false</c> (e.g. to allow resources to be re-used with another manager).
        /// </param>
        [ServiceLocatorConstructor]
        protected CancellableResourceManager(CancellationTokenSource tokenSource, bool disposeOnCancel)
        {
            _tokenSource = (tokenSource ?? new CancellationTokenSource());
            _tokenSource.Token.Register(() => this.Cancel());
            this.DisposeOnCancel = disposeOnCancel;
            this.Resources = ServiceLocator.GetInstance<CancellableResourceGroup>();
            this.Resources.CancellationToken = _tokenSource.Token;
        }

        /// <summary>
        /// Gets a value indicating whether or not to call <see cref="IDisposable.Dispose"/> on the
        /// resources being managed when the cancellation is requested.
        /// </summary>
        public bool DisposeOnCancel { get; }

        /// <summary>
        /// Gets the list of <see cref="ICancellableResource"/> objects that are notified when the
        /// current source is cancelled.
        /// </summary>
        public CancellableResourceGroup Resources { get; }

        /// <summary>
        /// Requests a cancellation for all of the resources managed by the current source.
        /// </summary>
        public virtual void Cancel()
        {
            if (!_tokenSource.IsCancellationRequested)
            {
                _tokenSource.Cancel(false);
            }
            this.Resources.OnCancelRequested(this);
            _cancelWaitEvent.Set();
        }

        /// <summary>
        /// Requests a cancellation for all of the resources managed by the current source.
        /// </summary>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the operation.</returns>
        [UsedImplicitly]
        public virtual async Task CancelAsync()
        {
            _tokenSource.Cancel();
            await this.Resources.OnCancelRequestedAsync(this);
            _completionSource.SetResult(true);
        }

        /// <summary>
        /// Creates a new <see cref="CancellableResourceManager"/> instance.
        /// </summary>
        /// <returns>A <see cref="CancellableResourceManager"/> object.</returns>
        public static CancellableResourceManager Create() => Create(new CancellationTokenSource(), false);

        /// <summary>
        /// Creates a new <see cref="CancellableResourceManager"/> instance.
        /// </summary>
        /// <param name="disposeOnCancel">
        /// <c>True</c> to dispose of the resources being managed when a cancellation is requested;
        /// otherwise; <c>false</c> (e.g. to allow resources to be re-used with another manager).
        /// </param>
        /// <returns>A <see cref="CancellableResourceManager"/> object.</returns>
        public static CancellableResourceManager Create(bool disposeOnCancel) => Create(new CancellationTokenSource(), disposeOnCancel);

        /// <summary>
        /// Creates a new <see cref="CancellableResourceManager"/> instance.
        /// </summary>
        /// <param name="tokenSource">
        /// The external <see cref="CancellationTokenSource"/> used to cancel synchronous tasks.
        /// </param>
        /// <returns>A <see cref="CancellableResourceManager"/> object.</returns>
        public static CancellableResourceManager Create(CancellationTokenSource tokenSource) => Create(tokenSource, false);

        /// <summary>
        /// Creates a new <see cref="CancellableResourceManager"/> instance.
        /// </summary>
        /// <param name="tokenSource">
        /// The external <see cref="CancellationTokenSource"/> used to cancel synchronous tasks.
        /// </param>
        /// <param name="disposeOnCancel">
        /// <c>True</c> to dispose of the resources being managed when a cancellation is requested;
        /// otherwise; <c>false</c> (e.g. to allow resources to be re-used with another manager).
        /// </param>
        /// <returns>A <see cref="CancellableResourceManager"/> object.</returns>
        public static CancellableResourceManager Create(CancellationTokenSource tokenSource, bool disposeOnCancel)
        {
            ArgumentValidator.ValidateNotNull(() => tokenSource);
            var resolvers = new ServiceResolver[]
            {
                new DependencyResolver<CancellationTokenSource>(tokenSource),
                new ParameterResolver(nameof(disposeOnCancel), disposeOnCancel), 
            };
            return ServiceLocator.GetInstance<CancellableResourceManager>(resolvers);
        }

        /// <inheritdoc />
        void IDisposable.Dispose() => _tokenSource.Dispose();

        /// <summary>
        /// Blocks the current thread until cancellation is requested by another thread.
        /// </summary>
        [UsedImplicitly]
        public virtual void Wait()
        {
            _cancelWaitEvent.Reset();
            _cancelWaitEvent.WaitOne();
        }

        /// <summary>
        /// Asynchronously waits for a cancellation request.
        /// </summary>
        /// <returns>The completed <see cref="Task"/> object.</returns>
        [UsedImplicitly]
        public virtual async Task WaitAsync() => await _completionSource.Task;
    }
}
