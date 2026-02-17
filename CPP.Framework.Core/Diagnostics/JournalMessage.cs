using System;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Provides context information for a write request from the journal.
    /// </summary>
    public abstract class JournalMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JournalMessage"/> class. 
        /// </summary>
        /// <param name="timestamp">
        /// The timestamp for the message, in UTC.
        /// </param>
        /// <param name="severity">
        /// The severity of the journal message.
        /// </param>
        /// <param name="source">
        /// The id of the source writing the journal message.
        /// </param>
        /// <param name="telemetry">
        /// The telemetry information associated with the message.
        /// </param>
        protected JournalMessage(DateTime timestamp, JournalSeverity severity, Guid source, JournalTelemetry telemetry)
        {
            ArgumentValidator.ValidateNotNull(() => telemetry);
            this.Severity = severity;
            this.Source = source;
            this.Telemetry = telemetry;
            this.Timestamp = timestamp.ToUniversalTime();
        }

        /// <summary>
        /// Gets the severity of the journal message.
        /// </summary>
        public JournalSeverity Severity { get; }

        /// <summary>
        /// Gets the id of the source writing the journal message.
        /// </summary>
        public Guid Source { get; }

        /// <summary>
        /// Gets the telemetry information associated with the message.
        /// </summary>
        public JournalTelemetry Telemetry { get; }

        /// <summary>
        /// Gets the timestamp for the message, in UTC.
        /// </summary>
        public DateTime Timestamp { get; }
    }
}
