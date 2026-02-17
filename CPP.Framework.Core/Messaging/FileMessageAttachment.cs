using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Represents an attachment for an email message whose content is stored as a file stream.
    /// </summary>
    [MailAttachmentProvider(typeof(FileAttachmentProvider))]
    [ExcludeFromCodeCoverage]
    public class FileMessageAttachment : IMailMessageAttachment
    {
        /// <summary>
        /// Gets or sets the name of the attachment for the email message.
        /// </summary>
        public string AttachmentName { get; set; }

        /// <summary>
        /// Gets or sets the location of the attachment content.
        /// </summary>
        public string Location { get; set; }
    }
}
