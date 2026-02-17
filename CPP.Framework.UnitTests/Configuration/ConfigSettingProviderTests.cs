using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
            Verify.AreEqual(expected, ConfigSettingProvider.Current);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void CurrentPropertyReturnsValidValue()
        {
            var provider = ConfigSettingProvider.Current;
            Verify.IsNotNull(provider);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKey()
        {
            const string expected = "Critical";

            StubFactory.CreateStub<ConfigurationManagerService>()
                .StubConfigSetting(ConfigSettingKey.CurrentTraceLevel, expected)
                .RegisterServiceStub();
            var actual = ConfigSettingProvider.Current.GetSetting(ConfigSettingKey.CurrentTraceLevel);

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetSettingWithConfigKeyAndIntValue()
        {
            const int expected = 1;

            StubFactory.CreateStub<ConfigurationManagerService>()
                .RegisterServiceStub();            
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
                .StubConfigSetting(StringSettingName, expected)
                .RegisterServiceStub();
            var actual = ConfigSettingProvider.Current.GetSetting(StringSettingName);

            Verify.AreEqual(expected, actual);
        }

        #region Test Class Helper Methods

        #endregion // Test Class Helper Methods
    }
}
