using System;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.DependencyInjection;

namespace CPP.Framework.Messaging
{
    /// <summary>
    /// Applied to a <see cref="IMailMessageAttachment"/> class to indicate what type of 
    /// <see cref="MailAttachmentProvider"/> is used to retrieve its content.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [ExcludeFromCodeCoverage]
    public class MailAttachmentProviderAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MailAttachmentProviderAttribute"/> class. 
        /// </summary>
        /// <param name="providerType">
        /// The type of the attachment provider.
        /// </param>
        public MailAttachmentProviderAttribute(Type providerType) : this(providerType, string.Empty) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MailAttachmentProviderAttribute"/> class. 
        /// </summary>
        /// <param name="providerType">The type of the attachment provider.</param>
        /// <param name="registrationName">The name of the registration to use when resolving the provider instance with the <see cref="ServiceLocator"/>.</param>
        public MailAttachmentProviderAttribute(Type providerType, string registrationName)
        {
            ArgumentValidator.ValidateNotNull(() => providerType);
            if (!typeof(MailAttachmentProvider).IsAssignableFrom(providerType))
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(
                    () => providerType,
                    ErrorStrings.InvalidMailAttachmentProvider,
                    providerType.FullName);
            }
            this.ProviderType = providerType;
            this.RegistrationName = (registrationName ?? string.Empty);
        }

        /// <summary>
        /// Gets the <see cref="Type"/> of the attachment provider.
        /// </summary>
        public Type ProviderType { get; }

        /// <summary>
        /// Gets the name of the registration to use when resolving the provider instance with the
        /// <see cref="ServiceLocator"/>.
        /// </summary>
        public string RegistrationName { get; }
    }
}
