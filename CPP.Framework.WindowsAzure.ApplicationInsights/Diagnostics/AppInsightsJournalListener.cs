using System;
using System.Collections.Generic;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// An <see cref="IJournalListener"/> that is used to write trace messages and exceptions to
    /// the currently configurated Application Insights server for the application.
    /// </summary>
    public sealed class AppInsightsJournalListener : IJournalListenerEx
    {
        /// <summary>
        /// Writes a message to the underlying storage location.
        /// </summary>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="source">The id of the source that generated the message.</param>
        /// <param name="message">The message to write.</param>
        void IJournalListener.Write(JournalSeverity severity, Guid source, string message)
        {
            AppInsightsProvider.Current.TrackTrace(message, severity);
        }

        /// <summary>
        /// Writes data for a journal message to the underlying storage location.
        /// </summary>
        /// <param name="message">
        /// A <see cref="JournalMessage"/> object that contains details about the message to 
        /// write, which can be either a <see cref="JournalFormattedMessage"/> a
        /// <see cref="JournalExceptionMessage"/> object, or a custom message object.
        /// </param>
        public void Write(JournalMessage message)
        {
            switch (message)
            {
                case JournalFormattedMessage formatted:
                    {
                        ExtractTelemetryValues(formatted.Telemetry, out var properties, out _);
                        AppInsightsProvider.Current.TrackTrace(formatted.Message, formatted.Severity, properties);
                    }
                    break;
                case JournalExceptionMessage exception:
                    {
                        ExtractTelemetryValues(exception.Telemetry, out var properties, out var statistics);
                        AppInsightsProvider.Current.TrackException(exception.Exception, properties, statistics);
                    }
                    break;
            }
        }

        /// <summary>
        /// Extracts the values of the property and statistic dictionaries from a 
        /// <see cref="JournalTelemetry"/> object, if they are available, and only if the telemetry
        /// object is not null.
        /// </summary>
        /// <param name="telemetry">The telemetry object.</param>
        /// <param name="properties">
        /// An output parameter that receives the properties dictionary if there are values 
        /// defined. Otherwise, this parameter is set to null on output. This parameter is also
        /// automatically set to null if <paramref name="telemetry"/> is null.
        /// </param>
        /// <param name="statistics">
        /// An output parameter that receives the statistics dictionary if there are values 
        /// defined. Otherwise, this parameter is set to null on output. This parameter is also
        /// automatically set to null if <paramref name="telemetry"/> is null.
        /// </param>
        private static void ExtractTelemetryValues(JournalTelemetry telemetry, out Dictionary<string, string> properties, out Dictionary<string, double> statistics)
        {
            properties = (telemetry?.HasProperties ?? false ? telemetry.Properties : null);
            statistics = (telemetry?.HasStatistics ?? false ? telemetry.Statistics : null);
        }
    }
}
