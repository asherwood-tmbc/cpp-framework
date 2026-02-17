namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Abstract base interfaces for all classes that represent an attachment for an email message.
    /// </summary>
    public interface IMailMessageAttachment
    {
        /// <summary>
        /// Gets the name of the attachment for the email message.
        /// </summary>
        string AttachmentName { get; }

        /// <summary>
        /// Gets the location of the attachment content.
        /// </summary>
        string Location { get; }
    }
}
