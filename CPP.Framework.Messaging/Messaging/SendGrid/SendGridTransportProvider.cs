using System;
using System.Net.Mail;
using CPP.Framework.DependencyInjection;
using SendGridMail.Transport;

using SendGridMessage = SendGridMail.SendGrid;

namespace CPP.Framework.Messaging.SendGrid
{
    /// <summary>
    /// Default provider for sending messages using the SendGrid API.
    /// </summary>
    public class SendGridTransportProvider : SmtpTransportProvider
    {
        private const string DefaultHostName = "smtp.sendgrid.net";

        /// <summary>
        /// Called by the base class to load the configuration settings for the mail server.
        /// </summary>
        /// <returns>A <see cref="MailTransportProvider.MailServerConfig"/> object.</returns>
        protected override MailServerConfig LoadServerConfig()
        {
            var defaults = base.LoadServerConfig();
            var config = new MailServerConfig
            (
                (String.IsNullOrWhiteSpace(defaults.HostName) ? DefaultHostName : defaults.HostName),
                (defaults.HostPort),
                (defaults.Credentials == null ? String.Empty : defaults.Credentials.UserName),
                (defaults.Credentials == null ? String.Empty : defaults.Credentials.Password),
                (defaults.ReplyTo)
            );
            return config;
        }

        /// <summary>
        /// Sends an email message.
        /// </summary>
        /// <param name="message">An <see cref="MailMessage"/> object that contains the message contents.</param>
        public override void Send(IMailMessage message)
        {
            ArgumentValidator.ValidateNotNull(() => message);
            ArgumentValidator.ValidateCustom(() => message, this);

            var instance = SendGridMessage.GetInstance();
            if (message.CopyRecipients != null)
            {
                instance.AddCc(message.CopyRecipients);
            }
            if (message.BlindRecipients != null)
            {
                instance.AddBcc(message.BlindRecipients);
            }
            instance.AddTo(message.Recipients);

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
                        instance.AddAttachment(stream, filename);
                    }
                }
            }

            var from = message.Sender;
            if (String.IsNullOrWhiteSpace(from))
            {
                from = this.DefaultSender;
            }
            instance.From = new MailAddress(from);
            instance.ReplyTo = new[] { instance.From };
            instance.Html = (message.Content ?? String.Empty);
            instance.Subject = (message.Subject ?? String.Empty);

            try
            {
                var transport = SMTP.GetInstance(this.ServerCredentials, this.HostName, this.HostPort);
                transport.Deliver(instance);
            }
            catch (Exception ex) { throw new MailTransportFailureException(ex); }
        }
    }
}
