using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.WindowsAzure.Storage
{
    /// <summary>
    /// Thrown when a metadata property on an Azure Storage table is missing or invalid.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AzureTablePropertyNotFoundException : Exception
    {
        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="tableName">The name of the table associated with the metadata property.</param>
        /// <param name="propertyName">The name of the target metadata property.</param>
        public AzureTablePropertyNotFoundException(string tableName, string propertyName) : this(FormatMessage(tableName, propertyName), (Exception)null) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="tableName">The name of the table associated with the metadata property.</param>
        /// <param name="propertyName">The name of the target metadata property.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        public AzureTablePropertyNotFoundException(string tableName, string propertyName, Exception innerException) : this(FormatMessage(tableName, propertyName), innerException) { }

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        /// <param name="message">The message text for the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        protected AzureTablePropertyNotFoundException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected AzureTablePropertyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Formats the error message for the exception.
        /// </summary>
        /// <param name="tableName">The name of the table associated with the metadata property.</param>
        /// <param name="propertyName">The name of the target metadata property.</param>
        /// <returns>A string that contains the error message.</returns>
        private static string FormatMessage(string tableName, string propertyName)
        {
            return String.Format(ErrorStrings.AzureTablePropertyNotFound, tableName, propertyName);
        }
    }
}
