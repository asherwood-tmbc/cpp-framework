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
    #region Sample Class Definitions

    public class CustomConfigurationProvider : ConfigSettingProvider { }

    #endregion // Sample Class Definitions

    /// <summary>
    /// Unit tests for the <see cref="ConfigSettingProvider"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ConfigSettingProviderTests
    {
        private const string InvalidSettingName = "InvalidSetting";
        private const string StringSettingName = "StringSetting";

        [TestCleanup]
        public void TestCleanup() { ServiceLocator.Unload(); }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void CurrentPropertyReturnsCustomInstance()
        {
            var expected = new CustomConfigurationProvider();
            ServiceLocator.Initialize();
            ServiceLocator.Register<ConfigSettingProvider>(expected);
            var actual = ConfigSettingProvider.Current;

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
            ConfigSettingProvider.Current.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void CurrentPropertyReturnsValidValue()
        {
            var provider = ConfigSettingProvider.Current;
            provider.Should().NotBeNull();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKey()
        {
            const string expected = "Critical";

            Substitute.For<ConfigurationManagerService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, expected)
                .RegisterServiceStub();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKeyAndIntValue()
        {
            const int expected = 1;

            Substitute.For<ConfigurationManagerService>()
                .RegisterServiceStub();
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
                .StubConfigSetting(StringSettingName, expected)
                .RegisterServiceStub();
            var actual = ConfigSettingProvider.Current.GetSetting(StringSettingName);

            actual.Should().Be(expected);
        }

        #region Test Class Helper Methods

        #endregion // Test Class Helper Methods
    }
}
