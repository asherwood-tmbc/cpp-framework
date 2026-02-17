using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Provides abstracted access to the static properties, events, and methods of the Windows
    /// Azure <see cref="ConfigurationManager"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConfigurationManagerService : SingletonServiceBase
    {
        /// <summary>
        /// The current instance of the <see cref="ConfigurationManagerService"/> for the 
        /// application.
        /// </summary>
        private static readonly ServiceInstance<ConfigurationManagerService> _ServiceInstance = new ServiceInstance<ConfigurationManagerService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationManagerService"/> class. 
        /// </summary>
        protected ConfigurationManagerService() { }

        /// <summary>
        /// Gets the current instance of the <see cref="ConfigurationManagerService"/> for the 
        /// application.
        /// </summary>
        public static ConfigurationManagerService Current => _ServiceInstance.GetInstance();

        /// <summary>
        /// Gets the <see cref="AppSettingsSection"/> data for the current application's default configuration.
        /// </summary>
        /// <returns>A <see cref="NameValueCollection"/> object that contains the contents of the <see cref="AppSettingsSection"/> object for the current application's default configuration.</returns>
        /// <exception cref="ConfigurationErrorsException">Could not retrieve a <see cref="NameValueCollection"/> object with the application settings data.</exception>
        public virtual NameValueCollection AppSettings => ConfigurationManager.AppSettings;

        /// <summary>
        /// Gets the <see cref="ConnectionStringsSection"/> data for the current application's 
        /// default configuration.
        /// </summary>
        /// <returns>A <see cref="ConnectionStringSettingsCollection"/> object that contains the contents of the <see cref="ConnectionStringsSection"/> object for the current application's default configuration.</returns>
        /// <exception cref="ConfigurationErrorsException">Could not retrieve a <see cref="ConnectionStringSettingsCollection"/> object.</exception>
        public virtual ConnectionStringSettingsCollection ConnectionStrings => ConfigurationManager.ConnectionStrings;

        /// <summary>
        /// Retrieves a specified configuration section for the current application's default 
        /// configuration.
        /// </summary>
        /// <param name="sectionName">The configuration section path and name.</param>
        /// <returns>The specified <see cref="ConfigurationSection"/> object, or null if the section does not exist.</returns>
        /// <exception cref="ConfigurationErrorsException">A configuration file could not be loaded.</exception>
        public virtual object GetSection(string sectionName)
        {
            return ConfigurationManager.GetSection(sectionName);
        }

        /// <summary>
        /// Opens the configuration file for the current application as a <see cref="Configuration"/> 
        /// object.
        /// </summary>
        /// <param name="userLevel">The <see cref="ConfigurationUserLevel"/> for which you are opening the configuration.</param>
        /// <returns>A <see cref="Configuration"/> object.</returns>
        /// <exception cref="ConfigurationErrorsException">A configuration file could not be loaded.</exception>
        public virtual System.Configuration.Configuration OpenExeConfiguration(ConfigurationUserLevel userLevel)
        {
            return ConfigurationManager.OpenExeConfiguration(userLevel);
        }

        /// <summary>
        /// Opens the specified client configuration file as a <see cref="Configuration"/> object.
        /// </summary>
        /// <param name="exePath">The path of the executable (exe) file.</param>
        /// <returns>A <see cref="Configuration"/> object.</returns>
        /// <exception cref="ConfigurationErrorsException">A configuration file could not be loaded.</exception>
        public virtual System.Configuration.Configuration OpenExeConfiguration(string exePath)
        {
            return ConfigurationManager.OpenExeConfiguration(exePath);
        }

        /// <summary>
        /// Opens the machine configuration file on the current computer as a 
        /// <see cref="Configuration"/> object.
        /// </summary>
        /// <returns>A <see cref="Configuration"/> object.</returns>
        /// <exception cref="ConfigurationErrorsException">A configuration file could not be loaded.</exception>
        public virtual System.Configuration.Configuration OpenMachineConfiguration()
        {
            return ConfigurationManager.OpenMachineConfiguration();
        }

        /// <summary>
        /// Opens the specified client configuration file as a <see cref="Configuration"/> object 
        /// that uses the specified file mapping and user level.
        /// </summary>
        /// <param name="fileMap">An <see cref="ExeConfigurationFileMap"/> object that references configuration file to use instead of the application default configuration file.</param>
        /// <param name="userLevel">The <see cref="ConfigurationUserLevel"/> object for which you are opening the configuration.</param>
        /// <returns>The configuration object.</returns>
        /// <exception cref="ConfigurationErrorsException">A configuration file could not be loaded.</exception>
        public virtual System.Configuration.Configuration OpenMappedExeConfiguration(ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel)
        {
            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, userLevel);
        }

        /// <summary>
        /// Opens the specified client configuration file as a <see cref="Configuration"/> object 
        /// that uses the specified file mapping, user level, and preload option.
        /// </summary>
        /// <param name="fileMap">An <see cref="ExeConfigurationFileMap"/> object that references configuration file to use instead of the application default configuration file.</param>
        /// <param name="userLevel">The <see cref="ConfigurationUserLevel"/> object for which you are opening the configuration.</param>
        /// <param name="preLoad">True to preload all section groups and sections; otherwise, false.</param>
        /// <returns>The configuration object.</returns>
        /// <exception cref="ConfigurationErrorsException">A configuration file could not be loaded.</exception>
        public virtual System.Configuration.Configuration OpenMappedExeConfiguration(ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel, bool preLoad)
        {
            return ConfigurationManager.OpenMappedExeConfiguration(fileMap, userLevel, preLoad);
        }

        /// <summary>
        /// Opens the machine configuration file as a <see cref="Configuration"/> object that uses 
        /// the specified file mapping.
        /// </summary>
        /// <param name="fileMap">An <see cref="ExeConfigurationFileMap"/> object that references configuration file to use instead of the application default configuration file.</param>
        /// <returns>A <see cref="Configuration"/> object.</returns>
        /// <exception cref="ConfigurationErrorsException">A configuration file could not be loaded.</exception>
        public virtual System.Configuration.Configuration OpenMappedMachineConfiguration(ConfigurationFileMap fileMap)
        {
            return ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
        }

        /// <summary>
        /// Refreshes the named section so the next time that it is retrieved it will be re-read 
        /// from disk.
        /// </summary>
        /// <param name="sectionName">The configuration section name or the configuration path and section name of the section to refresh.</param>
        public virtual void RefreshSection(string sectionName) { ConfigurationManager.RefreshSection(sectionName); }
    }
}
