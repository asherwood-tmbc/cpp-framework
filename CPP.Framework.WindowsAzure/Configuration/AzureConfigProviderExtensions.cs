using System;
using System.Configuration;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Defines extension methods for Azure-specific configuration settings.
    /// </summary>
    public static class AzureConfigProviderExtensions
    {
        /// <summary>
        /// Default Microsoft setting name for the Service Bus connection string.
        /// </summary>
        internal const string MSServiceBusConnectionStringSettingName = "Microsoft.ServiceBus.ConnectionString";

        /// <summary>
        /// Gets the currently configured connection string for the Azure Service Bus.
        /// </summary>
        /// <param name="provider">The current <see cref="ConfigSettingProvider"/> instance that contains the configuration settings.</param>
        /// <returns>The service bus connection string.</returns>
        public static string GetServiceBusConnectionString(this ConfigSettingProvider provider)
        {
            ArgumentValidator.ValidateThisObj(() => provider);
            try
            {
                return provider.GetSetting(ConfigSettingKey.ServiceBusConnectionString);
            }
            catch (ConfigurationErrorsException)
            {
                // ReSharper disable EmptyGeneralCatchClause
                try
                {
                    return provider.GetSetting(MSServiceBusConnectionStringSettingName);
                }
                catch (Exception)
                {
                    /* ignored */
                }
                // ReSharper restore EmptyGeneralCatchClause
                throw;  // the backup didn't work, so throw the original exception
            }
        }
    }
}
