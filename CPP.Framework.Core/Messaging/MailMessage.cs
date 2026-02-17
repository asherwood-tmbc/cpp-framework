using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Default base implementation of the <see cref="IMailMessage"/> interface.
    /// </summary>
    /// <typeparam name="TAttachment">The type of the message attachment objects.</typeparam>
    [ExcludeFromCodeCoverage]
    public class MailMessage<TAttachment> : IMailMessage where TAttachment : IMailMessageAttachment
    {
        /// <summary>
        /// The list of attachments for the email message.
        /// </summary>
        private List<TAttachment> _attachments;

        /// <summary>
        /// The list of blind copy recipients for the email message.
        /// </summary>
        private List<string> _blindRecipients;

        /// <summary>
        /// The list of recipient addresses for the email message.
        /// </summary>
        private List<string> _copyRecipients;

        /// <summary>
        /// The list of recipients for the email message.
        /// </summary>
        private List<string> _recipients;

        /// <summary>
        /// Gets the list of <see cref="IMailMessageAttachment"/> objects for the email message.
        /// </summary>
        IEnumerable<IMailMessageAttachment> IMailMessage.Attachments => this.Attachments.OfType<IMailMessageAttachment>();

        /// <summary>
        /// Gets or sets the list of attachments for the email message.
        /// </summary>
        public List<TAttachment> Attachments
        {
            get => (_attachments ?? (_attachments = new List<TAttachment>()));
            set => _attachments = (value ?? new List<TAttachment>());
        }

        /// <summary>
        /// Gets or sets the list of blind copy recipients for the email message.
        /// </summary>
        public List<string> BlindRecipients
        {
            get => (_blindRecipients ?? (_blindRecipients = new List<string>()));
            set => _blindRecipients = (value ?? new List<string>());
        }

        /// <summary>
        /// Gets or sets the list of recipient addresses for the email message.
        /// </summary>
        public List<string> CopyRecipients
        {
            get => (_copyRecipients ?? (_copyRecipients = new List<string>()));
            set => _copyRecipients = (value ?? new List<string>());
        }

        /// <summary>
        /// Gets or sets the content of email
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the list of recipients for the email message.
        /// </summary>
        public List<string> Recipients
        {
            get => (_recipients ?? (_recipients = new List<string>()));
            set => _recipients = (value ?? new List<string>());
        }

        /// <summary>
        /// Gets or sets the address of the sender of the email message.
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// Gets or sets the subject line of the email message.
        /// </summary>
        public string Subject { get; set; }
    }
}
