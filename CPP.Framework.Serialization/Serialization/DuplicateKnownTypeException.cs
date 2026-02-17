using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;

namespace CPP.Framework.Serialization
{
    /// <summary>
    /// Thrown when a known type is encountered that has the same indicator property as an existing
    /// known type.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DuplicateKnownTypeException : InvalidKnownTypeException
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="typeInfo">The invalid type.</param>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> of the indicator property for <paramref name="typeInfo"/>.</param>
        public DuplicateKnownTypeException(Type typeInfo, PropertyInfo propertyInfo)
            : base(typeInfo, FormatMessage(typeInfo, propertyInfo), null) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="typeInfo">The invalid type.</param>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> of the indicator property for <paramref name="typeInfo"/>.</param>
        /// <param name="innerException">The exception that generated the current exception.</param>
        public DuplicateKnownTypeException(Type typeInfo, PropertyInfo propertyInfo, Exception innerException)
            : base(typeInfo, FormatMessage(typeInfo, propertyInfo), innerException) { }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected DuplicateKnownTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Formats an exception message for a given type.
        /// </summary>
        /// <param name="typeInfo">The invalid type.</param>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> of the indicator property for <paramref name="typeInfo"/>.</param>
        /// <returns>A string that contains the formatted error message.</returns>
        private static string FormatMessage(Type typeInfo, PropertyInfo propertyInfo)
        {
            typeInfo = (typeInfo ?? typeof(Object));
            return String.Format(ErrorStrings.DuplicateKnownType, typeInfo.FullName, propertyInfo.Name);
        }
    }
}
