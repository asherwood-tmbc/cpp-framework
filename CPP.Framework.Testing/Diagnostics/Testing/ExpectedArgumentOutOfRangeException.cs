using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Diagnostics.Testing
{
    /// <summary>
    /// Applied to a test method to indicate that it is expected to throw an 
    /// <see cref="ArgumentNullException"/> with a specific parameter name.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ExpectedArgumentOutOfRangeException : ExpectedArgumentExceptionAttribute
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="paramName">The expected name of the parameter for the exception.</param>
        public ExpectedArgumentOutOfRangeException(string paramName) : base(paramName) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="paramName">The expected name of the parameter for the exception.</param>
        /// <param name="noExceptionMessage">The custom error message to display if the associated method doesn't throw the expected exception.</param>
        public ExpectedArgumentOutOfRangeException(string paramName, string noExceptionMessage) : base(paramName, noExceptionMessage) { }

        /// <summary>
        /// Gets the expected type of the exception.
        /// </summary>
        public override Type ExceptionType { get { return typeof(ArgumentOutOfRangeException); } }
    }
}
