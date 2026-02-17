using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.DependencyInjection;
using CPP.Framework.DependencyInjection.Resolvers;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Represents a request context for an operation against an <see cref="AzureStorageObject"/>.
    /// </summary>
    public class AzureRequestContext
    {
        /// <summary>
        /// The <see cref="IRetryPolicy"/> instance for the current request.
        /// </summary>
        private readonly Lazy<IRetryPolicy> _policy;

        /// <summary>
        /// The hash set of <see cref="HttpStatusCode"/> values that are included in the retry 
        /// policy after a request failure.
        /// </summary>
        private readonly HashSet<int> _included = new HashSet<int>();

        /// <summary>
        /// The hash set of <see cref="HttpStatusCode"/> values that are excluded from the retry 
        /// policy after a request failure.
        /// </summary>
        private readonly HashSet<int> _excluded = new HashSet<int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureRequestContext"/> class.
        /// </summary>
        /// <param name="options">
        /// The <see cref="AzureRequestOptions"/> object for the request.
        /// </param>
        protected AzureRequestContext(AzureRequestOptions options)
        {
            this.Options = (options ?? AzureRequestOptions.Default);
            this.OperationContext = new OperationContext();
            _policy = new Lazy<IRetryPolicy>(() => (this.Options.RetryPolicy?.GetPolicy()?.CreateInstance()));
        }

        /// <summary>
        /// Gets the <see cref="OperationContext"/> for the current request.
        /// </summary>
        protected OperationContext OperationContext { get; }

        /// <summary>
        /// Gets the <see cref="AzureRequestOptions"/> object for the request.
        /// </summary>
        protected AzureRequestOptions Options { get; }

        /// <summary>
        /// Gets or sets the number of retry attempts that have been made for the current request.
        /// </summary>
        private int RetryCount { get; set; }

        /// <summary>
        /// Gets the <see cref="IRetryPolicy"/> instance for the current request.
        /// </summary>
        protected IRetryPolicy RetryPolicy => _policy.Value;

        /// <summary>
        /// Creates a new instance of the <see cref="AzureRequestContext"/> class.
        /// </summary>
        /// <param name="options">
        /// The <see cref="AzureRequestOptions"/> for the request.
        /// </param>
        /// <returns>An <see cref="AzureRequestContext"/> object.</returns>
        public static AzureRequestContext Create(AzureRequestOptions options)
        {
            var resolvers = new ServiceResolver[]
            {
                new DependencyResolver(typeof(AzureRequestOptions), options), 
            };
            if (!ServiceLocator.TryGetInstance<AzureRequestContext>(out var context, resolvers))
            {
                context = new AzureRequestContext(options);
            }
            return context;
        }

        /// <summary>
        /// Sets the retry handling to the default status for a given <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="statusCode">The status code to set to the default.</param>
        public void DefaultHttpStatus(HttpStatusCode statusCode)
        {
            var code = ((int)statusCode);
            _excluded.Remove(code);
            _included.Remove(code);
        }

        /// <summary>
        /// Excludes an HTTP status code from the list of return values that are eligible for 
        /// retries after a failed request.
        /// </summary>
        /// <param name="statusCode">The status code to exclude.</param>
        public void ExcludeHttpStatus(HttpStatusCode statusCode)
        {
            var code = ((int)statusCode);
            _excluded.Add(code);
            _included.Remove(code);
        }

        /// <summary>
        /// Includes an HTTP status code in the list of return values that are eligible for retries
        /// after a failed request.
        /// </summary>
        /// <param name="statusCode">The status code to include.</param>
        public void IncludeHttpStatus(HttpStatusCode statusCode)
        {
            var code = ((int)statusCode);
            _included.Add(code);
            _excluded.Remove(code);
        }

        /// <summary>
        /// Determines whether or not the request can be retried after a failed attempt.
        /// </summary>
        /// <param name="ex">
        /// The <see cref="Exception"/> that was generated from the last failed attempt.
        /// </param>
        /// <returns>True if the operation can be retried; otherwise false.</returns>
        public bool ShouldRetry(Exception ex)
        {
            if (this.ShouldRetry(ex, out var interval))
            {
                this.RetryCount++;
                Thread.Sleep(interval);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether or not the request can be retried after a failed attempt.
        /// </summary>
        /// <param name="ex">
        /// The <see cref="Exception"/> that was generated from the last failed attempt.
        /// </param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> object to monitor for task cancellation requests.
        /// </param>
        /// <returns>True if the operation can be retried; otherwise false.</returns>
        public async Task<bool> ShouldRetryAsync(Exception ex, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this.ShouldRetry(ex, out var interval))
            {
                this.RetryCount++;
                await Task.Delay(interval, cancellationToken);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether or not the request can be retried after a failed attempt.
        /// </summary>
        /// <param name="ex">
        /// The <see cref="Exception"/> that was generated from the last failed attempt.
        /// </param>
        /// <param name="interval">
        /// An  output value receives the amount of time to wait until the next retry on success.
        /// </param>
        /// <returns>True if the operation can be retried; otherwise false.</returns>
        protected virtual bool ShouldRetry(Exception ex, out TimeSpan interval)
        {
            interval = TimeSpan.Zero;
            if (ex is StorageException storage)
            {
                var statusCode = storage.RequestInformation.HttpStatusCode;
                if (_excluded.Contains(statusCode))
                {
                    return false;   // if the code has been excluded, just exit immediately
                }
                if (_included.Contains(statusCode))
                {
                    // if the caller marked a status code as eligible for retries, then we need to
                    // change it to a value guaraneed to be ignored by the retry policy so that it
                    // will be included.
                    statusCode = ((int)HttpStatusCode.RequestTimeout);
                }
                return ((this.RetryPolicy != null) && this.RetryPolicy.ShouldRetry(this.RetryCount, statusCode, ex, out interval, this.OperationContext));
            }
            return false;
        }
    }
}
