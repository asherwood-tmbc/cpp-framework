using System.Collections.Generic;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Abstract base interface for all classes that represent and email message.
    /// </summary>
    public interface IMailMessage
    {
        /// <summary>
        /// Gets the list of <see cref="IMailMessageAttachment"/> objects for the email message.
        /// </summary>
        IEnumerable<IMailMessageAttachment> Attachments { get; }

        /// <summary>
        /// Gets the list of blind copy recipients for the email message.
        /// </summary>
        List<string> BlindRecipients { get; }

        /// <summary>
        /// Gets the list of recipient addresses for the email message.
        /// </summary>
        List<string> CopyRecipients { get; }

        /// <summary>
        /// Gets the content of the email message.
        /// </summary>
        string Content { get; }

        /// <summary>
        /// Gets the list of recipients for the email message.
        /// </summary>
        List<string> Recipients { get; }

        /// <summary>
        /// Gets the address of the sender of the email message.
        /// </summary>
        string Sender { get; }

        /// <summary>
        /// Gets the subject line of the email message.
        /// </summary>
        string Subject { get; }
    }
}
