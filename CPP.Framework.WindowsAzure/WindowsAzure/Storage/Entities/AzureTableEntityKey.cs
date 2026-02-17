using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.WindowsAzure.Storage.Entities
{
    /// <summary>
    /// Abstract base class for all objects that are used to generate unique keys for 
    /// <see cref="AzureTableEntity"/> objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class AzureTableEntityKey
    {
        /// <summary>
        /// Called by the framework to generate the partition key for the entity.
        /// </summary>
        /// <returns>A string that contains a new partition key.</returns>
        protected internal virtual string GeneratePartitionKey() => default(string);

        /// <summary>
        /// Called by the framework to generate the row key value for the entity.
        /// </summary>
        /// <returns>A string that contains the row key value.</returns>
        protected internal virtual string GenerateRowKey() => default(string);
    }
}
