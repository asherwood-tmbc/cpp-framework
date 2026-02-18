using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Diagnostics.Testing
{
    /// <summary>
    /// Applied to a test method to indicate that it's expected to throw an 
    /// <see cref="ArgumentException"/> with a specific parameter name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    [ExcludeFromCodeCoverage]
    public class ExpectedArgumentExceptionAttribute : ExpectedExceptionBaseAttribute
    {
        private readonly string _NoExceptionMessage;

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="paramName">The expected name of the parameter for the exception.</param>
        public ExpectedArgumentExceptionAttribute(string paramName) : this(paramName, null) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="paramName">The expected name of the parameter for the exception.</param>
        /// <param name="noExceptionMessage">The custom error message to display if the associated method doesn't throw the expected exception.</param>
        public ExpectedArgumentExceptionAttribute(string paramName, string noExceptionMessage)
            : base(noExceptionMessage)
        {
            this.ParamName = paramName;
            _NoExceptionMessage = (noExceptionMessage ?? String.Empty).Trim();
        }

        /// <summary>
        /// Gets the expected type of the exception.
        /// </summary>
        public virtual Type ExceptionType { get { return typeof(ArgumentException); } }

        /// <summary>
        /// Gets the custom error message to display if the associated method doesn't throw the
        /// expected exception.
        /// </summary>
        protected override string NoExceptionMessage
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(_NoExceptionMessage))
                {
                    return _NoExceptionMessage;
                }
                return base.NoExceptionMessage;
            }
        }

        /// <summary>
        /// Gets the expected name of the parameter associated with the exception.
        /// </summary>
        public string ParamName { get; private set; }

        /// <summary>
        /// Throws an <see cref="AssertFailedException"/> that indicates the verification failed.
        /// </summary>
        /// <param name="format">The format string for the exception message.</param>
        /// <param name="additionalArgs">One or more addition arguments to append to the assert message, which are replaced starting at index 5.</param>
        protected void ThrowAssertFailure(string format, params object[] additionalArgs)
        {
            var testFrame = new System.Diagnostics.StackTrace().GetFrames()
                ?.FirstOrDefault(f => f.GetMethod()
                    ?.GetCustomAttributes(typeof(TestMethodAttribute), true).Any() == true);
            var customExceptionMessage = (String.IsNullOrEmpty(_NoExceptionMessage)
                ? (_NoExceptionMessage)
                : (" " + _NoExceptionMessage));
            var arguments = Enumerable.Empty<object>()
                .Concat(new object[]
                {
                    (testFrame?.GetMethod()?.DeclaringType?.FullName ?? "???"),
                    (testFrame?.GetMethod()?.Name ?? "???"),
                    this.ExceptionType.FullName,
                    this.ParamName,
                    customExceptionMessage,
                })
                .Concat(additionalArgs);
            var message = String.Format(format, arguments.ToArray());
            throw new AssertFailedException(message);
        }

        /// <summary>
        /// Verifies that an exception thrown from a test matches the expected type and parameter
        /// name.
        /// </summary>
        /// <param name="exception">The exception that is thrown by the unit test.</param>
        protected override void Verify(Exception exception)
        {
            var candidate = (exception as ArgumentException);
            if ((candidate != null) && (exception.GetType() == this.ExceptionType))
            {
                if (candidate.ParamName == this.ParamName)
                {
                    return;
                }
                this.ThrowAssertFailure(ErrorStrings.Diagnostics.Testing.InvalidArgumentExceptionParam);
            }
            this.RethrowIfAssertException(exception);
            this.ThrowAssertFailure(ErrorStrings.Diagnostics.Testing.NotArgumentException);
        }
    }
}
