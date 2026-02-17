using System.IO;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Default provider for <see cref="IMailMessageAttachment"/> objects whose content is stored on
    /// the local file system.
    /// </summary>
    public class FileAttachmentProvider : MailAttachmentProvider
    {
        /// <summary>
        /// Retrieves the content of an attachment as a stream.
        /// </summary>
        /// <param name="attachment">The attachment for which to get the content.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public override Stream GetContentStream(IMailMessageAttachment attachment)
        {
            ArgumentValidator.ValidateNotNull(() => attachment);
            return new FileStream(attachment.Location, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
