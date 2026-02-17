using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Serialization
{
    /// <summary>
    /// Thrown when a <see cref="JsonKnownTypeConverter{TBase}"/> encounters an invalid known type
    /// defininition.
    /// /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidKnownTypeException : Exception
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="typeInfo">The invalid type.</param>
        public InvalidKnownTypeException(Type typeInfo) : this(typeInfo, null) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="typeInfo">The invalid type.</param>
        /// <param name="innerException">The exception that generated the current exception.</param>
        public InvalidKnownTypeException(Type typeInfo, Exception innerException) : this(typeInfo, FormatMessage(typeInfo), innerException) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="typeInfo">The invalid type.</param>
        /// <param name="message">The error message for the exception.</param>
        /// <param name="innerException">The exception that generated the current exception.</param>
        protected InvalidKnownTypeException(Type typeInfo, string message, Exception innerException)
            : base(message, innerException)
        {
            this.KnownType = typeInfo;
        }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected InvalidKnownTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Gets the <see cref="Type"/> object for the known type that caused the exception.
        /// </summary>
        public Type KnownType { get; private set; }

        /// <summary>
        /// Formats an exception message for a given type.
        /// </summary>
        /// <param name="typeInfo">The invalid type.</param>
        /// <returns>A string that contains the formatted error message.</returns>
        private static string FormatMessage(Type typeInfo)
        {
            typeInfo = (typeInfo ?? typeof(Object));
            return String.Format(ErrorStrings.InvalidKnownType, typeInfo.FullName);
        }
    }
}
