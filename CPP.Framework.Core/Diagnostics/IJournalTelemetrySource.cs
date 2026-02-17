namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Abstract interface implemented by objects that support writing their own predefined 
    /// telemetry values to the <see cref="Journal"/>.
    /// </summary>
    public interface IJournalTelemetrySource
    {
        /// <summary>
        /// Called by a <see cref="JournalSource"/> to allow an object to write any relevant 
        /// telemetry values related to the object.
        /// </summary>
        /// <param name="source">
        /// The <see cref="JournalSource"/> that should receive the telemetry data.
        /// </param>
        void WriteTelemetryValues(JournalSource source);
    }
}