using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// Contains a sequence of flags that contain the current support status for module features 
    /// that have been marked as deprecated.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class MigrationCategory
    {
        /// <summary>
        /// Create A Project Controller/API Methods.
        /// </summary>
        public const bool CreateProjectFlow = false;

        /// <summary>
        /// Entity Framework Action Methods
        /// </summary>
        public const bool EFServiceDataActions = false;

        /// <summary>
        /// OData Web Service Client Methods
        /// </summary>
        public const bool WCFDataClientActions = true;
    }
}
