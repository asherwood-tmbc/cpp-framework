using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

using CPP.Framework.DependencyInjection.Resolvers;
using CPP.Framework.Threading;

using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace CPP.Framework.DependencyInjection
{
    /// <summary>
    /// Delegate used to generate an instance of an object interface after it has been resolved.
    /// </summary>
    /// <param name="type">The type of the object interface.</param>
    /// <param name="name">The name of the specific object registration being resolved, which can be null for default (unnamed) registrations.</param>
    /// <returns>An instance of the object implementation.</returns>
    public delegate object ServiceFactoryDelegate(Type type, string name);

    /// <summary>
    /// Delegate used to generate an instance of an object interface after it has been resolved.
    /// </summary>
    /// <typeparam name="TService">The type of the object interface.</typeparam>
    /// <param name="name">The name of the specific object registration being resolved, which can be null for default (unnamed) registrations.</param>
    /// <returns>An instance of the object implementation.</returns>
    public delegate TService ServiceFactoryDelegate<out TService>(string name);

    /// <summary>
    /// Singleton class used to register, manage, and resolve application services at runtime.
    /// </summary>
    public sealed class ServiceLocator
    {
        /// <summary>
        /// The default name for the injection configuration section used to load service locator 
        /// registrations from the application's App.Config or Web.Config file.
        /// </summary>
        public const string DefaultConfigurationName = "CPP.Framework.DependencyInjection";

        /// <summary>
        /// A map of hash codes to their corresponding types.
        /// </summary>
        private static readonly Dictionary<int, Type> _RegistrationMap = new Dictionary<int, Type>();

        /// <summary>
        /// The <see cref="MultiAccessLock"/> used to synchronize access to the injection container
        /// across multiple threads.
        /// </summary>
        private static readonly MultiAccessLock _SyncLock = new MultiAccessLock(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// The Unity dependency injection container.
        /// </summary>
        private static IUnityContainer _container;

        /// <summary>
        /// The name of the currently loaded injection configuration from the application's 
        /// configuration file.
        /// </summary>
        private static string _configurationName = DefaultConfigurationName;

        /// <summary>
        /// Occurs when the active service locator is unloaded.
        /// </summary>
        public static event EventHandler Unloaded;

        /// <summary>
        /// Gets the name of the configuration that is currently being used by the underlying 
        /// dependency injection container.
        /// </summary>
        public static string ConfigurationName
        {
            get
            {
                using (_SyncLock.GetReaderAccess())
                {
                    return _configurationName;
                }
            }
        }

        /// <summary>
        /// Gets a reference to the shared dependency injection container used to register, manage, 
        /// and resolve object interfaces.
        /// </summary>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        internal static IUnityContainer Container => ServiceLocator.GetContainer();

        /// <summary>
        /// Calculates the hash code for a give <see cref="ContainerRegistration"/> entry.
        /// </summary>
        /// <param name="registration">The registration entry for which to calculate the hash code.</param>
        /// <returns>The hash code for <paramref name="registration"/>.</returns>
        [ExcludeFromCodeCoverage]
        private static int CalculateHashCode(ContainerRegistration registration)
        {
            return CalculateHashCode(registration.RegisteredType, registration.Name);
        }

        /// <summary>
        /// Calculates the hash code for a type registration.
        /// </summary>
        /// <param name="interfaceType">The registered object interface type.</param>
        /// <param name="name">The name of the provider registration. Pass null for this parameter to search for the default registration.</param>
        /// <returns>The hash code for the registration</returns>
        [ExcludeFromCodeCoverage]
        private static int CalculateHashCode(Type interfaceType, string name)
        {
            var hashcode = interfaceType.GetHashCode();
            if (!string.IsNullOrWhiteSpace(name))
            {
                hashcode ^= (927 * name.GetHashCode());
            }
            return hashcode;
        }

        /// <summary>
        /// Creates a new <see cref="IUnityContainer"/> object.
        /// </summary>
        /// <param name="configuration">The name of the section in the configuration file to use to configure the container, which can be null.</param>
        /// <returns>An <see cref="IUnityContainer"/> instance.</returns>
        /// <exception cref="InvalidInjectionConfigException"><paramref name="configuration"/> could not be loaded because it is missing or invalid.</exception>
        private static IUnityContainer CreateContainer(string configuration)
        {
            IUnityContainer container = null;
            try
            {
                container = new UnityContainer();
                container.AddExtension(new ServiceLocatorExtension());
                if (!string.IsNullOrWhiteSpace(configuration))
                {
                    try
                    {
                        var section = ConfigurationManager.GetSection(configuration);
                        if (section is UnityConfigurationSection unityConfigSection)
                        {
                            container.LoadConfiguration(unityConfigSection);
                        }
                        else if (!string.Equals(configuration, DefaultConfigurationName))
                        {
                            throw new InjectionConfigNotFoundException(configuration);
                        }
                    }
                    catch (Exception ex) when (!(ex is InvalidInjectionConfigException))
                    {
                        throw new InvalidInjectionConfigException(configuration, ex);
                    }
                }
                return container;
            }
            catch
            {
                ServiceLocator.DestroyContainer(container);
                throw;
            }
        }

        /// <summary>
        /// Creates an index for a given <see cref="IUnityContainer"/> object.
        /// </summary>
        /// <param name="container">The container object to index.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that can be used to iterate over the sequence of indexes.</returns>
        [ExcludeFromCodeCoverage]
        private static IEnumerable<KeyValuePair<int, Type>> CreateContainerIndexes(IUnityContainer container)
        {
            if (container?.Registrations != null)
            {
                var sequence = container.Registrations.Where(
                    registration => (registration != null));
                foreach (var registration in sequence)
                {
                    var mappedType = registration.MappedToType;
                    if (mappedType == registration.RegisteredType)
                    {
                        mappedType = null;
                    }
                    var hashcode = CalculateHashCode(registration);
                    yield return new KeyValuePair<int, Type>(hashcode, mappedType);
                }
            }
        }

        /// <summary>
        /// Destroys a dependency injection container.
        /// </summary>
        /// <param name="container">The container to destroy.</param>
        /// <returns>A reference to <paramref name="container"/> after it has been destroyed.</returns>
        public static IUnityContainer DestroyContainer(IUnityContainer container)
        {
            if (container != null)
            {
                // dispose of all of the disposable lifetime manager to ensure that CleanupInstance
                // is called for all of the service instances, since the default container Dispose
                // won't do it.
                var sequence = container.Registrations
                    .Select((reg) => reg.LifetimeManager)
                    .OfType<IDisposable>();
                foreach (var lifetime in sequence) lifetime.Dispose();

                container.Dispose();
                container = null;
            }
            return null;
        }

        /// <summary>
        /// Gets a reference to the current dependency injection container, creating and 
        /// configuring one if needed.
        /// </summary>
        /// <returns>An <see cref="IUnityContainer"/> instance.</returns>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        private static IUnityContainer GetContainer()
        {
            using (_SyncLock.GetReaderAccess())
            {
                if (_container != null) return _container;
            }
            using (_SyncLock.GetWriterAccess())
            {
                if (_container == null)
                {
                    _configurationName = (_configurationName ?? DefaultConfigurationName);
                    _container = ServiceLocator.CreateContainer(_configurationName);
                    _RegistrationMap.Clear();
                    _RegistrationMap.UnionWith(ServiceLocator.CreateContainerIndexes(_container));
                }
                return _container;
            }
        }

        /// <summary>
        /// Gets an instance of a class or service interface.
        /// </summary>
        /// <typeparam name="T">The type to retrieve (which can be a base class or interface).</typeparam>
        /// <param name="resolvers">A list of one or more optional <see cref="ServiceResolver"/> instances to use when resolving the service interface.</param>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static T GetInstance<T>(params ServiceResolver[] resolvers)
        {
            var overrides = resolvers.Select(r => r.CreateOverride()).ToArray();
            return ServiceLocator.Container.Resolve<T>(overrides);
        }

        /// <summary>
        /// Gets an instance of a class or service interface.
        /// </summary>
        /// <typeparam name="T">The type to retrieve (which can be a base class or interface).</typeparam>
        /// <param name="name">The name of the registration.</param>
        /// <param name="resolvers">A list of one or more optional <see cref="ServiceResolver"/> instances to use when resolving the service interface.</param>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or all whitespace characters.</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static T GetInstance<T>(string name, params ServiceResolver[] resolvers)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            var overrides = resolvers.Select(r => r.CreateOverride()).ToArray();
            return ServiceLocator.Container.Resolve<T>(name, overrides);
        }

        /// <summary>
        /// Resolves a service interface into its implementation.
        /// </summary>
        /// <param name="type">The type to retrieve (which can be a base class or interface).</param>
        /// <param name="resolvers">A list of one or more optional <see cref="ServiceResolver"/> instances to use when resolving the service interface.</param>
        /// <returns>An instance of <paramref name="type"/>.</returns>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static object GetInstance(Type type, params ServiceResolver[] resolvers)
        {
            var overrides = resolvers.Select(r => r.CreateOverride()).ToArray();
            return ServiceLocator.Container.Resolve(type, overrides);
        }

        /// <summary>
        /// Gets an instance of a class or service interface.
        /// </summary>
        /// <param name="type">The type to retrieve (which can be a base class or interface).</param>
        /// <param name="name">The name of the registration.</param>
        /// <param name="resolvers">A list of one or more optional <see cref="ServiceResolver"/> instances to use when resolving the service interface.</param>
        /// <returns>An instance of <paramref name="type"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or all whitespace characters.</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static object GetInstance(Type type, string name, params ServiceResolver[] resolvers)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            var overrides = resolvers.Select(r => r.CreateOverride()).ToArray();
            return ServiceLocator.Container.Resolve(type, name, overrides);
        }

        /// <summary>
        /// Retrieves the type that's mapped to an interface type.
        /// </summary>
        /// <typeparam name="T">The type to look up (which can be a base class or interface).</typeparam>
        /// <returns>The type that is mapped to <typeparamref name="T"/>, or null if <typeparamref name="T"/> is not mapped to a type.</returns>
        public static Type GetMappedType<T>() { return ServiceLocator.GetMappedType(typeof(T), null); }

        /// <summary>
        /// Retrieves the type that's mapped to an interface type.
        /// </summary>
        /// <typeparam name="T">The type to look up (which can be a base class or interface).</typeparam>
        /// <param name="name">The name of the registration.</param>
        /// <returns>The type that is mapped to <typeparamref name="T"/>, or null if <typeparamref name="T"/> is not mapped to a type.</returns>
        public static Type GetMappedType<T>(string name) { return ServiceLocator.GetMappedType(typeof(T), name); }

        /// <summary>
        /// Retrieves the type that's mapped to an interface type.
        /// </summary>
        /// <param name="type">The type to look up (which can be a base class or interface).</param>
        /// <returns>The type that is mapped to <paramref name="type"/>, or null if <paramref name="type"/> is not mapped to a type.</returns>
        public static Type GetMappedType(Type type) { return ServiceLocator.GetMappedType(type, null); }

        /// <summary>
        /// Retrieves the type that's mapped to an interface type.
        /// </summary>
        /// <param name="type">The type to look up (which can be a base class or interface).</param>
        /// <param name="name">The name of the registration, or a null/empty string for the default registration.</param>
        /// <returns>The type that is mapped to <paramref name="type"/>, or null if <paramref name="type"/> is not mapped to a type.</returns>
        public static Type GetMappedType(Type type, string name)
        {
            var hashcode = CalculateHashCode(type, name);
            using (_SyncLock.GetReaderAccess())
            {
                if (_RegistrationMap.ContainsKey(hashcode)) return _RegistrationMap[hashcode];
            }
            using (_SyncLock.GetWriterAccess())
            {
                if (!_RegistrationMap.ContainsKey(hashcode))
                {
                    var query = ServiceLocator.Container.Registrations
                        .Where(reg => (reg.RegisteredType == type));
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        query = query.Where(reg => (string.Equals(reg.Name, name, StringComparison.OrdinalIgnoreCase)));
                    }

                    var registration = query.FirstOrDefault();
                    if (registration != null)
                    {
                        var mappedType = registration.MappedToType;
                        if (mappedType == registration.RegisteredType)
                        {
                            mappedType = null;
                        }
                        _RegistrationMap[hashcode] = mappedType;
                    }
                    else return null;
                }
                return _RegistrationMap[hashcode];
            }
        }

        /// <summary>
        /// Initializes the underlying dependency injection container.
        /// </summary>
        /// <returns>True if the container was initialized; otherwise, false if it was already initialized.</returns>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static bool Initialize()
        {
            return ServiceLocator.Initialize(DefaultConfigurationName);
        }

        /// <summary>
        /// Initializes the underlying dependency injection container.
        /// </summary>
        /// <param name="configuration">The name of the section to use from the application settings when configuring the underlying dependency injection container.</param>
        /// <returns>True if the container was initialized; otherwise, false if it was already initialized.</returns>
        /// <exception cref="InvalidInjectionConfigException"><paramref name="configuration"/> could not be loaded because it is missing or invalid.</exception>
        public static bool Initialize(string configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration))
            {
                configuration = DefaultConfigurationName;
            }

            using (_SyncLock.GetReaderAccess())
            {
                if ((_container != null) && (string.Equals(_configurationName, configuration, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }
            using (_SyncLock.GetWriterAccess())
            {
                if (_container != null)
                {
                    _container = ServiceLocator.DestroyContainer(_container);
                    _RegistrationMap.Clear();
                    ServiceLocator.OnUnloaded();
                }
                _container = ServiceLocator.CreateContainer(configuration);
                _RegistrationMap.UnionWith(ServiceLocator.CreateContainerIndexes(_container));
                _configurationName = configuration;
                return true;
            }
        }

        /// <summary>
        /// Determines whether or not a default registration exists for a service interface.
        /// </summary>
        /// <typeparam name="TInterface">The object interface type to register (which can be a base class or interface).</typeparam>
        /// <returns>True if the service interface is registered; otherwise, false.</returns>
        public static bool IsRegistered<TInterface>()
        {
            return ServiceLocator.RegistrationExists(typeof(TInterface), null);
        }

        /// <summary>
        /// Determines whether or not a default registration exists for a service interface.
        /// </summary>
        /// <param name="interfaceType">The object interface type to find (which can be a base class or interface).</param>
        /// <returns>True if the service interface is registered; otherwise, false.</returns>
        public static bool IsRegistered(Type interfaceType)
        {
            return ServiceLocator.RegistrationExists(interfaceType, null);
        }

        /// <summary>
        /// Determines whether or not a registration exists for a service interface with a given
        /// registration name.
        /// </summary>
        /// <typeparam name="TInterface">The object interface type to register (which can be a base class or interface).</typeparam>
        /// <param name="name">The name of the provider registration.</param>
        /// <returns>True if the service interface is registered; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or all whitespace characters.</exception>
        public static bool IsRegistered<TInterface>(string name)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            return ServiceLocator.RegistrationExists(typeof(TInterface), name);
        }

        /// <summary>
        /// Determines whether or not a registration exists for a service interface with a given
        /// registration name.
        /// </summary>
        /// <param name="interfaceType">The object interface type to find (which can be a base class or interface).</param>
        /// <param name="name">The name of the provider registration.</param>
        /// <returns>True if the service interface is registered; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or all whitespace characters.</exception>
        public static bool IsRegistered(Type interfaceType, string name)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            return ServiceLocator.RegistrationExists(interfaceType, name);
        }

        /// <summary>
        /// Raises the <see cref="Unloaded"/> event.
        /// </summary>
        private static void OnUnloaded()
        {
            var handler = ServiceLocator.Unloaded;
            handler?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// Registers a default provider type for a given object interface.
        /// </summary>
        /// <typeparam name="TInterface">The object interface type to register (which can be a base class or interface).</typeparam>
        /// <typeparam name="TProvider">The type that provides the implementation for <typeparamref name="TInterface"/>.</typeparam>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register<TInterface, TProvider>()
            where TProvider : class, TInterface
        {
            ServiceLocator.Container.RegisterType<TInterface, TProvider>();
            ServiceLocator.UpdateRegistrationCache(typeof(TInterface), typeof(TProvider), null);
        }

        /// <summary>
        /// Registers a named provider type for a given object interface.
        /// </summary>
        /// <typeparam name="TInterface">The object interface type to register (which can be a base class or interface).</typeparam>
        /// <typeparam name="TProvider">The type that provides the implementation for <typeparamref name="TInterface"/>.</typeparam>
        /// <param name="name">The name of the provider registration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or all whitespace characters.</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register<TInterface, TProvider>(string name)
            where TProvider : class, TInterface
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            ServiceLocator.Container.RegisterType<TInterface, TProvider>(name);
            ServiceLocator.UpdateRegistrationCache(typeof(TInterface), typeof(TProvider), name);
        }

        /// <summary>
        /// Registers a named singleton provider instance for a given object interface.
        /// </summary>
        /// <typeparam name="TInterface">The object interface type to register (which can be a base class or interface).</typeparam>
        /// <param name="instance">The singleton instance to return when resolving the object interface.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register<TInterface>(TInterface instance) where TInterface : class
        {
            ArgumentValidator.ValidateNotNull(() => instance);
            ServiceLocator.Container.RegisterInstance(instance);
            ServiceLocator.UpdateRegistrationCache(typeof(TInterface), null, null);
        }

        /// <summary>
        /// Registers a named singleton provider instance for a given object interface.
        /// </summary>
        /// <typeparam name="TInterface">The object interface type to register (which can be a base class or interface).</typeparam>
        /// <param name="instance">The singleton instance to return when resolving the object interface.</param>
        /// <param name="name">The name of the provider registration.</param>
        /// <exception cref="ArgumentNullException">
        ///     <para><paramref name="instance"/> is a null reference (Nothing in Visual Basic).</para>
        ///     <para>-or-</para>
        ///     <para><paramref name="name"/> is a null reference (Nothing in Visual Basic).</para>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or all whitespace characters.</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register<TInterface>(TInterface instance, string name) where TInterface : class
        {
            ArgumentValidator.ValidateNotNull(() => instance);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            ServiceLocator.Container.RegisterInstance(name, instance);
            ServiceLocator.UpdateRegistrationCache(typeof(TInterface), null, name);
        }

        /// <summary>
        /// Registers an object factory for a given object interface.
        /// </summary>
        /// <typeparam name="TInterface">The object interface type to register (which can be a base class or interface).</typeparam>
        /// <param name="factory">A <see cref="ServiceFactoryDelegate{T}"/> delegate that returns instances of the object interface.</param>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register<TInterface>(ServiceFactoryDelegate<TInterface> factory)
        {
            ArgumentValidator.ValidateNotNull(() => factory);
            var injector = new InjectionFactory((uc, type, id) => factory(id));
            ServiceLocator.Container.RegisterType<TInterface, TInterface>(injector);
            ServiceLocator.UpdateRegistrationCache(typeof(TInterface), null, null);
        }

        /// <summary>
        /// Registers a named object factory for a given object interface.
        /// </summary>
        /// <typeparam name="TInterface">The object interface type to register (which can be a base class or interface).</typeparam>
        /// <param name="factory">A <see cref="ServiceFactoryDelegate{T}"/> delegate that returns instances of the object interface.</param>
        /// <param name="name">The name of the interface registration.</param>
        /// <exception cref="ArgumentNullException">
        ///     <para><paramref name="factory"/> is a null reference (Nothing in Visual Basic).</para>
        ///     <para>-or-</para>
        ///     <para><paramref name="name"/> is a null reference (Nothing in Visual Basic).</para>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or all whitespace characters.</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register<TInterface>(ServiceFactoryDelegate<TInterface> factory, string name)
        {
            ArgumentValidator.ValidateNotNull(() => factory);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            var injector = new InjectionFactory((uc, type, id) => factory(id));
            ServiceLocator.Container.RegisterType<TInterface, TInterface>(name, injector);
            ServiceLocator.UpdateRegistrationCache(typeof(TInterface), null, name);
        }

        /// <summary>
        /// Registers a named singleton provider instance for a given object interface.
        /// </summary>
        /// <param name="interfaceType">The object interface type to register (which can be a base class or interface).</param>
        /// <param name="instance">The singleton instance to return when resolving the object interface.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException"><paramref name="instance"/> is not assignable to variables of <paramref name="interfaceType"/>.</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register(Type interfaceType, object instance)
        {
            ArgumentValidator.ValidateNotNull(() => instance);
            ServiceLocator.ValidateProviderType(interfaceType, instance.GetType());
            ServiceLocator.Container.RegisterInstance(interfaceType, instance);
            ServiceLocator.UpdateRegistrationCache(interfaceType, null, null);
        }

        /// <summary>
        /// Registers a named singleton provider instance for a given object interface.
        /// </summary>
        /// <param name="interfaceType">The object interface type to register (which can be a base class or interface).</param>
        /// <param name="instance">The singleton instance to return when resolving the object interface.</param>
        /// <param name="name">The name of the provider registration.</param>
        /// <exception cref="ArgumentNullException">
        ///     <para><paramref name="instance"/> is a null reference (Nothing in Visual Basic).</para>
        ///     <para>-or-</para>
        ///     <para><paramref name="name"/> is a null reference (Nothing in Visual Basic).</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <para><paramref name="instance"/> is not assignable to variables of <paramref name="interfaceType"/>.</para>
        ///     <para>-or-</para>
        ///     <para><paramref name="name"/> is empty or all whitespace characters.</para>
        /// </exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register(Type interfaceType, object instance, string name)
        {
            ArgumentValidator.ValidateNotNull(() => instance);
            ServiceLocator.ValidateProviderType(interfaceType, instance.GetType());
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            ServiceLocator.Container.RegisterInstance(interfaceType, name, instance);
            ServiceLocator.UpdateRegistrationCache(interfaceType, null, name);
        }

        /// <summary>
        /// Registers an object factory for a given object interface.
        /// </summary>
        /// <param name="interfaceType">The object interface type to register (which can be a base class or interface).</param>
        /// <param name="factory">A <see cref="ServiceFactoryDelegate"/> delegate that returns instances of the object interface.</param>
        /// <exception cref="ArgumentNullException"><paramref name="factory"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register(Type interfaceType, ServiceFactoryDelegate factory)
        {
            ArgumentValidator.ValidateNotNull(() => factory);
            var injector = new InjectionFactory((uc, type, id) => factory(type, id));
            ServiceLocator.Container.RegisterType(interfaceType, interfaceType, injector);
            ServiceLocator.UpdateRegistrationCache(interfaceType, null, null);
        }

        /// <summary>
        /// Registers a named object factory for a given object interface.
        /// </summary>
        /// <param name="interfaceType">The object interface type to register (which can be a base class or interface).</param>
        /// <param name="factory">A <see cref="ServiceFactoryDelegate"/> delegate that returns instances of the object interface.</param>
        /// <param name="name">The name of the interface registration.</param>
        /// <exception cref="ArgumentNullException">
        ///     <para><paramref name="factory"/> is a null reference (Nothing in Visual Basic).</para>
        ///     <para>-or-</para>
        ///     <para><paramref name="name"/> is a null reference (Nothing in Visual Basic).</para>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or all whitespace characters.</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register(Type interfaceType, ServiceFactoryDelegate factory, string name)
        {
            ArgumentValidator.ValidateNotNull(() => factory);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            var injector = new InjectionFactory((uc, type, id) => factory(type, id));
            ServiceLocator.Container.RegisterType(interfaceType, interfaceType, name, injector);
            ServiceLocator.UpdateRegistrationCache(interfaceType, null, name);
        }

        /// <summary>
        /// Registers a default provider type for a given object interface.
        /// </summary>
        /// <param name="interfaceType">The object interface type to register (which can be a base class or interface).</param>
        /// <param name="providerType">The type that provides the implementation for <paramref name="interfaceType"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="providerType"/> is not assignable to variables of <paramref name="interfaceType"/>.</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register(Type interfaceType, Type providerType)
        {
            ServiceLocator.ValidateProviderType(interfaceType, providerType);
            ServiceLocator.Container.RegisterType(interfaceType, providerType);
            ServiceLocator.UpdateRegistrationCache(interfaceType, providerType, null);
        }

        /// <summary>
        /// Registers a named provider type for a given object interface.
        /// </summary>
        /// <param name="interfaceType">The object interface type to register (which can be a base class or interface).</param>
        /// <param name="providerType">The type that provides the implementation for <paramref name="interfaceType"/>.</param>
        /// <param name="name">The name of the provider registration.</param>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException">
        ///     <para><paramref name="providerType"/> is not assignable to variables of <paramref name="interfaceType"/>.</para>
        ///     <para>-or-</para>
        ///     <para><paramref name="name"/> is empty or all whitespace characters.</para>
        /// </exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static void Register(Type interfaceType, Type providerType, string name)
        {
            ServiceLocator.ValidateProviderType(interfaceType, providerType);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            ServiceLocator.Container.RegisterType(interfaceType, providerType, name);
            ServiceLocator.UpdateRegistrationCache(interfaceType, providerType, name);
        }

        /// <summary>
        /// Automatically registers all of the classes loaded into the current application domain
        /// that have been decorated with at least one <see cref="ServiceRegistrationAttribute"/>.
        /// </summary>
        /// <param name="assemblyNames">
        /// An optional list of assembly names that limits the list of assemblies that are searched
        /// for classes to register. If no assembly names are passed, then all of the assemblies in
        /// the current application domain will be searched. Please note that this method supports
        /// using wildcard characters (i.e. '*' or '?') in place of the exact assembly name.
        /// </param>
        public static void RegisterAll(params string[] assemblyNames)
        {
            var patterns = assemblyNames
                .Select((an) => (an == null) ? an : Regex.Escape(an))
                .Select((an) => an?.Replace(@"\*", @".*"))
                .Select((an) => an?.Replace(@"\?", @"."))
                .Select((an) => $"^{an}$")
                .Select((an) => new Regex(an, RegexOptions.Compiled))
                .ToList();
            bool IsMatchingAssembly(Assembly asm)
            {
                var name = asm.GetName().Name;
                var isMatch = patterns.Any((p) => p.IsMatch(name));
                return isMatch;
            }
            ServiceLocator.RegisterAll(AppDomain.CurrentDomain.GetAssemblies(), IsMatchingAssembly);
        }

        /// <summary>
        /// Automatically registers all of the classes loaded into the current application domain
        /// that have been decorated with at least one <see cref="ServiceRegistrationAttribute"/>.
        /// </summary>
        /// <param name="assemblyFilter">
        /// An optional delegate that is called for each assembly to determine whether or not to
        /// the classes defined in the assembly should be included in the search.
        /// </param>
        public static void RegisterAll(Func<Assembly, bool> assemblyFilter = null) => ServiceLocator.RegisterAll(AppDomain.CurrentDomain.GetAssemblies(), assemblyFilter);

        /// <summary>
        /// Automatically registers all of the classes from one or more assemblies that have been
        /// decorated with at least one <see cref="ServiceRegistrationAttribute"/>.
        /// </summary>
        /// <param name="assemblies">
        /// An <see cref="IEnumerable{T}"/> that contains the list of assemblies to search.
        /// </param>
        /// <param name="assemblyFilter">
        /// An optional delegate that is called for each assembly to determine whether or not to
        /// the classes defined in the assembly should be included in the search.
        /// </param>
        public static void RegisterAll(IEnumerable<Assembly> assemblies, Func<Assembly, bool> assemblyFilter = null)
        {
            assemblyFilter = (assemblyFilter ?? ((asm) => true));
            var typeList = (assemblies ?? Enumerable.Empty<Assembly>())
                .Where((asm) => (asm.GetName().Name != "DynamicProxyGenAssembly2"))
                .Where(assemblyFilter)
                .SelectMany((asm) => asm.GetTypes())
                .Where((type) => (type.IsClass))
                .Where((type) => (!type.IsAbstract) && (!type.ContainsGenericParameters));
            foreach (var serviceType in typeList)
            {
                foreach (var registration in serviceType.GetCustomAttributes<ServiceRegistrationAttribute>(false))
                {
                    var interfaceType = (registration.InterfaceType ?? serviceType);
                    ServiceLocator.Container.RegisterType(
                        interfaceType,
                        serviceType,
                        registration.Name);
                    ServiceLocator.UpdateRegistrationCache(interfaceType, serviceType, registration.Name);
                }
            }
        }

        /// <summary>
        /// Automatically registers all of the classes from an assembly that have been decorated
        /// with a <see cref="ServiceRegistrationAttribute"/>.
        /// </summary>
        /// <param name="assembly">The assembly that contains the classes to search.</param>
        /// <param name="additional">An optional list of additional assemblies to search.</param>
        [ExcludeFromCodeCoverage]
        public static void RegisterAll(Assembly assembly, params Assembly[] additional)
        {
            var sequence = (new[] { assembly })
                .Concat(additional)
                .Where((asm) => (asm != null));
            ServiceLocator.RegisterAll(sequence);
        }

        /// <summary>
        /// Determines whether or not a registration exists for a service interface.
        /// </summary>
        /// <param name="interfaceType">The object interface type to find (which can be a base class or interface).</param>
        /// <param name="name">The name of the provider registration. Pass null for this parameter to search for the default registration.</param>
        /// <returns>True if the service interface is registered; otherwise, false.</returns>
        private static bool RegistrationExists(Type interfaceType, string name)
        {
            var hashcode = CalculateHashCode(interfaceType, name);
            using (_SyncLock.GetReaderAccess())
            {
                if (_RegistrationMap.ContainsKey(hashcode)) return true;
            }
            using (_SyncLock.GetWriterAccess())
            {
                if (!_RegistrationMap.ContainsKey(hashcode))
                {
                    ServiceLocator.Initialize(ServiceLocator.ConfigurationName);
                }
                return _RegistrationMap.ContainsKey(hashcode);
            }
        }

        /// <summary>
        /// Gets an instance of a class or service interface.
        /// </summary>
        /// <typeparam name="T">The type to retrieve (which can be a base class or interface).</typeparam>
        /// <param name="instance">An output parameter that receives a reference to the resolved instance on success.</param>
        /// <param name="resolvers">A list of one or more optional <see cref="ServiceResolver"/> instances to use when resolving the service interface.</param>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static bool TryGetInstance<T>(out T instance, params ServiceResolver[] resolvers)
        {
            try
            {
                instance = ServiceLocator.GetInstance<T>(resolvers);
                return true;
            }
            catch (ResolutionFailedException)
            {
                instance = default(T);
            }
            return false;
        }

        /// <summary>
        /// Gets an instance of a class or service interface.
        /// </summary>
        /// <typeparam name="T">The type to retrieve (which can be a base class or interface).</typeparam>
        /// <param name="name">The name of the registration.</param>
        /// <param name="instance">An output parameter that receives a reference to the resolved instance on success.</param>
        /// <param name="resolvers">A list of one or more optional <see cref="ServiceResolver"/> instances to use when resolving the service interface.</param>
        /// <returns>An instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or all whitespace characters.</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static bool TryGetInstance<T>(string name, out T instance, params ServiceResolver[] resolvers)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => name);
            try
            {
                instance = ServiceLocator.GetInstance<T>(name, resolvers);
                return true;
            }
            catch (ResolutionFailedException)
            {
                instance = default(T);
            }
            return false;
        }

        /// <summary>
        /// Resolves a service interface into its implementation.
        /// </summary>
        /// <typeparam name="T">The expected output type (which can be a base class or interface).</typeparam>
        /// <param name="type">The type to retrieve (which can be a base class or interface).</param>
        /// <param name="instance">An output parameter that receives a reference to the resolved instance on success.</param>
        /// <param name="resolvers">A list of one or more optional <see cref="ServiceResolver"/> instances to use when resolving the service interface.</param>
        /// <returns>An instance of <paramref name="type"/>.</returns>
        /// <exception cref="InvalidCastException">The type of the <paramref name="instance"/> parameter does not derive from <paramref name="type"/>.</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static bool TryGetInstance<T>(Type type, out T instance, params ServiceResolver[] resolvers)
        {
            try
            {
                instance = ((T)ServiceLocator.GetInstance(type, resolvers));
                return true;
            }
            catch (ResolutionFailedException)
            {
                instance = default(T);
            }
            return false;
        }

        /// <summary>
        /// Gets an instance of a class or service interface.
        /// </summary>
        /// <typeparam name="T">The expected output type (which can be a base class or interface).</typeparam>
        /// <param name="type">The type to retrieve (which can be a base class or interface).</param>
        /// <param name="name">The name of the registration.</param>
        /// <param name="instance">An output parameter that receives a reference to the resolved instance on success.</param>
        /// <param name="resolvers">A list of one or more optional <see cref="ServiceResolver"/> instances to use when resolving the service interface.</param>
        /// <returns>An instance of <paramref name="type"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is a null reference (Nothing in Visual Basic).</exception>
        /// <exception cref="ArgumentException"><paramref name="name"/> is empty or all whitespace characters.</exception>
        /// <exception cref="InvalidCastException">The type of the <paramref name="instance"/> parameter does not derive from <paramref name="type"/>.</exception>
        /// <exception cref="InvalidInjectionConfigException"><see cref="ConfigurationName"/> could not be loaded because it is missing or invalid.</exception>
        public static bool TryGetInstance<T>(Type type, string name, out T instance, params ServiceResolver[] resolvers)
        {
            try
            {
                instance = ((T)ServiceLocator.GetInstance(type, name, resolvers));
                return true;
            }
            catch (ResolutionFailedException)
            {
                instance = default(T);
            }
            return false;
        }

        /// <summary>
        /// Clears all of the object interface registrations, and unloads the underlying dependency
        /// injection container.
        /// </summary>
        public static void Unload()
        {
            using (_SyncLock.GetWriterAccess())
            {
                _container = ServiceLocator.DestroyContainer(_container);
                _RegistrationMap.Clear();
                ServiceLocator.OnUnloaded();
            }
        }

        /// <summary>
        /// Updates the cache for a mapped interface type.
        /// </summary>
        /// <param name="interfaceType">The interface type to update.</param>
        /// <param name="mappedType">The type that is mapped to <paramref name="interfaceType"/>.</param>
        /// <param name="name">The name of the registration, or null for the default registration.</param>
        private static void UpdateRegistrationCache(Type interfaceType, Type mappedType, string name)
        {
            using (_SyncLock.GetWriterAccess())
            {
                var hashcode = CalculateHashCode(interfaceType, name);
                _RegistrationMap[hashcode] = mappedType;
            }
        }

        /// <summary>
        /// Verifies that a given provider type implements a specific interface type.
        /// </summary>
        /// <param name="interfaceType">The object interface type.</param>
        /// <param name="providerType">The candidate provider type to verify.</param>
        /// <exception cref="ArgumentException"><paramref name="providerType"/> is not assignable to variables of <paramref name="interfaceType"/>.</exception>
        private static void ValidateProviderType(Type interfaceType, Type providerType)
        {
            if (!interfaceType.IsAssignableFrom(providerType))
            {
                var message = string.Format(
                    ErrorStrings.InvalidProviderInterfaceType,
                    interfaceType.FullName,
                    providerType.FullName);
                throw new ArgumentException(message, nameof(providerType));
            }
        }
    }
}
