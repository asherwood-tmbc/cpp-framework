using System;
using System.Collections.Specialized;
using System.Configuration;

using CPP.Framework.Configuration;
using CPP.Framework.DependencyInjection;

using Rhino.Mocks;

namespace CPP.Framework.Diagnostics.Testing
{
    /// <summary>
    /// Provides extension methods used to stub configuration setting values.
    /// </summary>
    public static class ConfigSettingsStubExtensions
    {
        /// <summary>
        /// Gets a reference to the <see cref="NameValueCollection"/> that contains the stubbed
        /// settings for the <see cref="ConfigurationManagerService.AppSettings"/> property.
        /// </summary>
        /// <param name="service">The service instance to stub.</param>
        /// <returns>A <see cref="NameValueCollection"/> object.</returns>
        private static NameValueCollection GetAppSettingsStub(ConfigurationManagerService service)
        {
            const string AppSettingsKey = "App.Config.Settings";

            ArgumentValidator.ValidateNotNull(() => service);
            if (!ServiceLocator.IsRegistered<NameValueCollection>(AppSettingsKey))
            {
                var collection = StubFactory.CreatePartial<NameValueCollection>();
                ServiceLocator.Register(collection, AppSettingsKey);
                service.StubAction(stub => stub.AppSettings).Return(collection);
            }

            return ServiceLocator.GetInstance<NameValueCollection>(AppSettingsKey);
        }

        /// <summary>
        /// Gets a reference to the <see cref="ConnectionStringSettingsCollection"/> that contains 
        /// the stubbed settings for the <see cref="ConfigurationManagerService.ConnectionStrings"/> 
        /// property.
        /// </summary>
        /// <param name="service">The service instance to stub.</param>
        /// <returns>A <see cref="ConnectionStringSettingsCollection"/> object.</returns>
        private static ConnectionStringSettingsCollection GetConnectionStringSettingsStub(ConfigurationManagerService service)
        {
            const string ConnectionStringSettingsKey = "App.Config.Connections";

            ArgumentValidator.ValidateNotNull(() => service);
            if (!ServiceLocator.IsRegistered<ConnectionStringSettingsCollection>(ConnectionStringSettingsKey))
            {
                var collection = new ConnectionStringSettingsCollection();
                ServiceLocator.Register(collection, ConnectionStringSettingsKey);
                service.StubAction(stub => stub.ConnectionStrings).Return(collection);
            }

            return ServiceLocator.GetInstance<ConnectionStringSettingsCollection>(ConnectionStringSettingsKey);
        }

        /// <summary>
        /// Configures a connection string setting at runtime to return a specific value.
        /// </summary>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="setting">The <see cref="ConnectionStringSettings"/> to return.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static ConfigurationManagerService StubConnectionString(this ConfigurationManagerService service, ConnectionStringSettings setting)
        {
            ArgumentValidator.ValidateThisObj(() => service);
            ArgumentValidator.ValidateNotNull(() => setting);
            
            var collection = GetConnectionStringSettingsStub(service);
            if (collection[setting.Name] == null)
            {
                collection.Add(setting);
            }
            return service;
        }

        /// <summary>
        /// Configures a connection string setting at runtime to return a specific value.
        /// </summary>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="name">The name of the target connection.</param>
        /// <param name="connectionString">The connection string value to return.</param>
        /// <param name="providerName">The name of the data provider to return.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static ConfigurationManagerService StubConnectionString(this ConfigurationManagerService service, string name, string connectionString, string providerName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => connectionString);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => providerName);

            var setting = new ConnectionStringSettings(name, connectionString, providerName);
            StubConnectionString(service, setting);
            return service;
        }

