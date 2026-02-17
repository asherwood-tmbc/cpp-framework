using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using CPP.Framework.Configuration;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Provider used to write entries to the system event log for the local machine.
    /// </summary>
    public class EventLogProvider : SingletonServiceBase
    {
        /// <summary>
        /// The default event source name.
        /// </summary>
        private const string DefaultEventSource = "CPP Framework Application";

        /// <summary>
        /// The reference to the current service instance for the application.
        /// </summary>
        private static readonly ServiceInstance<EventLogProvider> _ServiceInstance = new ServiceInstance<EventLogProvider>(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Initializes a new instance of the <see cref="EventLogProvider"/> class. 
        /// </summary>
        protected EventLogProvider() { }

        /// <summary>
        /// Gets a reference to the current service instance for the application.
        /// </summary>
        public static EventLogProvider Current => _ServiceInstance.GetInstance();

        /// <summary>
        /// Gets a reference to the <see cref="EventLog"/> that receives output messages.
        /// </summary>
        protected EventLog OutputEventLog { get; private set; }

        /// <summary>
        /// Formats the contents of a <see cref="JournalMessage"/> into a string.
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <returns>The formatted message.</returns>
        protected string FormatMessage(JournalMessage message)
        {
            var content = new StringBuilder();

            if (message.Telemetry.HasProperties)
            {
                content.AppendLine("Properties: {");
                foreach (var entry in message.Telemetry.Properties)
                {
                    content.AppendLine($"    \"{entry.Key}\": \"{entry.Value}\"");
                }
                content.AppendLine("}");
                content.AppendLine();
            }
            if (message.Telemetry.HasStatistics)
            {
                content.AppendLine("Statistics: {");
                foreach (var entry in message.Telemetry.Statistics)
                {
                    content.AppendLine($"    \"{entry.Key}\": \"{entry.Value:F3}\"");
                }
                content.AppendLine("}");
                content.AppendLine();
            }

            switch (message)
            {
                case JournalExceptionMessage exception:
                    {
                        foreach (var line in Journal.GetInstance().FormatException(exception.Exception))
                        {
                            content.AppendLine(line);
                        }
                    }
                    break;
                case JournalFormattedMessage formatted:
                    {
                        content.AppendLine(formatted.Message);
                    }
                    break;
            }
            return content.ToString();
        }

        /// <summary>
        /// Called by the base class to perform any initialization tasks when the instance is being
        /// created.
        /// </summary>
        protected internal override void StartupInstance()
        {
            // get the event source name from the configuration, if available. if not, then use the
            // global default for framework applications.
            var source = ConfigSettingProvider.Current.GetSetting("EventSourceName", DefaultEventSource);

            // attempt to create the logging source in the system event log. if the process doesn't
            // have permission to create, then fall back to the default application log.
            try
            {
                source = (source ?? DefaultEventSource);
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, "Application");
                }
            }
            catch (Exception)
            {
                source = "Application";
            }

            this.OutputEventLog = new EventLog
            {
                Source = source,
            };
            base.StartupInstance();
        }

        /// <summary>
        /// Translates a <see cref="JournalSeverity"/> value into a <see cref="EventLogEntryType"/>
        /// value.
        /// </summary>
        /// <param name="severity">The severity to translate.</param>
        /// <returns>An <see cref="EventLogEntryType"/> value.</returns>
        protected EventLogEntryType TranslateSeverity(JournalSeverity severity)
        {
            switch (severity)
            {
                case JournalSeverity.Warning: return EventLogEntryType.Warning;
                case JournalSeverity.Debug:
                case JournalSeverity.Verbose:
                case JournalSeverity.Information: return EventLogEntryType.Information;
                case JournalSeverity.Critical:
                case JournalSeverity.Error: return EventLogEntryType.Error;
            }
            return EventLogEntryType.Error;
        }

        /// <summary>
        /// Writes a <see cref="JournalMessage"/> value to the system event log.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public void Write(JournalMessage message)
        {
            var formatted = this.FormatMessage(message);
            this.Write(message.Severity, formatted);
        }

        /// <summary>
        /// Writes a message to the system event log.
        /// </summary>
        /// <param name="severity">The severity of the target message.</param>
        /// <param name="message">The message to write.</param>
        public void Write(JournalSeverity severity, string message)
        {
            try
            {
                var type = this.TranslateSeverity(severity);
                this.OutputEventLog.WriteEntry(message, type);
            }
            catch
            {
                // always make a best attempt, but do not crash the app over failures
            }
        }

        /// <summary>
        /// Writes an exception to the system event log.
        /// </summary>
        /// <param name="severity">The severity of the target exception.</param>
        /// <param name="exception">The exception to write.</param>
        public void Write(JournalSeverity severity, Exception exception)
        {
            try
            {
                var message = string.Join("\r\n", Journal.GetInstance().FormatException(exception));
                this.Write(severity, message);
            }
            catch
            {
                // always make a best attempt, but do not crash the app over failures
            }
        }
    }
}
