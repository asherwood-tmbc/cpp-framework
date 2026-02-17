using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CPP.Framework.Threading.Tasks
{
    /// <summary>
    /// Manages a group of one or more <see cref="T:CPP.Framework.Threading.Tasks.ICancellableResource" /> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    // ReSharper disable once InheritdocConsiderUsage
    public class CancellableResourceGroup : ICancellableResource, IEnumerable<ICancellableResource>
    {
        /// <summary>
        /// The collection of cancellable resources in the group.
        /// </summary>
        private readonly HashSet<ICancellableResource> _childResources = new HashSet<ICancellableResource>();

        /// <summary>
        /// The <see cref="CancellationToken"/> for the group.
        /// </summary>
        private CancellationToken _cancellationToken = CancellationToken.None;

        /// <inheritdoc />
        /// <summary>
        /// Gets or sets the <see cref="CancellationToken" /> object to monitor for cancellation
        /// requests.
        /// </summary>
        CancellationToken ICancellableResource.CancellationToken
        {
            get => this.CancellationToken;
            set => this.CancellationToken = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="CancellationToken" /> object to monitor for cancellation
        /// requests.
        /// </summary>
        protected internal virtual CancellationToken CancellationToken
        {
            get => _cancellationToken;
            set
            {
                Parallel.ForEach(_childResources, res => res.CancellationToken = value);
                _cancellationToken = value;
            }
        }

        /// <summary>
        /// Adds a resource object to the current group.
        /// </summary>
        /// <param name="resource">The resource to add.</param>
        /// <returns>True if <paramref name="resource"/> was added; otherwise, false if it already exists in the group.</returns>
        public virtual bool Add(ICancellableResource resource)
        {
            if (_childResources.Add(resource))
            {
                resource.CancellationToken = this.CancellationToken;
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        IEnumerator<ICancellableResource> IEnumerable<ICancellableResource>.GetEnumerator() => _childResources.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => _childResources.GetEnumerator();

        /// <inheritdoc />
        /// <summary>
        /// Called by the framework to notify the object that a cancellation has been requested, so
        /// that it can perform any addition wait operations or cleanup tasks.
        /// </summary>
        /// <param name="source">
        /// The <see cref="CancellableResourceManager"/> that requested the cancellation.
        /// </param>
        /// <returns>A <see cref="T:System.Threading.Tasks.Task" /> object that can be used to monitor the progress of the operation.</returns>
        void ICancellableResource.OnCancelRequested(CancellableResourceManager source) => this.OnCancelRequested(source);

        /// <summary>
        /// Called by the framework to notify the object that a cancellation has been requested, so
        /// that it can perform any addition wait operations or cleanup tasks.
        /// </summary>
        /// <param name="source">
        /// The <see cref="CancellableResourceManager"/> that requested the cancellation.
        /// </param>
        protected internal virtual void OnCancelRequested(CancellableResourceManager source)
        {
            Parallel.ForEach(
                _childResources,
                (res) =>
                    {
                        res.OnCancelRequested(source);
                        if (source.DisposeOnCancel && (res is IDisposable disposable))
                        {
                            try
                            {
                                disposable.Dispose();
                            }
                            catch
                            {
                                /* ignored */
                            }
                        }
                    });
            if (source.DisposeOnCancel) _childResources.Clear();
        }

        /// <inheritdoc />
        /// <summary>
        /// Called by the framework to notify the object that a cancellation has been requested, so
        /// that it can perform any addition wait operations or cleanup tasks.
        /// </summary>
        /// <param name="source">
        /// The <see cref="CancellableResourceManager"/> that requested the cancellation.
        /// </param>
        /// <returns>A <see cref="Task" /> object that can be used to monitor the progress of the operation.</returns>
        async Task ICancellableResource.OnCancelRequestedAsync(CancellableResourceManager source) => await this.OnCancelRequestedAsync(source);

        /// <summary>
        /// Called by the framework to notify the object that a cancellation has been requested, so
        /// that it can perform any addition wait operations or cleanup tasks.
        /// </summary>
        /// <param name="source">
        /// The <see cref="CancellableResourceManager"/> that requested the cancellation.
        /// </param>
        /// <returns>A <see cref="Task" /> object that can be used to monitor the progress of the operation.</returns>
        protected internal virtual async Task OnCancelRequestedAsync(CancellableResourceManager source)
        {
            await Task.WhenAll(_childResources.Select(
                async (res) =>
                    {
                        await res.OnCancelRequestedAsync(source);
                        if (source.DisposeOnCancel && (res is IDisposable disposable))
                        {
                            try
                            {
                                disposable.Dispose();
                            }
                            catch
                            {
                                /* ignored */
                            }
                        }
                    }));
            if (source.DisposeOnCancel) _childResources.Clear();
        }

        /// <summary>
        /// Removes a resource object from the current group.
        /// </summary>
        /// <param name="resource">The resource to remove.</param>
        /// <returns>True if <paramref name="resource"/> was removed; otherwise, false if does not exist in the group.</returns>
        public virtual bool Remove(ICancellableResource resource)
        {
            if (_childResources.Remove(resource))
            {
                resource.CancellationToken = CancellationToken.None;
                return true;
            }
            return false;
        }
    }
}
