using System;
using System.Linq;

using CPP.Framework.DependencyInjection;
using CPP.Framework.Diagnostics;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Extension methods for the <see cref="ConfigSettingProvider"/> class.
    /// </summary>
    public static class ConfigSettingsProviderExtensions
    {
        /// <summary>
        /// Gets the currently configured value indicating whether or not to use development 
        /// sampling when writing messages to Application Insights, and therefore reduce the amount
        /// of storage space used per day.
        /// </summary>
        /// <param name="provider">
        /// The current <see cref="ConfigSettingProvider"/> instance that contains the 
        /// configuration settings.
        /// </param>
        /// <returns>True to use development sampling; otherwise, false.</returns>
        public static bool GetApplicationInsightsDevModeSetting(this ConfigSettingProvider provider)
        {
            ArgumentValidator.ValidateThisObj(() => provider);
            return provider.GetSetting("AppInsightsDevMode", Convert.ToBoolean, "false");
        }
        
        /// <summary>
        /// Ges the currently configured instrumentation key for the Application Insights account.
        /// </summary>
        /// <param name="provider">
        /// The current <see cref="ConfigSettingProvider"/> instance that contains the 
        /// configuration settings.
        /// </param>
        /// <returns>The instrumentation key.</returns>
        public static string GetApplicationInsightsInstrumentationKeySetting(this ConfigSettingProvider provider)
        {
            ArgumentValidator.ValidateThisObj(() => provider);
            if (!provider.TryGetSetting("ApplicationInsightsKey", out var setting))
            {
                setting = provider.GetSetting("APPINSIGHTS_INSTRUMENTATIONKEY");
            }
            return setting;
        }

        /// <summary>
        /// Configures the application to send log messages from the <see cref="Journal"/> to
        /// Application Insights.
        /// </summary>
        /// <param name="provider">The service instance to configure.</param>
        /// <returns>The value of <paramref name="provider"/> (for chaining).</returns>
        public static ConfigSettingProvider UseApplicationInghtsLogging(this ConfigSettingProvider provider)
        {
            // configure the journal listener for application insights.
            ServiceLocator.Register(new AppInsightsJournalListener());  // register as a singleton
            Journal.Listeners.Add(ServiceLocator.GetInstance<AppInsightsJournalListener>());

            // set the default configuration settings for app insights.
            AppInsightsProvider.SetDefaultConfigSettings(TelemetryConfiguration.Active);

            return provider;
        }

        /// <summary>
        /// Configures the application to collect live metrics for Application Insights.
        /// </summary>
        /// <param name="provider">The service instance to configure.</param>
        /// <returns>The value of <paramref name="provider"/> (for chaining).</returns>
        public static ConfigSettingProvider UseApplicationInsightsLiveMetrics(this ConfigSettingProvider provider)
        {
            if (provider.TryGetApplicationInsightsLiveMetricsKeySetting(out var setting))
            {
                var module = TelemetryModules.Instance.Modules
                    .OfType<QuickPulseTelemetryModule>()
                    .FirstOrDefault();
                if (module != null) module.AuthenticationApiKey = setting;
            }
            return provider;
        }

        /// <summary>
        /// Attempts to get the value of the instramentation key from the currently configured
        /// settings for the live metrics in Application Insights.
        /// </summary>
        /// <param name="provider">
        /// The current <see cref="ConfigSettingProvider"/> instance that contains the 
        /// configuration settings.
        /// </param>
        /// <param name="value">An output variable the receives the setting value on success.</param>
        /// <returns>
        /// <b>True</b> if the value was retrieved successfully; otherwise, <b>false</b> if the
        /// setting value is unavailable, or the setting has not been configured.
        /// </returns>
        public static bool TryGetApplicationInsightsLiveMetricsKeySetting(this ConfigSettingProvider provider, out string value)
        {
            return provider.TryGetSetting("LiveMetricsKey", out value);
        }
    }
}
