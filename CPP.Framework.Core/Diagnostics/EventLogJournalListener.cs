using System;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Journal listener class used to write messages to the local system event log.
    /// </summary>
    public class EventLogJournalListener : IJournalListenerEx
    {
        /// <summary>
        /// Writes a message to the underlying storage location.
        /// </summary>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="source">The id of the source that generated the message.</param>
        /// <param name="message">The message to write.</param>
        public void Write(JournalSeverity severity, Guid source, string message)
        {
            EventLogProvider.Current.Write(severity, message);
        }

        /// <summary>
        /// Writes data for a thrown exception to the underlying storage location.
        /// </summary>
        /// <param name="message">
        /// A <see cref="JournalMessage"/> object that contains details about the message to 
        /// write, which can be either a <see cref="JournalFormattedMessage"/> a
        /// <see cref="JournalExceptionMessage"/> object, or a custom message object.
        /// </param>
        public void Write(JournalMessage message) => EventLogProvider.Current.Write(message);
    }
}
