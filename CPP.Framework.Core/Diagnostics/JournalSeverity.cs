using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Defines the available severity levels for recording messages with the 
    /// <see cref="Journal"/> class.
    /// </summary>
    [Flags]
    public enum JournalSeverity
    {
        /// <summary>
        /// Logging Disabled
        /// </summary>
        Off = 0x00,

        /// <summary>
        /// Critical Application Error
        /// </summary>
        Critical = 0x01,

        /// <summary>
        /// Unexpected Fatal Error
        /// </summary>
        Error = 0x02,

        /// <summary>
        /// Warning or Non-Fatal Error
        /// </summary>
        Warning = 0x04,

        /// <summary>
        /// Informational or Status Message
        /// </summary>
        Information = 0x08,

        /// <summary>
        /// Debug Message
        /// </summary>
        Debug = 0x10,

        /// <summary>
        /// Verbose Debug Message
        /// </summary>
        Verbose = 0x20,
    }

    /// <summary>
    /// Extension method for the <see cref="JournalSeverity"/> enumeration.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class JournalSeverityExtensions
    {
        /// <summary>
        /// Converts a <see cref="JournalSeverity"/> value to a <see cref="TraceLevel"/> value.
        /// </summary>
        /// <param name="severity">The severity level.</param>
        /// <returns>The <see cref="TraceLevel"/> value for <paramref name="severity"/>.</returns>
        public static TraceLevel AsTraceLevel(this JournalSeverity severity)
        {
            switch (severity)
            {
                case JournalSeverity.Critical:
                case JournalSeverity.Error: return TraceLevel.Error;
                case JournalSeverity.Debug: return TraceLevel.Verbose;
                case JournalSeverity.Information: return TraceLevel.Info;
                case JournalSeverity.Verbose: return TraceLevel.Verbose;
                case JournalSeverity.Warning: return TraceLevel.Warning;
            }
            return TraceLevel.Verbose;
        }
    }
}
