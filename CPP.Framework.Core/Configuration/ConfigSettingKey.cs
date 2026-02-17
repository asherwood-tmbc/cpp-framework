using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace CPP.Framework.Configuration
{
    /// <summary>
    /// Common Predefined Configuration Key Values
    /// </summary>
    public enum ConfigSettingKey
    {
        /// <summary>
        /// The DataConnectionString Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.ConnectionString)]
        DataConnectionString,

        /// <summary>
        /// The DataCenterIdString Setting
        /// </summary>
        DataCenterIdString,

        /// <summary>
        /// The PlatformType Setting
        /// </summary>
        PlatformType,

        /// <summary>
        /// The Ignoreoutputpath Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        Ignoreoutputpath,

        /// <summary>
        /// The OutputcontainerName Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        OutputcontainerName,

        /// <summary>
        /// The ServiceInterval Setting
        /// </summary>
        ServiceInterval,

        /// <summary>
        /// The RootFolder Setting
        /// </summary>
        RootFolder,

        /// <summary>
        /// The QueueReference Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.CloudQueueReference)]
        QueueReference,

        /// <summary>
        /// The DisableLogging Setting
        /// </summary>
        DisableLogging,

        /// <summary>
        /// The LogFilePerRequest Setting
        /// </summary>
        LogFilePerRequest,

        /// <summary>
        /// The LogPath Setting
        /// </summary>
        LogPath,

        /// <summary>
        /// The LogFileName Setting
        /// </summary>
        LogFileName,

        /// <summary>
        /// The LogContainer Setting
        /// </summary>
        [DefaultValue("logcontainer")]
        LogContainer,

        /// <summary>
        /// The VisibilityTimeout Setting
        /// </summary>
        VisibilityTimeout,

        /// <summary>
        /// The MessagePlatform Setting
        /// </summary>
        MessagePlatform,

        /// <summary>
        /// The WcfMetadataIp Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        WcfMetadataIp,

        /// <summary>
        /// The WcfMetadataPort Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        WcfMetadataPort,

        /// <summary>
        /// The WcfEndPointIp Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        WcfEndPointIp,

        /// <summary>
        /// The WcfEndPointPort Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        WcfEndPointPort,

        /// <summary>
        /// The StorageConnectionString Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.StorageConnectionString)]
        StorageConnectionString,

        /// <summary>
        /// The StorageSkPortConnectionString Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        [ConfigSettingUsage(ConfigSettingTarget.StorageConnectionString)]
        StorageSkPortConnectionString,

        /// <summary>
        /// The CPPDataAnalyticsStorage Setting
        /// </summary>
        CPPDataAnalyticsStorage,

        /// <summary>
        /// The SendGridUserName Setting
        /// </summary>
        SendGridUserName,

        /// <summary>
        /// The SendGridPassword Setting
        /// </summary>
        SendGridPassword,

        /// <summary>
        /// The HostAddress Setting
        /// </summary>
        HostAddress,

        /// <summary>
        /// The HostPort Setting
        /// </summary>
        HostPort,

        /// <summary>
        /// The Partner Setting
        /// </summary>
        Partner,

        /// <summary>
        /// The Vendor Setting
        /// </summary>
        Vendor,

        /// <summary>
        /// The User Setting
        /// </summary>
        User,

        /// <summary>
        /// The Pwd Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        Pwd,

        /// <summary>
        /// The TransactionTimeout Setting
        /// </summary>
        TransactionTimeout,

        /// <summary>
        /// The ProxyAddress Setting
        /// </summary>
        ProxyAddress,

        /// <summary>
        /// The ProxyPort Setting
        /// </summary>
        ProxyPort,

        /// <summary>
        /// The ProxyLogon Setting
        /// </summary>
        ProxyLogon,

        /// <summary>
        /// The ProxyPassword Setting
        /// </summary>
        ProxyPassword,

        /// <summary>
        /// The EnableFraudFilters Setting
        /// </summary>
        EnableFraudFilters,

        /// <summary>
        /// The HOST Setting
        /// </summary>
        HOST,

        /// <summary>
        /// The ReportProcessListenerQueueReference Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.CloudQueueReference)]
        ReportProcessListenerQueueReference,

        /// <summary>
        /// The ReportProcessQueueReference Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.CloudQueueReference)]
        ReportProcessQueueReference,

        /// <summary>
        /// The ServiceBusConnectionString Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.ConnectionString)]
        ServiceBusConnectionString,

        /// <summary>
        /// The ReportsBaseURL Setting
        /// </summary>
        ReportsBaseURL,

        /// <summary>
        /// The ElectronicActivitiesBaseURL Setting
        /// </summary>
        ElectronicActivitiesBaseURL,

        /// <summary>
        /// The ElectronicPdfBaseURL Setting
        /// </summary>
        ElectronicPdfBaseURL,

        /// <summary>
        /// The SiteBaseURL Setting
        /// </summary>
        SiteBaseURL,

        /// <summary>
        /// The SiteContentURL Setting
        /// </summary>
        SiteContentURL,

        /// <summary>
        /// The ElevateSiteURL Setting
        /// </summary>
        ElevateSiteURL,

        /// <summary>
        /// The RootFolderForActivities Setting
        /// </summary>
        RootFolderForActivities,

        /// <summary>
        /// The LoginSiteBaseUrl Setting
        /// </summary>
        LoginSiteBaseUrl,

        /// <summary>
        /// The MessageQueueReference Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.CloudQueueReference)]
        MessageQueueReference,

        /// <summary>
        /// The TwilioSID Setting
        /// </summary>
        TwilioSID,

        /// <summary>
        /// The TwilioToken Setting
        /// </summary>
        TwilioToken,

        /// <summary>
        /// The TwilioPhoneNumber Setting
        /// </summary>
        TwilioPhoneNumber,

        /// <summary>
        /// The PDPTrustedSites Setting
        /// </summary>
        PDPTrustedSites,

        /// <summary>
        /// The CPPFromEmailAddress Setting
        /// </summary>
        [DefaultValue("noreply@cpp.com")]
        CPPFromEmailAddress,

        /// <summary>
        /// The SkillsonePortBaseUrl Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        SkillsonePortBaseUrl,

        /// <summary>
        /// The SkillsonePortQueue Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.CloudQueueReference)]
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        SkillsonePortQueue,

        /// <summary>
        /// The ProvisionRespondentQueue Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.CloudQueueReference)]
        ProvisionRespondentQueue,

        /// <summary>
        /// The ServiceBusOrganizationSubscriptionList Setting
        /// </summary>
        ServiceBusOrganizationSubscriptionList,

        /// <summary>
        /// The SkillsonePortReportQueue Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.CloudQueueReference)]
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        SkillsonePortReportQueue,

        /// <summary>
        /// The SkillsoneInventoryPortRequestQueue Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.CloudQueueReference)]
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        SkillsoneInventoryPortRequestQueue,

        /// <summary>
        /// The SkillsoneLookUpQueue Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.CloudQueueReference)]
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        SkillsoneLookUpQueue,

        /// <summary>
        /// The SkillsoneLookUpQueueLimit Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.CloudQueueReference)]
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        SkillsoneLookUpQueueLimit,

        /// <summary>
        /// The orderMessageDeliveryDelayInSeconds Setting
        /// </summary>
        orderMessageDeliveryDelayInSeconds,

        /// <summary>
        /// The memberMessageDeliveryDelayInSeconds Setting
        /// </summary>
        memberMessageDeliveryDelayInSeconds,

        /// <summary>
        /// The SampleReportsContainerName Setting
        /// </summary>
        SampleReportsContainerName,

        /// <summary>
        /// The SkillsonePortTimeSpanLimit Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        SkillsonePortTimeSpanLimit,

        /// <summary>
        /// The EmailRequestQueue Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.CloudQueueReference)]
        [DefaultValue("emailrequestqueue")]
        EmailRequestQueue,

        /// <summary>
        /// The MaxProcessingThreads Setting
        /// </summary>
        [DefaultValue("1")]
        MaxProcessingThreads,

        /// <summary>
        /// The CurrentTraceLevel Setting
        /// </summary>
#if DEBUG
        [DefaultValue("Debug")]
#else
        [DefaultValue("Error")]
#endif // DEBUG
        CurrentTraceLevel,

        /// <summary>
        /// The ContentImageBaseURL Setting
        /// </summary>
        ContentImageBaseURL,

        /// <summary>
        /// The BrandedImageBaseURL Setting
        /// </summary>
        BrandedImageBaseURL,

        /// <summary>
        /// The ImageConnectionString Setting
        /// </summary>
        [ConfigSettingUsage(ConfigSettingTarget.StorageConnectionString)]
        ImageConnectionString,

        /// <summary>
        /// The ServiceBusNameSpace Setting
        /// </summary>
        ServiceBusNameSpace,

        /// <summary>
        /// The IttTokenInternalAuthenticationKey Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        IttTokenInternalAuthenticationKey,

        /// <summary>
        /// The IttTokenExternalAuthenticationKey Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        IttTokenExternalAuthenticationKey,

        /// <summary>
        /// The EnableSkillsoneDeactivation Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        EnableSkillsoneDeactivation,

        /// <summary>
        /// The PayPalExpressBaseUrl Setting
        /// </summary>
        PayPalExpressBaseUrl,

        /// <summary>
        /// The SsoLegacyBearerTokenTimeOut Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        SsoLegacyBearerTokenTimeOut,

        /// <summary>
        /// The SsoOauth2BearerTokenTimeOut Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        SsoOauth2BearerTokenTimeOut,

        /// <summary>
        /// The PayPalReturnUrl Setting
        /// </summary>
        PayPalReturnUrl,

        /// <summary>
        /// The FedexAccountNumber Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        FedexAccountNumber,

        /// <summary>
        /// The FedexMeterNumber Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        FedexMeterNumber,

        /// <summary>
        /// The FedexUserKey Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        FedexUserKey,

        /// <summary>
        /// The FedexUserPassword Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        FedexUserPassword,

        /// <summary>
        /// The FedexProdUrl Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        FedexProdUrl,

        /// <summary>
        /// The DefaultCacheProvider Setting
        /// </summary>
        DefaultCacheProvider,

        /// <summary>
        /// The RedisCacheConnectionString Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        RedisCacheConnectionString,

        /// <summary>
        /// The GuidedTour Setting
        /// </summary>
        GuidedTour,

        /// <summary>
        /// The ReCaptchaPublicKey Setting
        /// </summary>
        ReCaptchaPublicKey,

        /// <summary>
        /// The ReCaptchaPrivateKey Setting
        /// </summary>
        ReCaptchaPrivateKey,

        /// <summary>
        /// The CaptchaON Setting
        /// </summary>
        CaptchaON,

        /// <summary>
        /// The MarketoHost Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        MarketoHost,

        /// <summary>
        /// The MarketoClientID Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        MarketoClientID,

        /// <summary>
        /// The MarketoClientSecret Setting
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        MarketoClientSecret,

        /// <summary>
        /// The Sandbox Setting
        /// </summary>
        Sandbox,

        /// <summary>
        /// The CPP.Elevate.ReportProcessing.ServiceBus.IssuerSecret Setting
        /// </summary>
        CPP_Elevate_ReportProcessing_ServiceBus_IssuerSecret,

        /// <summary>
        /// The CPP.Elevate.ReportProcessing.ServiceBus.Namespace Setting
        /// </summary>
        CPP_Elevate_ReportProcessing_ServiceBus_Namespace,

        /// <summary>
        /// CsrChatUIEnabled - determine if customer service chat is enabled.  Only true for Prod.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Comment reflects property name")]
        CsrChatUIEnabled,

        /// <summary>
        /// DNS Host Name for the Mail Server.
        /// </summary>
        MailHostName,

        /// <summary>
        /// TCP/IP Port Number for the Mail Server.
        /// </summary>
        [DefaultValue("25")]
        MailHostPort,

        /// <summary>
        /// Username for the SMTP Mail Server  Credentials.
        /// </summary>
        MailUserName,

        /// <summary>
        /// Password for the SMTP Mail Server  Credentials.
        /// </summary>
        MailPassword,

        /// <summary>
        /// Maximum Number of Concurrent Storage Queue Messages.
        /// </summary>
        [DefaultValue(16)]
        WebJobQueueBatchSize,
        
        /// <summary>
        /// Maximum Number of Attempts to Process a Storage Queue Message.
        /// </summary>
        [DefaultValue(10)]
        WebJobQueueMaxDequeueCount,
        
        /// <summary>
        /// Maximum Polling Interval for New Messages When Idle (in Seconds)
        /// </summary>
        [DefaultValue(15)]
        WebJobQueueMaxPollingInterval,

        /// <summary>
        /// Maximum Allowable Log File Size (in Kilobytes)
        /// </summary>
        [DefaultValue(5120)]    // 5 Megabytes
        LogFileSizeMax,
    }

    #region ConfigSettingKey Extension Methods

    /// <summary>
    /// Custom extension methods for the <see cref="ConfigSettingKey"/> type.
    /// </summary>
    public static class ConfigSettingKeyExtensions
    {
        /// <summary>
        /// Gets a sequence of custom attributes that have been applied to a 
        /// <see cref="ConfigSettingKey"/> member.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the custom attribute.</typeparam>
        /// <param name="configKey">The configuration setting value.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> instance that can be used to iterate over the sequence.</returns>
        private static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(ConfigSettingKey configKey)
            where TAttribute : Attribute
        {
            var fieldName = Enum.GetName(typeof(ConfigSettingKey), configKey);
            var fieldInfo = ((fieldName != null) ? typeof(ConfigSettingKey).GetField(fieldName) : null);
            if (fieldInfo == null)
            {
                throw CreateInvalidConfigSettingKeyException(configKey);
            }
            Contract.Assume(fieldInfo != null);

            var attributes = fieldInfo.GetCustomAttributes(typeof(TAttribute), false)
                .OfType<TAttribute>();
            foreach (var attr in attributes) yield return attr;
        }

        /// <summary>
        /// Gets the setting name for a <see cref="ConfigSettingKey"/> value.
        /// </summary>
        /// <param name="configKey">The configuration setting value.</param>
        /// <returns>The setting name for <paramref name="configKey"/>.</returns>
        public static string GetConfigSettingName(this ConfigSettingKey configKey)
        {
            var name = Enum.GetName(typeof(ConfigSettingKey), configKey);
            if (name == null)
            {
                throw CreateInvalidConfigSettingKeyException(configKey);
            }
            return name;
        }

        /// <summary>
        /// Gets the default setting value assigned to a <see cref="ConfigSettingKey"/> value.
        /// </summary>
        /// <param name="configKey">The configuration setting key.</param>
        /// <returns>The default setting value, or null if no default value is assigned.</returns>
        public static string GetDefaultValue(this ConfigSettingKey configKey)
        {
            var attribute = GetCustomAttributes<DefaultValueAttribute>(configKey)
                .FirstOrDefault();
            return ((attribute == null) ? null : Convert.ToString(attribute.Value));
        }

        /// <summary>
        /// Gets the setting category target assigned to a <see cref="ConfigSettingKey"/> value.
        /// </summary>
        /// <param name="configKey">The configuration setting key.</param>
        /// <returns>A <see cref="ConfigSettingTarget"/> value that identifies the target category for <paramref name="configKey"/>.</returns>
        public static ConfigSettingTarget GetTarget(this ConfigSettingKey configKey)
        {
            var attribute = GetCustomAttributes<ConfigSettingUsageAttribute>(configKey)
                .FirstOrDefault();
            return attribute?.Target ?? ConfigSettingTarget.None;
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> for an invalid configuration setting key.
        /// </summary>
        /// <param name="configKey">The invalid configuration setting key.</param>
        /// <returns>An <see cref="ArgumentException"/> value.</returns>
        private static ArgumentException CreateInvalidConfigSettingKeyException(ConfigSettingKey configKey)
        {
            return ArgumentValidator.CreateArgumentExceptionFor(
                () => configKey,
                ErrorStrings.InvalidConfigSettingKey,
                $"{configKey:G}");
        }
    }

    #endregion // ConfigSettingKey Extension Methods
}
