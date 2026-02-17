using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CPP.Framework.Data.Entities
{
    /// <summary>
    /// Represents the definition of a relationship between two entities.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class DataEntityRelation
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        public DataEntityRelation()
        {
            this.Source = new DataEntityRelationProperty();
            this.Target = new DataEntityRelationProperty();
        }

        /// <summary>
        /// Gets the definition for the source (internal) end of the relationship.
        /// </summary>
        public DataEntityRelationProperty Source { get; private set; }

        /// <summary>
        /// Gets the definition for the target (external) end of the relationship.
        /// </summary>
        public DataEntityRelationProperty Target { get; private set; }
    }
}
