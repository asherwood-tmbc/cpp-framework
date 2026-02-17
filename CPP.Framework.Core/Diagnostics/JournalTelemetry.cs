using System;
using System.Collections.Generic;
using System.Threading;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Provides telemetry information to the <see cref="Journal"/> for advanced logging scenarios.
    /// </summary>
    public class JournalTelemetry
    {
        /// <summary>
        /// The map of telemetry properties to their names.
        /// </summary>
        private readonly Lazy<Dictionary<string, string>> _telementyPropertiesMap;

        /// <summary>
        /// The map of telemetry statistics to their names.
        /// </summary>
        private readonly Lazy<Dictionary<string, double>> _telemetryStatisticsMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="JournalTelemetry"/> class. 
        /// </summary>
        public JournalTelemetry()
        {
            _telementyPropertiesMap = new Lazy<Dictionary<string, string>>(CreatePropertiesMap, LazyThreadSafetyMode.PublicationOnly);
            _telemetryStatisticsMap = new Lazy<Dictionary<string, double>>(CreateStatisticsMap, LazyThreadSafetyMode.PublicationOnly);
        }

        /// <summary>
        /// Gets a flag that indicates whether or not telemetry properties have been added.
        /// </summary>
        public bool HasProperties => (_telementyPropertiesMap.IsValueCreated && (_telementyPropertiesMap.Value.Count >= 1));

        /// <summary>
        /// Gets a flag that indicates whether or not telemetry statistics have been added.
        /// </summary>
        public bool HasStatistics => (_telemetryStatisticsMap.IsValueCreated && (_telemetryStatisticsMap.Value.Count >= 1));

        /// <summary>
        /// Gets the telemetry property map for the current journal source.
        /// </summary>
        public Dictionary<string, string> Properties => _telementyPropertiesMap.Value;

        /// <summary>
        /// Gets the telemetry statistics map for the current journal source.
        /// </summary>
        public Dictionary<string, double> Statistics => _telemetryStatisticsMap.Value;

        /// <summary>
        /// Creates the name-value map for the telemetry property values.
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> object.</returns>
        protected internal virtual Dictionary<string, string> CreatePropertiesMap() => new Dictionary<string, string>();

        /// <summary>
        /// Creates the name-value map for the telemetry statistic values.
        /// </summary>
        /// <returns>A <see cref="Dictionary{TKey,TValue}"/> object.</returns>
        protected internal virtual Dictionary<string, double> CreateStatisticsMap() => new Dictionary<string, double>();
    }
}
