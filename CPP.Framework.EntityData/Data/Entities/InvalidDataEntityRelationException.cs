using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Thrown when the entity relationship definition for a navigation property is not valid.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidDataEntityRelationException : DataEntityException
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="propertyName">The name of the invalid navigation property.</param>
        public InvalidDataEntityRelationException(Type entityType, string propertyName)
            : this(entityType, propertyName, null)
        {
        }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="propertyName">The name of the invalid navigation property.</param>
        /// <param name="innerException">The exception that generated the current exception.</param>
        public InvalidDataEntityRelationException(Type entityType, string propertyName, Exception innerException)
            : this(entityType, propertyName, FormatMessage(entityType, propertyName), innerException)
        {
        }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="propertyName">The name of the invalid navigation property.</param>
        /// <param name="message">The error message for the exception.</param>
        /// <param name="innerException">The exception that generated the current exception.</param>
        protected InvalidDataEntityRelationException(Type entityType, string propertyName, string message, Exception innerException)
            : base(entityType, message, innerException)
        {
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected InvalidDataEntityRelationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            info.LoadProperty(this, (x => x.PropertyName));
        }

        /// <summary>
        /// Gets the name of the invalid navigation property.
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// Formats the error message for the exception.
        /// </summary>
        /// <param name="entityType">The type of the entity.</param>
        /// <param name="propertyName">The name of the invalid navigation property.</param>
        /// <returns>The error message for the exception.</returns>
        private static string FormatMessage(Type entityType, string propertyName)
        {
            return String.Format(ErrorStrings.InvalidDataEntityRelation, entityType, propertyName);
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with 
        /// information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic).</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SaveProperty(this, (x => x.PropertyName));
            base.GetObjectData(info, context);
        }
    }
}
