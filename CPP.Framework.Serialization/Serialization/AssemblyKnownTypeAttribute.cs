using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Serialization
{
    /// <summary>
    /// Applied to an assembly to specify a serializable type that may be a known type for a parent
    /// class in the object's inheritance tree. Specifically, this attribute has the same purpose 
    /// as the <see cref="KnownTypeAttribute"/>, but at an assembly level, rather than at the class
    /// level, and only for types serialized using the <see cref="JsonKnownTypeResolver"/>, such as
    /// the <see cref="JsonKnownTypeConverter{TBase}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    [ExcludeFromCodeCoverage]
    public sealed class AssemblyKnownTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyKnownTypeAttribute"/> class. 
        /// </summary>
        /// <param name="type">
        /// The type to include the parent's classes known type list.
        /// </param>
        public AssemblyKnownTypeAttribute(Type type) => this.Type = type;

        /// <summary>
        /// Gets the type to include the parent's classes known type list.
        /// </summary>
        public Type Type { get; }
    }
}
