using System.Collections.Generic;

namespace CPP.Framework.Collections
{
    /// <summary>
    /// Abstract interface for types that wrap an <see cref="ICollection{T}"/> so that its
    /// elements can be treated as contravariant.
    /// </summary>
    public interface IContraVariantCollection : ICollection<object>
    {
    }
}
