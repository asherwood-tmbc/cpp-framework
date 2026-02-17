using System;
using System.IO;
using System.Net;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Default attachment provider for files that are accessed through a web <see cref="Uri"/>.
    /// </summary>
    public class WebAttachmentProvider : MailAttachmentProvider
    {
        /// <summary>
        /// Retrieves the content of an attachment as a stream.
        /// </summary>
        /// <param name="attachment">The attachment for which to get the content.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public override Stream GetContentStream(IMailMessageAttachment attachment)
        {
            using (var client = new WebClient())
            {
                var uri = new Uri(attachment.Location, UriKind.Absolute);
                var buffer = client.DownloadData(uri);
                return new MemoryStream(buffer);
            }
        }
    }
}
