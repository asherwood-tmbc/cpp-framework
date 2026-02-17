using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace CPP.Framework.WindowsAzure.Storage.Policies
{
    /// <summary>
    /// Represents a retry policy that specifies no retries should be performed.
    /// </summary>
    internal sealed class NoRetryPolicy : AzureRetryPolicy
    {
        private readonly IRetryPolicy _StoragePolicy = new NoRetry();

        /// <summary>
        /// Creates an instance of the <see cref="IRetryPolicy"/> for the current object.
        /// </summary>
        /// <returns>An <see cref="IRetryPolicy"/> object.</returns>
        protected internal override IRetryPolicy GetPolicy() { return _StoragePolicy; }
    }
}
