using System;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Abstract interfaces for classes that listen for <see cref="Journal"/> trace messages to
    /// persists them to a storage location.
    /// </summary>
    public interface IJournalListener
    {
        /// <summary>
        /// Writes a message to the underlying storage location.
        /// </summary>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="source">The id of the source that generated the message.</param>
        /// <param name="message">The message to write.</param>
        void Write(JournalSeverity severity, Guid source, string message);
    }
}
