using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Journal listener used to write messages to the <see cref="Trace"/> listeners collection.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TraceJournalListener : IJournalListener
    {
        /// <summary>
        /// Writes a message to the underlying storage location.
        /// </summary>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="source">The id of the source that generated the message.</param>
        /// <param name="message">The message to write.</param>
        public void Write(JournalSeverity severity, Guid source, string message)
        {
            switch (severity)
            {
                case JournalSeverity.Critical:
                case JournalSeverity.Error:
                    {
                        Trace.TraceError(message);
                    }
                    break;
                case JournalSeverity.Warning:
                    {
                        Trace.TraceWarning(message);
                    }
                    break;
                case JournalSeverity.Information:
                case JournalSeverity.Debug:
                    {
                        Trace.TraceInformation(message);
                    }
                    break;
                case JournalSeverity.Verbose:
                    {
                        Trace.WriteLine(message);
                    }
                    break;
            }
        }
    }
}
