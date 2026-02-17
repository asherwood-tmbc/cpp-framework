using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework
{
    /// <summary>
    /// Defines the error strings for the library.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class EntityDataErrorStrings
    {
        /// <summary>
        /// The action "{0}" is not bound to entity "{1}" or is not defined.
        /// </summary>
        internal const string DataEntityActionNotFound = "The action \"{0}\" is not bound to entity \"{1}\" or is not defined.";

        /// <summary>
        /// An entity of type "{0}" with an id of "{1}" was not found in the data source.
        /// </summary>
        internal const string DataEntityNotFound = "An entity of type \"{0}\" with an id of \"{1}\" was not found in the data source.";

        /// <summary>
        /// The navigation property "{0}.{1}" does not have an entity relationship defined.
        /// </summary>
        internal const string DataEntityRelationNotFound = "The navigation property \"{0}.{1}\" does not have an entity relationship defined.";

        /// <summary>
        /// The global context action "{0}" was not found in the data source.
        /// </summary>
        internal const string DataSourceActionNotFound = "The global context action \"{0}\" was not found in the data source.";

        /// <summary>
        /// Class "{0}" is not a valid data object type.
        /// </summary>
        internal const string InvalidDataObjectType = "Class \"{0}\" is not a valid data object type.";

        /// <summary>
        /// The entity relationship definition for the "{0}.{1}" navigation property is not valid.
        /// </summary>
        internal const string InvalidDataEntityRelation = "The relationship definition for the \"{0}.{1}\" navigation property is not valid.";

        /// <summary>
        /// Class "{0}" is not a valid data entity type.
        /// </summary>
        internal const string InvalidDataEntityType = "Class \"{0}\" is not a valid data entity type.";

        /// <summary>
        /// The source context type does not reference a valid {0}.
        /// </summary>
        internal const string InvalidDataSourceContextType = "The source context type does not reference a valid {0}.";

        /// <summary>
        /// The underlying data source context does not support executing dynamic actions.
        /// </summary>
        internal const string InvalidOrMissingActionDispatcher = "The underlying data source context does not support executing dynamic actions.";

        /// <summary>
        /// The type "{0}" is not a valid entity, or does not have a name defined.
        /// </summary>
        internal const string MissingEntityName = "The type \"{0}\" is not a valid entity, or does not have a name defined.";
    }
}
