using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.DependencyInjection
{
    /// <summary>
    /// Exception that is thrown when the configuration settings for a dependency injection 
    /// container are invalid, or cannot be loaded.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InvalidInjectionConfigException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInjectionConfigException"/> class. 
        /// </summary>
        /// <param name="configuration">
        /// The name of the configuration that generated the exception.
        /// </param>
        public InvalidInjectionConfigException(string configuration)
            : base(FormatMessage(configuration))
        {
            this.ConfigurationName = (configuration ?? string.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInjectionConfigException"/> class. 
        /// </summary>
        /// <param name="configuration">The name of the configuration that generated the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        public InvalidInjectionConfigException(string configuration, Exception innerException)
            : base(FormatMessage(configuration), innerException)
        {
            this.ConfigurationName = (configuration ?? string.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInjectionConfigException"/> class. 
        /// </summary>
        /// <param name="configuration">The name of the configuration that generated the exception.</param>
        /// <param name="message">The message text for the exception.</param>
        public InvalidInjectionConfigException(string configuration, string message)
            : base(message)
        {
            this.ConfigurationName = (configuration ?? string.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInjectionConfigException"/> class. 
        /// </summary>
        /// <param name="configuration">The name of the configuration that generated the exception.</param>
        /// <param name="message">The message text for the exception.</param>
        /// <param name="innerException">The <see cref="Exception"/> that caused the current exception.</param>
        public InvalidInjectionConfigException(string configuration, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ConfigurationName = (configuration ?? string.Empty);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInjectionConfigException"/> class 
        /// with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected InvalidInjectionConfigException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.ConfigurationName = info.GetString("ConfigurationName");
        }

        /// <summary>
        /// Gets the name of the configuration that generated the exception.
        /// </summary>
        public string ConfigurationName { get; }

        /// <summary>
        /// Formats the default message text for the exception.
        /// </summary>
        /// <param name="configuration">The name of the configuration that generated the exception.</param>
        /// <returns>The message text for the exception.</returns>
        private static string FormatMessage(string configuration)
        {
            return string.Format(ErrorStrings.InvalidInjectionConfig, configuration);
        }

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> object with the parameter name and additional 
        /// exception information.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> object is a null reference (Nothing in Visual Basic).</exception>
        /// <PermissionSet><IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/></PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ConfigurationName", this.ConfigurationName);
            base.GetObjectData(info, context);
        }
    }
}
