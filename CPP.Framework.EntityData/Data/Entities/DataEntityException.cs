using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Abstract base class for all data entity-based exceptions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public abstract class DataEntityException : Exception
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the entity that caused the exception.</param>
        /// <param name="message">An error message that describes the reason for the exception.</param>
        protected DataEntityException(Type entityType, string message) : this(entityType, message, null) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the entity that caused the exception.</param>
        /// <param name="message">An error message that describes the reason for the exception.</param>
        /// <param name="innerException">An <see cref="Exception"/> instance that caused the current exception.</param>
        protected DataEntityException(Type entityType, string message, Exception innerException)
            : base(message, innerException)
        {
            this.EntityType = entityType;
        }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected DataEntityException(SerializationInfo info, StreamingContext context)
        {
            info.LoadProperty(this, (obj => obj.EntityType), typeof(object));
        }

        /// <summary>
        /// Gets the type of the entity that is the target of the action.
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with 
        /// information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is a null reference (Nothing in Visual Basic).</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SaveProperty(this, (obj => obj.EntityType));
            base.GetObjectData(info, context);
        }
    }
}
