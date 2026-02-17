using System;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace CPP.Framework.WindowsAzure.Storage.Policies
{
    /// <summary>
    /// Defines a policy that specifies a linear back-off period that is added to the wait interval 
    /// between retries.
    /// </summary>
    public sealed class LinearRetryPolicy : AzureRetryPolicy
    {
        /// <summary>
        /// Gets a <see cref="LinearRetryPolicy"/> object that has been initialized with the 
        /// default values of a 30 second back-off interval, and a maximum of 3 attempts.
        /// </summary>
        public static readonly LinearRetryPolicy Default = new LinearRetryPolicy();

        private readonly LinearRetry _StoragePolicy;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        private LinearRetryPolicy() { _StoragePolicy = new LinearRetry(); }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="backoff">The back-off interval to add between retries, in milliseconds.</param>
        /// <param name="maxAttempts">The maximum number of retry attempts.</param>
        public LinearRetryPolicy(int backoff, int maxAttempts)
        {
            _StoragePolicy = new LinearRetry(TimeSpan.FromMilliseconds(backoff), maxAttempts);
        }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="backoff">The back-off interval to add between retries.</param>
        /// <param name="maxAttempts">The maximum number of retry attempts.</param>
        public LinearRetryPolicy(TimeSpan backoff, int maxAttempts)
        {
            _StoragePolicy = new LinearRetry(backoff, maxAttempts);
        }

        /// <summary>
        /// Creates an instance of the <see cref="IRetryPolicy"/> for the current object.
        /// </summary>
        /// <returns>An <see cref="IRetryPolicy"/> object.</returns>
        protected internal override IRetryPolicy GetPolicy() { return _StoragePolicy; }
    }
}
