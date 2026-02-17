using System;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Provides context for journal messages that are written as message strings.
    /// </summary>
    public class JournalFormattedMessage : JournalMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JournalFormattedMessage"/> class. 
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
        /// <param name="message">
        /// The formatted string for the message.
        /// </param>
        protected internal JournalFormattedMessage(DateTime timestamp, JournalSeverity severity, Guid source, JournalTelemetry telemetry, string message)
            : base(timestamp, severity, source, telemetry)
        {
            this.Message = message ?? string.Empty;
        }

        /// <summary>
        /// Gets the formatted string for the message.
        /// </summary>
        public string Message { get; }
    }
}
