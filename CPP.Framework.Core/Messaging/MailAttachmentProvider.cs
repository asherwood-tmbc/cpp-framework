using System;
using System.IO;
using System.Threading;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Abstract base class for all objects that provide access to the contents of an 
    /// <see cref="IMailMessageAttachment"/> object.
    /// </summary>
    public abstract class MailAttachmentProvider : IDisposable
    {
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
                // ReSharper disable EmptyGeneralCatchClause
                try
                {
                    this.OnDispose();
                }
                catch (Exception)
                {
                }
                // ReSharper restore EmptyGeneralCatchClause
            }
        }

        /// <summary>
        /// Retrieves the content of an attachment.
        /// </summary>
        /// <param name="attachment">The attachment for which to get the content.</param>
        /// <returns>A <see cref="byte"/> array that contains the attachment content.</returns>
        public virtual byte[] GetContent(IMailMessageAttachment attachment)
        {
            using (var target = new MemoryStream())
            using (var source = this.GetContentStream(attachment))
            {
                source.CopyTo(target);
                return target.ToArray();
            }
        }

        /// <summary>
        /// Retrieves the content of an attachment as a stream.
        /// </summary>
        /// <param name="attachment">The attachment for which to get the content.</param>
        /// <returns>A <see cref="Stream"/> object.</returns>
        public abstract Stream GetContentStream(IMailMessageAttachment attachment);

        /// <summary>
        /// Called by the base class to perform application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        protected virtual void OnDispose() { }
    }
}
