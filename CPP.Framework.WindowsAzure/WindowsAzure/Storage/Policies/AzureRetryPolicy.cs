using System;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace CPP.Framework.WindowsAzure.Storage.Policies
{
    /// <summary>
    /// Abstract base class for all objects that specify a retry policy for an Azure Cloud Storage
    /// request.
    /// </summary>
    public abstract class AzureRetryPolicy
    {
        /// <summary>
        /// Defines a policy that specifies no retries should be performed.
        /// </summary>
        public static readonly AzureRetryPolicy None = new NoRetryPolicy();

        /// <summary>
        /// Creates an instance of the <see cref="IRetryPolicy"/> for the current object.
        /// </summary>
        /// <returns>An <see cref="IRetryPolicy"/> object.</returns>
        protected internal abstract IRetryPolicy GetPolicy();
    }
}
