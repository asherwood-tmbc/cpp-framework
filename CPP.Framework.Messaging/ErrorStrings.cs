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
        /// <summary>
        /// Unable to send the requested email message to the mail server.
        /// </summary>
        internal const string MailTransportFailure = "Unable to send the requested email message to the mail server.";
    }
}
