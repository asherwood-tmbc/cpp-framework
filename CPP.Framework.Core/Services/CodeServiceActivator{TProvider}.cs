using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using CPP.Framework.DependencyInjection;

using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Manages activation of an implementation for a service interface.
    /// </summary>
    /// <typeparam name="TProvider">The type of the service provider class.</typeparam>
    internal class CodeServiceActivator<TProvider> : CodeServiceActivator where TProvider : ICodeService
    {
        /// <summary>
        /// The delegate to call when create instances of the service via the default constructor.
        /// </summary>
        private static readonly Func<TProvider> CreateDefaultInstance;

        /// <summary>
        /// The delegate to call when creating instances of the service for named registrations.
        /// </summary>
        private static readonly Func<string, TProvider> CreateServiceFactory;

        /// <summary>
        /// A value indicating whether or not the service implementation supports singleton
        /// instances (i.e. one instance per application or named registration).
        /// </summary>
        // ReSharper disable once StaticMemberInGenericType
        private static readonly bool _SupportsSingletonInstances;

        /// <summary>
        /// Initializes static members of the <see cref="CodeServiceActivator{TProvider}"/>
        /// class.
        /// </summary>
        static CodeServiceActivator()
        {
            const BindingFlags SearchFlags = (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            // first, check if there is a factory method available.
            var methodInfo = typeof(TProvider)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(mi => (mi.GetCustomAttributes(typeof(CreateServiceInstanceAttribute), false).Any()))
                .WithSignature(new[] { typeof(string) }, typeof(TProvider));
            if (methodInfo != null)
            {
                // we've found a static factory method, so create a delegate for it.
                var parameters = new[] { Expression.Parameter(typeof(string), "name"), };
                var expression = (Expression.Call(methodInfo, parameters[0]) as Expression);
                CreateServiceFactory = Expression.Lambda<Func<string, TProvider>>(expression, parameters).Compile();
            }

            // now try to look for a parameterless default constructor.
            var ctor = typeof(TProvider).GetConstructor(SearchFlags, null, Type.EmptyTypes, null);
            if (ctor != null)
            {
                var expression = Expression.Convert(Expression.New(ctor), typeof(TProvider));
                CreateDefaultInstance = Expression.Lambda<Func<TProvider>>(expression).Compile();
            }

            // at this point, the provider must have either a default constructor or factory method 
            // defined, otherwise it cannot be considered a valid implementation for the service.
            if ((CreateServiceFactory == null) && (CreateDefaultInstance == null))
            {
                var message = string.Format(
                    ErrorStrings.InvalidServiceClassDefinition,
                    typeof(TProvider).FullName);
                throw new InvalidOperationException(message);
            }
            _SupportsSingletonInstances = typeof(ICodeServiceSingleton).IsAssignableFrom(typeof(TProvider));
        }

        /// <summary>
        /// Gets a value indicating whether or not the service implementation supports creating
        /// named instances (i.e. different instance per registration name).
        /// </summary>
        internal override bool SupportsNamedRegistrations => (CreateServiceFactory != null);

        /// <summary>
        /// Gets a value indicating whether or not the service implementation supports singleton
        /// instances (i.e. one instance per application or named registration).
        /// </summary>
        internal override bool SupportsSingletonInstances => _SupportsSingletonInstances;

        /// <summary>
        /// Creates an instance of the service.
        /// </summary>
        /// <param name="serviceType">
        /// The type of the service interface being requested.
        /// </param>
        /// <param name="registrationName">
        ///     The registered name of the service instance to create, or a null/empty string for the
        ///     default unnamed instance.
        /// </param>
        /// <returns>An instance of the service.</returns>
        internal override object CreateInstance(Type serviceType, string registrationName)
        {
            if (string.IsNullOrWhiteSpace(registrationName))
            {
                registrationName = null;
            }
            var buildKey = new NamedTypeBuildKey(serviceType, registrationName);
            var instance = default(TProvider);
            var success = false;
            var lifetime = default(ILifetimePolicy);
            var configuration = default(IServiceLocatorConfiguration);

            if (this.SupportsSingletonInstances)
            {
                // if the provider type is a singleton, check whether or not an instance of the
                // object has already been created through the ServiceLocator class (which will
                // also ensure there is only one instance via a custom BuilderStrategy object).
                configuration = ServiceLocator.Container.Configure<IServiceLocatorConfiguration>();
                if (configuration != null)
                {
                    lifetime = configuration.GetLifetimePolicy(buildKey);
                }
                if ((lifetime is ContainerControlledLifetimeManager) || (lifetime is CodeServiceSingletonLifetimeManager))
                {
                    instance = ((TProvider)lifetime.GetValue());
                    success = (instance != null);
                }
            }

            // if no instances have been found, then check if we are allowed to create one through
            // the ServiceLocator, which we can't do with automaticly registered services due to a
            // circular reference issue (namely, the service is registered in ServiceLocator using
            // this function as a factory method during automatic service registration).
            if (!success && CodeServiceProvider.CanUseServiceLocator(serviceType, registrationName))
            {
                success = (!string.IsNullOrWhiteSpace(registrationName)
                    ? ServiceLocator.TryGetInstance(serviceType, registrationName, out instance)
                    : ServiceLocator.TryGetInstance(serviceType, out instance));
            }

            // if still don't have an instance at this point, then try to create one by directly
            // calling the constructor.
            if (!success || (instance == null))
            {
                instance = (!this.SupportsNamedRegistrations
                    ? CreateDefaultInstance()
                    : CreateServiceFactory(registrationName));
                if (this.SupportsSingletonInstances && (configuration != null))
                {
                    // since this is the first time the service instance has been created, we need
                    // to configure ServiceLocator to return the same instance when it is resolving
                    // the service interface type.
                    lifetime = (lifetime ?? new CodeServiceSingletonLifetimeManager());
                    lifetime.SetValue(instance);
                    configuration.SetLifetimePolicy(buildKey, lifetime);
                }
            }
            return instance;
        }
    }
}
