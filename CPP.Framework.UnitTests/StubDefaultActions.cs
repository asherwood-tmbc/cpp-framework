using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.Configuration;
using CPP.Framework.DependencyInjection;
using CPP.Framework.IO;
using CPP.Framework.UnitTests.Testing;
using Microsoft.WindowsAzure.ServiceRuntime;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CPP.Framework
{
    [ExcludeFromCodeCoverage]
    internal static partial class StubDefaultActions
    {
        /// <summary>
        /// Registers a stubbed service implementation with the <see cref="ServiceLocator"/> for
        /// consumption by a unit test.
        /// </summary>
        /// <param name="service">The service implementation to register.</param>
        /// <returns>The value of the <paramref name="service"/> parameter.</returns>
        internal static ConfigurationManagerService RegisterServiceStub(this ConfigurationManagerService service)
        {
            return ServiceStubHelper.RegisterServiceStub(service, StubDefaultActions.SetupDefaultConfig);
        }

        /// <summary>
        /// Registers a stubbed service implementation with the <see cref="ServiceLocator"/> for
        /// consumption by a unit test.
        /// </summary>
        /// <param name="service">The service implementation to register.</param>
        /// <returns>The value of the <paramref name="service"/> parameter.</returns>
        internal static FileService RegisterServiceStub(this FileService service)
        {
            return ServiceStubHelper.RegisterServiceStub(service, StubDefaultActions.SetupDefaultConfig);
        }

        /// <summary>
        /// Registers a stubbed service implementation with the <see cref="ServiceLocator"/> for
        /// consumption by a unit test.
        /// </summary>
        /// <param name="service">The service implementation to register.</param>
        /// <returns>The value of the <paramref name="service"/> parameter.</returns>
        internal static RoleEnvironmentService RegisterServiceStub(this RoleEnvironmentService service)
        {
            return ServiceStubHelper.RegisterServiceStub(service, StubDefaultActions.SetupDefaultConfig);
        }

        /// <summary>
        /// Configures a stub object with the default actions required for testing methods that
        /// access the default <see cref="ConfigSettingProvider"/>.
        /// </summary>
        /// <param name="service">The stub object to register.</param>
        internal static void SetupDefaultConfig(ConfigurationManagerService service)
        {
            var configurationErrorsException = ReflectionHelper.CreateInstance<ConfigurationErrorsException>();

            ConfigSettingsStubExtensions.GetOrCreateAppSettings(service);
            ConfigSettingsStubExtensions.GetOrCreateConnectionStrings(service);
            service.GetSection(Arg.Any<string>()).Returns(null);
            service.OpenExeConfiguration(Arg.Any<ConfigurationUserLevel>()).Throws(configurationErrorsException);
            service.OpenExeConfiguration(Arg.Any<string>()).Throws(configurationErrorsException);
            service.OpenMachineConfiguration().Throws(configurationErrorsException);
            service.OpenMappedExeConfiguration(Arg.Any<ExeConfigurationFileMap>(), Arg.Any<ConfigurationUserLevel>()).Throws(configurationErrorsException);
            service.OpenMappedExeConfiguration(Arg.Any<ExeConfigurationFileMap>(), Arg.Any<ConfigurationUserLevel>(), Arg.Any<bool>()).Throws(configurationErrorsException);
            service.OpenMappedMachineConfiguration(Arg.Any<ConfigurationFileMap>()).Throws(configurationErrorsException);
            service.When(svc => svc.RefreshSection(Arg.Any<string>())).Throw(configurationErrorsException);
        }

        /// <summary>
        /// Configures a stub object with the default actions required for testing methods that
        /// access the <see cref="FileService"/>.
        /// </summary>
        /// <param name="service">The stub object to configure.</param>
        internal static void SetupDefaultConfig(FileService service)
        {
            service.Exists(Arg.Any<string>()).Returns(false);
        }

        /// <summary>
        /// Configures a stub object with the default actions required for testing methods that
        /// access the <see cref="RoleConfigProvider"/>.
        /// </summary>
        /// <param name="service">The stub object to configure.</param>
        internal static void SetupDefaultConfig(RoleEnvironmentService service)
        {
            var invalidOperationException = new InvalidOperationException();
            var roleEnvironmentException = ReflectionHelper.CreateInstance<RoleEnvironmentException>();

            service.CurrentRoleInstance.Throws(invalidOperationException);
            service.DeloymentId.Throws(invalidOperationException);
            service.IsAvailable.Returns(false);
            service.IsEmulated.Throws(invalidOperationException);
            service.IsAzureStorageEmulatorActive.Returns(false);
            service.Roles.Throws(invalidOperationException);
            service.TraceSource.Returns(new TraceSource(typeof(RoleEnvironment).FullName, SourceLevels.Information));
            service.GetConfigurationSettingValue(Arg.Any<string>()).Throws(roleEnvironmentException);
            service.GetLocalResource(Arg.Any<string>()).Throws(roleEnvironmentException);
            service.When(svc => svc.RequestRecycle()).Throw(roleEnvironmentException);
        }
    }
}
