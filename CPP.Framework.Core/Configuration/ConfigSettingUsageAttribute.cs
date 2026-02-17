using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Configuration
{
    #region ConfigurationTarget Enum Declaration

    /// <summary>
    /// Defines the possible targets available to the <see cref="ConfigSettingUsageAttribute"/> 
    /// attribute.
    /// </summary>
    public enum ConfigSettingTarget
    {
        /// <summary>
        /// No Target Specified
        /// </summary>
        None,

        /// <summary>
        /// An Azure Cloud Queue Reference
        /// </summary>
        CloudQueueReference,

        /// <summary>
        /// A Connection String
        /// </summary>
        ConnectionString,

        /// <summary>
        /// An Azure Storage Connection String
        /// </summary>
        StorageConnectionString,
    }

    #endregion // ConfigurationTarget Enum Declaration

    /// <summary>
    /// Applied to a <see cref="ConfigSettingKey"/> to identify the configuration category of the 
    /// key.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConfigSettingUsageAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSettingUsageAttribute"/> class. 
        /// </summary>
        /// <param name="target">
        /// The target category assigned to the <see cref="ConfigSettingKey"/>.
        /// </param>
        public ConfigSettingUsageAttribute(ConfigSettingTarget target)
        {
            this.Target = target;
        }

        /// <summary>
        /// Gets the target category assigned to the <see cref="ConfigSettingKey"/>.
        /// </summary>
        public ConfigSettingTarget Target { get; }
    }
}
