using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Represents an attachment for an email message whose content is accessed using a web address.
    /// </summary>
    [MailAttachmentProvider(typeof(WebAttachmentProvider))]
    [ExcludeFromCodeCoverage]
    public class WebMessageAttachment : FileMessageAttachment { }
}