        /// <summary>
        /// Configures a setting to throw an exception when the value for it is requested.
        /// </summary>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="configKey">The <see cref="ConfigSettingKey"/> of the target setting.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static ConfigurationManagerService StubConfigSetting(this ConfigurationManagerService service, ConfigSettingKey configKey)
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), (() => new Exception()));
        }

        /// <summary>
        /// Configures a setting to throw an exception when the value for it is requested.
        /// </summary>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="configKey">The <see cref="ConfigSettingKey"/> of the target setting.</param>
        /// <param name="factory">A factor method that is called to create an instance of the exception to throw.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static ConfigurationManagerService StubConfigSetting(this ConfigurationManagerService service, ConfigSettingKey configKey, Func<Exception> factory)
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), factory);
        }

        /// <summary>
        /// Configures a setting to throw an exception when the value for it is requested.
        /// </summary>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="name">The name of the target setting.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static ConfigurationManagerService StubConfigSetting(this ConfigurationManagerService service, string name)
        {
            return StubConfigSetting(service, name, (() => new Exception()));
        }

        /// <summary>
        /// Configures a setting to throw an exception when the value for it is requested.
        /// </summary>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="name">The name of the target setting.</param>
        /// <param name="factory">A factor method that is called to create an instance of the exception to throw.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static ConfigurationManagerService StubConfigSetting(this ConfigurationManagerService service, string name, Func<Exception> factory)
        {
            ArgumentValidator.ValidateThisObj(() => service);
            ArgumentValidator.ValidateNotNull(() => factory);

            var exception = (factory?.Invoke() ?? new Exception());
            GetAppSettingsStub(service).StubAction(stub => stub.Get(Arg<string>.Is.Equal(name))).Throw(exception);
            return service;
        }

        /// <summary>
        /// Configures a setting at runtime to return a specific value.
        /// </summary>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="configKey">The <see cref="ConfigSettingKey"/> of the target setting.</param>
        /// <param name="value">The setting value to return.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static ConfigurationManagerService StubConfigSetting(this ConfigurationManagerService service, ConfigSettingKey configKey, string value)
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), value);
        }

        /// <summary>
        /// Configures a setting at runtime to return a specific value.
        /// </summary>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="name">The name of the target setting.</param>
        /// <param name="value">The setting value to return.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static ConfigurationManagerService StubConfigSetting(this ConfigurationManagerService service, string name, string value)
        {
            ArgumentValidator.ValidateThisObj(() => service);
            GetAppSettingsStub(service).StubAction(stub => stub.Get(Arg<string>.Is.Equal(name))).Return(value);
            return service;
        }

        /// <summary>
        /// Configures a setting to throw an exception when the value for it is requested.
        /// </summary>
        /// <typeparam name="TService">The type of the setting manager service.</typeparam>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="configKey">The <see cref="ConfigSettingKey"/> of the target setting.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static TService StubConfigSetting<TService>(this TService service, ConfigSettingKey configKey)
            where TService : class, IConfiguarationManagerService
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), (() => new Exception()));
        }

        /// <summary>
        /// Configures a setting to throw an exception when the value for it is requested.
        /// </summary>
        /// <typeparam name="TService">The type of the setting manager service.</typeparam>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="configKey">The <see cref="ConfigSettingKey"/> of the target setting.</param>
        /// <param name="factory">A factor method that is called to create an instance of the exception to throw.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static TService StubConfigSetting<TService>(this TService service, ConfigSettingKey configKey, Func<Exception> factory)
            where TService : class, IConfiguarationManagerService
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), factory);
        }

        /// <summary>
        /// Configures a setting to throw an exception when the value for it is requested.
        /// </summary>
        /// <typeparam name="TService">The type of the setting manager service.</typeparam>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="name">The name of the target setting.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static TService StubConfigSetting<TService>(this TService service, string name)
            where TService : class, IConfiguarationManagerService
        {
            return StubConfigSetting(service, name, (() => new Exception()));
        }

        /// <summary>
        /// Configures a setting to throw an exception when the value for it is requested.
        /// </summary>
        /// <typeparam name="TService">The type of the setting manager service.</typeparam>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="name">The name of the target setting.</param>
        /// <param name="factory">A factor method that is called to create an instance of the exception to throw.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static TService StubConfigSetting<TService>(this TService service, string name, Func<Exception> factory)
            where TService : class, IConfiguarationManagerService
        {
            ArgumentValidator.ValidateThisObj(() => service);
            ArgumentValidator.ValidateNotNull(() => factory);

            var exception = (factory?.Invoke() ?? new Exception());
            service.StubAction(stub => stub.IsAvailable).Return(true);
            service.StubAction(stub => stub.GetConfigurationSettingValue(Arg<string>.Is.Equal(name))).Throw(exception);
            return service;
        }

        /// <summary>
        /// Configures a setting at runtime to return a specific value.
        /// </summary>
        /// <typeparam name="TService">The type of the setting manager service.</typeparam>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="configKey">The <see cref="ConfigSettingKey"/> of the target setting.</param>
        /// <param name="value">The setting value to return.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static TService StubConfigSetting<TService>(this TService service, ConfigSettingKey configKey, string value)
            where TService : class, IConfiguarationManagerService
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), value);
        }

        /// <summary>
        /// Configures a setting at runtime to return a specific value.
        /// </summary>
        /// <typeparam name="TService">The type of the setting manager service.</typeparam>
        /// <param name="service">The setting manager service being stubbed.</param>
        /// <param name="name">The name of the target setting.</param>
        /// <param name="value">The setting value to return.</param>
        /// <returns>A reference to <paramref name="service"/>.</returns>
        public static TService StubConfigSetting<TService>(this TService service, string name, string value)
            where TService : class, IConfiguarationManagerService
        {
            ArgumentValidator.ValidateThisObj(() => service);
            service.StubAction(stub => stub.IsAvailable).Return(true);
            service.StubAction(stub => stub.GetConfigurationSettingValue(Arg<string>.Is.Equal(name))).Return(value);
            return service;
        }
    }
}
