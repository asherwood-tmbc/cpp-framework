using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Applied to an entity class to mark it as consumable by a data source context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    [ExcludeFromCodeCoverage]
    public sealed class DataEntityAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntityAttribute"/> class. 
        /// </summary>
        /// <param name="entityName">
        /// The name of the entity within the data source.
        /// </param>
        /// <param name="entitySetName">
        /// The name of the entity sets within the data source.
        /// </param>
        /// <param name="entitySchema">
        /// The name of the schema that contains the entity within the data source.
        /// </param>
        public DataEntityAttribute(string entityName, string entitySetName, string entitySchema)
            : this(entityName, entitySetName, entitySchema, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntityAttribute"/> class. 
        /// </summary>
        /// <param name="entityName">
        /// The name of the entity within the data source.
        /// </param>
        /// <param name="entitySetName">
        /// The name of the entity sets within the data source.
        /// </param>
        /// <param name="entitySchema">
        /// The name of the schema that contains the entity within the data source.
        /// </param>
        /// <param name="entityBaseType">
        /// The base entity interface <see cref="Type"/> from which the current type inherits when 
        /// participating as a child entity for a TPT (Table-Per-Type) definition.
        /// </param>
        public DataEntityAttribute(string entityName, string entitySetName, string entitySchema, Type entityBaseType)
        {
            this.EntityBaseType = entityBaseType;
            this.EntityName = entityName;
            this.EntitySchema = entitySchema;
            this.EntitySetName = entitySetName;
        }

        /// <summary>
        /// Gets the base entity interface <see cref="Type"/> from which the current type inherits 
        /// when participating as a child entity for a TPT (Table-Per-Type) definition.
        /// </summary>
        public Type EntityBaseType { get; }

        /// <summary>
        /// Gets the name of the entity.
        /// </summary>
        public string EntityName { get; }

        /// <summary>
        /// Gets the name of the sets that contain collections of the entity.
        /// </summary>
        public string EntitySetName { get; }

        /// <summary>
        /// Gets the name of the data source schema where the entity is defined.
        /// </summary>
        public string EntitySchema { get; }
    }
}
