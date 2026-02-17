using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.IO
{
    /// <summary>
    /// Exception that is thrown when the contents of an input stream are not in the expected 
    /// format when being read.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidStreamFormatException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStreamFormatException"/> class. 
        /// </summary>
        public InvalidStreamFormatException() : base(ErrorStrings.InvalidStreamContentFormat) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStreamFormatException"/> class. 
        /// </summary>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        public InvalidStreamFormatException(Exception innerException) : base(ErrorStrings.InvalidStreamContentFormat, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidStreamFormatException"/> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected InvalidStreamFormatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
