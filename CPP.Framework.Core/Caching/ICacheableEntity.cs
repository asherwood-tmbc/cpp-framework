using System;

namespace CPP.Framework.Caching
{
    /// <summary>
    /// Abstract interface for objects that can be stored in cache.
    /// </summary>
    public interface ICacheableEntity
    {
        /// <summary>
        /// Gets or the globally unique identifier of the entity.
        /// </summary>
        Guid EntityId { get; }
    }
}
