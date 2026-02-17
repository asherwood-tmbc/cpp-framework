using System.Threading;
using System.Threading.Tasks;

namespace CPP.Framework.Threading.Tasks
{
    /// <summary>
    /// Abstract interface for all classes that manage resources whose operations can be cancelled 
    /// using a <see cref="CancellationToken"/> object.
    /// </summary>
    public interface ICancellableResource
    {
        /// <summary>
        /// Gets or sets the <see cref="CancellationToken"/> object to monitor for cancellation
        /// requests.
        /// </summary>
        CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Called by the framework to notify the object that a cancellation has been requested, so
        /// that it can perform any addition wait operations or cleanup tasks.
        /// </summary>
        /// <param name="source">
        /// The <see cref="CancellableResourceManager"/> that requested the cancellation.
        /// </param>
        void OnCancelRequested(CancellableResourceManager source);

        /// <summary>
        /// Called by the framework to notify the object that a cancellation has been requested, so
        /// that it can perform any addition wait operations or cleanup tasks.
        /// </summary>
        /// <param name="source">
        /// The <see cref="CancellableResourceManager"/> that requested the cancellation.
        /// </param>
        /// <returns>A <see cref="Task"/> object that can be used to monitor the progress of the operation.</returns>
        Task OnCancelRequestedAsync(CancellableResourceManager source);
    }
}
