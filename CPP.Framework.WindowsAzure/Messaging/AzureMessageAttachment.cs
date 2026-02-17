using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Represents an attachment for an email message whose content is stored in Azure Storage.
    /// </summary>
    [MailAttachmentProvider(typeof(AzureAttachmentProvider))]
    [ExcludeFromCodeCoverage]
    public class AzureMessageAttachment : FileMessageAttachment
    {
    }
}
