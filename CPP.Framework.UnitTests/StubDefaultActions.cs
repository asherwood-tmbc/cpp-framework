using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.Configuration;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Diagnostics.Testing;
using CPP.Framework.IO;
using Microsoft.WindowsAzure.ServiceRuntime;
using Rhino.Mocks;

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
            return service.RegisterServiceStub(StubDefaultActions.SetupDefaultConfig);
        }

        /// <summary>
        /// Registers a stubbed service implementation with the <see cref="ServiceLocator"/> for
        /// consumption by a unit test.
        /// </summary>
        /// <param name="service">The service implementation to register.</param>
        /// <returns>The value of the <paramref name="service"/> parameter.</returns>
        internal static FileService RegisterServiceStub(this FileService service)
        {
            return service.RegisterServiceStub(StubDefaultActions.SetupDefaultConfig);
        }

        /// <summary>
        /// Registers a stubbed service implementation with the <see cref="ServiceLocator"/> for
        /// consumption by a unit test.
        /// </summary>
        /// <param name="service">The service implementation to register.</param>
        /// <returns>The value of the <paramref name="service"/> parameter.</returns>
        internal static RoleEnvironmentService RegisterServiceStub(this RoleEnvironmentService service)
        {
            return service.RegisterServiceStub(StubDefaultActions.SetupDefaultConfig);
        }

        /// <summary>
        /// Configures a stub object with the default actions required for testing methods that 
        /// access the default <see cref="ConfigSettingProvider"/>.
        /// </summary>
        /// <param name="service">The stub object to register.</param>
        internal static void SetupDefaultConfig(ConfigurationManagerService service)
        {
            var configurationErrorsException = StubFactory.CreateInstance<ConfigurationErrorsException>();

            service
                .StubAction(svc => svc.AppSettings).Return(new NameValueCollection())
                .StubAction(svc => svc.ConnectionStrings).Return(new ConnectionStringSettingsCollection());

            service
                .StubAction(svc => svc.GetSection(Arg<string>.Is.Anything)).Return(null)
                .StubAction(svc => svc.OpenExeConfiguration(Arg<ConfigurationUserLevel>.Is.Anything)).Throw(configurationErrorsException)
                .StubAction(svc => svc.OpenExeConfiguration(Arg<string>.Is.Anything)).Throw(configurationErrorsException)
                .StubAction(svc => svc.OpenMachineConfiguration()).Throw(configurationErrorsException)
                .StubAction(svc => svc.OpenMappedExeConfiguration(Arg<ExeConfigurationFileMap>.Is.Anything, Arg<ConfigurationUserLevel>.Is.Anything)).Throw(configurationErrorsException)
                .StubAction(svc => svc.OpenMappedExeConfiguration(Arg<ExeConfigurationFileMap>.Is.Anything, Arg<ConfigurationUserLevel>.Is.Anything, Arg<bool>.Is.Anything)).Throw(configurationErrorsException)
                .StubAction(svc => svc.OpenMappedMachineConfiguration(Arg<ConfigurationFileMap>.Is.Anything)).Throw(configurationErrorsException)
                .StubAction(svc => svc.RefreshSection(Arg<string>.Is.Anything)).Throw(configurationErrorsException);
        }

        /// <summary>
        /// Configures a stub object with the default actions required for testing methods that 
        /// access the <see cref="FileService"/>.
        /// </summary>
        /// <param name="service">The stub object to configure.</param>
        internal static void SetupDefaultConfig(FileService service)
        {
            service.StubAction(svc => svc.Exists(Arg<string>.Is.Anything)).Return(false);
        }

        /// <summary>
        /// Configures a stub object with the default actions required for testing methods that 
        /// access the <see cref="RoleConfigProvider"/>.
        /// </summary>
        /// <param name="service">The stub object to configure.</param>
        internal static void SetupDefaultConfig(RoleEnvironmentService service)
        {
            var invalidOperationException = new InvalidOperationException();
            var roleEnvironmentException = StubFactory.CreateInstance<RoleEnvironmentException>();

            service
                .StubAction(svc => svc.CurrentRoleInstance).Throw(invalidOperationException)
                .StubAction(svc => svc.DeloymentId).Throw(invalidOperationException)
                .StubAction(svc => svc.IsAvailable).Return(false)
                .StubAction(svc => svc.IsEmulated).Throw(invalidOperationException)
                .StubAction(svc => svc.IsAzureStorageEmulatorActive).Return(false)
                .StubAction(svc => svc.Roles).Throw(invalidOperationException)
                .StubAction(svc => svc.TraceSource).Return(new TraceSource(typeof(RoleEnvironment).FullName, SourceLevels.Information));

            service
                .StubAction(svc => svc.GetConfigurationSettingValue(Arg<string>.Is.Anything)).Throw(roleEnvironmentException)
                .StubAction(svc => svc.GetLocalResource(Arg<string>.Is.Anything)).Throw(roleEnvironmentException)
                .StubAction(svc => svc.RequestRecycle()).Throw(roleEnvironmentException);
        }
    }
}
