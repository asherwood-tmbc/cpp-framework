using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using CPP.Framework.Configuration;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Threading;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Provides functionality to send email messages using an SMTP server.
    /// </summary>
    public class SmtpTransportProvider : MailTransportProvider
    {
        /// <summary>
        /// Sends an email message.
        /// </summary>
        /// <param name="message">An <see cref="IMailMessage"/> object that contains the message contents.</param>
        public override void Send(IMailMessage message)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            ArgumentValidator.ValidateCustom(() => message, this);

            var instance = new System.Net.Mail.MailMessage();
            if (message.CopyRecipients != null)
            {
                instance.CC.AddRange(message.CopyRecipients.Select(s => new MailAddress(s)));
            }
            if (message.BlindRecipients != null)
            {
                instance.Bcc.AddRange(message.BlindRecipients.Select(s => new MailAddress(s)));
            }
            instance.To.AddRange(message.Recipients.Select(s => new MailAddress(s)));

            using (var resolver = ServiceLocator.GetInstance<MailAttachmentResolver>())
            {
                var count = 0;
                foreach (var attachment in message.Attachments)
                {
                    count++;
                    using (var stream = resolver.GetContentStream(attachment))
                    {
                        var filename = attachment.AttachmentName;
                        if (String.IsNullOrWhiteSpace(filename))
                        {
                            filename = String.Format("Attachment{0:D4}", count);
                        }
                        instance.Attachments.Add(new Attachment(stream, filename));
                    }
                }
            }

            var from = message.Sender;
            if (String.IsNullOrWhiteSpace(from))
            {
                from = this.DefaultSender;
            }
            instance.From = new MailAddress(from);
            instance.ReplyToList.Add(instance.From);
            instance.Body = (message.Content ?? String.Empty);
            instance.IsBodyHtml = (!String.IsNullOrWhiteSpace(instance.Body));
            instance.Subject = (message.Subject ?? String.Empty);

            var client = new SmtpClient(this.HostName, this.HostPort);
            if (this.ServerCredentials != null)
            {
                client.UseDefaultCredentials = false;
                client.Credentials = this.ServerCredentials;
            }
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Send(instance);
        }
    }
}
