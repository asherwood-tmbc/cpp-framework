using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using CPP.Framework.DependencyInjection;

namespace CPP.Framework.UnitTests.Testing
{
    [ExcludeFromCodeCoverage]
    internal static class ServiceStubHelper
    {
        public static TService RegisterServiceStub<TService>(this TService service)
            where TService : class
        {
            return RegisterServiceStub(service, null, null);
        }

        public static TService RegisterServiceStub<TService>(this TService service, string registrationName)
            where TService : class
        {
            return RegisterServiceStub(service, null, registrationName);
        }

        public static TService RegisterServiceStub<TService>(this TService service, Action<TService> setupAction)
            where TService : class
        {
            return RegisterServiceStub(service, setupAction, null);
        }

        public static TService RegisterServiceStub<TService>(this TService service, Action<TService> setupAction, string registrationName)
            where TService : class
        {
            ArgumentValidator.ValidateThisObj(() => service);
            setupAction?.Invoke(service);

            if (typeof(IPrincipal).IsAssignableFrom(typeof(TService)))
            {
                ServiceLocator.Register((IPrincipal)service);
            }
            else
            {
                var registrations = typeof(TService)
                    .GetCustomAttributes(typeof(StubServiceRegistrationAttribute), false)
                    .OfType<StubServiceRegistrationAttribute>()
                    .DefaultIfEmpty(new StubServiceRegistrationAttribute(typeof(TService), registrationName));

                foreach (var info in registrations)
                {
                    if (!string.IsNullOrWhiteSpace(info.RegistrationName))
                        ServiceLocator.Register(info.InterfaceType, service, info.RegistrationName);
                    else
                        ServiceLocator.Register(info.InterfaceType, service);
                }
            }

            return service;
        }

        public static void RegisterInterfaceStub<TInterface, TProvider>()
            where TProvider : class, TInterface
        {
            ServiceLocator.Register<TInterface, TProvider>();
        }

        public static TPrincipal RegisterPrincipal<TPrincipal>(this TPrincipal principal)
            where TPrincipal : class, IPrincipal
        {
            ArgumentValidator.ValidateNotNull(() => principal);
            ServiceLocator.Register<IPrincipal>(principal);
            return principal;
        }
    }
}
