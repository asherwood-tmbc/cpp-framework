using CPP.Framework.Configuration;

namespace Microsoft.Extensions.Configuration
{
    /// <summary>
    /// Provides extension methods for adding the ConfigurationManagerBridge to the IConfigurationBuilder.
    /// </summary>
    public static class ConfigurationManagerBridgeBuilderExtensions
    {
        /// <summary>
        /// Adds the ConfigurationManagerBridge to the IConfigurationBuilder.
        /// </summary>
        /// <param name="configurationBuilder">The configuration builder.</param>
        /// <returns>The configuration builder with the ConfigurationManagerBridge added.</returns>
        public static IConfigurationBuilder AddConfigurationManagerBridge(this IConfigurationBuilder configurationBuilder)
        {
            return configurationBuilder.Add(new ConfigurationManagerBridge());
        }
    }
}