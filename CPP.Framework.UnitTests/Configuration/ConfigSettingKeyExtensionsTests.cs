using System;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
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
            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetConfigSettingNameWithInvalidEnum()
        {
            Action act = () => { ((ConfigSettingKey)(-1)).GetConfigSettingName(); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("configKey");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetDefaultValue()
        {
            const string expected = "emailrequestqueue";
            var actual = ConfigSettingKey.EmailRequestQueue.GetDefaultValue();
            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetDefaultValueWithInvalidEnum()
        {
            Action act = () => { ((ConfigSettingKey)(-1)).GetDefaultValue(); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("configKey");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetDefaultValueWithNoValue()
        {
            var actual = ConfigSettingKey.SiteBaseURL.GetDefaultValue();
            actual.Should().BeNull();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetTarget()
        {
            const ConfigSettingTarget expected = ConfigSettingTarget.CloudQueueReference;
            var actual = ConfigSettingKey.EmailRequestQueue.GetTarget();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetTargetWithInvalidEnum()
        {
            Action act = () => { ((ConfigSettingKey)(-1)).GetTarget(); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("configKey");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Configuration)]
        public void GetTargetWithNoValue()
        {
            const ConfigSettingTarget expected = ConfigSettingTarget.None;
            var actual = ConfigSettingKey.SiteBaseURL.GetTarget();
            actual.Should().Be(expected);
        }
    }
}
