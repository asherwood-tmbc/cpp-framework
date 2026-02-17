using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;

using CPP.Framework.DependencyInjection;
using CPP.Framework.Security;
using CPP.Framework.Security.Policies;
using JetBrains.Annotations;

using Rhino.Mocks;

namespace CPP.Framework.Diagnostics.Testing
{
    /// <summary>
    /// Factory class used to generate stubs and object instances for test dependendies.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class StubFactory
    {
        /// <summary>
        /// Creates stubs for objects that are dependent on the current stub.
        /// </summary>
        /// <typeparam name="TService">The type of the service class.</typeparam>
        /// <param name="service">The service instance.</param>
        /// <param name="action">An expression that receives the service stub.</param>
        /// <returns>The value of the <paramref name="service"/> parameter.</returns>
        [UsedImplicitly]
        public static TService CreateDependentStubs<TService>(this TService service, Action<TService> action)
            where TService : class
        {
            ArgumentValidator.ValidateThisObj(() => service);
            ArgumentValidator.ValidateNotNull(() => action);
            action(service);
            return service;
        }

        /// <summary>
        /// Creates stubs for objects that are dependent on the current stub.
        /// </summary>
        /// <typeparam name="TService">The type of the service class.</typeparam>
        /// <typeparam name="TArg">The type of the callback argument.</typeparam>
        /// <param name="service">The service instance.</param>
        /// <param name="action">An expression that receives the service stub.</param>
        /// <param name="arg">An optional object to pass to the <paramref name="action"/> delegate.</param>
        /// <returns>The value of the <paramref name="service"/> parameter.</returns>
        [UsedImplicitly]
        public static TService CreateDependentStubs<TService, TArg>(this TService service, Action<TService, TArg> action, TArg arg)
            where TService : class
        {
            ArgumentValidator.ValidateThisObj(() => service);
            ArgumentValidator.ValidateNotNull(() => action);
            action(service, arg);
            return service;
        }

        /// <summary>
        /// Creates an instance of a class regardless of it's defined scope (public, private, 
        /// internal, etc).
        /// </summary>
        /// <typeparam name="T">The type of the class.</typeparam>
        /// <param name="constructorArguments">An optional list of parameters to pass to the constructor.</param>
        /// <returns>The new object instance.</returns>
        [UsedImplicitly]
        public static T CreateInstance<T>(params object[] constructorArguments)
        {
            const BindingFlags InternalBindingFlags = (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var types = constructorArguments.Select(arg => arg.GetType()).ToArray();
            var ctor = typeof(T).GetConstructor(InternalBindingFlags, null, types, null);
            // ReSharper disable once PossibleNullReferenceException
            return ((T)ctor.Invoke(constructorArguments));
        }

        /// <summary>
        /// Generates a partial stub object for a given class or interface (i.e. a stub that 
        /// defaults to the original implementation, unless it has specifically been stubbed).
        /// </summary>
        /// <typeparam name="T">The type of the class to stub</typeparam>
        /// <param name="constructorArguments">An optional list of arguments to pass to the stub's constructor.</param>
        /// <returns>An instance of the stubbed object.</returns>
        [UsedImplicitly]
        public static T CreatePartial<T>(params object[] constructorArguments)
            where T : class
        {
            return MockRepository.GeneratePartialMock<T>(constructorArguments);
        }

        /// <summary>
        /// Generates a new stub for a security principal object.
        /// </summary>
        /// <param name="authenticationType">
        /// The type of authentication, or null/empty for an anonymous user.
        /// </param>
        /// <returns>An <see cref="ClaimsPrincipal"/> object.</returns>
        [UsedImplicitly]
        public static ClaimsPrincipal CreatePrincipal(string authenticationType)
        {
            var identity = ((string.IsNullOrWhiteSpace(authenticationType))
                ? new ClaimsIdentity()
                : new ClaimsIdentity(authenticationType));
            return new ClaimsPrincipal(identity);
        }

        /// <summary>
        /// Generates a stub object for a given class or interface.
        /// </summary>
        /// <typeparam name="T">The type of the class to stub</typeparam>
        /// <param name="constructorArguments">An optional list of arguments to pass to the stub's constructor.</param>
        /// <returns>An instance of the stubbed object.</returns>
        [UsedImplicitly]
        public static T CreateStub<T>(params object[] constructorArguments)
            where T : class
        {
            return MockRepository.GenerateStub<T>(constructorArguments);
        }

        /// <summary>
        /// Grants a security access right to a claims principal.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to modify.</param>
        /// <param name="accessRight">The internal name of the access right to grant.</param>
        /// <returns>A reference to <paramref name="principal"/>.</returns>
        [UsedImplicitly]
        public static ClaimsPrincipal GrantAccessRight(this ClaimsPrincipal principal, string accessRight)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => accessRight);
            return principal.GrantClaim(CommonClaimTypes.AccessRight, accessRight);
        }

