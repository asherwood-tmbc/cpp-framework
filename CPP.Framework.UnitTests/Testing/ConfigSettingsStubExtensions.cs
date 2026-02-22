using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using CPP.Framework.Configuration;
using CPP.Framework.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.Extensions;

namespace CPP.Framework.UnitTests.Testing
{
    [ExcludeFromCodeCoverage]
    internal static class ConfigSettingsStubExtensions
    {
        // Per-substitute caches. ConditionalWeakTable holds values only as long as the key
        // (the substitute instance) is alive, so there is no memory leak between tests.
        private static readonly ConditionalWeakTable<ConfigurationManagerService, NameValueCollection> _appSettings =
            new ConditionalWeakTable<ConfigurationManagerService, NameValueCollection>();

        private static readonly ConditionalWeakTable<ConfigurationManagerService, ConnectionStringSettingsCollection> _connectionStrings =
            new ConditionalWeakTable<ConfigurationManagerService, ConnectionStringSettingsCollection>();

        // -----------------------------------------------------------------------------------------
        // Internal helpers — used by StubDefaultActions.SetupDefaultConfig as well so that both
        // share the same collection instance regardless of call order.
        // -----------------------------------------------------------------------------------------

        internal static NameValueCollection GetOrCreateAppSettings(ConfigurationManagerService service)
        {
            var collection = _appSettings.GetOrCreateValue(service);
            service.AppSettings.Returns(collection);
            return collection;
        }

        internal static ConnectionStringSettingsCollection GetOrCreateConnectionStrings(ConfigurationManagerService service)
        {
            var collection = _connectionStrings.GetOrCreateValue(service);
            service.ConnectionStrings.Returns(collection);
            return collection;
        }

        // -----------------------------------------------------------------------------------------
        // ConfigurationManagerService — concrete class.
        // AppSettings is backed by a real NameValueCollection (not a NSubstitute stub), because
        // NameValueCollection.Get(string) is non-virtual. Entries are added directly to the
        // shared collection. A missing key returns null; the SUT interprets null as "not found"
        // and raises ConfigurationErrorsException itself.
        // -----------------------------------------------------------------------------------------

        public static ConfigurationManagerService StubConfigSetting(
            this ConfigurationManagerService service, ConfigSettingKey configKey, string value)
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), value);
        }

        public static ConfigurationManagerService StubConfigSetting(
            this ConfigurationManagerService service, string name, string value)
        {
            ArgumentValidator.ValidateThisObj(() => service);
            GetOrCreateAppSettings(service)[name] = value;
            return service;
        }

        public static ConfigurationManagerService StubConfigSetting(
            this ConfigurationManagerService service, ConfigSettingKey configKey)
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), factory: null);
        }

        public static ConfigurationManagerService StubConfigSetting(
            this ConfigurationManagerService service, string name)
        {
            return StubConfigSetting(service, name, factory: null);
        }

        public static ConfigurationManagerService StubConfigSetting(
            this ConfigurationManagerService service, ConfigSettingKey configKey, Func<Exception> factory)
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), factory);
        }

        /// <summary>
        /// Marks a setting as unavailable. The key is simply absent from the collection;
        /// NameValueCollection.Get(name) returns null and the SUT raises
        /// ConfigurationErrorsException through its own missing-key path.
        /// </summary>
        public static ConfigurationManagerService StubConfigSetting(
            this ConfigurationManagerService service, string name, Func<Exception> factory)
        {
            ArgumentValidator.ValidateThisObj(() => service);
            GetOrCreateAppSettings(service);    // ensure AppSettings is wired up; key is absent
            return service;
        }

        public static ConfigurationManagerService StubConnectionString(
            this ConfigurationManagerService service, ConnectionStringSettings setting)
        {
            ArgumentValidator.ValidateThisObj(() => service);
            ArgumentValidator.ValidateNotNull(() => setting);
            var collection = GetOrCreateConnectionStrings(service);
            if (collection[setting.Name] == null)
                collection.Add(setting);
            return service;
        }

        public static ConfigurationManagerService StubConnectionString(
            this ConfigurationManagerService service, string name, string connectionString, string providerName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => connectionString);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => providerName);
            return service.StubConnectionString(new ConnectionStringSettings(name, connectionString, providerName));
        }

        // -----------------------------------------------------------------------------------------
        // IConfiguarationManagerService — generic variant used for RoleEnvironmentService and
        // similar Azure-backed services.
        //
        // IMPORTANT ordering note: NSubstitute evaluates setups in reverse order (most recent
        // first). Specific argument returns MUST be set up AFTER any wildcard throws so they take
        // priority. In test code this means calling RegisterServiceStub() (which runs
        // SetupDefaultConfig with wildcards) BEFORE calling StubConfigSetting (which applies the
        // specific returns).
        //
        //   Correct pattern:
        //     Substitute.For<RoleEnvironmentService>()
        //         .RegisterServiceStub()           // wildcards first
        //         .StubConfigSetting(key, value);  // specifics after → override wildcards
        // -----------------------------------------------------------------------------------------

        public static TService StubConfigSetting<TService>(
            this TService service, ConfigSettingKey configKey, string value)
            where TService : class, IConfiguarationManagerService
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), value);
        }

        public static TService StubConfigSetting<TService>(
            this TService service, string name, string value)
            where TService : class, IConfiguarationManagerService
        {
            ArgumentValidator.ValidateThisObj(() => service);
            service.IsAvailable.Returns(true);
            // Use Configure() to suppress any previously configured Throws (e.g. from
            // SetupDefaultConfig) so the setup call does not trigger the wildcard throw.
            service.Configure().GetConfigurationSettingValue(name).Returns(value);
            return service;
        }

        public static TService StubConfigSetting<TService>(
            this TService service, ConfigSettingKey configKey)
            where TService : class, IConfiguarationManagerService
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), () => new Exception());
        }

        public static TService StubConfigSetting<TService>(
            this TService service, string name)
            where TService : class, IConfiguarationManagerService
        {
            return StubConfigSetting(service, name, () => new Exception());
        }

        public static TService StubConfigSetting<TService>(
            this TService service, ConfigSettingKey configKey, Func<Exception> factory)
            where TService : class, IConfiguarationManagerService
        {
            return StubConfigSetting(service, configKey.GetConfigSettingName(), factory);
        }

        public static TService StubConfigSetting<TService>(
            this TService service, string name, Func<Exception> factory)
            where TService : class, IConfiguarationManagerService
        {
            ArgumentValidator.ValidateThisObj(() => service);
            ArgumentValidator.ValidateNotNull(() => factory);
            service.IsAvailable.Returns(true);
            // Use Configure() to suppress any previously configured Throws (e.g. from
            // SetupDefaultConfig) so the setup call does not trigger the wildcard throw.
            service.Configure().GetConfigurationSettingValue(name).Throws(factory?.Invoke() ?? new Exception());
            return service;
        }
    }
}
