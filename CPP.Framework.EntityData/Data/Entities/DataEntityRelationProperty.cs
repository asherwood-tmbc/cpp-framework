using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Represents the relationship for one end of a <see cref="DataEntityRelation"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class DataEntityRelationProperty
    {
        /// <summary>
        /// Gets the multiplicity of the source (internal) end of the relationship.
        /// </summary>
        public Multiplicity Multiplicity { get; internal set; }

        /// <summary>
        /// Gets the <see cref="PropertyInfo"/> for the source navigation property.
        /// </summary>
        public PropertyInfo PropertyInfo { get; internal set; }
    }
}