        /// <summary>
        /// Grants a security access right to a claims principal.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to modify.</param>
        /// <param name="claimType">The type of the claim.</param>
        /// <param name="value">The value of the claim.</param>
        /// <param name="valueType">
        /// The data type of <paramref name="value"/> (from <see cref="ClaimValueTypes"/>).
        /// </param>
        /// <returns>A reference to <paramref name="principal"/>.</returns>
        [UsedImplicitly]
        public static ClaimsPrincipal GrantClaim(this ClaimsPrincipal principal, string claimType, string value, string valueType = ClaimValueTypes.String)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => value);

            var claim = new Claim(claimType, value, (valueType ?? ClaimValueTypes.String));
            ((ClaimsIdentity)principal.Identity).AddClaim(claim);
            
            return principal;
        }

        /// <summary>
        /// Grants a security feature to a claims principal.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to modify.</param>
        /// <param name="featureName">The internal name of the feature to grant.</param>
        /// <returns>A reference to <paramref name="principal"/>.</returns>
        [UsedImplicitly]
        public static ClaimsPrincipal GrantFeatureName(this ClaimsPrincipal principal, string featureName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => featureName);
            return principal.GrantClaim(CommonClaimTypes.FeatureName, featureName);
        }

        /// <summary>
        /// Sets the user name for a <see cref="ClaimsPrincipal"/> object.
        /// </summary>
        /// <param name="principal">The <see cref="ClaimsPrincipal"/> to modify.</param>
        /// <param name="userName">The user name value to set.</param>
        /// <returns>A reference to <paramref name="principal"/>.</returns>
        [UsedImplicitly]
        public static ClaimsPrincipal GrantUserName(this ClaimsPrincipal principal, string userName)
        {
            ArgumentValidator.ValidateNotNull(() => principal);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => userName);

            var identity = ((ClaimsIdentity)principal.Identity);
            var existing = identity.Claims
                .Where(clm => (clm.Type == ClaimTypes.Name))
                .ToList();
            existing.ForEach(clm => identity.TryRemoveClaim(clm));
            identity.AddClaim(new Claim(identity.NameClaimType, userName));

            return principal;
        }

        /// <summary>
        /// Registers a stubbed object to an interface so that requests for that interface return 
        /// instances of the stub.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to register.</typeparam>
        /// <typeparam name="TProvider">The implementation stub for <typeparamref name="TInterface"/>.</typeparam>
        [UsedImplicitly]
        public static void RegisterInterfaceStub<TInterface, TProvider>()
            where TProvider : class, TInterface
        {
            ServiceLocator.Register<TInterface, TProvider>();
        }

        /// <summary>
        /// Registers an <see cref="IPrincipal"/> with the <see cref="ServiceLocator"/>.
        /// </summary>
        /// <typeparam name="TPrincipal">The type of the principal.</typeparam>
        /// <param name="principal">The principal to register.</param>
        /// <returns>A reference to <paramref name="principal"/>.</returns>
        [UsedImplicitly]
        public static TPrincipal RegisterPrincipal<TPrincipal>(this TPrincipal principal)
            where TPrincipal : class, IPrincipal
        {
            ArgumentValidator.ValidateNotNull(() => principal);
            ServiceLocator.Register<IPrincipal>(principal);
            return principal;
        }

        /// <summary>
        /// Registers a stubbed service implementation with the <see cref="ServiceLocator"/> for
        /// consumption by a unit test.
        /// </summary>
        /// <typeparam name="TService">The type of the service class.</typeparam>
        /// <param name="service">The service implementation to register.</param>
        /// <returns>The value of the <paramref name="service"/> parameter.</returns>
        [UsedImplicitly]
        public static TService RegisterServiceStub<TService>(this TService service)
            where TService : class
        {
            return RegisterServiceStub(service, null, null);
        }

        /// <summary>
        /// Registers a stubbed service implementation with the <see cref="ServiceLocator"/> for
        /// consumption by a unit test.
        /// </summary>
        /// <typeparam name="TService">The type of the service class.</typeparam>
        /// <param name="service">The service implementation to register.</param>
        /// <param name="registrationName">The name of the service registration.</param>
        /// <returns>The value of the <paramref name="service"/> parameter.</returns>
        [UsedImplicitly]
        public static TService RegisterServiceStub<TService>(this TService service, string registrationName)
            where TService : class
        {
            return RegisterServiceStub(service, null, registrationName);
        }

        /// <summary>
        /// Registers a stubbed service implementation with the <see cref="ServiceLocator"/> for
        /// consumption by a unit test.
        /// </summary>
        /// <typeparam name="TService">The type of the service class.</typeparam>
        /// <param name="service">The service implementation to register.</param>
        /// <param name="setupAction">A delegate that is called to stub the default actions for <paramref name="service"/>.</param>
        /// <returns>The value of the <paramref name="service"/> parameter.</returns>
        [UsedImplicitly]
        public static TService RegisterServiceStub<TService>(this TService service, Action<TService> setupAction)
            where TService : class
        {
            return StubFactory.RegisterServiceStub(service, setupAction, null);
        }

        /// <summary>
        /// Registers a stubbed service implementation with the <see cref="ServiceLocator"/> for
        /// consumption by a unit test.
        /// </summary>
        /// <typeparam name="TService">The type of the service class.</typeparam>
        /// <param name="service">The service implementation to register.</param>
        /// <param name="setupAction">A delegate that is called to stub the default actions for <paramref name="service"/>.</param>
        /// <param name="registrationName">The name of the service registration.</param>
        /// <returns>The value of the <paramref name="service"/> parameter.</returns>
        [UsedImplicitly]
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
                    {
                        ServiceLocator.Register(info.InterfaceType, service, info.RegistrationName);
                    }
                    else ServiceLocator.Register(info.InterfaceType, service);
                }
            }
            return service;
        }

        /// <summary>
        /// Grants a security access right to a claims identity.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsPrincipal"/> to modify.</param>
        /// <param name="accessRight">The internal name of the access right to grant.</param>
        /// <returns>A reference to <paramref name="identity"/>.</returns>
        [UsedImplicitly]
        public static ClaimsIdentity GrantAccessRight(this ClaimsIdentity identity, string accessRight)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => accessRight);
            return ((ClaimsIdentity)identity).Grant(SecurityAccessRightPolicy.Create(accessRight));
        }

        /// <summary>
        /// Revokes a security access right to a claims identity.
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsPrincipal"/> to modify.</param>
        /// <param name="accessRight">The internal name of the access right to grant.</param>
        /// <returns>A reference to <paramref name="identity"/>.</returns>
        [UsedImplicitly]
        public static ClaimsIdentity RevokeAccessRight(this ClaimsIdentity identity, string accessRight)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => accessRight);
            return ((ClaimsIdentity)identity).Revoke(SecurityAccessRightPolicy.Create(accessRight));
        }

        /// <summary>
        /// Generates a custom implementation for method from a stubbed service instance. The 
        /// operation must be completed by calling a method on the return value, otherwise the
        /// stub implementation will not be generated.
        /// </summary>
        /// <typeparam name="TService">The type of the service class.</typeparam>
        /// <param name="service">The service instance.</param>
        /// <param name="action">A delegate that evaluates to the method being stubbed.</param>
        /// <returns>A <see cref="StubActionContext{TService}"/> instance.</returns>
        [UsedImplicitly]
        public static StubActionContext<TService> StubAction<TService>(this TService service, System.Action<TService> action)
            where TService : class
        {
            return new StubActionContext<TService>(service, action);
        }

        /// <summary>
        /// Generates a custom implementation for a property or method from a stubbed service 
        /// instance. The operation must be completed by calling a method on the return value, 
        /// otherwise the stub implementation will not be generated.
        /// </summary>
        /// <typeparam name="TService">The type of the service class.</typeparam>
        /// <typeparam name="TReturn">The return type of the action.</typeparam>
        /// <param name="service">The service instance.</param>
        /// <param name="action">A delegate that evaluates to the method being stubbed.</param>
        /// <returns>A <see cref="StubActionContext{TService}"/> instance.</returns>
        [UsedImplicitly]
        public static StubActionContext<TService, TReturn> StubAction<TService, TReturn>(this TService service, Expression<Function<TService, TReturn>> action)
            where TService : class
        {
            return new StubActionContext<TService, TReturn>(service, action);
        }
    }
}
