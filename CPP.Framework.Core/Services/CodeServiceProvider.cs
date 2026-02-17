using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;

using CPP.Framework.DependencyInjection;

using JetBrains.Annotations;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Provides methods to register and access code services for the application.
    /// </summary>
    public static class CodeServiceProvider
    {
        /// <summary>
        /// The default binding flags to use when searching for members through reflection.
        /// </summary>
        private const BindingFlags DefaultBindingFlags = (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        /// <summary>
        /// The set of service interface types that were automatically registered with the service
        /// locator as a part of the service registration.
        /// </summary>
        private static readonly HashSet<ServiceKey> _AutoRegistrationServiceKeys = new HashSet<ServiceKey>();

        /// <summary>
        /// The mapping of register service types to their lifetime containers.
        /// </summary>
        private static readonly ConcurrentDictionary<ServiceKey, CodeServiceContainer> _RegisteredServiceMap = new ConcurrentDictionary<ServiceKey, CodeServiceContainer>();

        /// <summary>
        /// The mapping of register service implementation to their lifetime containers.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, CodeServiceContainer> _ServiceContainerMap = new ConcurrentDictionary<Type, CodeServiceContainer>();

        /// <summary>
        /// Initializes static members of the <see cref="CodeServiceProvider"/> class.
        /// </summary>
        static CodeServiceProvider()
        {
            ServiceLocator.Unloaded += ((sender, args) => CodeServiceProvider.Unload());
        }

        /// <summary>
        /// Gets a value indicating whether or not the <see cref="ServiceLocator"/> can be used to
        /// resolve the implementation type for a give service registration.
        /// </summary>
        /// <param name="serviceType">The type of the service interface.</param>
        /// <param name="registrationName">The name of the registration instance to unload.</param>
        /// <returns>
        /// <c>True</c> if the <see cref="ServiceLocator"/> can be used to resolve the service
        /// implementation type; otherwise, <c>false</c>.
        /// </returns>
        internal static bool CanUseServiceLocator(Type serviceType, string registrationName)
        {
            return (!_AutoRegistrationServiceKeys.Contains(ServiceKey.Create(serviceType, registrationName)));
        }

        /// <summary>
        /// Gets an instance of a register service.
        /// </summary>
        /// <param name="serviceType">The type of the service interface.</param>
        /// <returns>A service object.</returns>
        [ExcludeFromCodeCoverage]
        public static object GetService(Type serviceType) => GetService(serviceType, null);

        /// <summary>
        /// Gets an instance of a register service.
        /// </summary>
        /// <param name="serviceType">The type of the service interface.</param>
        /// <param name="name">The name of the service instance to retrieve.</param>
        /// <returns>A service object.</returns>
        public static object GetService(Type serviceType, string name)
        {
            var serviceKey = ServiceKey.Create(serviceType, name);
            if (!_RegisteredServiceMap.TryGetValue(serviceKey, out var container))
            {
                var registration = serviceType.GetCustomAttribute<AutoRegisterServiceAttribute>();
                if (registration != null)
                {
                    var providerType = (string.IsNullOrWhiteSpace(name)
                        ? ServiceLocator.GetMappedType(serviceType)
                        : ServiceLocator.GetMappedType(serviceType, name));
                    providerType = (providerType ?? registration.ProviderType ?? serviceType);
                    Register(serviceType, providerType, name, registration.RecursionPolicy, out container);
                }
            }
            if (container != null)
            {
                return container.GetInstance(serviceType, name);
            }
            throw new MissingServiceRegistrationException(serviceType);
        }

        /// <summary>
        /// Gets an instance of a register service.
        /// </summary>
        /// <typeparam name="TService">The type of the service interface.</typeparam>
        /// <returns>An <typeparamref name="TService"/> object.</returns>
        [ExcludeFromCodeCoverage]
        public static TService GetService<TService>()
            where TService : class
        {
            return ((TService)GetService(typeof(TService), null));
        }

        /// <summary>
        /// Gets an instance of a register service.
        /// </summary>
        /// <typeparam name="TService">The type of the service interface.</typeparam>
        /// <param name="name">The name of the service instance to retrieve.</param>
        /// <returns>An <typeparamref name="TService"/> object.</returns>
        [ExcludeFromCodeCoverage]
        public static TService GetService<TService>(string name)
            where TService : class
        {
            return ((TService)GetService(typeof(TService), name));
        }

        /// <summary>
        /// Registers a service implementation with the provider.
        /// </summary>
        /// <param name="serviceType">The type of the service interface.</param>
        /// <param name="providerType">The type of the service implementation class.</param>
        /// <param name="registrationName">The unique name of the new service registration.</param>
        /// <param name="recursionPolicy">
        /// The <see cref="LockRecursionPolicy"/> for the service instance access lock.
        /// </param>
        /// <param name="container">
        /// An output parameter that receives the value of the <see cref="CodeServiceContainer"/>
        /// that was used for the service registration.
        /// </param>
        /// <returns>
        /// <c>True</c> if the provider was successfully registered for the service interface;
        /// otherwise <c>false</c> if the service interface is already registered.
        /// </returns>
        private static bool Register(Type serviceType, Type providerType, string registrationName, LockRecursionPolicy recursionPolicy, out CodeServiceContainer container)
        {
            // ensure that the provider derives from or implements the service interface.
            if (!serviceType.IsAssignableFrom(providerType))
            {
                throw new ArgumentException(ErrorStrings.IncompatibleProviderServiceType);
            }

            // retrieve or create container for the service implementation first.
            var factory = container = _ServiceContainerMap.GetOrAdd(
                providerType,
                (ti) =>
                {
                    var ActivatorType = typeof(CodeServiceActivator<>)
                        .MakeGenericType(ti);
                    var activator = default(CodeServiceActivator);

                    try
                    {
                        activator = ((CodeServiceActivator)Activator.CreateInstance(ActivatorType, DefaultBindingFlags, null, new object[0], null));
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw new InvalidServiceRegistrationException(ti, UnwrapActivatorException(ex));
                    }

                    var ContainerType = default(Type);
                    var arguments = default(object[]);
                    if (!activator.SupportsSingletonInstances)
                    {
                        ContainerType = typeof(CodeServicePerUseLifetimeContainer<>);
                        arguments = new object[] { activator };
                    }
                    else
                    {
                        ContainerType = (activator.SupportsNamedRegistrations
                            ? typeof(CodeServiceNamedSingletonContainer<>)
                            : typeof(CodeServiceBasicSingletonContainer<>));
                        arguments = new object[] { activator, recursionPolicy };
                    }
                    ContainerType = ContainerType.MakeGenericType(providerType);

                    return ((CodeServiceContainer)Activator.CreateInstance(ContainerType, DefaultBindingFlags, null, arguments, null));
                });
            var serviceKey = ServiceKey.Create(serviceType, registrationName);

            // attempt to register the container with the service
            if (_RegisteredServiceMap.TryAdd(serviceKey, container))
            {
                // since this is a new registration, we also need to register a factory method with
                // the ServiceLocator, for backwards compatibility.
                if (string.IsNullOrWhiteSpace(registrationName))
                {
                    if (!ServiceLocator.IsRegistered(serviceType))
                    {
                        ServiceLocator.Register(serviceType, (ti, name) => factory.GetInstance(ti, name));
                        _AutoRegistrationServiceKeys.Add(serviceKey);
                    }
                }
                else if (!ServiceLocator.IsRegistered(serviceType, registrationName))
                {
                    ServiceLocator.Register(serviceType, (ti, name) => factory.GetInstance(ti, name), registrationName);
                    _AutoRegistrationServiceKeys.Add(serviceKey);
                }
            }
            return false;   // the service interface was already registered.
        }

        /// <summary>
        /// Registers a service implementation with the provider.
        /// </summary>
        /// <param name="serviceType">The type of the service interface.</param>
        /// <param name="providerType">The type of the service implementation class.</param>
        /// <returns>
        /// <c>True</c> if the provider was successfully registered for the service interface;
        /// otherwise <c>false</c> if the service interface is already registered.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static bool Register(Type serviceType, Type providerType)
        {
            return Register(serviceType, providerType, null, LockRecursionPolicy.NoRecursion, out _);
        }

        /// <summary>
        /// Registers a service implementation with the provider.
        /// </summary>
        /// <param name="serviceType">The type of the service interface.</param>
        /// <param name="providerType">The type of the service implementation class.</param>
        /// <param name="recursionPolicy">
        /// The <see cref="LockRecursionPolicy"/> for the service instance access lock.
        /// </param>
        /// <returns>
        /// <c>True</c> if the provider was successfully registered for the service interface;
        /// otherwise <c>false</c> if the service interface is already registered.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static bool Register(Type serviceType, Type providerType, LockRecursionPolicy recursionPolicy)
        {
            return Register(serviceType, providerType, null, recursionPolicy, out _);
        }

        /// <summary>
        /// Registers a service implementation with the provider.
        /// </summary>
        /// <param name="serviceType">The type of the service interface.</param>
        /// <param name="providerType">The type of the service implementation class.</param>
        /// <param name="registrationName">The unique name of the new service registration.</param>
        /// <returns>
        /// <c>True</c> if the provider was successfully registered for the service interface;
        /// otherwise <c>false</c> if the service interface is already registered.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static bool Register(Type serviceType, Type providerType, string registrationName)
        {
            return Register(serviceType, providerType, registrationName, LockRecursionPolicy.NoRecursion, out _);
        }

        /// <summary>
        /// Registers a service implementation with the provider.
        /// </summary>
        /// <param name="serviceType">The type of the service interface.</param>
        /// <param name="providerType">The type of the service implementation class.</param>
        /// <param name="registrationName">The unique name of the new service registration.</param>
        /// <param name="recursionPolicy">
        /// The <see cref="LockRecursionPolicy"/> for the service instance access lock.
        /// </param>
        /// <returns>
        /// <c>True</c> if the provider was successfully registered for the service interface;
        /// otherwise <c>false</c> if the service interface is already registered.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static bool Register(Type serviceType, Type providerType, string registrationName, LockRecursionPolicy recursionPolicy)
        {
            return Register(serviceType, providerType, registrationName, recursionPolicy, out _);
        }

        ////-------------------------------------------------------------------------------------------////

        /// <summary>
        /// Registers a service implementation with the provider.
        /// </summary>
        /// <typeparam name="TService">The type of the service interface.</typeparam>
        /// <typeparam name="TProvider">The type of the service implementation class.</typeparam>
        /// <returns>
        /// <c>True</c> if the provider was successfully registered for the service interface;
        /// otherwise <c>false</c> if the service interface is already registered.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static bool Register<TService, TProvider>()
            where TService : class
            where TProvider : ICodeService, TService
        {
            return Register(typeof(TService), typeof(TProvider), null, LockRecursionPolicy.NoRecursion, out _);
        }

        /// <summary>
        /// Registers a service implementation with the provider.
        /// </summary>
        /// <typeparam name="TService">The type of the service interface.</typeparam>
        /// <typeparam name="TProvider">The type of the service implementation class.</typeparam>
        /// <param name="recursionPolicy">
        /// The <see cref="LockRecursionPolicy"/> for the service instance access lock.
        /// </param>
        /// <returns>
        /// <c>True</c> if the provider was successfully registered for the service interface;
        /// otherwise <c>false</c> if the service interface is already registered.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static bool Register<TService, TProvider>(LockRecursionPolicy recursionPolicy)
            where TService : class
            where TProvider : ICodeService, TService
        {
            return Register(typeof(TService), typeof(TProvider), null, recursionPolicy, out _);
        }

        /// <summary>
        /// Registers a service implementation with the provider.
        /// </summary>
        /// <typeparam name="TService">The type of the service interface.</typeparam>
        /// <typeparam name="TProvider">The type of the service implementation class.</typeparam>
        /// <param name="registrationName">The unique name of the new service registration.</param>
        /// <returns>
        /// <c>True</c> if the provider was successfully registered for the service interface;
        /// otherwise <c>false</c> if the service interface is already registered.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static bool Register<TService, TProvider>(string registrationName)
            where TService : class
            where TProvider : ICodeService, TService
        {
            return Register(typeof(TService), typeof(TProvider), registrationName, LockRecursionPolicy.NoRecursion, out _);
        }

        /// <summary>
        /// Registers a service implementation with the provider.
        /// </summary>
        /// <typeparam name="TService">The type of the service interface.</typeparam>
        /// <typeparam name="TProvider">The type of the service implementation class.</typeparam>
        /// <param name="registrationName">The unique name of the new service registration.</param>
        /// <param name="recursionPolicy">
        /// The <see cref="LockRecursionPolicy"/> for the service instance access lock.
        /// </param>
        /// <returns>
        /// <c>True</c> if the provider was successfully registered for the service interface;
        /// otherwise <c>false</c> if the service interface is already registered.
        /// </returns>
        [ExcludeFromCodeCoverage]
        public static bool Register<TService, TProvider>(string registrationName, LockRecursionPolicy recursionPolicy)
            where TService : class
            where TProvider : ICodeService, TService
        {
            return Register(typeof(TService), typeof(TProvider), registrationName, recursionPolicy, out _);
        }

        /// <summary>
        /// Unloads any currently loaded service instances for the default registration. Please 
        /// note that this method only works for singleton service registrations, so instances that 
        /// are returned from per-use services are not affected. In addition, calling this method 
        /// will not remove the registration from the provider, unlike the <see cref="Unload()"/> 
        /// method.
        /// </summary>
        /// <typeparam name="TService">The type of the service interface.</typeparam>
        [ExcludeFromCodeCoverage]
        [UsedImplicitly]
        public static void Release<TService>() where TService : class => Release<TService>(null);

        /// <summary>
        /// Unloads any currently loaded service instances for a named service registration. Please 
        /// note that this method only works for singleton service registrations, so instances that 
        /// are returned from per-use services are not affected. In addition, calling this method
        /// will not remove the registration from the provider, unlike the <see cref="Unload()"/>
        /// method.
        /// </summary>
        /// <typeparam name="TService">The type of the service interface.</typeparam>
        /// <param name="registrationName">The name of the registration instance to unload.</param>
        public static void Release<TService>(string registrationName) where TService : class
        {
            var serviceKey = ServiceKey.Create<TService>(registrationName);
            if (_RegisteredServiceMap.TryGetValue(serviceKey, out var container))
            {
                container.Release(registrationName);
            }
        }

        /// <summary>
        /// Unloads all of the services currently registered with the provider.
        /// </summary>
        public static void Unload()
        {
            foreach (var container in _ServiceContainerMap.Values)
            {
                container.ReleaseAll();
            }
            _RegisteredServiceMap.Clear();
            _ServiceContainerMap.Clear();
        }

        /// <summary>
        /// Unwraps the exception thrown by the <see cref="Activator"/> when creating an object.
        /// </summary>
        /// <param name="ex">The exception to unwrap.</param>
        /// <returns>The unwrapped exception.</returns>
        private static Exception UnwrapActivatorException(Exception ex)
        {
            var unwrapped = ex;
            if (ex is TargetInvocationException target)
            {
                unwrapped = UnwrapActivatorException(target.InnerException);
            }
            else if (ex is TypeInitializationException tpinit)
            {
                unwrapped = UnwrapActivatorException(tpinit.InnerException);
            }
            return unwrapped;
        }

        #region ServiceKey Class Declaration

        /// <summary>
        /// Defines a unique key for a service registration.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private sealed class ServiceKey : IEquatable<ServiceKey>
        {
            /// <summary>
            /// The storage collection for the cached <see cref="ServiceKey"/> instances.
            /// </summary>
            private static readonly ConcurrentDictionary<int, ServiceKey> _ServiceKeyMap = new ConcurrentDictionary<int, ServiceKey>();

            /// <summary>
            /// Initializes a new instance of the <see cref="ServiceKey"/> class.
            /// class.
            /// </summary>
            /// <param name="type">The type of the service interface.</param>
            /// <param name="name">The name of the registration, or null/empty for the default.</param>
            private ServiceKey(Type type, string name)
            {
                this.Name = name;
                this.Type = type;
            }

            /// <summary>
            /// Gets the name of the registration, or <see cref="string.Empty"/> for the default 
            /// registration.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets the type of the registered service interface
            /// </summary>
            public Type Type { get; }

            /// <summary>
            /// Creates a new <see cref="ServiceKey"/> instance.
            /// </summary>
            /// <typeparam name="TService">The type of the registered service interface.</typeparam>
            /// <param name="name">The name of the registration, or null/empty for the default.</param>
            /// <returns>A <see cref="ServiceKey"/> object.</returns>
            public static ServiceKey Create<TService>(string name) => Create(typeof(TService), name);

            /// <summary>
            /// Creates a new <see cref="ServiceKey"/> instance.
            /// </summary>
            /// <param name="serviceType">The type of the registered service interface.</param>
            /// <param name="name">The name of the registration, or null/empty for the default.</param>
            /// <returns>A <see cref="ServiceKey"/> object.</returns>
            public static ServiceKey Create(Type serviceType, string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = string.Empty;
                }
                var hashcode = GetHashCode(serviceType, name);

                var serviceKey = _ServiceKeyMap.GetOrAdd(
                    hashcode,
                    (hc) => new ServiceKey(serviceType, name));
                return serviceKey;
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="that">An object to compare with this object.</param>
            /// <returns>
            /// <c>True</c> if the current object is equal to <paramref name="that" />; otherwise,
            /// <c>false</c>.
            /// </returns>
            public bool Equals(ServiceKey that)
            {
                if (ReferenceEquals(null, that)) return false;
                if (ReferenceEquals(this, that)) return true;
                return string.Equals(this.Name, that.Name) && this.Type.Equals(that.Type);
            }

            /// <summary>
            /// Determines whether the specified object is equal to the current object.
            /// </summary>
            /// <param name="that">The object to compare with the current object. </param>
            /// <returns>
            /// <c>True</c> if the specified object is equal to the current object; otherwise,
            /// <c>false</c>.
            /// </returns>
            public override bool Equals(object that)
            {
                if (ReferenceEquals(null, that)) return false;
                if (ReferenceEquals(this, that)) return true;
                return that is ServiceKey key && this.Equals(key);
            }

            /// <summary>
            /// Serves as the default hash function.
            /// </summary>
            /// <returns>A hash code for the current object.</returns>
            public override int GetHashCode() => GetHashCode(this.Type, this.Name);

            /// <summary>
            /// Calculates the combined hash code for a given pair of <see cref="Type"/> and 
            /// <see cref="string"/> objects.
            /// </summary>
            /// <param name="type">The type object to calculate against.</param>
            /// <param name="name">The name string to calculate against.</param>
            /// <returns>A hash code value.</returns>
            private static int GetHashCode([NotNull] Type type, [NotNull] string name)
            {
                unchecked
                {
                    return (name.GetHashCode() * 397) ^ type.GetHashCode();
                }
            }

            /// <summary>
            /// Returns a value that indicates whether the values of two <see cref="ServiceKey" /> 
            /// objects are equal.
            /// </summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>
            /// <c>True</c> if the <paramref name="left" /> and <paramref name="right" /> have the 
            /// same value; otherwise, false.
            /// </returns>
            public static bool operator ==(ServiceKey left, ServiceKey right)
            {
                return Equals(left, right);
            }

            /// <summary>
            /// Returns a value that indicates whether the values of two <see cref="ServiceKey" /> 
            /// objects are different.
            /// </summary>
            /// <param name="left">The first value to compare.</param>
            /// <param name="right">The second value to compare.</param>
            /// <returns>
            /// <c>True</c> if the <paramref name="left" /> and <paramref name="right" /> do not 
            /// have the same value; otherwise, false.
            /// </returns>
            public static bool operator !=(ServiceKey left, ServiceKey right)
            {
                return !Equals(left, right);
            }
        }

        #endregion // ServiceKey Class Declaration
    }
}
