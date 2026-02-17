using Microsoft.Azure;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Provides abstracted access to the static properties, events, and methods of the Windows
    /// Azure <see cref="CloudConfigurationManager"/> class.
    /// </summary>
    public class CloudConfigurationManagerService : SingletonServiceBase, IConfiguarationManagerService
    {
        private static readonly ServiceInstance<CloudConfigurationManagerService> _ServiceInstance = new ServiceInstance<CloudConfigurationManagerService>();

        /// <summary>
        /// Initializes an instance of the class.
        /// </summary>
        protected CloudConfigurationManagerService() { }

        /// <summary>
        /// Gets a reference to the service instance for the application.
        /// </summary>
        public static CloudConfigurationManagerService Current => _ServiceInstance.GetInstance();

        /// <summary>
        /// Gets a flag that indicates whether or not the service is available within the current 
        /// execution environment.
        /// </summary>
        public virtual bool IsAvailable => true;

        /// <summary>
        /// Gets a configuration setting with a given name.
        /// </summary>
        /// <param name="configurationSettingName">The name of the setting to get.</param>
        /// <returns>A string that contains the setting value.</returns>
        public virtual string GetConfigurationSettingValue(string configurationSettingName) => CloudConfigurationManager.GetSetting(configurationSettingName);
    }
}
