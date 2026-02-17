namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Abstract interfaces for classes that listen for <see cref="Journal"/> trace messages and
    /// telemetry data to persist them to a storage location.
    /// </summary>
    public interface IJournalListenerEx : IJournalListener
    {
        /// <summary>
        /// Writes data for a thrown exception to the underlying storage location.
        /// </summary>
        /// <param name="message">
        /// A <see cref="JournalMessage"/> object that contains details about the message to 
        /// write, which can be either a <see cref="JournalFormattedMessage"/> a
        /// <see cref="JournalExceptionMessage"/> object, or a custom message object.
        /// </param>
        void Write(JournalMessage message);
    }
}
