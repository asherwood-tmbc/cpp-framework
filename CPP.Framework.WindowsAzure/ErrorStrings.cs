using System.Diagnostics.CodeAnalysis;

using CPP.Framework.WindowsAzure.ServiceBus;
using CPP.Framework.WindowsAzure.ServiceBus.Queue;

namespace CPP.Framework
{
    [ExcludeFromCodeCoverage]
    internal static class ErrorStrings
    {
        /// <summary>
        /// Unable to process message {0} because the process has received a request to shut down.
        /// </summary>
        internal const string AzureServiceBusConnectionShutdown = "Unable to process message {0} because the process has received a request to shut down.";

        /// <summary>
        /// The requested operation could not be completed because the storage object has one or
        /// more external references attached.
        /// </summary>
        internal const string AzureStorageObjectHasExternalReferences = "The requested operation could not be completed because the storage object has one or more external references attached.";

        /// <summary>
        /// Unable to locate a value for the metadata property {0}.{1} in table storage.
        /// </summary>
        internal const string AzureTablePropertyNotFound = "Unable to locate a value for the metadata property {0}.{1} in table storage.";

        /// <summary>
        /// The property {0}.{1} returns a type that requires a format string, but none was
        /// provided--ignoring.
        /// </summary>
        internal const string InvalidBrokeredMessagePropertyType = "The property {0}.{1} returns a type that requires a format string, but none was provided--ignoring.";

        /// <summary>
        /// The requested entity could not be attached as an external reference to the blob because
        /// it does not have a valid id.
        /// </summary>
        internal const string InvalidAzureBlobEntityReference = "The requested entity could not be attached as an external reference to the blob because it does not have a valid id.";

        /// <summary>
        /// The path "{0}" is not a valid blob storage location.
        /// </summary>
        internal const string InvalidAzureBlobLocation = "The path \"{0}\" is not a valid blob storage location.";

        /// <summary>
        /// The value "{0}" is not a valid blob storage container name.
        /// </summary>
        internal const string InvalidAzureContainerName = "The value \"{0}\" is not a valid blob storage container name.";

        /// <summary>
        /// The duration of the lease must be between 15 to 60 seconds, or infinite (i.e. -1, Timeout.InfiniteTimeSpan, or Timeout.Infinite).
        /// </summary>
        internal const string InvalidAzureLeaseDuration = "The duration of the lease must be either infinite, or between 15 to 60 seconds.";

        /// <summary>
        /// The requested metadata property value cannot be cast to type {0}.
        /// </summary>
        internal const string InvalidAzureMetadataPropertyValueCast = "The requested metadata property value cannot be cast to type {0}.";

        /// <summary>
        /// The type \"{0}\" is not a valid table entity key type.
        /// </summary>
        internal const string InvalidAzureTableEntityKeyType = "The type \"{0}\" is not a valid table entity key type.";

        /// <summary>
        /// The partition key for the entity is missing, or invalid.
        /// </summary>
        internal const string InvalidAzureTablePartitionKey = "The partition key for the entity is missing, or invalid.";

        /// <summary>
        /// The row key for the entity is missing, or invalid.
        /// </summary>
        internal const string InvalidAzureTableRowKey = "The row key for the entity is missing, or invalid.";

        /// <summary>
        /// Only the characters A-Z, 0-9, "-", "_", ".", "/", or "~" are valid in an Azure Storage path.
        /// </summary>
        internal const string InvalidAzurePathCharacters = "Only the characters A-Z, 0-9, \"-\", \"_\", \".\", \"/\", or \"~\" are valid in an Azure Storage path.";

        /// <summary>
        /// Unable to serialize property "{0}.{1}" of type "{2}" because it is a complex type.
        /// </summary>
        internal const string InvalidAzurePropertyType = "Unable to serialize property \"{0}.{1}\" of type \"{2}\" because it is a complex type.";

        /// <summary>
        /// The contents of the queue message are missing or invalid.
        /// </summary>
        internal const string InvalidAzureQueueMessage = "The contents of the queue message are missing or invalid.";

        /// <summary>
        /// The message type {0} must be decorated with a <see cref="AzureServiceBusQueueAttribute"/> 
        /// attribute.
        /// </summary>
        internal const string InvalidAzureQueueMessageMetadata = "The message type {0} must be decorated with a " + nameof(AzureServiceBusQueueAttribute) + " attribute.";

        /// <summary>
        /// The message type {0} must be decorated with a <see cref="AzureServiceBusTopicAttribute"/> 
        /// attribute.
        /// </summary>
        internal const string InvalidAzureTopicMessageMetadata = "The message type {0} must be decorated with a " + nameof(AzureServiceBusTopicAttribute) + " attribute.";

        /// <summary>
        /// The expression is not a valid property access expression.
        /// </summary>
        internal const string InvalidPropertyAccessExpression = "The expression is not a valid property access expression.";

        /// <summary>
        /// The account associated with the subscription does not match the one used to create the job host.
        /// </summary>
        internal const string JobHostAccountMismatch = "The account associated with the subscription does not match the one used to create the job host.";

        /// <summary>
        /// The schedule date of "{0}" exceeds the maximum allowable timespan of 7 days.
        /// </summary>
        internal const string MaxScheduleDateExceeded = "The schedule date of \"{0}\" exceeds the maximum allowable timespan of 7 days.";
    }
}
