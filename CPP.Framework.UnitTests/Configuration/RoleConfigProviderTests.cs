using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Diagnostics.Testing;
using CPP.Framework.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.ServiceRuntime;
using Rhino.Mocks;

namespace CPP.Framework.Configuration
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RoleConfigProviderTests
    {
        private const string InvalidSettingName = "InvalidSetting";
        private const string StringSettingName = "StringSetting";

        [TestInitialize]
        public void TestStartup() { ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>(); }

        [TestCleanup]
        public void TestCleanup() { ServiceLocator.Unload(); }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetServiceBusConnectionString()
        {
            const string expected = "UseDevelopmentStorage=true";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .StubConfigSetting(ConfigSettingKey.ServiceBusConnectionString, expected)
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetServiceBusConnectionString();

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetServiceBusConnectionStringWithAlternateName()
        {
            const string expected = "UseDevelopmentStorage=true";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .StubConfigSetting(ConfigSettingKey.ServiceBusConnectionString, String.Empty)
                .StubConfigSetting(AzureConfigProviderExtensions.MSServiceBusConnectionStringSettingName, expected)
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetServiceBusConnectionString();

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetServiceBusConnectionStringWithFallbackToAppConfig()
        {
            const string expected = "UseDevelopmentStorage=true";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .StubConfigSetting(ConfigSettingKey.ServiceBusConnectionString, expected)
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .StubConfigSetting(ConfigSettingKey.ServiceBusConnectionString, () => StubFactory.CreateInstance<RoleEnvironmentException>())
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetServiceBusConnectionString();

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void GetServiceBusConnectionStringWithNoValue()
        {
            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .StubConfigSetting(ConfigSettingKey.ServiceBusConnectionString, String.Empty)
                .StubConfigSetting(AzureConfigProviderExtensions.MSServiceBusConnectionStringSettingName, String.Empty)
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            ConfigSettingProvider.Current.GetServiceBusConnectionString();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKey()
        {
            const string expected = "Critical";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, expected)
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigurationErrorsExceptionThrown()
        {
            const string expected = "Critical";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, expected)
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, () => new ConfigurationErrorsException())
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithEmptyValue()
        {
            const string expected = "Critical";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, expected)
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, String.Empty)
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKeyAndFallbackToAppConfig()
        {
            const string expected = "Critical";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, expected)
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, () => StubFactory.CreateInstance<RoleEnvironmentException>())
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKeyAndFallbackToDefault()
        {
            var expected = ConfigSettingKey.CurrentTraceLevel.GetDefaultValue();

            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, () => StubFactory.CreateInstance<RoleEnvironmentException>())
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKeyAndIntValue()
        {
            const int expected = 12;

            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .StubConfigSetting(ConfigSettingKey.MaxProcessingThreads, expected.ToString("D"))
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.MaxProcessingThreads, Convert.ToInt32);

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithDefaultValue()
        {
            const string expected = "SampleValue";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting("SomeSetting", expected);

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void GetSettingWithInvalidName()
        {
            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            ConfigSettingProvider.Current.GetSetting(InvalidSettingName);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void GetSettingWithMissingConfigKey()
        {
            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CPP_Elevate_ReportProcessing_ServiceBus_IssuerSecret);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithMissingConfigKeyAndDefaultValue()
        {
            const string expected = "SomeValue";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CPP_Elevate_ReportProcessing_ServiceBus_IssuerSecret, expected);
            
            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithValidName()
        {
            const string expected = "TestValue";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();
            StubFactory.CreateStub<RoleEnvironmentService>()
                .StubConfigSetting(StringSettingName, expected)
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(StringSettingName);

            Verify.AreEqual(expected, actual);
        }

        #region Test Class Helper Methods

        #endregion // Test Class Helper Methods
    }
}
