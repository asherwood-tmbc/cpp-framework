using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Thrown when an entity relationship definition cannot be found for a navigation property.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DataEntityRelationNotFoundException : InvalidDataEntityRelationException
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="propertyName">The name of the invalid navigation property.</param>
        public DataEntityRelationNotFoundException(Type entityType, string propertyName) : this(entityType, propertyName, null) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="propertyName">The name of the invalid navigation property.</param>
        /// <param name="innerException">The exception that generated the current exception.</param>
        public DataEntityRelationNotFoundException(Type entityType, string propertyName, Exception innerException)
            : base(entityType, propertyName, FormatMessage(entityType, propertyName), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected DataEntityRelationNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Formats the error message for the exception.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="propertyName">The name of the invalid navigation property.</param>
        /// <returns>The error message for the exception.</returns>
        private static string FormatMessage(Type entityType, string propertyName)
        {
            return String.Format(ErrorStrings.DataEntityRelationNotFound, entityType, propertyName);
        }
    }
}
