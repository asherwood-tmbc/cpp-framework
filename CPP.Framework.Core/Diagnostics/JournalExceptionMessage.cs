using System;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Provides context for journal messages being written for exceptions.
    /// </summary>
    public class JournalExceptionMessage : JournalMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JournalExceptionMessage"/> class. 
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
        /// <param name="exception">
        /// The exception being written to the journal.
        /// </param>
        protected internal JournalExceptionMessage(DateTime timestamp, JournalSeverity severity, Guid source, JournalTelemetry telemetry, Exception exception)
            : base(timestamp, severity, source, telemetry)
        {
            ArgumentValidator.ValidateNotNull(() => exception);
            this.Exception = exception;
        }

        /// <summary>
        /// Gets the exception being written to the journal.
        /// </summary>
        public Exception Exception { get; }
    }
}
