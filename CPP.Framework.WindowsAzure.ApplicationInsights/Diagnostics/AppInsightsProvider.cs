using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using CPP.Framework.Configuration;
using CPP.Framework.Threading;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Service provider class for the Microsoft Application Insights API.
    /// </summary>
    public class AppInsightsProvider : SingletonServiceBase
    {
        private static readonly ServiceInstance<AppInsightsProvider> _ServiceInstance = new ServiceInstance<AppInsightsProvider>();

        private readonly MultiAccessLock _syncLock = new MultiAccessLock(LockRecursionPolicy.SupportsRecursion);
        private TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppInsightsProvider"/> class. 
        /// </summary>
        protected AppInsightsProvider() { }

        /// <summary>
        /// Gets a reference to the current provider instance for the application.
        /// </summary>
        public static AppInsightsProvider Current => _ServiceInstance.GetInstance();

        /// <summary>
        /// Retrieves a reference to the current <see cref="TelemetryClient"/> instance.
        /// </summary>
        /// <param name="reconnect">
        /// True if the method should dispose of any current reference and recreate it; otherwise,
        /// false to use the current reference. If the current reference has not been initialized,
        /// then a new instance will still be created.
        /// </param>
        /// <returns>A <see cref="TelemetryClient"/> object.</returns>
        private TelemetryClient GetTelemetryClient(bool reconnect = false)
        {
            using (_syncLock.GetReaderAccess())
            {
                if (!reconnect && (_telemetryClient != null))
                {
                    return _telemetryClient;
                }
            }
            using (_syncLock.GetWriterAccess())
            {
                if (reconnect || (_telemetryClient == null))
                {
                    _telemetryClient = null;    // for garbage collection purposes
                    var config = this.CreateTelemetryConfiguration();
                    this.SetTelemetryConfiguration(config);
                    _telemetryClient = new TelemetryClient(config);
                }
                return _telemetryClient;
            }
        }

        /// <summary>
        /// Creates the configuration settings object for an Application Insights telemetry client.
        /// </summary>
        /// <returns>A <see cref="TelemetryConfiguration"/> object.</returns>
        protected internal virtual TelemetryConfiguration CreateTelemetryConfiguration() => TelemetryConfiguration.CreateDefault(); // the default just reads from the ApplicationInsights.config file

        /// <summary>
        /// Flushes the in-memory buffer for the telemetry data.
        /// </summary>
        public void Flush() => this.GetTelemetryClient().Flush();

        /// <summary>
        /// Sets the default configuration values for a <see cref="TelemetryConfiguration"/> object.
        /// </summary>
        /// <param name="config">The object to configure.</param>
        protected internal static void SetDefaultConfigSettings(TelemetryConfiguration config)
        {
            ArgumentValidator.ValidateNotNull(() => config);

            config.InstrumentationKey = ConfigSettingProvider.Current.GetApplicationInsightsInstrumentationKeySetting();
            config.TelemetryChannel.DeveloperMode = ConfigSettingProvider.Current.GetApplicationInsightsDevModeSetting();
        }
        
        /// <summary>
        /// Called by the base class to set the values for the <see cref="TelemetryConfiguration"/>
        /// used by the current provider.
        /// </summary>
        /// <param name="config">The configuration to update.</param>
        protected internal virtual void SetTelemetryConfiguration(TelemetryConfiguration config) => SetDefaultConfigSettings(config);

        /// <summary>
        /// Sends event telemetry to the application insights server.
        /// </summary>
        /// <param name="name">The name of the event.</param>
        /// <param name="properties">
        /// A <see cref="IDictionary{TKey,TValue}"/> of named properties that can be used to search 
        /// and classify the event.
        /// </param>
        /// <param name="metrics">
        /// A <see cref="IDictionary{TKey,TValue}"/> that contains named statistic measurements
        /// associated with the event
        /// </param>
        public virtual void TrackEvent(string name, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.GetTelemetryClient().TrackEvent(name, properties, metrics);
        }

        /// <summary>
        /// Sends exception telemetry to the application insights server.
        /// </summary>
        /// <param name="exception">The <see cref="Exception"/> to track.</param>
        /// <param name="properties">
        /// A <see cref="IDictionary{TKey,TValue}"/> of named properties that can be used to search 
        /// and classify the exception.
        /// </param>
        /// <param name="metrics">
        /// A <see cref="IDictionary{TKey,TValue}"/> that contains named statistic measurements
        /// associated with the exception
        /// </param>
        public virtual void TrackException(Exception exception, IDictionary<string, string> properties = null, IDictionary<string, double> metrics = null)
        {
            this.GetTelemetryClient().TrackException(exception, properties, metrics);
        }

        /// <summary>
        /// Sends information about viewed in the application to the application insights server.
        /// </summary>
        /// <param name="name">The name of the page.</param>
        public virtual void TrackPageView(string name) => this.GetTelemetryClient().TrackPageView(name);

        /// <summary>
        /// Sends a trace message to the application insights server.
        /// </summary>
        /// <param name="message">The trace message to send.</param>
        public virtual void TrackTrace(string message) => this.GetTelemetryClient().TrackTrace(message);

        /// <summary>
        /// Sends a trace message to the application insights server.
        /// </summary>
        /// <param name="message">The trace message to send.</param>
        /// <param name="severity">The severity level of the message.</param>
        public virtual void TrackTrace(string message, JournalSeverity severity)
        {
            var severityLevel = severity.AsInsightsSeverityLevel();
            this.GetTelemetryClient().TrackTrace(message, severityLevel);
        }

        /// <summary>
        /// Sends a trace message to the application insights server.
        /// </summary>
        /// <param name="message">The trace message to send.</param>
        /// <param name="properties">
        /// A <see cref="IDictionary{TKey,TValue}"/> of named properties that can be used to search
        /// and classify the trace message.
        /// </param>
        public virtual void TrackTrace(string message, IDictionary<string, string> properties)
        {
            this.GetTelemetryClient().TrackTrace(message, properties);
        }

        /// <summary>
        /// Sends a trace message to the application insights server.
        /// </summary>
        /// <param name="message">The trace message to send.</param>
        /// <param name="severity">The severity level of the message.</param>
        /// <param name="properties">
        /// A <see cref="IDictionary{TKey,TValue}"/> of named properties that can be used to search
        /// and classify the trace message.
        /// </param>
        public virtual void TrackTrace(string message, JournalSeverity severity, IDictionary<string, string> properties)
        {
            var severityLevel = severity.AsInsightsSeverityLevel();
            this.GetTelemetryClient().TrackTrace(message, severityLevel, properties);
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="SeverityLevel"/> type.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Reviewed. Suppression is OK here.")]
    public static class SeverityLevelExtensions
    {
        /// <summary>
        /// Converts a <see cref="JournalSeverity"/> value to an Application Insights 
        /// <see cref="SeverityLevel"/> value.
        /// </summary>
        /// <param name="severity">The journal severity to convert.</param>
        /// <returns>A <see cref="SeverityLevel"/> value.</returns>
        public static SeverityLevel AsInsightsSeverityLevel(this JournalSeverity severity)
        {
            switch (severity)
            {
                // ReSharper disable RedundantCaseLabel
                case JournalSeverity.Critical: return SeverityLevel.Critical;
                case JournalSeverity.Error: return SeverityLevel.Error;
                case JournalSeverity.Information: return SeverityLevel.Information;
                case JournalSeverity.Warning: return SeverityLevel.Warning;
                case JournalSeverity.Debug:
                case JournalSeverity.Verbose:
                default: return SeverityLevel.Verbose;
                // ReSharper restore RedundantCaseLabel
            }
        }
    }
}
