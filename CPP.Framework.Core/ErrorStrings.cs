using System;
using System.Diagnostics.CodeAnalysis;

using CPP.Framework.Services;

namespace CPP.Framework
{
    /// <summary>
    /// Contains the internal error message strings for any of the <see cref="Exception"/> objects 
    /// thrown by the current library.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class ErrorStrings
    {
        /// <summary>
        /// The timeout period elapsed before the lock could be acquired.
        /// </summary>
        internal const string AcquireLockTimedOut = "The timeout period elapsed before the lock could be acquired.";

        /// <summary>
        /// The type {0} has multiple constructors of length {1}. Unable to disambiguate.
        /// </summary>
        internal const string AmbiguousInjectionConstructor = "The type {0} has multiple constructors of length {1}. Unable to disambiguate.";

        /// <summary>
        /// The request value could not be decrypted. Please check the inner exception for more details.
        /// </summary>
        internal const string CannotDecryptMessagePayload = "The request value could not be decrypted. Please check the inner exception for more details.";

        /// <summary>
        /// The request value could not be encrypted. Please check the inner exception for more details.
        /// </summary>
        internal const string CannotEncryptMessagePayload = "The request value could not be encrypted. Please check the inner exception for more details.";

        /// <summary>
        /// The column names cannot be changed in the current stream state.
        /// </summary>
        internal const string CannotUpdateColumnNames = "The column names cannot be changed in the current stream state.";

        /// <summary>
        /// Failed to create an activator for the service "{0}"; please consult the inner exception
        /// for more information.
        /// </summary>
        internal const string CreateServiceActivatorFailed = "Failed to create an activator for the service provider class \"{0}\"; please consult the inner exception for more information.";

        /// <summary>
        /// The guid value cannot be all zeros.
        /// </summary>
        internal const string EmptyGuidValue = "The guid value cannot be all zeros.";

        /// <summary>
        /// The string value cannot be empty.
        /// </summary>
        internal const string EmptyStringValue = "The string value cannot be empty.";

        /// <summary>
        /// The identity assigned to the principal could not be authorized.
        /// </summary>
        internal const string IdentityAuthorizationFailed = "The identity assigned to the principal could not be authorized.";

        /// <summary>
        /// The provider cannot be registered because it is not compatible with the service 
        /// interface.
        /// </summary>
        internal const string IncompatibleProviderServiceType = "The provider cannot be registered because it is not compatible with the service interface.";

        /// <summary>
        /// The configuration setting key "{0}" is not valid.
        /// </summary>
        internal const string InvalidConfigSettingKey = "The configuration setting key \"{0}\" is not valid.";

        /// <summary>
        /// The configuration key "{0:G}" is not targeted for the "{1:G}" category.
        /// </summary>
        internal const string InvalidConfigSettingKeyTarget = "The configuration key \"{0:G}\" is not targeted for the \"{1:G}\" category.";

        /// <summary>
        /// The requested input value cannot be processed by the current bundle (v{0}).
        /// </summary>
        internal const string InvalidCryptoMessagePayload = "The requested input value cannot be processed by the current bundle (v{0}).";

        /// <summary>
        /// The input value must contain at least 16 bytes.
        /// </summary>
        internal const string InsufficientCryptoBufferSize = "The input value must contain at least 16 bytes.";

        /// <summary>
        /// The input value is not a valid string of 32 hexadecimal characters.
        /// </summary>
        internal const string InvalidCryptoThumbprintFormat = "The input value is not a valid string of 32 hexidecimal characters.";

        /// <summary>
        /// The authentication for the identity assigned to the principal is invalid.
        /// </summary>
        internal const string InvalidIdentityAuthentication = "The identity assigned to the principal could not be authenticated.";

        /// <summary>
        /// Unable to load the dependency injection configuration \"{0}\" from the application settings file.
        /// </summary>
        internal const string InvalidInjectionConfig = "Unable to load the dependency injection configuration \"{0}\" from the application settings file.";

        /// <summary>
        /// Unable to select the key for object of type "{0}" using trait "{1}" because the delegate function is null.
        /// </summary>
        internal const string InvalidKeySelectorDelegate = "Unable to select the key for object of type \"{0}\" using trait \"{1}\" because the delegate function is null.";

        /// <summary>
        /// The type "{0}" is not a valid MailAttachmentProvider.
        /// </summary>
        internal const string InvalidMailAttachmentProvider = "The type \"{0}\" is not a valid MailAttachmentProvider.";

        /// <summary>
        /// The list of recipients for the mail message cannot be empty.
        /// </summary>
        internal const string InvalidMailRecipientList = "The list of recipients for the mail message cannot be empty.";

        /// <summary>
        /// The address of the mail server is missing or invalid.
        /// </summary>
        internal const string InvalidMailServerAddress = "The address of the mail server is missing or invalid.";

        /// <summary>
        /// The expression is not a valid member-access expression.
        /// </summary>
        internal const string InvalidMemberAccessExpression = "The expression is not a valid member-access expression.";

        /// <summary>
        /// The expression is not a valid method call expression.
        /// </summary>
        internal const string InvalidMethodCallExpression = "The expression is not a valid method call expression.";

        /// <summary>
        /// The encoded XML for the permission class is not valid.
        /// </summary>
        internal const string InvalidPermissionSecurityXml = "The security XML for the permission class is not valid.";

        /// <summary>
        /// The security XML contains a claim element that is missing the required "{0}" attribute.
        /// </summary>
        internal const string InvalidPermissionClaimXml = "The security XML contains a claim element that is missing the required \"{0}\" attribute.";

        /// <summary>
        /// The security XML references class "{0}", but "{1}" was excepted.
        /// </summary>
        internal const string InvalidPermissionClassXml = "The security XML references class \"{0}\", but \"{1}\" was excepted.";

        /// <summary>
        /// The member provided is not a valid property or field definition.
        /// </summary>
        internal const string InvalidPropertyOrFieldMemberInfo = "The member provided is not a valid property or field definition.";

        /// <summary>
        /// The provider type "{0}" is not assignable to interface type "{1}"
        /// </summary>
        internal const string InvalidProviderInterfaceType = "The provider type \"{0}\" is not assignable to interface type \"{1}\"";

        /// <summary>
        /// The service class "{0}" must have either a parameterless default constructor, or provide 
        /// a static creation method decorated with the <see cref="CreateServiceInstanceAttribute"/>
        /// attribute.
        /// </summary>
        internal const string InvalidServiceClassDefinition = "The service class \"{0}\" must have either a parameterless default contructor, or provide a static creation method decorated with the CreateServiceInstanceAttribute attribute.";

        /// <summary>
        /// Cannot change state from {0} to {1} because the transition is not allowed.
        /// </summary>
        internal const string InvalidStateChange = "Cannot change state from {0} to {1} because the transition is not allowed.";

        /// <summary>
        /// The contents of the input stream are not in the expected format.
        /// </summary>
        internal const string InvalidStreamContentFormat = "The contents of the input stream are not in the expected format.";

        /// <summary>
        /// The configuration setting "{0}" is missing is invalid.
        /// </summary>
        internal const string MissingConfigurationValue = "The configuration setting \"{0}\" is missing is invalid.";

        /// <summary>
        /// There is no implementation registered for the service interface "{0}."
        /// </summary>
        internal const string MissingServiceRegistration = "There is no implementation registered for the service interface \"{0}.\"";

        /// <summary>
        /// The type {0} has multiple constructors marked with either the InjectionConstructor or ServiceConstructor attribute. Unable to disambiguate.
        /// </summary>
        internal const string MultipleInjectionConstructors = "The type {0} has multiple constructors marked with either the InjectionConstructor or ServiceConstructor attribute. Unable to disambiguate.";

        /// <summary>
        /// The id {0:B} is reserved for the pre-defined constant {1}, and cannot be assigned to a bundle directly.
        /// </summary>
        internal const string ReservedCryptoBundleVersion = "The id {0:B} is reserved for the pre-defined constant {1}, and cannot be assigned to a bundle directly.";

        /// <summary>
        /// The security claim with the type "{0}" could be found on the requested identity, or there is more than one assigned.
        /// </summary>
        internal const string SecurityClaimTypeNotFound = "The security claim with the type \"{0}\" could be found on the requested identity, or there is more than one assigned.";

        /// <summary>
        /// The value could not be encrypted because there are no suitable bundles available process it.
        /// </summary>
        internal const string SuitableCryptoBundleUnavailable = "The value could not be encrypted because there are no suitable bundles available.";

        /// <summary>
        /// An unexpected error occurred reading the application configuration settings.
        /// </summary>
        internal const string UnexpectedConfigurationError = "An unexpected error occurred reading the application configuration settings.";

        /// <summary>
        /// A certificate with the thumbprint {2} could not be found. name: {0}; location: {1};
        /// </summary>
        internal const string UnknownCertificateThumbprint = "A certificate with the thumbprint {2} could not be found. name: {0}; location: {1};";

        /// <summary>
        /// Unable to locate a crypto bundle with a version number of {0}.
        /// </summary>
        internal const string UnknownCryptoBundleVersion = "Unable to locate a crypto bundle with a version number of {0}.";

        /// <summary>
        /// The string value cannot be all whitespace characters.
        /// </summary>
        internal const string WhiteSpaceStringValue = "The string value cannot be all whitespace characters.";
    }
}
