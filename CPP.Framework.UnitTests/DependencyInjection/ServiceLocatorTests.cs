using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CPP.Framework.DependencyInjection.Resolvers;
using CPP.Framework.Diagnostics.Testing;
using CPP.Framework.Services;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.DependencyInjection
{
    #region Sample Class Definitions
    
    public abstract class ObjectInterfaceClass { }
    public sealed class ProviderClassAnonName : ObjectInterfaceClass { }
    public sealed class ProviderClassSingleton : ObjectInterfaceClass { }
    public sealed class ProviderClassAtConfig : ObjectInterfaceClass { }
    public sealed class ProviderClassWithFact : ObjectInterfaceClass { }
    public sealed class ProviderClassWithName : ObjectInterfaceClass { }

    public interface IAutoRegisterService { }

    public interface IAutoRegisterService2 { }

    [AutoRegisterService]
    //[ServiceRegistration(typeof(AutoRegisterServiceOne))]
    //[ServiceRegistration(typeof(AutoRegisterServiceOne), ServiceLocatorTests.AutoRegisterServiceOneName)]
    [ServiceRegistration(typeof(IAutoRegisterService))]
    [ServiceRegistration(typeof(IAutoRegisterService), ServiceLocatorTests.AutoRegisterServiceOneName)]
    public sealed class AutoRegisterServiceOne : IAutoRegisterService { }

    [AutoRegisterService]
    //[ServiceRegistration(typeof(AutoRegisterServiceTwo))]
    //[ServiceRegistration(typeof(AutoRegisterServiceTwo), ServiceLocatorTests.AutoRegisterServiceTwoName)]
    [ServiceRegistration(typeof(IAutoRegisterService), ServiceLocatorTests.AutoRegisterServiceTwoName)]
    public sealed class AutoRegisterServiceTwo : IAutoRegisterService { }

    [AutoRegisterService]
    //[ServiceRegistration(typeof(AutoRegisterSingleton))]
    //[ServiceRegistration(typeof(AutoRegisterSingleton), ServiceLocatorTests.AutoRegisterServiceSingleton)]
    [ServiceRegistration(typeof(IAutoRegisterService), ServiceLocatorTests.AutoRegisterServiceSingleton)]
    public sealed class AutoRegisterSingleton : CodeServiceSingleton, IAutoRegisterService
    {
        public int ReferenceCount { get; private set; } = 0;

        /// <inheritdoc />
        protected internal override void CleanupInstance()
        {
            this.ReferenceCount--;
            base.CleanupInstance();
        }

        /// <inheritdoc />
        protected internal override void StartupInstance()
        {
            this.ReferenceCount++;
            base.StartupInstance();
        }
    }

    [AutoRegisterService]
    [ServiceRegistration(typeof(AutoRegisterInterface))]
    [ServiceRegistration(typeof(AutoRegisterInterface), ServiceLocatorTests.AutoRegisterServiceInterface)]
    [ServiceRegistration(typeof(IAutoRegisterService), ServiceLocatorTests.AutoRegisterServiceInterface)]
    public sealed class AutoRegisterInterface : ICodeServiceSingleton, IAutoRegisterService
    {
        public int ReferenceCount { get; private set; } = 0;

        [SingletonInstanceCleanupMethod]
        private void CleanupInstance()
        {
            this.ReferenceCount--;
        }

        [SingletonInstanceStartupMethod]
        private void StartupInstance()
        {
            this.ReferenceCount++;
        }
    }

    [ServiceRegistration(typeof(IAutoRegisterService), ServiceLocatorTests.AutoRegisterServiceMultiple)]
    [ServiceRegistration(typeof(IAutoRegisterService2), ServiceLocatorTests.AutoRegisterServiceMultiple)]
    public sealed class AutoRegisterMultiple : ICodeServiceSingleton, IAutoRegisterService, IAutoRegisterService2
    {
    }

    [ExcludeFromCodeCoverage]
    public sealed class SampleService
    {
        public SampleService() { this.Guid = Guid.NewGuid(); }
        public Guid Guid { get; private set; }
    }
    [ExcludeFromCodeCoverage]
    public class CtorInterfaceClassAnon
    {
        public CtorInterfaceClassAnon(SampleService service)
        {
            ArgumentValidator.ValidateNotNull(() => service);
            this.Guid = service.Guid;
        }
        public Guid Guid { get; private set; }
    }
    [ExcludeFromCodeCoverage]
    public sealed class CtorInterfaceClassNamed : CtorInterfaceClassAnon
    {
        public CtorInterfaceClassNamed(SampleService service) : base(service) { }
    }
    [ExcludeFromCodeCoverage]
    public sealed class GenericInternalClass
    {
        [ServiceLocatorConstructor]
        private GenericInternalClass() { }
    }

    [ExcludeFromCodeCoverage]
    public sealed class UnityContainerInjectionClass
    {
        public UnityContainerInjectionClass(IUnityContainer container)
        {
            this.Container = container;
        }

        public IUnityContainer Container { get; }
    }

    #endregion // Sample Class Definitions

    /// <summary>
    /// Unit tests for the <see cref="ServiceLocator"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ServiceLocatorTests
    {
        public const string AutoRegisterServiceOneName = "AutoClass1";
        public const string AutoRegisterServiceMultiple = "Multiple";
        public const string AutoRegisterServiceSingleton = "Singleton";
        public const string AutoRegisterServiceInterface = "Interface";
        public const string AutoRegisterServiceTwoName = "AutoClass2";

        private const string BrokenConfigurationName = "CPP.Framework.DependencyInjection.Broken";
        private const string CustomConfigurationName = "CPP.Framework.DependencyInjection.Custom";
        private const string MissedConfigurationName = "CPP.Framework.DependencyInjection.Unknown";

        [TestCleanup]
        public void AfterTestCase() { ServiceLocator.Unload(); }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericCheckConfigRegistration()
        {
            ServiceLocator.Initialize();
            Verify.IsTrue(ServiceLocator.IsRegistered<ObjectInterfaceClass>());
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericCheckDefaultRegistration()
        {
            Verify.IsFalse(ServiceLocator.IsRegistered<Object>());
            ServiceLocator.Register<Object, String>();
            Verify.IsTrue(ServiceLocator.IsRegistered<Object>());
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericCheckNamedRegistration()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            Verify.IsTrue(ServiceLocator.IsRegistered<ObjectInterfaceClass>());
            Verify.IsFalse(ServiceLocator.IsRegistered<ObjectInterfaceClass>(RegistrationName));
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassWithName>(RegistrationName);
            Verify.IsTrue(ServiceLocator.IsRegistered<ObjectInterfaceClass>(RegistrationName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void GenericCheckRegistrationWithEmptyName()
        {
            ServiceLocator.IsRegistered<ObjectInterfaceClass>("");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("name")]
        public void GenericCheckRegistrationWithNullName()
        {
            ServiceLocator.IsRegistered<ObjectInterfaceClass>(null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetInstanceWithDependencyResolver()
        {
            var expected = new SampleService();
            var resolver = new DependencyResolver(typeof(SampleService), expected);
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>(resolver);
                Verify.IsNotNull(actual);
                Verify.AreEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>();
                Verify.IsNotNull(actual);
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetInstanceWithGenericInternalClass()
        {
            ServiceLocator.GetInstance<GenericInternalClass>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetInstanceWithNameAndDependencyResolver()
        {
            const string RegistrationName = "named";
            var expected = new SampleService();
            var resolver = new DependencyResolver(typeof(SampleService), expected);
            ServiceLocator.Register<CtorInterfaceClassAnon, CtorInterfaceClassNamed>(RegistrationName);
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>(RegistrationName, resolver);
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>(RegistrationName);
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>();
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassAnon));
                Verify.IsNotInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetInstanceWithNameAndParameterResolver()
        {
            const string RegistrationName = "named";
            var expected = new SampleService();
            var resolver = new ParameterResolver("service", expected);
            ServiceLocator.Register<CtorInterfaceClassAnon, CtorInterfaceClassNamed>(RegistrationName);
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>(RegistrationName, resolver);
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>(RegistrationName);
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>();
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassAnon));
                Verify.IsNotInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetInstanceWithParameterResolver()
        {
            var expected = new SampleService();
            var resolver = new ParameterResolver("service", expected);
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>(resolver);
                Verify.IsNotNull(actual);
                Verify.AreEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>();
                Verify.IsNotNull(actual);
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithDefault()
        {
            var expected = typeof(ProviderClassAnonName);
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            var actual = ServiceLocator.GetMappedType<ObjectInterfaceClass>();

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithDefaultAndFactory()
        {
            ServiceLocator.Register<ObjectInterfaceClass>(name => new ProviderClassWithFact());
            Verify.IsNull(ServiceLocator.GetMappedType<ObjectInterfaceClass>());
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithDefaultAndSingleton()
        {
            ServiceLocator.Register<ObjectInterfaceClass>(new ProviderClassSingleton());
            Verify.IsNull(ServiceLocator.GetMappedType<ObjectInterfaceClass>());
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithName()
        {
            const string RegistrationName = "named";
            var expected = typeof(ProviderClassWithName);
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassWithName>(RegistrationName);
            var actual = ServiceLocator.GetMappedType<ObjectInterfaceClass>(RegistrationName);

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithNameAndFactory()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ServiceLocator.Register<ObjectInterfaceClass>(name => new ProviderClassWithFact(), RegistrationName);
            Verify.IsNull(ServiceLocator.GetMappedType<ObjectInterfaceClass>(RegistrationName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithNameAndSingleton()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ServiceLocator.Register<ObjectInterfaceClass>(new ProviderClassSingleton(), RegistrationName);
            Verify.IsNull(ServiceLocator.GetMappedType<ObjectInterfaceClass>(RegistrationName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithUnmappedType()
        {
            Verify.IsNull(ServiceLocator.GetMappedType<Object>());
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterFactoryAndDefault()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ServiceLocator.Register<ObjectInterfaceClass>(name => new ProviderClassWithFact(), RegistrationName);
            ObjectInterfaceClass instance = null;

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>();
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassAnonName));

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>(RegistrationName);
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassWithFact));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterFactoryAsDefault()
        {
            ServiceLocator.Register<ObjectInterfaceClass>(name => new ProviderClassWithFact());
            ObjectInterfaceClass instance = null;

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>();
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassWithFact));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void GenericRegisterFactoryWithEmptyName()
        {
            ServiceLocator.Register<ObjectInterfaceClass>((name => null), "");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("name")]
        public void GenericRegisterFactoryWithNullName()
        {
            ServiceLocator.Register<ObjectInterfaceClass>((name => null), null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("factory")]
        public void GenericRegisterFactoryWithNull()
        {
            ServiceLocator.Register((ServiceFactoryDelegate<ObjectInterfaceClass>)null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("factory")]
        public void GenericRegisterFactoryWithNullAndName()
        {
            ServiceLocator.Register((ServiceFactoryDelegate<ObjectInterfaceClass>)null, null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void GenericRegisterFactoryWithWhiteSpaceName()
        {
            ServiceLocator.Register<ObjectInterfaceClass>((name => null), new string(' ', 4));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterNamedAndDefault()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassWithName>(RegistrationName);
            ObjectInterfaceClass instance = null;

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>();
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassAnonName));

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>(RegistrationName);
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassWithName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterSingletonAsDefault()
        {
            var singleton = new ProviderClassSingleton();
            ServiceLocator.Register<ObjectInterfaceClass>(singleton);
            ObjectInterfaceClass instance = null;

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>();
            Verify.IsNotNull(instance);
            Verify.AreSame(singleton, instance);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterSingletonWithDefault()
        {
            const string RegistrationName = "named";
            var singleton = new ProviderClassSingleton();

            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ServiceLocator.Register<ObjectInterfaceClass>(singleton, RegistrationName);
            ObjectInterfaceClass instance = null;

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>();
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassAnonName));

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>(RegistrationName);
            Verify.IsNotNull(instance);
            Verify.AreSame(singleton, instance);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void GenericRegisterSingletonWithEmptyName()
        {
            var instance = new ProviderClassAnonName();
            ServiceLocator.Register<ObjectInterfaceClass>(instance, "");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("instance")]
        public void GenericRegisterSingletonWithNullInstance()
        {
            ServiceLocator.Register((ObjectInterfaceClass)null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("instance")]
        public void GenericRegisterSingletonWithNullInstanceAndName()
        {
            ServiceLocator.Register((ObjectInterfaceClass)null, null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("name")]
        public void GenericRegisterSingletonWithNullName()
        {
            var instance = new ProviderClassAnonName();
            ServiceLocator.Register<ObjectInterfaceClass>(instance, null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void GenericRegisterSingletonWithWhiteSpaceName()
        {
            var instance = new ProviderClassAnonName();
            ServiceLocator.Register<ObjectInterfaceClass>(instance, new string(' ', 4));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void GenericRegisterWithEmptyName()
        {
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>("");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("name")]
        public void GenericRegisterWithNullName()
        {
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>(null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void GenericRegisterWithWhiteSpaceName()
        {
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>(new string(' ', 4));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstance()
        {
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ObjectInterfaceClass instance = null;
            Verify.IsTrue(ServiceLocator.TryGetInstance(out instance));
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ProviderClassAnonName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstanceWithDependencyResolver()
        {
            var expected = new SampleService();
            var resolver = new DependencyResolver(typeof(SampleService), expected);
            CtorInterfaceClassAnon actual = null;

            Verify.IsTrue(ServiceLocator.TryGetInstance(out actual, resolver));
            Verify.IsNotNull(actual);
            Verify.AreEqual(expected.Guid, actual.Guid);

            Verify.IsTrue(ServiceLocator.TryGetInstance(out actual));
            Verify.IsNotNull(actual);
            Verify.AreNotEqual(expected.Guid, actual.Guid);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstanceWithNameAndDependencyResolver()
        {
            const string RegistrationName = "named";
            var expected = new SampleService();
            var resolver = new DependencyResolver(typeof(SampleService), expected);
            ServiceLocator.Register<CtorInterfaceClassAnon, CtorInterfaceClassNamed>(RegistrationName);
            CtorInterfaceClassAnon actual = null;

            Verify.IsTrue(ServiceLocator.TryGetInstance(RegistrationName, out actual, resolver));
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreEqual(expected.Guid, actual.Guid);

            Verify.IsTrue(ServiceLocator.TryGetInstance(RegistrationName, out actual));
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreNotEqual(expected.Guid, actual.Guid);
            
            Verify.IsTrue(ServiceLocator.TryGetInstance(out actual));
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassAnon));
            Verify.IsNotInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreNotEqual(expected.Guid, actual.Guid);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstanceWithNameAndParameterResolver()
        {
            const string RegistrationName = "named";
            var expected = new SampleService();
            var resolver = new ParameterResolver("service", expected);
            ServiceLocator.Register<CtorInterfaceClassAnon, CtorInterfaceClassNamed>(RegistrationName);
            CtorInterfaceClassAnon actual = null;

            Verify.IsTrue(ServiceLocator.TryGetInstance(RegistrationName, out actual, resolver));
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreEqual(expected.Guid, actual.Guid);

            Verify.IsTrue(ServiceLocator.TryGetInstance(RegistrationName, out actual));
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreNotEqual(expected.Guid, actual.Guid);

            Verify.IsTrue(ServiceLocator.TryGetInstance(out actual));
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassAnon));
            Verify.IsNotInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreNotEqual(expected.Guid, actual.Guid);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstanceWithParameterResolver()
        {
            var expected = new SampleService();
            var resolver = new ParameterResolver("service", expected);
            CtorInterfaceClassAnon actual = null;

            Verify.IsTrue(ServiceLocator.TryGetInstance(out actual, resolver));
            Verify.IsNotNull(actual);
            Verify.AreEqual(expected.Guid, actual.Guid);

            Verify.IsTrue(ServiceLocator.TryGetInstance(out actual));
            Verify.IsNotNull(actual);
            Verify.AreNotEqual(expected.Guid, actual.Guid);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstanceWithInvalidRegistration()
        {
            ObjectInterfaceClass instance = null;
            ServiceLocator.Register<ObjectInterfaceClass, ObjectInterfaceClass>();
            Verify.IsFalse(ServiceLocator.TryGetInstance(out instance));
            Verify.IsNull(instance);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstanceWithName()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassWithName>(RegistrationName);

            ObjectInterfaceClass instance = null;
            Verify.IsTrue(ServiceLocator.TryGetInstance(RegistrationName, out instance));
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ProviderClassWithName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstanceWithNameAndInvalidRegistration()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ServiceLocator.Register<ObjectInterfaceClass, ObjectInterfaceClass>(RegistrationName);

            ObjectInterfaceClass instance = null;
            Verify.IsFalse(ServiceLocator.TryGetInstance(RegistrationName, out instance));
            Verify.IsNull(instance);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GetInstanceWithInjectedContainer()
        {
            var actual = ServiceLocator.GetInstance<UnityContainerInjectionClass>(UnityContainerResolver.Instance);
            Verify.IsNotNull(actual);
            Verify.AreSame(ServiceLocator.Container, actual.Container);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GetInstanceWithSingletonWithoutRegistration()
        {
            var service1 = ServiceLocator.GetInstance<AutoRegisterInterface>();
            Verify.IsNotNull(service1);
            var service2 = ServiceLocator.GetInstance<AutoRegisterInterface>();
            Verify.IsNotNull(service2);
            Verify.AreSame(service1, service2);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void InitializeTwiceWithDifferentConfig()
        {
            Verify.IsTrue(ServiceLocator.Initialize());
            Verify.AreEqual(ServiceLocator.ConfigurationName, ServiceLocator.DefaultConfigurationName);
            Verify.IsTrue(ServiceLocator.Initialize(CustomConfigurationName));
            Verify.AreEqual(ServiceLocator.ConfigurationName, CustomConfigurationName);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void InitializeTwiceWithSameConfig()
        {
            Verify.IsTrue(ServiceLocator.Initialize());
            Verify.AreEqual(ServiceLocator.ConfigurationName, ServiceLocator.DefaultConfigurationName);
            Verify.IsFalse(ServiceLocator.Initialize());
            Verify.AreEqual(ServiceLocator.ConfigurationName, ServiceLocator.DefaultConfigurationName);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedException(typeof(InvalidInjectionConfigException))]
        public void InitializeWithBrokenConfig()
        {
            ServiceLocator.Initialize(BrokenConfigurationName);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void InitializeWithCustomConfig()
        {
            ServiceLocator.Initialize(CustomConfigurationName);
            var instance = ServiceLocator.GetInstance<ObjectInterfaceClass>();

            Verify.AreEqual(ServiceLocator.ConfigurationName, CustomConfigurationName);
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassAnonName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void InitializeWithDefaultConfig()
        {
            ServiceLocator.Initialize();
            var instance = ServiceLocator.GetInstance<ObjectInterfaceClass>();
            Verify.AreEqual(ServiceLocator.ConfigurationName, ServiceLocator.DefaultConfigurationName);
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassAtConfig));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedException(typeof(InjectionConfigNotFoundException))]
        public void IntializeWithMissingConfig()
        {
            ServiceLocator.Initialize(MissedConfigurationName);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void InitializeWithNullConfig()
        {
            ServiceLocator.Initialize(null);
            var instance = ServiceLocator.GetInstance<ObjectInterfaceClass>();
            Verify.AreEqual(ServiceLocator.ConfigurationName, ServiceLocator.DefaultConfigurationName);
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassAtConfig));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericCheckConfigRegistration()
        {
            Verify.IsTrue(ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass)));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericCheckDefaultRegistration()
        {
            Verify.IsFalse(ServiceLocator.IsRegistered(typeof(Object)));
            ServiceLocator.Register(typeof(Object), typeof(String));
            Verify.IsTrue(ServiceLocator.IsRegistered(typeof(Object)));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericCheckNamedRegistration()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            Verify.IsTrue(ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass)));
            Verify.IsFalse(ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass), RegistrationName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassWithName), RegistrationName);
            Verify.IsTrue(ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass), RegistrationName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void NonGenericCheckRegistrationWithEmptyName()
        {
            ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass), "");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("name")]
        public void NonGenericCheckRegistrationWithNullName()
        {
            ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass), null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetInstanceWithDependencyResolver()
        {
            var expected = new SampleService();
            var resolver = new DependencyResolver(typeof(SampleService), expected);
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon), resolver) as CtorInterfaceClassAnon);
                Verify.IsNotNull(actual);
                Verify.AreEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon)) as CtorInterfaceClassAnon);
                Verify.IsNotNull(actual);
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetInstanceWithNameAndDependencyResolver()
        {
            const string RegistrationName = "named";
            var expected = new SampleService();
            var resolver = new DependencyResolver(typeof(SampleService), expected);
            ServiceLocator.Register(typeof(CtorInterfaceClassAnon), typeof(CtorInterfaceClassNamed), RegistrationName);
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon), RegistrationName, resolver) as CtorInterfaceClassAnon);
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon), RegistrationName) as CtorInterfaceClassAnon);
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon)) as CtorInterfaceClassAnon);
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassAnon));
                Verify.IsNotInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetInstanceWithNameAndParameterResolver()
        {
            const string RegistrationName = "named";
            var expected = new SampleService();
            var resolver = new ParameterResolver("service", expected);
            ServiceLocator.Register(typeof(CtorInterfaceClassAnon), typeof(CtorInterfaceClassNamed), RegistrationName);
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon), RegistrationName, resolver) as CtorInterfaceClassAnon);
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon), RegistrationName) as CtorInterfaceClassAnon);
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon)) as CtorInterfaceClassAnon);
                Verify.IsNotNull(actual);
                Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassAnon));
                Verify.IsNotInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetInstanceWithParameterResolver()
        {
            var expected = new SampleService();
            var resolver = new ParameterResolver("service", expected);
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon), resolver) as CtorInterfaceClassAnon);
                Verify.IsNotNull(actual);
                Verify.AreEqual(expected.Guid, actual.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon)) as CtorInterfaceClassAnon);
                Verify.IsNotNull(actual);
                Verify.AreNotEqual(expected.Guid, actual.Guid);
            }
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithDefault()
        {
            var expected = typeof(ProviderClassAnonName);
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            var actual = ServiceLocator.GetMappedType(typeof(ObjectInterfaceClass));

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithDefaultAndFactory()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), ((type, name) => new ProviderClassWithFact()));
            Verify.IsNull(ServiceLocator.GetMappedType(typeof(ObjectInterfaceClass)));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithDefaultAndSingleton()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), new ProviderClassSingleton());
            Verify.IsNull(ServiceLocator.GetMappedType(typeof(ObjectInterfaceClass)));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithName()
        {
            const string RegistrationName = "named";
            var expected = typeof(ProviderClassWithName);
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassWithName), RegistrationName);
            var actual = ServiceLocator.GetMappedType(typeof(ObjectInterfaceClass), RegistrationName);

            Verify.IsNotNull(actual);
            Verify.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithNameAndFactory()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), (type, name) => new ProviderClassWithFact(), RegistrationName);
            Verify.IsNull(ServiceLocator.GetMappedType(typeof(ObjectInterfaceClass), RegistrationName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithNameAndSingleton()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), new ProviderClassSingleton(), RegistrationName);
            Verify.IsNull(ServiceLocator.GetMappedType<ObjectInterfaceClass>(RegistrationName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithUnmappedType()
        {
            Verify.IsNull(ServiceLocator.GetMappedType(typeof(Object)));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterFactoryAndDefault()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), (type, name) => new ProviderClassWithFact(), RegistrationName);
            ObjectInterfaceClass instance = null;

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass)) as ObjectInterfaceClass);
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassAnonName));

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass), RegistrationName) as ObjectInterfaceClass);
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassWithFact));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterFactoryAsDefault()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), (type, name) => new ProviderClassWithFact());
            ObjectInterfaceClass instance = null;

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass)) as ObjectInterfaceClass);
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassWithFact));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void NonGenericRegisterFactoryWithEmptyName()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), ((type, name) => null), "");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("name")]
        public void NonGenericRegisterFactoryWithNullName()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), ((type, name) => null), null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("factory")]
        public void NonGenericRegisterFactoryWithNull()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), (ServiceFactoryDelegate)null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("factory")]
        public void NonGenericRegisterFactoryWithNullAndName()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), (ServiceFactoryDelegate)null, null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void NonGenericRegisterFactoryWithWhiteSpaceName()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), ((type, name) => null), new string(' ', 4));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterNamedAndDefault()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassWithName), RegistrationName);
            ObjectInterfaceClass instance = null;

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass)) as ObjectInterfaceClass);
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassAnonName));

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass), RegistrationName) as ObjectInterfaceClass);
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassWithName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterSingletonAsDefault()
        {
            var singleton = new ProviderClassSingleton();
            ServiceLocator.Register(typeof(ObjectInterfaceClass), singleton);
            ObjectInterfaceClass instance = null;

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass)) as ObjectInterfaceClass);
            Verify.IsNotNull(instance);
            Verify.AreSame(singleton, instance);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterSingletonWithDefault()
        {
            const string RegistrationName = "named";
            var singleton = new ProviderClassSingleton();

            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), singleton, RegistrationName);
            ObjectInterfaceClass instance = null;

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass)) as ObjectInterfaceClass);
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ObjectInterfaceClass));
            Verify.IsInstanceOfType(instance, typeof(ProviderClassAnonName));

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass), RegistrationName) as ObjectInterfaceClass);
            Verify.IsNotNull(instance);
            Verify.AreSame(singleton, instance);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void NonGenericRegisterSingletonWithEmptyName()
        {
            var instance = new ProviderClassAnonName();
            ServiceLocator.Register(typeof(ObjectInterfaceClass), instance, "");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("instance")]
        public void NonGenericRegisterSingletonWithNullInstance()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), (ObjectInterfaceClass)null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("instance")]
        public void NonGenericRegisterSingletonWithNullInstanceAndName()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), (ObjectInterfaceClass)null, null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("name")]
        public void NonGenericRegisterSingletonWithNullName()
        {
            var instance = new ProviderClassAnonName();
            ServiceLocator.Register(typeof(ObjectInterfaceClass), instance, null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void NonGenericRegisterSingletonWithWhiteSpaceName()
        {
            var instance = new ProviderClassAnonName();
            ServiceLocator.Register(typeof(ObjectInterfaceClass), instance, new string(' ', 4));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void NonGenericRegisterWithEmptyName()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName), "");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("providerType")]
        public void NonGenericRegisterWithInvalidProvider()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(Object));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentNullException("name")]
        public void NonGenericRegisterWithNullName()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName), null);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedArgumentException("name")]
        public void NonGenericRegisterWithWhiteSpaceName()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName), new string(' ', 4));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericTryGetInstance()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            object instance = null;
            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(ObjectInterfaceClass), out instance));
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ProviderClassAnonName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericTryGetInstanceWithDependencyResolver()
        {
            var expected = new SampleService();
            var resolver = new DependencyResolver(typeof(SampleService), expected);
            CtorInterfaceClassAnon actual = null;

            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual, resolver));
            Verify.IsNotNull(actual);
            Verify.AreEqual(expected.Guid, actual.Guid);

            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual));
            Verify.IsNotNull(actual);
            Verify.AreNotEqual(expected.Guid, actual.Guid);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericTryGetInstanceWithNameAndDependencyResolver()
        {
            const string RegistrationName = "named";
            var expected = new SampleService();
            var resolver = new DependencyResolver(typeof(SampleService), expected);
            ServiceLocator.Register(typeof(CtorInterfaceClassAnon), typeof(CtorInterfaceClassNamed), RegistrationName);
            CtorInterfaceClassAnon actual = null;

            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), RegistrationName, out actual, resolver));
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreEqual(expected.Guid, actual.Guid);

            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), RegistrationName, out actual));
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreNotEqual(expected.Guid, actual.Guid);

            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual));
            Verify.IsNotNull(actual);
            Verify.IsNotInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreNotEqual(expected.Guid, actual.Guid);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericTryGetInstanceWithNameAndParameterResolver()
        {
            const string RegistrationName = "named";
            var expected = new SampleService();
            var resolver = new ParameterResolver("service", expected);
            ServiceLocator.Register<CtorInterfaceClassAnon, CtorInterfaceClassNamed>(RegistrationName);
            CtorInterfaceClassAnon actual = null;

            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), RegistrationName, out actual, resolver));
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreEqual(expected.Guid, actual.Guid);

            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), RegistrationName, out actual));
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreNotEqual(expected.Guid, actual.Guid);

            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual));
            Verify.IsNotNull(actual);
            Verify.IsNotInstanceOfType(actual, typeof(CtorInterfaceClassNamed));
            Verify.AreNotEqual(expected.Guid, actual.Guid);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericTryGetInstanceWithParameterResolver()
        {
            var expected = new SampleService();
            var resolver = new ParameterResolver("service", expected);
            CtorInterfaceClassAnon actual = null;

            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual, resolver));
            Verify.IsNotNull(actual);
            Verify.AreEqual(expected.Guid, actual.Guid);

            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual));
            Verify.IsNotNull(actual);
            Verify.AreNotEqual(expected.Guid, actual.Guid);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedException(typeof(InvalidCastException))]
        public void NonGenericTryGetInstanceWithInvalidOutputType()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ProviderClassWithName instance = null;
            ServiceLocator.TryGetInstance(typeof(ObjectInterfaceClass), out instance);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericTryGetInstanceWithInvalidRegistration()
        {
            ObjectInterfaceClass instance = null;
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ObjectInterfaceClass));
            Verify.IsFalse(ServiceLocator.TryGetInstance(typeof(ObjectInterfaceClass), out instance));
            Verify.IsNull(instance);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericTryGetInstanceWithName()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassWithName), RegistrationName);

            ObjectInterfaceClass instance = null;
            Verify.IsTrue(ServiceLocator.TryGetInstance(typeof(ObjectInterfaceClass), RegistrationName, out instance));
            Verify.IsNotNull(instance);
            Verify.IsInstanceOfType(instance, typeof(ProviderClassWithName));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        [ExpectedException(typeof(InvalidCastException))]
        public void NonGenericTryGetInstanceWithNameAndInvalidOutputType()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassWithName), RegistrationName);
            ProviderClassAnonName instance = null;
            ServiceLocator.TryGetInstance(typeof(ObjectInterfaceClass), RegistrationName, out instance);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericTryGetInstanceWithNameAndInvalidRegistration()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ObjectInterfaceClass), RegistrationName);

            ObjectInterfaceClass instance = null;
            Verify.IsFalse(ServiceLocator.TryGetInstance(typeof(ObjectInterfaceClass), RegistrationName, out instance));
            Verify.IsNull(instance);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssemblyAndClasses()
        {
            ServiceLocator.RegisterAll(typeof(IAutoRegisterService).Assembly);
            var actual = default(IAutoRegisterService);

            actual = ServiceLocator.GetInstance<IAutoRegisterService>();
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(AutoRegisterServiceOne));
            Verify.IsNotInstanceOfType(actual, typeof(AutoRegisterServiceTwo));

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceOneName);
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(AutoRegisterServiceOne));
            Verify.IsNotInstanceOfType(actual, typeof(AutoRegisterServiceTwo));

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceTwoName);
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(AutoRegisterServiceTwo));
            Verify.IsNotInstanceOfType(actual, typeof(AutoRegisterServiceOne));
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssemblyAndCodeServiceSingletonAndCodeServiceLocatorFirst()
        {
            ServiceLocator.RegisterAll(typeof(AutoRegisterSingleton).Assembly);
            var actual = default(IAutoRegisterService);
            var service = default(AutoRegisterSingleton);

            service = CodeServiceProvider.GetService<AutoRegisterSingleton>(AutoRegisterServiceSingleton);
            Verify.IsNotNull(service);
            Verify.AreEqual(1, service.ReferenceCount);

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceSingleton);
            Verify.IsNotNull(actual);
            Verify.AreSame(service, actual);
            Verify.AreEqual(1, service.ReferenceCount);

            ServiceLocator.Unload();
            Verify.AreEqual(0, service.ReferenceCount);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssemblyAndCodeServiceSingletonAndServiceLocatorFirst()
        {
            var assembly = typeof(AutoRegisterSingleton).Assembly;
            ServiceLocator.RegisterAll((asm) => (asm.FullName == assembly.FullName));
            var actual = default(IAutoRegisterService);
            var service = default(AutoRegisterSingleton);

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceSingleton);
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(AutoRegisterSingleton));
            Verify.AreEqual(1, ((AutoRegisterSingleton)actual).ReferenceCount);

            service = CodeServiceProvider.GetService<AutoRegisterSingleton>(AutoRegisterServiceSingleton);
            Verify.IsNotNull(service);
            Verify.AreEqual(service, actual);
            Verify.AreEqual(1, service.ReferenceCount);

            ServiceLocator.Unload();
            Verify.AreEqual(0, service.ReferenceCount);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssemblyAndICodeServiceSingletonAndCodeServiceLocatorFirst()
        {
            ServiceLocator.RegisterAll(typeof(AutoRegisterInterface).Assembly);
            var actual = default(IAutoRegisterService);
            var service = default(AutoRegisterInterface);

            service = CodeServiceProvider.GetService<AutoRegisterInterface>(AutoRegisterServiceInterface);
            Verify.IsNotNull(service);
            Verify.AreEqual(1, service.ReferenceCount);

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceInterface);
            Verify.IsNotNull(actual);
            Verify.AreEqual(service, actual);
            Verify.AreEqual(1, service.ReferenceCount);

            ServiceLocator.Unload();
            Verify.AreEqual(0, service.ReferenceCount);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssemblyAndICodeServiceSingletonAndServiceLocatorFirst()
        {
            var assembly = typeof(AutoRegisterInterface).Assembly;
            ServiceLocator.RegisterAll((asm) => (asm.FullName == assembly.FullName));
            var actual = default(IAutoRegisterService);
            var service = default(AutoRegisterInterface);

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceInterface);
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(AutoRegisterInterface));
            Verify.AreEqual(1, ((AutoRegisterInterface)actual).ReferenceCount);

            service = CodeServiceProvider.GetService<AutoRegisterInterface>(AutoRegisterServiceInterface);
            Verify.IsNotNull(service);
            Verify.AreEqual(service, actual);
            Verify.AreEqual(1, service.ReferenceCount);

            ServiceLocator.Unload();
            Verify.AreEqual(0, service.ReferenceCount);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssesmblyAndIsRegistered()
        {
            ServiceLocator.RegisterAll(Assembly.GetExecutingAssembly());
            var actual = default(IAutoRegisterService);
            
            actual = ServiceLocator.GetInstance<IAutoRegisterService>();
            Verify.IsNotNull(actual);
            Verify.IsTrue(ServiceLocator.IsRegistered<IAutoRegisterService>());

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceSingleton);
            Verify.IsNotNull(actual);
            Verify.IsTrue(ServiceLocator.IsRegistered<IAutoRegisterService>(AutoRegisterServiceSingleton));
        }

        [ExpectedException(typeof(ResolutionFailedException))]
        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssemblyAndNoClasses()
        {
            ServiceLocator.RegisterAll(typeof(Int32).Assembly);
            var actual = ServiceLocator.GetInstance<IAutoRegisterService>();
            Verify.IsNull(actual);  // we shouldn't get to this point
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssemblyAndSingletonAndMultipleInterfaces()
        {
            ServiceLocator.RegisterAll(typeof(AutoRegisterMultiple).Assembly);
            var expect = ServiceLocator.GetInstance<IAutoRegisterService>(ServiceLocatorTests.AutoRegisterServiceMultiple);
            Verify.IsNotNull(expect);

            var actual = ServiceLocator.GetInstance<IAutoRegisterService2>(ServiceLocatorTests.AutoRegisterServiceMultiple);
            Verify.IsNotNull(actual);
            Verify.AreSame(expect, actual);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithWildcardAndClasses()
        {
            ServiceLocator.RegisterAll("CPP.Framework.*");
            var actual = default(IAutoRegisterService);
            
            actual = ServiceLocator.GetInstance<IAutoRegisterService>();
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(AutoRegisterServiceOne));
            Verify.IsNotInstanceOfType(actual, typeof(AutoRegisterServiceTwo));

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceOneName);
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(AutoRegisterServiceOne));
            Verify.IsNotInstanceOfType(actual, typeof(AutoRegisterServiceTwo));

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceTwoName);
            Verify.IsNotNull(actual);
            Verify.IsInstanceOfType(actual, typeof(AutoRegisterServiceTwo));
            Verify.IsNotInstanceOfType(actual, typeof(AutoRegisterServiceOne));
        }

        [ExpectedException(typeof(ResolutionFailedException))]
        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithWildcardAndNoClasses()
        {
            ServiceLocator.RegisterAll("Microsoft.Practices.*");
            var actual = ServiceLocator.GetInstance<IAutoRegisterService>();
            Verify.IsNull(actual);
        }
    }
}
