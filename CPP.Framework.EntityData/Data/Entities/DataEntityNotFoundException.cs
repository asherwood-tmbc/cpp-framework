using System;
using System.Runtime.Serialization;

namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Thrown when an entity cannot be found within the data source.
    /// </summary>
    public abstract class DataEntityNotFoundException : DataEntityException
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the target entity.</param>
        /// <param name="entityId">The id of the target entity.</param>
        protected DataEntityNotFoundException(Type entityType, Guid entityId) : this(entityType, entityId, null) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityType">The type of the target entity.</param>
        /// <param name="entityId">The id of the target entity.</param>
        /// <param name="innerException">The <see cref="Exception"/> object that caused the current exception.</param>
        protected DataEntityNotFoundException(Type entityType, Guid entityId, Exception innerException)
            : base(entityType, FormatMessage(entityType, entityId), innerException)
        {
            this.EntityId = entityId;
        }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected DataEntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            info.LoadProperty(this, (x => x.EntityId));
        }

        /// <summary>
        /// Gets the id of the target entity.
        /// </summary>
        public Guid EntityId { get; private set; }

        /// <summary>
        /// Formats an exception message for a given entity.
        /// </summary>
        /// <param name="entityType">The type of the target entity.</param>
        /// <param name="entityId">The id of the entity being located.</param>
        /// <returns>A string that contains the formatted error message.</returns>
        private static string FormatMessage(Type entityType, Guid entityId)
        {
            entityType = (entityType ?? typeof(Object));
            return String.Format(EntityDataErrorStrings.DataEntityNotFound, entityType.Name, entityId);
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
            info.SaveProperty(this, (x => x.EntityId));
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// Thrown when an entity cannot be found within the data source.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class DataEntityNotFoundException<TEntity> :
        DataEntityNotFoundException
        where TEntity : class
    {

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityId">The id of the target entity.</param>
        public DataEntityNotFoundException(Guid entityId) : base(typeof(TEntity), entityId) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="entityId">The id of the target entity.</param>
        /// <param name="innerException">The <see cref="Exception"/> object that caused the current exception.</param>
        public DataEntityNotFoundException(Guid entityId, Exception innerException) : base(typeof(TEntity), entityId, innerException) { }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected DataEntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
