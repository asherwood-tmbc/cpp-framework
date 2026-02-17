using System;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace CPP.Framework.WindowsAzure.Storage.Policies
{
    /// <summary>
    /// Defines a policy that specifies an exponental back-off period that is added to the wait 
    /// interval between retries. The minimum wait interval starts off at 3 seconds, and increases
    /// exponentially after every retry, up to a maximum of 120 seconds (2 minutes).
    /// </summary>
    public class ExponentialRetryPolicy : AzureRetryPolicy
    {
        /// <summary>
        /// Gets a <see cref="LinearRetryPolicy"/> object that has been initialized with the 
        /// default values of a 4 second back-off interval, and a maximum of 3 attempts.
        /// </summary>
        public static readonly ExponentialRetryPolicy Default = new ExponentialRetryPolicy();

        private readonly ExponentialRetry _StoragePolicy;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        private ExponentialRetryPolicy() { _StoragePolicy = new ExponentialRetry(); }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="backoff">The back-off interval to add between retries, in milliseconds.</param>
        /// <param name="maxAttempts">The maximum number of retry attempts.</param>
        public ExponentialRetryPolicy(int backoff, int maxAttempts)
        {
            _StoragePolicy = new ExponentialRetry(TimeSpan.FromMilliseconds(backoff), maxAttempts);
        }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="backoff">The back-off interval to add between retries.</param>
        /// <param name="maxAttempts">The maximum number of retry attempts.</param>
        public ExponentialRetryPolicy(TimeSpan backoff, int maxAttempts)
        {
            _StoragePolicy = new ExponentialRetry(backoff, maxAttempts);
        }

        /// <summary>
        /// Creates an instance of the <see cref="IRetryPolicy"/> for the current object.
        /// </summary>
        /// <returns>An <see cref="IRetryPolicy"/> object.</returns>
        protected internal override IRetryPolicy GetPolicy() { return _StoragePolicy; }
    }
}
