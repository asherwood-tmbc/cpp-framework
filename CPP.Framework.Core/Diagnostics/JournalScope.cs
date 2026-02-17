using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Manages and tracks the information associated with the logical execution context of the 
    /// caller for the <see cref="Journal"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class JournalScope : MarshalByRefObject, ISerializable   // this class has to be serializable to be usable with CallContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JournalScope"/> class. 
        /// </summary>
        public JournalScope()
        {
            this.ID = GuidGeneratorService.Current.NewGuid(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JournalScope"/> class. 
        /// </summary>
        /// <param name="scopeId">The unique id to assign to the scope.</param>
        protected internal JournalScope(Guid scopeId)
        {
            this.ID = scopeId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JournalScope"/> class from a serialization 
        /// stream.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> object that contains the class data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> object for the serialization stream.</param>
        protected JournalScope(SerializationInfo info, StreamingContext context)
        {
            this.ID = ((Guid)info.GetValue("ID", typeof(Guid)));
        }

        /// <summary>
        /// Gets the unique of the scope.
        /// </summary>
        public Guid ID { get; }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the 
        /// target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="StreamingContext"/>) for this serialization.</param>
        /// <exception cref="SecurityException">The caller does not have the required permission.</exception>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ID", this.ID, typeof(Guid));
        }
    }
}
