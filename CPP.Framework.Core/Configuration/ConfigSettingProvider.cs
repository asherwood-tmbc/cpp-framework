using System;
using System.Configuration;

using CPP.Framework.Services;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Manages and retrieves the configuration and setting values for the application.
    /// </summary>
    [AutoRegisterService]
    public class ConfigSettingProvider : CodeServiceSingleton
    { 
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigSettingProvider"/> class. 
        /// </summary>
        protected ConfigSettingProvider() { }

        /// <summary>
        /// Gets the current instance of the <see cref="ConfigSettingProvider"/> for the 
        /// application.
        /// </summary>
        public static ConfigSettingProvider Current => CodeServiceProvider.GetService<ConfigSettingProvider>();

        /// <summary>
        /// Called by the base class to get the value of a connection string from the underlying
        /// configuration source(s).
        /// </summary>
        /// <param name="name">The name of the connection string.</param>
        /// <param name="value">
        /// An output <see cref="ConnectionStringSettings"/> value that receives the connection 
        /// string settings on success.
        /// </param>
        /// <returns><c>True</c> if the connection string was found; otherwise, <c>false</c>.</returns>
        protected virtual bool GetConnectionString(string name, out ConnectionStringSettings value)
        {
            string envValue = Environment.GetEnvironmentVariable($"SQLAZURECONNSTR_{name}");

            if (envValue is null)
                value = ConfigurationManagerService.Current.ConnectionStrings[name];
            else
                value = new ConnectionStringSettings(name, envValue);

            return (value != null);
        }

        /// <summary>
        /// Gets the connection string settings for a given name.
        /// </summary>
        /// <param name="name">The name of the connection string.</param>
        /// <returns>A <see cref="ConnectionStringSettings"/> object.</returns>
        /// <exception cref="ConfigurationErrorsException">
        ///     <para>A setting for <paramref name="name"/> could not be found in any of the configuration sources.</para>
        ///     <para>-or-</para>
        ///     <para>An error occurred reading the configuration values.</para>
        /// </exception>
        public ConnectionStringSettings GetConnectionString(string name)
        {
            if (!this.GetConnectionString(name, out var settings))
            {
                var message = string.Format(ErrorStrings.MissingConfigurationValue, name);
                throw new ConfigurationErrorsException(message);
            }
            return settings;
        }

        /// <summary>
        /// Called by the base class to load the setting value from the underlying configuration
        /// source(s).
        /// </summary>
        /// <param name="name">The name of the configuration setting.</param>
        /// <param name="value">A variable that receives the setting value on success.</param>
        /// <returns>True if a value for <paramref name="name"/> was found; otherwise, false.</returns>
        protected virtual bool GetConfigSettingValue(string name, out string value)
        {
            value = Environment.GetEnvironmentVariable($"APPSETTING_{name}");

            if (value is null)
                value = ConfigurationManagerService.Current.AppSettings[name];

            return (value != null);
        }

        /// <summary>
        /// Gets the value assigned to a setting.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="configKey">The <see cref="ConfigSettingKey"/> of the setting.</param>
        /// <param name="converter">A callback function that can be used to convert the setting value to the target type.</param>
        /// <returns>The setting value.</returns>
        /// <exception cref="ConfigurationErrorsException">
        ///     <para>A setting for <paramref name="configKey"/> could not be found in any of the configuration sources.</para>
        ///     <para>-or-</para>
        ///     <para>An error occurred reading the configuration values.</para>
        /// </exception>
        public TValue GetSetting<TValue>(ConfigSettingKey configKey, Func<string, TValue> converter)
        {
            return this.GetSetting(configKey, converter, null);
        }

        /// <summary>
        /// Gets the value assigned to a setting.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="configKey">The <see cref="ConfigSettingKey"/> of the setting.</param>
        /// <param name="converter">A callback function that can be used to convert the setting value to the target type.</param>
        /// <param name="defaultValue">The default value to use if a setting for <paramref name="configKey"/> cannot be found.</param>
        /// <returns>The setting value.</returns>
        /// <exception cref="ConfigurationErrorsException">
        ///     <para>A setting for <paramref name="configKey"/> could not be found in any of the configuration sources.</para>
        ///     <para>-or-</para>
        ///     <para>An error occurred reading the configuration values.</para>
        /// </exception>
        public TValue GetSetting<TValue>(ConfigSettingKey configKey, Func<string, TValue> converter, string defaultValue)
        {
            var name = configKey.GetConfigSettingName();
            defaultValue = (defaultValue ?? configKey.GetDefaultValue());
            return this.GetSetting(name, converter, defaultValue);
        }

        /// <summary>
        /// Gets the value assigned to a setting.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the setting.</param>
        /// <param name="converter">A callback function that can be used to convert the setting value to the target type.</param>
        /// <returns>The setting value.</returns>
        /// <exception cref="ConfigurationErrorsException">
        ///     <para>A setting for <paramref name="name"/> could not be found in any of the configuration sources.</para>
        ///     <para>-or-</para>
        ///     <para>An error occurred reading the configuration values.</para>
        /// </exception>
        public TValue GetSetting<TValue>(string name, Func<string, TValue> converter)
        {
            return this.GetSetting(name, converter, null);
        }

        /// <summary>
        /// Gets the value assigned to a setting.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">The name of the setting.</param>
        /// <param name="converter">A callback function that can be used to convert the setting value to the target type.</param>
        /// <param name="defaultValue">The default value to use if a setting for <paramref name="name"/> cannot be found.</param>
        /// <returns>The setting value.</returns>
        /// <exception cref="ConfigurationErrorsException">
        ///     <para>A setting for <paramref name="name"/> could not be found in any of the configuration sources.</para>
        ///     <para>-or-</para>
        ///     <para>An error occurred reading the configuration values.</para>
        /// </exception>
        public TValue GetSetting<TValue>(string name, Func<string, TValue> converter, string defaultValue)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            ArgumentValidator.ValidateNotNull(() => converter);

            if (!this.GetConfigSettingValue(name, out var value))
            {
                if (defaultValue == null)
                {
                    var message = string.Format(ErrorStrings.MissingConfigurationValue, name);
                    throw new ConfigurationErrorsException(message);
                }
                value = defaultValue;
            }
            return converter(value);
        }

        /// <summary>
        /// Gets the value assigned to a setting.
        /// </summary>
        /// <param name="configKey">The <see cref="ConfigSettingKey"/> of the setting.</param>
        /// <returns>The setting value.</returns>
        /// <exception cref="ConfigurationErrorsException">
        ///     <para>A setting for <paramref name="configKey"/> could not be found in any of the configuration sources.</para>
        ///     <para>-or-</para>
        ///     <para>An error occurred reading the configuration values.</para>
        /// </exception>
        public string GetSetting(ConfigSettingKey configKey) { return this.GetSetting(configKey, ((val) => val), null); }

        /// <summary>
        /// Gets the value assigned to a setting.
        /// </summary>
        /// <param name="configKey">The <see cref="ConfigSettingKey"/> of the setting.</param>
        /// <param name="defaultValue">The default value to use if a setting for <paramref name="configKey"/> cannot be found.</param>
        /// <returns>The setting value.</returns>
        /// <exception cref="ConfigurationErrorsException">
        ///     <para>A setting for <paramref name="configKey"/> could not be found in any of the configuration sources.</para>
        ///     <para>-or-</para>
        ///     <para>An error occurred reading the configuration values.</para>
        /// </exception>
        public string GetSetting(ConfigSettingKey configKey, string defaultValue) { return this.GetSetting(configKey, ((val) => val), defaultValue); }

        /// <summary>
        /// Gets the value assigned to a setting.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <returns>The setting value.</returns>
        public string GetSetting(string name) { return this.GetSetting(name, (val) => val); }

        /// <summary>
        /// Gets the value assigned to a setting.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value to use if a setting for <paramref name="name"/> cannot be found.</param>
        /// <returns>The setting value.</returns>
        public string GetSetting(string name, string defaultValue) { return this.GetSetting(name, (val) => val, defaultValue); }

        /// <summary>
        /// Attempts to get the connection string settings for a given name.
        /// </summary>
        /// <param name="name">The name of the configuration setting.</param>
        /// <param name="value">A variable that receives the setting value on success.</param>
        /// <returns>True if a value for <paramref name="name"/> was found; otherwise, false.</returns>
        public bool TryGetConnectionString(string name, out ConnectionStringSettings value)
        {
            return this.GetConnectionString(name, out value);
        }

        /// <summary>
        /// Attempts to retrieve a setting from the application's configuration.
        /// </summary>
        /// <param name="name">
        /// The name of the setting to retrieve.
        /// </param>
        /// <param name="value">
        /// An output parameter that receives the setting value on success.
        /// </param>
        /// <returns>True if a value for <paramref name="name"/> exists; otherwise, false.</returns>
        public bool TryGetSetting(string name, out string value)
        {
            return this.TryGetSetting(name, (s => s), out value);
        }

        /// <summary>
        /// Attempts to retrieve a setting from the application's configuration.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="name">
        /// The name of the setting to retrieve.
        /// </param>
        /// <param name="converter">
        /// A delegate that is used to convert the setting value from a string to the target type.
        /// </param>
        /// <param name="value">
        /// An output parameter that receives the setting value on success.
        /// </param>
        /// <returns>True if a value for <paramref name="name"/> exists; otherwise, false.</returns>
        public bool TryGetSetting<TValue>(string name, Func<string, TValue> converter, out TValue value)
        {
            value = default(TValue);
            try
            {
                value = this.GetSetting(name, converter);
                return true;
            }
            catch (ConfigurationErrorsException)
            {
                return false;
            }
        }
    }
}
