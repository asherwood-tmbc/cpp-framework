using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CPP.Framework.DependencyInjection.Resolvers;
using CPP.Framework.UnitTests.Testing;
using CPP.Framework.Services;
using FluentAssertions;
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
            ServiceLocator.IsRegistered<ObjectInterfaceClass>().Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericCheckDefaultRegistration()
        {
            ServiceLocator.IsRegistered<Object>().Should().BeFalse();
            ServiceLocator.Register<Object, String>();
            ServiceLocator.IsRegistered<Object>().Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericCheckNamedRegistration()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ServiceLocator.IsRegistered<ObjectInterfaceClass>().Should().BeTrue();
            ServiceLocator.IsRegistered<ObjectInterfaceClass>(RegistrationName).Should().BeFalse();
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassWithName>(RegistrationName);
            ServiceLocator.IsRegistered<ObjectInterfaceClass>(RegistrationName).Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericCheckRegistrationWithEmptyName()
        {
            Action act = () => { ServiceLocator.IsRegistered<ObjectInterfaceClass>(""); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericCheckRegistrationWithNullName()
        {
            Action act = () => { ServiceLocator.IsRegistered<ObjectInterfaceClass>(null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("name");
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
                actual.Should().NotBeNull();
                actual.Guid.Should().Be(expected.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>();
                actual.Should().NotBeNull();
                actual.Guid.Should().NotBe(expected.Guid);
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
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().Be(expected.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>(RegistrationName);
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().NotBe(expected.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>();
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassAnon>();
                actual.Should().NotBeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().NotBe(expected.Guid);
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
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().Be(expected.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>(RegistrationName);
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().NotBe(expected.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>();
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassAnon>();
                actual.Should().NotBeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().NotBe(expected.Guid);
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
                actual.Should().NotBeNull();
                actual.Guid.Should().Be(expected.Guid);
            }
            {
                var actual = ServiceLocator.GetInstance<CtorInterfaceClassAnon>();
                actual.Should().NotBeNull();
                actual.Guid.Should().NotBe(expected.Guid);
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

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithDefaultAndFactory()
        {
            ServiceLocator.Register<ObjectInterfaceClass>(name => new ProviderClassWithFact());
            ServiceLocator.GetMappedType<ObjectInterfaceClass>().Should().BeNull();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithDefaultAndSingleton()
        {
            ServiceLocator.Register<ObjectInterfaceClass>(new ProviderClassSingleton());
            ServiceLocator.GetMappedType<ObjectInterfaceClass>().Should().BeNull();
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

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithNameAndFactory()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ServiceLocator.Register<ObjectInterfaceClass>(name => new ProviderClassWithFact(), RegistrationName);
            ServiceLocator.GetMappedType<ObjectInterfaceClass>(RegistrationName).Should().BeNull();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithNameAndSingleton()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ServiceLocator.Register<ObjectInterfaceClass>(new ProviderClassSingleton(), RegistrationName);
            ServiceLocator.GetMappedType<ObjectInterfaceClass>(RegistrationName).Should().BeNull();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericGetMappedTypeWithUnmappedType()
        {
            ServiceLocator.GetMappedType<Object>().Should().BeNull();
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
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassAnonName>();

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>(RegistrationName);
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassWithFact>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterFactoryAsDefault()
        {
            ServiceLocator.Register<ObjectInterfaceClass>(name => new ProviderClassWithFact());
            ObjectInterfaceClass instance = null;

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>();
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassWithFact>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterFactoryWithEmptyName()
        {
            Action act = () => { ServiceLocator.Register<ObjectInterfaceClass>((name => null), ""); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterFactoryWithNullName()
        {
            Action act = () => { ServiceLocator.Register<ObjectInterfaceClass>((name => null), null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterFactoryWithNull()
        {
            Action act = () => { ServiceLocator.Register((ServiceFactoryDelegate<ObjectInterfaceClass>)null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("factory");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterFactoryWithNullAndName()
        {
            Action act = () => { ServiceLocator.Register((ServiceFactoryDelegate<ObjectInterfaceClass>)null, null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("factory");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterFactoryWithWhiteSpaceName()
        {
            Action act = () => { ServiceLocator.Register<ObjectInterfaceClass>((name => null), new string(' ', 4)); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
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
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassAnonName>();

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>(RegistrationName);
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassWithName>();
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
            instance.Should().NotBeNull();
            instance.Should().BeSameAs(singleton);
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
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassAnonName>();

            instance = ServiceLocator.GetInstance<ObjectInterfaceClass>(RegistrationName);
            instance.Should().NotBeNull();
            instance.Should().BeSameAs(singleton);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterSingletonWithEmptyName()
        {
            var instance = new ProviderClassAnonName();
            Action act = () => { ServiceLocator.Register<ObjectInterfaceClass>(instance, ""); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterSingletonWithNullInstance()
        {
            Action act = () => { ServiceLocator.Register((ObjectInterfaceClass)null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instance");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterSingletonWithNullInstanceAndName()
        {
            Action act = () => { ServiceLocator.Register((ObjectInterfaceClass)null, null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instance");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterSingletonWithNullName()
        {
            var instance = new ProviderClassAnonName();
            Action act = () => { ServiceLocator.Register<ObjectInterfaceClass>(instance, null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterSingletonWithWhiteSpaceName()
        {
            var instance = new ProviderClassAnonName();
            Action act = () => { ServiceLocator.Register<ObjectInterfaceClass>(instance, new string(' ', 4)); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterWithEmptyName()
        {
            Action act = () => { ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>(""); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterWithNullName()
        {
            Action act = () => { ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>(null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericRegisterWithWhiteSpaceName()
        {
            Action act = () => { ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>(new string(' ', 4)); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstance()
        {
            ServiceLocator.Register<ObjectInterfaceClass, ProviderClassAnonName>();
            ObjectInterfaceClass instance = null;
            ServiceLocator.TryGetInstance(out instance).Should().BeTrue();
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassAnonName>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstanceWithDependencyResolver()
        {
            var expected = new SampleService();
            var resolver = new DependencyResolver(typeof(SampleService), expected);
            CtorInterfaceClassAnon actual = null;

            ServiceLocator.TryGetInstance(out actual, resolver).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Guid.Should().Be(expected.Guid);

            ServiceLocator.TryGetInstance(out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Guid.Should().NotBe(expected.Guid);
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

            ServiceLocator.TryGetInstance(RegistrationName, out actual, resolver).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().Be(expected.Guid);

            ServiceLocator.TryGetInstance(RegistrationName, out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().NotBe(expected.Guid);

            ServiceLocator.TryGetInstance(out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<CtorInterfaceClassAnon>();
            actual.Should().NotBeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().NotBe(expected.Guid);
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

            ServiceLocator.TryGetInstance(RegistrationName, out actual, resolver).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().Be(expected.Guid);

            ServiceLocator.TryGetInstance(RegistrationName, out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().NotBe(expected.Guid);

            ServiceLocator.TryGetInstance(out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<CtorInterfaceClassAnon>();
            actual.Should().NotBeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().NotBe(expected.Guid);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstanceWithParameterResolver()
        {
            var expected = new SampleService();
            var resolver = new ParameterResolver("service", expected);
            CtorInterfaceClassAnon actual = null;

            ServiceLocator.TryGetInstance(out actual, resolver).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Guid.Should().Be(expected.Guid);

            ServiceLocator.TryGetInstance(out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Guid.Should().NotBe(expected.Guid);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GenericTryGetInstanceWithInvalidRegistration()
        {
            ObjectInterfaceClass instance = null;
            ServiceLocator.Register<ObjectInterfaceClass, ObjectInterfaceClass>();
            ServiceLocator.TryGetInstance(out instance).Should().BeFalse();
            instance.Should().BeNull();
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
            ServiceLocator.TryGetInstance(RegistrationName, out instance).Should().BeTrue();
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassWithName>();
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
            ServiceLocator.TryGetInstance(RegistrationName, out instance).Should().BeFalse();
            instance.Should().BeNull();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GetInstanceWithInjectedContainer()
        {
            var actual = ServiceLocator.GetInstance<UnityContainerInjectionClass>(UnityContainerResolver.Instance);
            actual.Should().NotBeNull();
            actual.Container.Should().BeSameAs(ServiceLocator.Container);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void GetInstanceWithSingletonWithoutRegistration()
        {
            var service1 = ServiceLocator.GetInstance<AutoRegisterInterface>();
            service1.Should().NotBeNull();
            var service2 = ServiceLocator.GetInstance<AutoRegisterInterface>();
            service2.Should().NotBeNull();
            service2.Should().BeSameAs(service1);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void InitializeTwiceWithDifferentConfig()
        {
            ServiceLocator.Initialize().Should().BeTrue();
            ServiceLocator.ConfigurationName.Should().Be(ServiceLocator.DefaultConfigurationName);
            ServiceLocator.Initialize(CustomConfigurationName).Should().BeTrue();
            ServiceLocator.ConfigurationName.Should().Be(CustomConfigurationName);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void InitializeTwiceWithSameConfig()
        {
            ServiceLocator.Initialize().Should().BeTrue();
            ServiceLocator.ConfigurationName.Should().Be(ServiceLocator.DefaultConfigurationName);
            ServiceLocator.Initialize().Should().BeFalse();
            ServiceLocator.ConfigurationName.Should().Be(ServiceLocator.DefaultConfigurationName);
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

            ServiceLocator.ConfigurationName.Should().Be(CustomConfigurationName);
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassAnonName>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void InitializeWithDefaultConfig()
        {
            ServiceLocator.Initialize();
            var instance = ServiceLocator.GetInstance<ObjectInterfaceClass>();
            ServiceLocator.ConfigurationName.Should().Be(ServiceLocator.DefaultConfigurationName);
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassAtConfig>();
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
            ServiceLocator.ConfigurationName.Should().Be(ServiceLocator.DefaultConfigurationName);
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassAtConfig>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericCheckConfigRegistration()
        {
            ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass)).Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericCheckDefaultRegistration()
        {
            ServiceLocator.IsRegistered(typeof(Object)).Should().BeFalse();
            ServiceLocator.Register(typeof(Object), typeof(String));
            ServiceLocator.IsRegistered(typeof(Object)).Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericCheckNamedRegistration()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass)).Should().BeTrue();
            ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass), RegistrationName).Should().BeFalse();
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassWithName), RegistrationName);
            ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass), RegistrationName).Should().BeTrue();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericCheckRegistrationWithEmptyName()
        {
            Action act = () => { ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass), ""); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericCheckRegistrationWithNullName()
        {
            Action act = () => { ServiceLocator.IsRegistered(typeof(ObjectInterfaceClass), null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("name");
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
                actual.Should().NotBeNull();
                actual.Guid.Should().Be(expected.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon)) as CtorInterfaceClassAnon);
                actual.Should().NotBeNull();
                actual.Guid.Should().NotBe(expected.Guid);
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
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().Be(expected.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon), RegistrationName) as CtorInterfaceClassAnon);
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().NotBe(expected.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon)) as CtorInterfaceClassAnon);
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassAnon>();
                actual.Should().NotBeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().NotBe(expected.Guid);
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
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().Be(expected.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon), RegistrationName) as CtorInterfaceClassAnon);
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().NotBe(expected.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon)) as CtorInterfaceClassAnon);
                actual.Should().NotBeNull();
                actual.Should().BeOfType<CtorInterfaceClassAnon>();
                actual.Should().NotBeOfType<CtorInterfaceClassNamed>();
                actual.Guid.Should().NotBe(expected.Guid);
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
                actual.Should().NotBeNull();
                actual.Guid.Should().Be(expected.Guid);
            }
            {
                var actual = (ServiceLocator.GetInstance(typeof(CtorInterfaceClassAnon)) as CtorInterfaceClassAnon);
                actual.Should().NotBeNull();
                actual.Guid.Should().NotBe(expected.Guid);
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

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithDefaultAndFactory()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), ((type, name) => new ProviderClassWithFact()));
            ServiceLocator.GetMappedType(typeof(ObjectInterfaceClass)).Should().BeNull();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithDefaultAndSingleton()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), new ProviderClassSingleton());
            ServiceLocator.GetMappedType(typeof(ObjectInterfaceClass)).Should().BeNull();
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

            actual.Should().NotBeNull();
            actual.Should().Be(expected);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithNameAndFactory()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), (type, name) => new ProviderClassWithFact(), RegistrationName);
            ServiceLocator.GetMappedType(typeof(ObjectInterfaceClass), RegistrationName).Should().BeNull();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithNameAndSingleton()
        {
            const string RegistrationName = "named";
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            ServiceLocator.Register(typeof(ObjectInterfaceClass), new ProviderClassSingleton(), RegistrationName);
            ServiceLocator.GetMappedType<ObjectInterfaceClass>(RegistrationName).Should().BeNull();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericGetMappedTypeWithUnmappedType()
        {
            ServiceLocator.GetMappedType(typeof(Object)).Should().BeNull();
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
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassAnonName>();

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass), RegistrationName) as ObjectInterfaceClass);
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassWithFact>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterFactoryAsDefault()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), (type, name) => new ProviderClassWithFact());
            ObjectInterfaceClass instance = null;

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass)) as ObjectInterfaceClass);
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassWithFact>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterFactoryWithEmptyName()
        {
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), ((type, name) => null), ""); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterFactoryWithNullName()
        {
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), ((type, name) => null), null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterFactoryWithNull()
        {
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), (ServiceFactoryDelegate)null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("factory");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterFactoryWithNullAndName()
        {
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), (ServiceFactoryDelegate)null, null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("factory");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterFactoryWithWhiteSpaceName()
        {
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), ((type, name) => null), new string(' ', 4)); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
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
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassAnonName>();

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass), RegistrationName) as ObjectInterfaceClass);
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassWithName>();
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
            instance.Should().NotBeNull();
            instance.Should().BeSameAs(singleton);
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
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassAnonName>();

            instance = (ServiceLocator.GetInstance(typeof(ObjectInterfaceClass), RegistrationName) as ObjectInterfaceClass);
            instance.Should().NotBeNull();
            instance.Should().BeSameAs(singleton);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterSingletonWithEmptyName()
        {
            var instance = new ProviderClassAnonName();
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), instance, ""); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterSingletonWithNullInstance()
        {
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), (ObjectInterfaceClass)null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instance");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterSingletonWithNullInstanceAndName()
        {
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), (ObjectInterfaceClass)null, null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("instance");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterSingletonWithNullName()
        {
            var instance = new ProviderClassAnonName();
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), instance, null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterSingletonWithWhiteSpaceName()
        {
            var instance = new ProviderClassAnonName();
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), instance, new string(' ', 4)); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterWithEmptyName()
        {
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName), ""); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterWithInvalidProvider()
        {
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(Object)); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("providerType");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterWithNullName()
        {
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName), null); };
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericRegisterWithWhiteSpaceName()
        {
            Action act = () => { ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName), new string(' ', 4)); };
            act.Should().Throw<ArgumentException>().And.ParamName.Should().Be("name");
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericTryGetInstance()
        {
            ServiceLocator.Register(typeof(ObjectInterfaceClass), typeof(ProviderClassAnonName));
            object instance = null;
            ServiceLocator.TryGetInstance(typeof(ObjectInterfaceClass), out instance).Should().BeTrue();
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassAnonName>();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericTryGetInstanceWithDependencyResolver()
        {
            var expected = new SampleService();
            var resolver = new DependencyResolver(typeof(SampleService), expected);
            CtorInterfaceClassAnon actual = null;

            ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual, resolver).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Guid.Should().Be(expected.Guid);

            ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Guid.Should().NotBe(expected.Guid);
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

            ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), RegistrationName, out actual, resolver).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().Be(expected.Guid);

            ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), RegistrationName, out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().NotBe(expected.Guid);

            ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().NotBeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().NotBe(expected.Guid);
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

            ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), RegistrationName, out actual, resolver).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().Be(expected.Guid);

            ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), RegistrationName, out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().NotBe(expected.Guid);

            ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Should().NotBeOfType<CtorInterfaceClassNamed>();
            actual.Guid.Should().NotBe(expected.Guid);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void NonGenericTryGetInstanceWithParameterResolver()
        {
            var expected = new SampleService();
            var resolver = new ParameterResolver("service", expected);
            CtorInterfaceClassAnon actual = null;

            ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual, resolver).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Guid.Should().Be(expected.Guid);

            ServiceLocator.TryGetInstance(typeof(CtorInterfaceClassAnon), out actual).Should().BeTrue();
            actual.Should().NotBeNull();
            actual.Guid.Should().NotBe(expected.Guid);
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
            ServiceLocator.TryGetInstance(typeof(ObjectInterfaceClass), out instance).Should().BeFalse();
            instance.Should().BeNull();
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
            ServiceLocator.TryGetInstance(typeof(ObjectInterfaceClass), RegistrationName, out instance).Should().BeTrue();
            instance.Should().NotBeNull();
            instance.Should().BeOfType<ProviderClassWithName>();
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
            ServiceLocator.TryGetInstance(typeof(ObjectInterfaceClass), RegistrationName, out instance).Should().BeFalse();
            instance.Should().BeNull();
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssemblyAndClasses()
        {
            ServiceLocator.RegisterAll(typeof(IAutoRegisterService).Assembly);
            var actual = default(IAutoRegisterService);

            actual = ServiceLocator.GetInstance<IAutoRegisterService>();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<AutoRegisterServiceOne>();

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceOneName);
            actual.Should().NotBeNull();
            actual.Should().BeOfType<AutoRegisterServiceOne>();

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceTwoName);
            actual.Should().NotBeNull();
            actual.Should().BeOfType<AutoRegisterServiceTwo>();
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
            service.Should().NotBeNull();
            service.ReferenceCount.Should().Be(1);

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceSingleton);
            actual.Should().NotBeNull();
            actual.Should().BeSameAs(service);
            service.ReferenceCount.Should().Be(1);

            ServiceLocator.Unload();
            service.ReferenceCount.Should().Be(0);
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
            actual.Should().NotBeNull();
            actual.Should().BeOfType<AutoRegisterSingleton>();
            ((AutoRegisterSingleton)actual).ReferenceCount.Should().Be(1);

            service = CodeServiceProvider.GetService<AutoRegisterSingleton>(AutoRegisterServiceSingleton);
            service.Should().NotBeNull();
            service.Should().Be(actual);
            service.ReferenceCount.Should().Be(1);

            ServiceLocator.Unload();
            service.ReferenceCount.Should().Be(0);
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
            service.Should().NotBeNull();
            service.ReferenceCount.Should().Be(1);

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceInterface);
            actual.Should().NotBeNull();
            actual.Should().Be(service);
            service.ReferenceCount.Should().Be(1);

            ServiceLocator.Unload();
            service.ReferenceCount.Should().Be(0);
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
            actual.Should().NotBeNull();
            actual.Should().BeOfType<AutoRegisterInterface>();
            ((AutoRegisterInterface)actual).ReferenceCount.Should().Be(1);

            service = CodeServiceProvider.GetService<AutoRegisterInterface>(AutoRegisterServiceInterface);
            service.Should().NotBeNull();
            service.Should().Be(actual);
            service.ReferenceCount.Should().Be(1);

            ServiceLocator.Unload();
            service.ReferenceCount.Should().Be(0);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssesmblyAndIsRegistered()
        {
            ServiceLocator.RegisterAll(Assembly.GetExecutingAssembly());
            var actual = default(IAutoRegisterService);

            actual = ServiceLocator.GetInstance<IAutoRegisterService>();
            actual.Should().NotBeNull();
            ServiceLocator.IsRegistered<IAutoRegisterService>().Should().BeTrue();

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceSingleton);
            actual.Should().NotBeNull();
            ServiceLocator.IsRegistered<IAutoRegisterService>(AutoRegisterServiceSingleton).Should().BeTrue();
        }

        [ExpectedException(typeof(ResolutionFailedException))]
        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssemblyAndNoClasses()
        {
            ServiceLocator.RegisterAll(typeof(Int32).Assembly);
            var actual = ServiceLocator.GetInstance<IAutoRegisterService>();
            actual.Should().BeNull();  // we shouldn't get to this point
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithAssemblyAndSingletonAndMultipleInterfaces()
        {
            ServiceLocator.RegisterAll(typeof(AutoRegisterMultiple).Assembly);
            var expect = ServiceLocator.GetInstance<IAutoRegisterService>(ServiceLocatorTests.AutoRegisterServiceMultiple);
            expect.Should().NotBeNull();

            var actual = ServiceLocator.GetInstance<IAutoRegisterService2>(ServiceLocatorTests.AutoRegisterServiceMultiple);
            actual.Should().NotBeNull();
            actual.Should().BeSameAs(expect);
        }

        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithWildcardAndClasses()
        {
            ServiceLocator.RegisterAll("CPP.Framework.*");
            var actual = default(IAutoRegisterService);

            actual = ServiceLocator.GetInstance<IAutoRegisterService>();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<AutoRegisterServiceOne>();

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceOneName);
            actual.Should().NotBeNull();
            actual.Should().BeOfType<AutoRegisterServiceOne>();

            actual = ServiceLocator.GetInstance<IAutoRegisterService>(AutoRegisterServiceTwoName);
            actual.Should().NotBeNull();
            actual.Should().BeOfType<AutoRegisterServiceTwo>();
        }

        [ExpectedException(typeof(ResolutionFailedException))]
        [TestMethod]
        [TestGroup(TestGroupTarget.Core)]
        [TestGroup(TestGroupTarget.DependencyInjection)]
        public void RegisterAllWithWildcardAndNoClasses()
        {
            ServiceLocator.RegisterAll("Microsoft.Practices.*");
            var actual = ServiceLocator.GetInstance<IAutoRegisterService>();
            actual.Should().BeNull();
        }
    }
}
