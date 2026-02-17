using System.Linq.Expressions;

namespace CPP.Framework.WindowsAzure.Storage.Filters
{
    /// <summary>
    /// Represents a filter condition against a Windows Azure table.
    /// </summary>
    public abstract class AzureTableFilter
    {
        /// <summary>
        /// Generates a filter string for a filter.
        /// </summary>
        /// <returns>The filter string.</returns>
        public abstract string GenerateFilterString();
    }
}
