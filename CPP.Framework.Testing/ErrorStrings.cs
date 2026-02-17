using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework
{
    /// <summary>
    /// Contains the internal error message strings for any of the <see cref="Exception"/> objects 
    /// thrown by the current library.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class ErrorStrings
    {
        internal static class Diagnostics
        {
            internal static class Testing
            {
                /// <summary>
                /// Test method {0}.{1} did not throw exception for expected parameter {3}.{4}
                /// </summary>
                public const string InvalidArgumentExceptionParam = "Test method {0}.{1} did not throw exception for expected parameter {3}.{4}";

                /// <summary>
                /// Test method {0}.{1} did not throw expected exception {2}.{4}
                /// </summary>
                public const string NotArgumentException = "Test method {0}.{1} did not throw expected exception {2}.{4}";
            }
        }
    }
}
