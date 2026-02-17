using System.Diagnostics.CodeAnalysis;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Unit tests for the <see cref="ConfigSettingKeyExtensions"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ConfigSettingKeyExtensionsTests
    {
        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetConfigSettingName()
        {
            const string expected = "SiteBaseURL";
            var actual = ConfigSettingKey.SiteBaseURL.GetConfigSettingName();
            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        [ExpectedArgumentException("configKey")]
        public void GetConfigSettingNameWithInvalidEnum()
        {
            ((ConfigSettingKey)(-1)).GetConfigSettingName();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetDefaultValue()
        {
            const string expected = "emailrequestqueue";
            var actual = ConfigSettingKey.EmailRequestQueue.GetDefaultValue();
            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        [ExpectedArgumentException("configKey")]
        public void GetDefaultValueWithInvalidEnum()
        {
            ((ConfigSettingKey)(-1)).GetDefaultValue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetDefaultValueWithNoValue()
        {
            var actual = ConfigSettingKey.SiteBaseURL.GetDefaultValue();
            Verify.IsNull(actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetTarget()
        {
            const ConfigSettingTarget expected = ConfigSettingTarget.CloudQueueReference;
            var actual = ConfigSettingKey.EmailRequestQueue.GetTarget();
            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        [ExpectedArgumentException("configKey")]
        public void GetTargetWithInvalidEnum()
        {
            ((ConfigSettingKey)(-1)).GetTarget();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetTargetWithNoValue()
        {
            const ConfigSettingTarget expected = ConfigSettingTarget.None;
            var actual = ConfigSettingKey.SiteBaseURL.GetTarget();
            Verify.AreEqual(expected, actual);
        }
    }
}
