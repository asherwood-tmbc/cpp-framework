using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Applied to a navigation property with a multiplicity of one to indicate the name of the 
    /// local class property that represents the value of the foreign key for the relationship.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [ExcludeFromCodeCoverage]
    public class DataEntityForeignKeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntityForeignKeyAttribute"/> class. 
        /// </summary>
        /// <param name="propertyName">
        /// The name of the foreign key property on the current class.
        /// </param>
        public DataEntityForeignKeyAttribute(string propertyName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => propertyName);
            this.PropertyName = propertyName;
        }

        /// <summary>
        /// Gets the name of the foreign key property on the current class.
        /// </summary>
        public string PropertyName { get; }
    }
}
