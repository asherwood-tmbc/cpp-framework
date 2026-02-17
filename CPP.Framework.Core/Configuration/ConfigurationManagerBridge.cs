using System.Configuration;

using Microsoft.Extensions.Configuration;

using SystemConfigurationManager = System.Configuration.ConfigurationManager;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Provides a bridge between System.Configuration and Microsoft.Extensions.Configuration.
    /// </summary>
    public class ConfigurationManagerBridge : IConfigurationSource
    {
        /// <summary>
        /// Builds the configuration provider for this source.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <returns>The configuration provider.</returns>
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new _ConfigurationManagerProvider();
        }

        /// <summary>
        /// A provider that loads configuration settings from System.Configuration.
        /// </summary>
        private class _ConfigurationManagerProvider : ConfigurationProvider
        {
            /// <summary>
            /// Loads the configuration settings from System.Configuration.
            /// </summary>
            public override void Load()
            {
                foreach (var key in SystemConfigurationManager.AppSettings.AllKeys)
                    Data.Add(key, SystemConfigurationManager.AppSettings[key]);

                foreach (ConnectionStringSettings connectionString in SystemConfigurationManager.ConnectionStrings)
                    Data.Add($"ConnectionStrings:{connectionString.Name}", connectionString.ConnectionString);
            }
        }
    }
}