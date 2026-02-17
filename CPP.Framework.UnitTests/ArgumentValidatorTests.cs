using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using CPP.Framework.Configuration;
using CPP.Framework.Diagnostics.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework
{
    /// <summary>
    /// Unit tests for the <see cref="ArgumentValidator"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ArgumentValidatorTests
    {
        #region Test Generic Helper Methods

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentException("argument")]
        public void ThrowArgumentExceptionFor()
        {
            var argument = "argument";
            var expected = new ArgumentException("Default exception message 1", argument);
            
            try
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => argument, "Default exception message {0}", 1);
            }
            catch (ArgumentException ex)
            {
                Verify.AreEqual(expected.Message, ex.Message);
                Verify.AreEqual(expected.ParamName, ex.ParamName);
                Verify.AreEqual(expected.InnerException, ex.InnerException);
                Verify.AreEqual(Assembly.GetExecutingAssembly().GetName().Name, ex.Source);
                throw;
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentException("argument")]
        public void ThrowArgumentExceptionForWithObjArgument()
        {
            var argument = new KeyValuePair<string, object>("Key", new Object());
            var expected = new ArgumentException("Default exception message 1", "argument");

            try
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => argument, "Default exception message {0}", 1);
            }
            catch (ArgumentException ex)
            {
                Verify.AreEqual(expected.Message, ex.Message);
                Verify.AreEqual(expected.ParamName, ex.ParamName);
                Verify.AreEqual(expected.InnerException, ex.InnerException);
                Verify.AreEqual(Assembly.GetExecutingAssembly().GetName().Name, ex.Source);
                throw;
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentException("argument")]
        public void ThrowArgumentExceptionForWithInner()
        {
            var argument = "argument";

            var innerException = new Exception("innerException");
            var expected = new ArgumentException("Default exception message 1", argument, innerException);
            
            try
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => argument, innerException, "Default exception message {0}", 1);
            }
            catch(ArgumentException ex)
            {
                Verify.AreEqual(expected.Message, ex.Message);
                Verify.AreEqual(expected.ParamName, ex.ParamName);
                Verify.AreEqual(expected.InnerException, ex.InnerException);
                Verify.AreEqual(Assembly.GetExecutingAssembly().GetName().Name, ex.Source);
                throw;
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentException("argument")]
        public void ThrowArgumentExceptionForWithNullInnerExplicit()
        {
            var argument = "argument";

            var expected = new ArgumentException("Default exception message 1", argument, null);

            try
            {
                throw ArgumentValidator.CreateArgumentExceptionFor(() => argument, null, "Default exception message {0}", 1);
            }
            catch (ArgumentException ex)
            {
                Verify.AreEqual(expected.Message, ex.Message);
                Verify.AreEqual(expected.ParamName, ex.ParamName);
                Verify.AreEqual(expected.InnerException, ex.InnerException);
                Verify.AreEqual(Assembly.GetExecutingAssembly().GetName().Name, ex.Source);
                throw;
            }
        }

        #endregion

        #region Test ConfigSettingKey Argument Validation

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateConfigCategory()
        {
            // Source: CPP.Framework.Configuration.ConfigSettingKey.cs
            var configSettings = new []
            {
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.DataConnectionString, ConfigSettingTarget.ConnectionString ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.DataCenterIdString, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.PlatformType, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.Ignoreoutputpath, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.OutputcontainerName, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ServiceInterval, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.RootFolder, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.QueueReference, ConfigSettingTarget.CloudQueueReference ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.DisableLogging, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.LogFilePerRequest, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.LogPath, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.LogFileName, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.LogContainer, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.VisibilityTimeout, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.MessagePlatform, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.WcfMetadataIp, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.WcfMetadataPort, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.WcfEndPointIp, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.WcfEndPointPort, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.StorageConnectionString, ConfigSettingTarget.StorageConnectionString ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.CPPDataAnalyticsStorage, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SendGridUserName, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SendGridPassword, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.HostAddress, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.HostPort, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.Partner, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.Vendor, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.User, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.Pwd, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.TransactionTimeout, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ProxyAddress, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ProxyPort, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ProxyLogon, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ProxyPassword, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.EnableFraudFilters, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.HOST, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ReportProcessListenerQueueReference, ConfigSettingTarget.CloudQueueReference ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ReportProcessQueueReference, ConfigSettingTarget.CloudQueueReference ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ServiceBusConnectionString, ConfigSettingTarget.ConnectionString ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ReportsBaseURL, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ElectronicActivitiesBaseURL, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ElectronicPdfBaseURL, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SiteBaseURL, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SiteContentURL, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.RootFolderForActivities, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.LoginSiteBaseUrl, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.MessageQueueReference, ConfigSettingTarget.CloudQueueReference ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.TwilioSID, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.TwilioToken, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.TwilioPhoneNumber, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.PDPTrustedSites, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.CPPFromEmailAddress, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SkillsonePortBaseUrl, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SkillsonePortQueue, ConfigSettingTarget.CloudQueueReference ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ProvisionRespondentQueue, ConfigSettingTarget.CloudQueueReference ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ServiceBusOrganizationSubscriptionList, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SkillsonePortReportQueue, ConfigSettingTarget.CloudQueueReference ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SkillsoneInventoryPortRequestQueue, ConfigSettingTarget.CloudQueueReference ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SkillsoneLookUpQueue, ConfigSettingTarget.CloudQueueReference ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SkillsoneLookUpQueueLimit, ConfigSettingTarget.CloudQueueReference ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.orderMessageDeliveryDelayInSeconds, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.memberMessageDeliveryDelayInSeconds, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SampleReportsContainerName, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SkillsonePortTimeSpanLimit, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.EmailRequestQueue, ConfigSettingTarget.CloudQueueReference ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.MaxProcessingThreads, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.CurrentTraceLevel, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ContentImageBaseURL, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.BrandedImageBaseURL, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ImageConnectionString, ConfigSettingTarget.StorageConnectionString ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ServiceBusNameSpace, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.IttTokenInternalAuthenticationKey, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.IttTokenExternalAuthenticationKey, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.EnableSkillsoneDeactivation, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.PayPalExpressBaseUrl, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.SsoLegacyBearerTokenTimeOut, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.PayPalReturnUrl, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.FedexAccountNumber, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.FedexMeterNumber, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.FedexUserKey, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.FedexUserPassword, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.FedexProdUrl, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.DefaultCacheProvider, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.RedisCacheConnectionString, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.GuidedTour, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ReCaptchaPublicKey, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.ReCaptchaPrivateKey, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.CaptchaON, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.MarketoHost, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.MarketoClientID, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.MarketoClientSecret, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.Sandbox, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.CPP_Elevate_ReportProcessing_ServiceBus_IssuerSecret, ConfigSettingTarget.None ),
                new KeyValuePair<ConfigSettingKey, ConfigSettingTarget>( ConfigSettingKey.CPP_Elevate_ReportProcessing_ServiceBus_Namespace, ConfigSettingTarget.None)
            };

            foreach (var configSetting in configSettings)
            {
                // ReSharper disable once AccessToForEachVariableInClosure
                ArgumentValidator.ValidateConfigCategory(() => configSetting.Key, configSetting.Value);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentException("argument")]
        public void ValidateConfigCategoryWithInvalidTarget()
        {
            // ReSharper disable ConvertToConstant.Local

            // contrary to ReSharper's suggestion, this cannot be defined as a constant, because it 
            // will cause the test to always fail. the ArgumentValidator class requires the lambda
            // to be a member-access expression, which provides access to the variable name needed
            // to create the exception correctly on failure, whereas a constant expression does not.
            var argument = ConfigSettingKey.EmailRequestQueue;
            
            // ReSharper restore ConvertToConstant.Local
            ArgumentValidator.ValidateConfigCategory(() => argument, ConfigSettingTarget.None);
        }

        #endregion

        #region Test Guid Argument Validation

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentException("argument")]
        public void ValidateNotEmptyWithEmptyValue()
        {
            var argument = Guid.Empty;
            ArgumentValidator.ValidateNotEmpty(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotEmptyWithValidValue()
        {
            var argument = Guid.NewGuid();
            ArgumentValidator.ValidateNotEmpty(() => argument);
        }

        #endregion

        #region Test String Reference Validation

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentException("argument")]
        public void ValidateNotNullOrEmptyWithEmtpyValue()
        {
            var argument = String.Empty;
            ArgumentValidator.ValidateNotNullOrEmpty(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentNullException("argument")]
        public void ValidateNotNullOrEmptyWithNullValue()
        {
            string argument = null;
            ArgumentValidator.ValidateNotNullOrEmpty(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotNullOrEmptyWithValidValue()
        {
            var argument = new string('a', 4);
            ArgumentValidator.ValidateNotNullOrEmpty(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotNullOrEmptyWithValidValueAndPaddingSpaces()
        {
            var argument = "  aaaa  ";
            ArgumentValidator.ValidateNotNullOrEmpty(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotNullOrEmptyWithValidValueAndBeginningSpaces()
        {
            var argument = "  aaaa";
            ArgumentValidator.ValidateNotNullOrEmpty(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotNullOrEmptyWithValidValueAndTrailingSpaces()
        {
            var argument = "aaaa  ";
            ArgumentValidator.ValidateNotNullOrEmpty(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotNullOrEmptyWithValidValueAndEnclosingSpaces()
        {
            var argument = "aa  aa";
            ArgumentValidator.ValidateNotNullOrEmpty(() => argument);
        }


        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentException("argument")]
        public void ValidateNotNullOrWhiteSpaceWithEmtpyValue()
        {
            var argument = String.Empty;
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentNullException("argument")]
        public void ValidateNotNullOrWhiteSpaceWithNullValue()
        {
            string argument = null;
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotNullOrWhiteSpaceWithValidValue()
        {
            var argument = new string('a', 4);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotNullOrWhiteSpaceWithValidValueTrailingSpaces()
        {
            var argument = "aaaa  ";
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotNullOrWhiteSpaceWithValidValueStartingSpaces()
        {
            var argument = "  aaaa";
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotNullOrWhiteSpaceWithValidValuePaddingSpaces()
        {
            var argument = "  aaaa  ";
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotNullOrWhiteSpaceWithValidValueEnclosingSpaces()
        {
            var argument = "aa aa";
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentException("argument")]
        public void ValidateNotNullOrWhiteSpaceWithWhiteSpaceValue()
        {
            var argument = new string(' ', 4);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => argument);
        }

        #endregion

        #region Test Object Reference Validation

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentException("expression")]
        public void ValidateNotNullWithInvalidExpression()
        {
            ArgumentValidator.ValidateNotNull(() => "");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentNullException("expression")]
        public void ValidateNotNullWithNullExpression()
        {
            ArgumentValidator.ValidateNotNull((Expression<Func<object>>)null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentNullException("argument")]
        public void ValidateNotNullWithNullValue()
        {
            object argument = null;
            ArgumentValidator.ValidateNotNull(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateNotNullWithNullableValue()
        {
            Guid? argument = Guid.NewGuid();
            ArgumentValidator.ValidateNotNull(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentNullException("argument")]
        public void ValidateNotNullWithNullableValueAsNull()
        {
            Guid? argument = null;
            ArgumentValidator.ValidateNotNull(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateThisObj()
        {
            var argument = new object();
            ArgumentValidator.ValidateThisObj(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentNullException("expression")]
        public void ValidateThisObjWithInvalidExpression()
        {
            ArgumentValidator.ValidateThisObj((Expression<Func<object>>)null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        public void ValidateThisObjWithNullable()
        {
            bool? argument = true;
            ArgumentValidator.ValidateThisObj(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedArgumentNullException("expression")]
        public void ValidateThisObjWithNullableAndInvalidExpression()
        {
            ArgumentValidator.ValidateThisObj((Expression<Func<bool?>>)null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedException(typeof(NullReferenceException))]
        public void ValidateThisObjWithNullableAndNullValue()
        {
            bool? argument = null;
            ArgumentValidator.ValidateThisObj(() => argument);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.Validation)]
        [ExpectedException(typeof(NullReferenceException))]
        public void ValidateThisObjWithNullValue()
        {
            var argument = ((object)null);
            ArgumentValidator.ValidateThisObj(() => argument);
        }

        #endregion
    }
}
