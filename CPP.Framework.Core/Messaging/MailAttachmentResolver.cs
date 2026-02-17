using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using CPP.Framework.DependencyInjection;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Resolves the contents for one or more <see cref="IMailMessageAttachment"/> objects.
    /// </summary>
    public class MailAttachmentResolver : IDisposable
    {
        /// <summary>
        /// The map of attachment providers to the model type.
        /// </summary>
        private readonly ConcurrentDictionary<Type, MailAttachmentProvider> _attachmentProviderMap = new ConcurrentDictionary<Type, MailAttachmentProvider>();

        /// <summary>
        /// The flag the indicates whether or not the current class has been disposed.
        /// </summary>
        private int _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                this.OnDispose();
                foreach (var provider in _attachmentProviderMap.Values)
                {
                    provider.Dispose();
                }
                _attachmentProviderMap.Clear();
            }
        }

        /// <summary>
        /// Retrieves the content for an email message attachment.
        /// </summary>
        /// <param name="attachment">The <see cref="IMailMessageAttachment"/> instance.</param>
        /// <returns>A <see cref="byte"/> array that contains the attachment content.</returns>
        public virtual byte[] GetContent(IMailMessageAttachment attachment)
        {
            ArgumentValidator.ValidateNotNull(() => attachment);
            var provider = this.GetAttachmentProvider(attachment);
            return provider.GetContent(attachment);
        }

        /// <summary>
        /// Retrieves the content for an email message attachment.
        /// </summary>
        /// <param name="attachment">The <see cref="IMailMessageAttachment"/> instance.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public virtual Stream GetContentStream(IMailMessageAttachment attachment)
        {
            ArgumentValidator.ValidateNotNull(() => attachment);
            var provider = this.GetAttachmentProvider(attachment);
            return provider.GetContentStream(attachment);
        }

        /// <summary>
        /// Gets the cached provider for a given <see cref="IMailMessageAttachment"/>.
        /// </summary>
        /// <param name="attachment">The <see cref="IMailMessageAttachment"/> instance.</param>
        /// <returns>A <see cref="MailAttachmentProvider"/> object.</returns>
        private MailAttachmentProvider GetAttachmentProvider(IMailMessageAttachment attachment)
        {
            var provider = _attachmentProviderMap.GetOrAdd(
                attachment.GetType(),
                ti =>
                {
                    var instance = default(object);
                    var attr = ti.GetCustomAttributes(typeof(MailAttachmentProviderAttribute), true)
                        .OfType<MailAttachmentProviderAttribute>()
                        .FirstOrDefault();
                    if (attr != null)
                    {
                        instance = (!string.IsNullOrWhiteSpace(attr.RegistrationName)
                            ? ServiceLocator.GetInstance(attr.ProviderType, attr.RegistrationName)
                            : ServiceLocator.GetInstance(attr.ProviderType));
                    }
                    else instance = ServiceLocator.GetInstance<FileAttachmentProvider>();
                    return ((MailAttachmentProvider)instance);
                });
            return provider;
        }

        /// <summary>
        /// Called by the base class to perform application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        protected virtual void OnDispose() { }
    }
}
