using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Thrown when an entity type definition is not valid.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidDataEntityException : DataEntityException
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        public InvalidDataEntityException(Type entityType) : this(entityType, null) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="innerException">The exception that generated the current exception.</param>
        protected InvalidDataEntityException(Type entityType, Exception innerException)
            : base(entityType, FormatMessage(entityType), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected InvalidDataEntityException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Formats an exception message for a given entity.
        /// </summary>
        /// <param name="entityType">The type of the target entity.</param>
        /// <returns>A string that contains the formatted error message.</returns>
        private static string FormatMessage(Type entityType)
        {
            entityType = (entityType ?? typeof(Object));
            return String.Format(ErrorStrings.InvalidDataEntityType, entityType.FullName);
        }
    }

    /// <summary>
    /// Thrown when an entity type definition is not valid.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidDataEntityException<TEntity> :
        InvalidDataEntityException
        where TEntity : class
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        public InvalidDataEntityException() : base(typeof(TEntity)) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="innerException">The exception that generated the current exception.</param>
        protected InvalidDataEntityException(Exception innerException) : base(typeof(TEntity), innerException) { }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected InvalidDataEntityException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
