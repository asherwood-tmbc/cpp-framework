using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.DependencyInjection;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Represents a source for a group of one or more messages for the <see cref="Journal"/>.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class JournalSource
    {
        /// <summary>
        /// The unique id of the journal source.
        /// </summary>
        private readonly Guid? _sourceGuid;

        /// <summary>
        /// The name of the source writing to the journal.
        /// </summary>
        private readonly string _sourceName;

        /// <summary>
        /// The telemetry data assigned to the current source.
        /// </summary>
        private readonly JournalTelemetry _telemetryData;

        /// <summary>
        /// The index value to use for the next unnamed telemetry property.
        /// </summary>
        private int _unnamedPropertiesIndex = 1;

        /// <summary>
        /// The index value to use for the next unnamed telemetry statistic.
        /// </summary>
        private int _unnamedStatisticsIndex = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="JournalSource"/> class. 
        /// </summary>
        protected internal JournalSource() : this(Guid.Empty, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JournalSource"/> class. 
        /// </summary>
        /// <param name="source">The unique id for the trace messages logged from the source.</param>
        /// <param name="sourceName">The name of the source writing to the journal.</param>
        protected internal JournalSource(Guid source, string sourceName)
        {
            _sourceGuid = ((source == Guid.Empty) ? null : ((Guid?)source));
            _sourceName = (string.IsNullOrWhiteSpace(sourceName) ? null : sourceName);
            _telemetryData = ServiceLocator.GetInstance<JournalTelemetry>();
        }

        /// <summary>
        /// Raised before a <see cref="JournalSource"/> has written the telemetry values for a 
        /// message to the <see cref="Journal"/>. This gives the application a chance to add any
        /// global values to the telemetry data for each message before it has been written.
        /// </summary>
        public static event EventHandler BeforeTelemetryWritten;

        /// <summary>
        /// Gets the telemetry property map for the current journal source.
        /// </summary>
        protected Dictionary<string, string> Properties => _telemetryData.Properties;

        /// <summary>
        /// Gets the telemetry statistics map for the current journal source.
        /// </summary>
        protected Dictionary<string, double> Statistics => _telemetryData.Statistics;

        /// <summary>
        /// Gets the id of the current journal source.
        /// </summary>
        /// <returns>A <see cref="Guid"/> value.</returns>
        protected virtual Guid GetSource()
        {
            if (!_sourceGuid.HasValue)
            {
                return GuidGeneratorService.Current.NewGuid();
            }
            return _sourceGuid.Value;
        }

        /// <summary>
        /// Raises the <see cref="BeforeTelemetryWritten"/> event.
        /// </summary>
        protected internal virtual void OnBeforeTelemetryWritten()
        {
            try
            {
                BeforeTelemetryWritten?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Journal.JournalErrorMessageFormat, typeof(JournalSource).FullName, nameof(OnBeforeTelemetryWritten), ex);
            }
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the journal.
        /// </summary>
        /// <param name="severity">The severity of the trace message.</param>
        /// <param name="source">The name of the source for the trace message.</param>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        private JournalSource Write(JournalSeverity severity, Guid source, Exception exception)
        {
            try
            {
                this.WriteCallerContextTelemetry(); // ensure that the context information is written
                var timestamp = DateTimeService.Current.UtcNow;
                Journal.GetInstance().Write(timestamp, severity, source, _telemetryData, exception);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Journal.JournalErrorMessageFormat, typeof(JournalSource).FullName, nameof(Write), ex);
            }
            finally
            {
                this.Properties.Clear();
                this.Statistics.Clear();
                _unnamedPropertiesIndex = _unnamedStatisticsIndex = 1;
            }
            return this;
        }

        /// <summary>
        /// Writes a trace message to the journal.
        /// </summary>
        /// <param name="severity">The severity of the trace message.</param>
        /// <param name="source">The id of the source for the trace message.</param>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        private JournalSource Write(JournalSeverity severity, Guid source, string format, params object[] formatArgs)
        {
            try
            {
                this.WriteCallerContextTelemetry(); // ensure that the context information is written
                var timestamp = DateTimeService.Current.UtcNow;
                Journal.GetInstance().Write(timestamp, severity, source, _telemetryData, format, formatArgs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Journal.JournalErrorMessageFormat, typeof(JournalSource).FullName, nameof(Write), ex);
            }
            finally
            {
                this.Properties.Clear();
                this.Statistics.Clear();
                _unnamedPropertiesIndex = _unnamedStatisticsIndex = 1;
            }
            return this;
        }

        /// <summary>
        /// Ensures that information about the source location and application name are written to
        /// the telemetry for the next log message.
        /// </summary>
        protected virtual void WriteCallerContextTelemetry()
        {
            var applicatiionName = Journal.ApplicationName;
            if (!this.Properties.ContainsKey("source") && (!string.IsNullOrWhiteSpace(_sourceName)))
            {
                this.WriteTelemetryValue("source", _sourceName);
            }
            if (!this.Properties.ContainsKey("machine"))
            {
                this.WriteTelemetryValue("machine", Environment.MachineName);
            }
            if (!this.Properties.ContainsKey("application") && (!string.IsNullOrWhiteSpace(applicatiionName)))
            {
                this.WriteTelemetryValue("application", applicatiionName);
            }
            this.OnBeforeTelemetryWritten();    // let the caller know
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Critical"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteCritical(Exception exception)
        {
            return this.Write(JournalSeverity.Critical, this.GetSource(), exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Critical"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteCritical(string format, params object[] formatArgs)
        {
            return this.Write(JournalSeverity.Critical, this.GetSource(), format, formatArgs);
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Debug"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteDebug(Exception exception)
        {
            return this.Write(JournalSeverity.Debug, this.GetSource(), exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Debug"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteDebug(string format, params object[] formatArgs)
        {
            return this.Write(JournalSeverity.Debug, this.GetSource(), format, formatArgs);
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Error"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteError(Exception exception)
        {
            return this.Write(JournalSeverity.Error, this.GetSource(), exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Error"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteError(string format, params object[] formatArgs)
        {
            return this.Write(JournalSeverity.Error, this.GetSource(), format, formatArgs);
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Information"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteInfo(Exception exception)
        {
            return this.Write(JournalSeverity.Information, this.GetSource(), exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Information"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteInfo(string format, params object[] formatArgs)
        {
            return this.Write(JournalSeverity.Information, this.GetSource(), format, formatArgs);
        }

        /// <summary>
        /// Associates the statistic values for an object with the next journal message written to 
        /// the current source.
        /// </summary>
        /// <param name="source">The source object that contains the telemetry values to write.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteStatistic(IJournalStatisticSource source)
        {
            try
            {
                source?.WriteStatisticValues(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Journal.JournalErrorMessageFormat, typeof(JournalSource).FullName, nameof(WriteStatistic), ex);
            }
            return this;
        }

        /// <summary>
        /// Associates a named statistic value with the next journal message written to the current
        /// source.
        /// </summary>
        /// <param name="name">
        /// The name of the value. If the value already exists, then the existing value will be 
        /// overwritten. Additionally, if this value is null or all whitespace characters, then the
        /// call is ignored without an error.
        /// </param>
        /// <param name="value">The value of the statistic.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteStatisticValue(string name, double value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"UnnamedStatistic{_unnamedStatisticsIndex++}";
            }
            this.Statistics[name] = value;
            return this;
        }

        /// <summary>
        /// Associates the telemetry values for an object with the next journal message written to 
        /// the current source.
        /// </summary>
        /// <param name="source">The source object that contains the telemetry values to write.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteTelemetry(IJournalTelemetrySource source)
        {
            try
            {
                source?.WriteTelemetryValues(this);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Journal.JournalErrorMessageFormat, typeof(JournalSource).FullName, nameof(WriteTelemetry), ex);
            }
            return this;
        }

        /// <summary>
        /// Associates a named property value with the next journal message written to the current 
        /// source.
        /// </summary>
        /// <param name="name">The name of the telemetry property.</param>
        /// <param name="value">The object value to associate.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteTelemetryValue(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"UnnamedProperty{_unnamedPropertiesIndex++}";
            }
            this.Properties[name] = value;
            return this;
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Verbose"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteVerbose(Exception exception)
        {
            return this.Write(JournalSeverity.Verbose, this.GetSource(), exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Critical"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteVerbose(string format, params object[] formatArgs)
        {
            return this.Write(JournalSeverity.Verbose, this.GetSource(), format, formatArgs);
        }

        /// <summary>
        /// Writes an <see cref="Exception"/> to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Warning"/>.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteWarning(Exception exception)
        {
            return this.Write(JournalSeverity.Warning, this.GetSource(), exception);
        }

        /// <summary>
        /// Writes a message to the trace journal with a severity of 
        /// <see cref="JournalSeverity.Warning"/>.
        /// </summary>
        /// <param name="format">The format string for the trace message.</param>
        /// <param name="formatArgs">A variable list of argument values for the <paramref name="format"/> string.</param>
        /// <returns>A reference to the current instance.</returns>
        public JournalSource WriteWarning(string format, params object[] formatArgs)
        {
            return this.Write(JournalSeverity.Warning, this.GetSource(), format, formatArgs);
        }
    }
}
