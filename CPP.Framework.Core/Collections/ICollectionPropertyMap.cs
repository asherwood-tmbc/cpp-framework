using System.Collections.Generic;

namespace CPP.Framework.Collections
{
    /// <summary>
    /// Abstract interface for types that automatically map class properties that return 
    /// <see cref="ICollection{T}"/> compatible types to their corresponding property names using 
    /// reflection. Please note that only collection properties for reference types are mapped; 
    /// collections of value types are not supported.
    /// </summary>
    public interface ICollectionPropertyMap
    {
        /// <summary>
        /// Gets the <see cref="IContraVariantCollection"/> for a given property name.
        /// </summary>
        /// <param name="propertyName">The name of the collection property.</param>
        /// <returns>An <see cref="IContraVariantCollection"/> instance.</returns>
        IContraVariantCollection this[string propertyName] { get; }
    }
}
