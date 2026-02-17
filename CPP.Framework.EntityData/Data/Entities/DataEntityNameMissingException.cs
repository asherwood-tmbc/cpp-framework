using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Thrown when a <see cref="DataEntityAttribute"/> has not been applied to a target entity.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DataEntityNameMissingException : DataEntityException
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="type">The type of the target entity.</param>
        protected internal DataEntityNameMissingException(Type type) : this(type, null) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the target entity.</param>
        /// <param name="innerException">The <see cref="Exception"/> object that caused the current exception.</param>
        protected internal DataEntityNameMissingException(Type entityType, Exception innerException)
            : base(entityType, FormatMessage(entityType), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected DataEntityNameMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Formats an exception message for a given entity.
        /// </summary>
        /// <param name="type">The type of the target entity.</param>
        /// <returns>A string that contains the formatted error message.</returns>
        private static string FormatMessage(Type type)
        {
            type = (type ?? typeof(Object));
            return String.Format(ErrorStrings.MissingEntityName, type.Name);
        }
    }

    /// <summary>
    /// Thrown when a <see cref="DataEntityAttribute"/> has not been applied to a target entity.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DataEntityNameMissingException<TEntity> :
        DataEntityNameMissingException
        where TEntity : class
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        protected internal DataEntityNameMissingException() : base(typeof(TEntity)) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="innerException">The <see cref="Exception"/> object that caused the current exception.</param>
        protected internal DataEntityNameMissingException(Exception innerException) : base(typeof(TEntity), innerException) { }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected internal DataEntityNameMissingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
