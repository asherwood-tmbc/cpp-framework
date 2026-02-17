using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.WindowsAzure
{
    /// <summary>
    /// Applied to a message model class to indicate the default storage account to use when 
    /// sending messages through Azure Service Bus or Azure Storage.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [ExcludeFromCodeCoverage]
    public sealed class AzureAccountNameAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AzureAccountNameAttribute"/> class.
        /// </summary>
        /// <param name="accountName">The name of the storage account.</param>
        public AzureAccountNameAttribute(string accountName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => accountName);
            this.AccountName = accountName;
        }

        /// <summary>
        /// Gets the name of the storage account.
        /// </summary>
        public string AccountName { get; }
    }
}
