using System.Configuration;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Abstract interfaces for all classes that implement services around a static configuration
    /// manager class (like <see cref="ConfigurationManager"/>) for injecting application settings
    /// at runtime.
    /// </summary>
    public interface IConfiguarationManagerService
    {
        /// <summary>
        /// Gets a value indicating whether or not the service is available within the current 
        /// execution environment.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Retrieves the value of a setting from the application's current configuration.
        /// </summary>
        /// <param name="configurationSettingName">The name of the configuration setting.</param>
        /// <returns>A String that contains the value of the configuration setting.</returns>
        string GetConfigurationSettingValue(string configurationSettingName);
    }
}
