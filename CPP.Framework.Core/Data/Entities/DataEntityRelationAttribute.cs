using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Data.Entities
{
    #region Multiplicity Enumeration Declaration

    /// <summary>
    /// Available multiplicity values for one side of an entity relationship.
    /// </summary>
    public enum Multiplicity
    {
        /// <summary>
        /// Multiple Records
        /// </summary>
        Many,

        /// <summary>
        /// One Record
        /// </summary>
        One,

        /// <summary>
        /// Zero to One Record
        /// </summary>
        Zero,

        /// <summary>
        /// Ignore Relationship (One-Way Navigation)
        /// </summary>
        Ignore,
    }

    #endregion // Multiplicity Enumeration Declaration

    /// <summary>
    /// Applied to a navigation property on an extracted entity interface to indicate that an 
    /// relationship exists with another entity interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [ExcludeFromCodeCoverage]
    public class DataEntityRelationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntityRelationAttribute"/> class. 
        /// </summary>
        /// <param name="sourceRelationType">
        /// The multiplicity of the source (internal) end of the relationship.
        /// </param>
        /// <param name="targetRelationType">
        /// The multiplicity of the target (external) end of the relationship.
        /// </param>
        public DataEntityRelationAttribute(Multiplicity sourceRelationType, Multiplicity targetRelationType)
        {
            this.PropertyName = string.Empty;
            this.SourceRelationType = sourceRelationType;
            this.TargetRelationType = targetRelationType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntityRelationAttribute"/> class. 
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="sourceRelationType">
        /// The multiplicity of the source (internal) end of the relationship.
        /// </param>
        /// <param name="targetRelationType">
        /// The multiplicity of the target (external) end of the relationship.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property on the target entity that relates back to the current entity.
        /// </param>
        public DataEntityRelationAttribute(Multiplicity sourceRelationType, Multiplicity targetRelationType, string propertyName)
        {
            this.PropertyName = propertyName;
            this.SourceRelationType = sourceRelationType;
            this.TargetRelationType = targetRelationType;
        }

        /// <summary>
        /// Gets the name of the property on the target entity that relates back to the current 
        /// entity.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the multiplicity of the source (internal) end of the relationship.
        /// </summary>
        public Multiplicity SourceRelationType { get; }

        /// <summary>
        /// Gets the multiplicity of the source (external) end of the relationship.
        /// </summary>
        public Multiplicity TargetRelationType { get; }

        /// <summary>
        /// Gets a flags that indicates whether or not to use the <see cref="PropertyName"/> value
        /// when looking up the target property (true), or use the default applied to the entity via
        /// the DataEntityAttribute (false).
        /// </summary>
        public bool UsePropertyName => (!string.IsNullOrWhiteSpace(this.PropertyName));
    }
}
