using System;

namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Applied to a IDataEntity class or interface to indicate whether or the entity
    /// should automatically be assigned an new ID value when new instances are created through the
    /// data source context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public sealed class AutoGenerateKeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoGenerateKeyAttribute"/> class.
        /// </summary>
        public AutoGenerateKeyAttribute() : this(true) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoGenerateKeyAttribute"/> class.
        /// </summary>
        /// <param name="sequential">
        /// True to generate key values using a sequential algorithm (i.e. using NEWSEQUENTIALID); 
        /// otherwise, false to use the legacy algorithm (i.e. NEWID) to generate the key values. 
        /// Please note that this option is only provide for compatibility reasons, and passing 
        /// a value of false can lead to increased page splits in the database, resulting in high 
        /// fragmentation if the column is part of the clustered index for the table.
        /// </param>
        public AutoGenerateKeyAttribute(bool sequential) => this.Sequential = sequential;

        /// <summary>
        /// Gets a value indicating whether or not to generate key values using a sequential
        /// algorithm.
        /// </summary>
        public bool Sequential { get; }
    }
}
