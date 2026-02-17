using CPP.Framework.Diagnostics;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Configuration provider for applications that are hosted in an Azure WebJobs environment.
    /// </summary>
    public class CloudConfigProvider : ConfigSettingProvider
    {
        /// <summary>
        /// Called by the base class to perform any initialization tasks when the instance is being
        /// created.
        /// </summary>
        protected override void StartupInstance()
        {
            Journal.ConfigureInstanceName(
                () =>
                    {
                        var applicationName = default(string);
                        if (RoleEnvironmentService.Current.IsAvailable)
                        {
                            applicationName = RoleEnvironmentService.Current?.CurrentRoleInstance?.Role?.Name;
                        }
                        return applicationName;
                    });
            base.StartupInstance();
        }

        /// <summary>
        /// Called by the base class to load the setting value from the underlying configuration
        /// source(s).
        /// </summary>
        /// <param name="name">The name of the configuration setting.</param>
        /// <param name="value">A variable that receives the setting value on success.</param>
        /// <returns>True if a value for <paramref name="name"/> was found; otherwise, false.</returns>
        protected override bool GetConfigSettingValue(string name, out string value)
        {
            if ((value = CloudConfigurationManagerService.Current.GetConfigurationSettingValue(name)) == null)
            {
                return base.GetConfigSettingValue(name, out value);
            }
            return true;
        }
    }
}
