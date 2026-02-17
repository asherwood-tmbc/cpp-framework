using System;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Delegate used to handle events related to a journal listener.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="args">
    /// A <see cref="JournalListenerEventArgs"/> object that contains more information about the
    /// event.
    /// </param>
    public delegate void JournalListenerEvent(object sender, JournalListenerEventArgs args);

    /// <summary>
    /// Provides more information about an event for a <see cref="JournalListenerEvent"/> delegate.
    /// </summary>
    public class JournalListenerEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JournalListenerEventArgs"/> class. 
        /// </summary>
        /// <param name="listener">
        /// The listener that triggered the event.
        /// </param>
        /// <param name="message">
        /// The <see cref="JournalMessage"/> that was being processed at the time of the event.
        /// </param>
        /// <param name="exception">
        /// The <see cref="Exception"/> that was caught if the event is an error event.
        /// </param>
        public JournalListenerEventArgs(IJournalListener listener, JournalMessage message, Exception exception)
        {
            ArgumentValidator.ValidateNotNull(() => listener);
            ArgumentValidator.ValidateNotNull(() => message);
            this.Exception = exception;
            this.Listener = listener;
            this.Message = message;
        }

        /// <summary>
        /// Gets the <see cref="Exception"/> that was caught if the event is an error event.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not a delegate handled the message (true), 
        /// or is the framework needs to perform default processing for the message (false).
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the listener that triggered the event.
        /// </summary>
        public IJournalListener Listener { get; }

        /// <summary>
        /// Gets the <see cref="JournalMessage"/> that was being processed at the time of the event.
        /// </summary>
        public JournalMessage Message { get; }
    }
}
