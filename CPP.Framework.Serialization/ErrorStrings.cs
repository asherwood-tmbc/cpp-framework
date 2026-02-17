using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework
{
    /// <summary>
    /// Defines the error string for the library.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class ErrorStrings
    {
        /// <summary>
        /// The known type {0} cannot use indicator property {1} because it is already defined by an existing known type.
        /// </summary>
        public const string DuplicateKnownType = "The known type {0} cannot use indicator property {1} because it is already defined by an existing known type.";

        /// <summary>
        /// The known type {0} is invalid because it does not have an indicator property defined.
        /// </summary>
        public const string InvalidKnownType = "The known type {0} is invalid because it does not have an indicator property defined.";
    }
}
