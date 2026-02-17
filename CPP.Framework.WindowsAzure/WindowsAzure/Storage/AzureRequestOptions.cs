using System;
using CPP.Framework.WindowsAzure.Storage.Policies;
using Microsoft.WindowsAzure.Storage;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Defines the available options for a server request against an Azure Storage Object.
    /// </summary>
    public sealed class AzureRequestOptions
    {
        /// <summary>
        /// Defines the default server request options.
        /// </summary>
        public static readonly AzureRequestOptions Default = new AzureRequestOptions
        {
            MaximumExecutionTime = null,
            RetryPolicy = ExponentialRetryPolicy.Default,
            ServerTimeout = null,
        };

        /// <summary>
        /// Gets or sets the maximum amount of time to wait for the request to execute both locally
        /// and on the server. A null value means that the option isn't used and should be ignored.
        /// </summary>
        public TimeSpan? MaximumExecutionTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="AzureRetryPolicy"/> object for the request. A null value is
        /// equivalent to specifying <see cref="AzureRetryPolicy.None"/>.
        /// </summary>
        public AzureRetryPolicy RetryPolicy { get; set; }

        /// <summary>
        /// Gets or sets the maximum amount of time to wait for a response from the server. A null 
        /// value means that the option isn't used and should be ignored.
        /// </summary>
        public TimeSpan? ServerTimeout { get; set; }
    }

    /// <summary>
    /// Extension methods for the <see cref="AzureRequestOptions"/> class.
    /// </summary>
    internal static class AzureRequestOptionsExtensions
    {
        /// <summary>
        /// Creates a copy of an exising <see cref="AzureRequestOptions"/> instance.
        /// </summary>
        /// <param name="source">The <see cref="AzureRequestOptions"/> object to copy the values from.</param>
        /// <returns>An <see cref="AzureRequestOptions"/> object.</returns>
        internal static AzureRequestOptions Clone(this AzureRequestOptions source)
        {
            ArgumentValidator.ValidateThisObj(() => source);
            var clone = new AzureRequestOptions
            {
                MaximumExecutionTime = source.MaximumExecutionTime,
                RetryPolicy = source.RetryPolicy,
                ServerTimeout = source.ServerTimeout,
            };
            return clone;
        }

        /// <summary>
        /// Creates a new Azure Storage <see cref="IRequestOptions"/> object that is based on the
        /// current values of an <see cref="AzureRequestOptions"/> object.
        /// </summary>
        /// <typeparam name="TOptions">The type of the options object.</typeparam>
        /// <param name="source">The <see cref="AzureRequestOptions"/> object to copy the values from.</param>
        /// <returns>An <see cref="IRequestOptions"/> object of the requested type.</returns>
        internal static TOptions CreateOptions<TOptions>(this AzureRequestOptions source)
            where TOptions : IRequestOptions, new()
        {
            var options = new TOptions
            {
                MaximumExecutionTime = null,
                RetryPolicy = AzureRetryPolicy.None.GetPolicy(),
                ServerTimeout = null,
            };
            if (source != null)
            {
                options.MaximumExecutionTime = source.MaximumExecutionTime;
                options.RetryPolicy = (source.RetryPolicy ?? AzureRetryPolicy.None).GetPolicy();
                options.ServerTimeout = source.ServerTimeout;
            }
            return options;
        }
    }
}
