using System.Data;
using System.Data.Entity.Core;

// ReSharper disable once CheckNamespace
namespace CPP.Framework.Diagnostics
{
    /// <summary>
    ///   Extension methods for the <see cref="JournalSource"/> class.
    /// </summary>
    public static class JournalSourceExtensions
    {
        /// <summary>
        ///   Writes telemetry data for the database entries that generated an
        ///   <see cref="OptimisticConcurrencyException"/> during a database update.
        /// </summary>
        /// <param name="source">The source to write the telemetry to.</param>
        /// <param name="ex">The exception that contains the telemetry to write.</param>
        /// <returns>A reference to the current instance.</returns>
        public static JournalSource WriteTelemetry(this JournalSource source, OptimisticConcurrencyException ex)
        {
            var entities = 0;
            foreach (var ose in ex.StateEntries)
            {
                var prefix = $"Entities[{entities++}]";
                source.WriteTelemetryValue($"{prefix}.EntitySet", ose.EntitySet.Name);

                foreach (var ekv in ose.EntityKey.EntityKeyValues)
                {
                    source.WriteTelemetryValue($"{prefix}.{ekv.Key}]", $"{ekv.Value}");
                }
            }
            source.WriteTelemetryValue("Entities.Count", $"{entities}");
            return source;
        }
    }
}
