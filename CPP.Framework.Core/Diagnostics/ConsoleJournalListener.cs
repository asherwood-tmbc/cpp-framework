using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Journal listener used to write messages to the console output
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConsoleJournalListener : IJournalListener
    {
        /// <summary>
        /// Writes a message to the underlying storage location.
        /// </summary>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="source">The name of the source that generated the message.</param>
        /// <param name="message">The message to write.</param>
        public void Write(JournalSeverity severity, Guid source, string message) { Console.WriteLine(message); }
    }
}
