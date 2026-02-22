using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.DependencyInjection;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

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

            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub()
                .StubConfigSetting(ConfigSettingKey.ServiceBusConnectionString, expected);
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetServiceBusConnectionString();

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetServiceBusConnectionStringWithAlternateName()
        {
            const string expected = "UseDevelopmentStorage=true";

            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub()
                .StubConfigSetting(ConfigSettingKey.ServiceBusConnectionString, String.Empty)
                .StubConfigSetting(AzureConfigProviderExtensions.MSServiceBusConnectionStringSettingName, expected);
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetServiceBusConnectionString();

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetServiceBusConnectionStringWithFallbackToAppConfig()
        {
            const string expected = "UseDevelopmentStorage=true";

            Substitute.For<ConfigurationManagerService>()
                .StubConfigSetting(ConfigSettingKey.ServiceBusConnectionString, expected)
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub()
                .StubConfigSetting(ConfigSettingKey.ServiceBusConnectionString, () => ReflectionHelper.CreateInstance<Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironmentException>());
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetServiceBusConnectionString();

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetServiceBusConnectionStringWithNoValue()
        {
            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub()
                .StubConfigSetting(ConfigSettingKey.ServiceBusConnectionString, String.Empty)
                .StubConfigSetting(AzureConfigProviderExtensions.MSServiceBusConnectionStringSettingName, String.Empty);
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            Action act = () => ConfigSettingProvider.Current.GetServiceBusConnectionString();
            act.Should().Throw<ConfigurationErrorsException>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKey()
        {
            const string expected = "Critical";

            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, expected);
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigurationErrorsExceptionThrown()
        {
            const string expected = "Critical";

            Substitute.For<ConfigurationManagerService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, expected)
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, () => new ConfigurationErrorsException());
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithEmptyValue()
        {
            const string expected = "Critical";

            Substitute.For<ConfigurationManagerService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, expected)
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, String.Empty);
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKeyAndFallbackToAppConfig()
        {
            const string expected = "Critical";

            Substitute.For<ConfigurationManagerService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, expected)
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, () => ReflectionHelper.CreateInstance<Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironmentException>());
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKeyAndFallbackToDefault()
        {
            var expected = ConfigSettingKey.CurrentTraceLevel.GetDefaultValue();

            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, () => ReflectionHelper.CreateInstance<Microsoft.WindowsAzure.ServiceRuntime.RoleEnvironmentException>());
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKeyAndIntValue()
        {
            const int expected = 12;

            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub()
                .StubConfigSetting(ConfigSettingKey.MaxProcessingThreads, expected.ToString("D"));
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.MaxProcessingThreads, Convert.ToInt32);

            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithDefaultValue()
        {
            const string expected = "SampleValue";

            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting("SomeSetting", expected);

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithInvalidName()
        {
            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            Action act = () => ConfigSettingProvider.Current.GetSetting(InvalidSettingName);
            act.Should().Throw<ConfigurationErrorsException>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithMissingConfigKey()
        {
            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            Action act = () => ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CPP_Elevate_ReportProcessing_ServiceBus_IssuerSecret);
            act.Should().Throw<ConfigurationErrorsException>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithMissingConfigKeyAndDefaultValue()
        {
            const string expected = "SomeValue";

            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub();
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CPP_Elevate_ReportProcessing_ServiceBus_IssuerSecret, expected);

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithValidName()
        {
            const string expected = "TestValue";

            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
            Substitute.For<RoleEnvironmentService>()
                .RegisterServiceStub()
                .StubConfigSetting(StringSettingName, expected);
            ServiceLocator.Register<ConfigSettingProvider, RoleConfigProvider>();
            var actual = ConfigSettingProvider.Current.GetSetting(StringSettingName);

            actual.Should().Be(expected);
        }

        #region Test Class Helper Methods

        #endregion // Test Class Helper Methods
    }
}
