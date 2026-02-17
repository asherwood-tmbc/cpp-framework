using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Serialization
{
    /// <summary>
    /// Attribute that is applied to a property to indicate that it the identifying property for a
    /// derived type during deserialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    [ExcludeFromCodeCoverage]
    public class JsonKnownTypeIndicatorAttribute : Attribute { }
}
