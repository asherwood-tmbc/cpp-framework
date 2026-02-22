using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CPP.Framework.DependencyInjection;
using CPP.Framework.UnitTests.Testing;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CPP.Framework.Services
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed. Suppression is OK here.")]
    public class CodeServiceProviderTests
    {
        private const string ServiceName1 = "$(__ServiceOne__)";
        private const string ServiceName2 = "$(__ServiceTwo__)";

        /// <summary>
        /// Called by the testing framework just after executing a test method.
        /// </summary>
        [TestCleanup]
        public void OnTestCleanup() => ServiceLocator.Unload();

        [TestMethod]
        public void GetServiceWithInvalidRegistrationName()
        {
            CodeServiceProvider.Register<ITestService1, NamedSingletonService>();
            CodeServiceProvider.Register<ITestService2, NamedSingletonService>();

            var expect = CodeServiceProvider.GetService<ITestService1>();
            var actual = CodeServiceProvider.GetService<ITestService2>();
            actual.Should().BeSameAs(expect);
            actual.Name.Should().Be(expect.Name);

            Action act = () => CodeServiceProvider.GetService<ITestService2>(ServiceName2);
            act.Should().Throw<MissingServiceRegistrationException>();
        }

        [TestMethod]
        public void RegisterWithAutoAttributeAndImpl()
        {
            var actual = CodeServiceProvider.GetService<AutoRegisteredService>();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<ImplRegisteredService>();

            CodeServiceProvider.Release<AutoRegisteredService>();
        }

        [TestMethod]
        public void RegisterWithAutoAttributeAndSelf()
        {
            var actual = CodeServiceProvider.GetService<SelfRegisteredService>();
            actual.Should().NotBeNull();
            actual.Should().BeOfType<SelfRegisteredService>();

            CodeServiceProvider.Release<SelfRegisteredService>();
        }

        [TestMethod]
        public void RegisterWithBasicPerUseLifetime()
        {
            ITestService1 expect;
            ITestService2 actual;

            CodeServiceProvider.Register<ITestService1, BasicPerUseLifetimeService>();
            CodeServiceProvider.Register<ITestService1, BasicPerUseLifetimeService>(ServiceName1);
            CodeServiceProvider.Register<ITestService1, BasicPerUseLifetimeService>(ServiceName2);
            CodeServiceProvider.Register<ITestService2, BasicPerUseLifetimeService>();
            CodeServiceProvider.Register<ITestService2, BasicPerUseLifetimeService>(ServiceName1);
            CodeServiceProvider.Register<ITestService2, BasicPerUseLifetimeService>(ServiceName2);
            var created = new HashSet<object>();

            created.Add(expect = CodeServiceProvider.GetService<ITestService1>());
            created.Add(actual = CodeServiceProvider.GetService<ITestService2>());
            actual.Should().NotBeSameAs(expect);
            actual.Name.Should().NotBe(expect.Name);

            expect = CodeServiceProvider.GetService<ITestService1>();
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName1);
            actual.Should().NotBeSameAs(expect);
            created.Contains(expect).Should().BeFalse();
            created.Contains(actual).Should().BeFalse();

            created.Add(expect);
            created.Add(actual);
            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName1);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName2);
            actual.Should().NotBeSameAs(expect);
            created.Contains(expect).Should().BeFalse();
            created.Contains(actual).Should().BeFalse();

            created.Add(expect);
            created.Add(actual);
            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName1);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName1);
            actual.Should().NotBeSameAs(expect);
            created.Contains(expect).Should().BeFalse();
            created.Contains(actual).Should().BeFalse();

            created.Add(expect);
            created.Add(actual);
            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName2);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName2);
            actual.Should().NotBeSameAs(expect);

            CodeServiceProvider.Release<ITestService1>();
            CodeServiceProvider.Release<ITestService1>(ServiceName1);
            CodeServiceProvider.Release<ITestService1>(ServiceName2);
            CodeServiceProvider.Release<ITestService2>();
            CodeServiceProvider.Release<ITestService2>(ServiceName1);
            CodeServiceProvider.Release<ITestService2>(ServiceName2);
        }

        [TestMethod]
        public void RegisterWithBasicSingleton()
        {
            CodeServiceProvider.Register<ITestService1, BasicSingletonService>();
            CodeServiceProvider.Register<ITestService1, BasicSingletonService>(ServiceName1);
            CodeServiceProvider.Register<ITestService1, BasicSingletonService>(ServiceName2);
            CodeServiceProvider.Register<ITestService2, BasicSingletonService>();
            CodeServiceProvider.Register<ITestService2, BasicSingletonService>(ServiceName1);
            CodeServiceProvider.Register<ITestService2, BasicSingletonService>(ServiceName2);

            var expect = CodeServiceProvider.GetService<ITestService1>();
            var actual = CodeServiceProvider.GetService<ITestService2>();
            actual.Should().BeSameAs(expect);
            actual.Should().BeOfType<BasicSingletonService>();

            expect = CodeServiceProvider.GetService<ITestService1>();
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName1);
            actual.Should().BeSameAs(expect);
            actual.Should().BeOfType<BasicSingletonService>();

            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName1);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName2);
            actual.Should().BeSameAs(expect);
            actual.Should().BeOfType<BasicSingletonService>();

            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName1);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName1);
            actual.Should().BeSameAs(expect);
            actual.Should().BeOfType<BasicSingletonService>();

            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName2);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName2);
            actual.Should().BeSameAs(expect);
            actual.Should().BeOfType<BasicSingletonService>();

            CodeServiceProvider.Release<ITestService1>();
            CodeServiceProvider.Release<ITestService1>(ServiceName1);
            CodeServiceProvider.Release<ITestService1>(ServiceName2);
            CodeServiceProvider.Release<ITestService2>();
            CodeServiceProvider.Release<ITestService2>(ServiceName1);
            CodeServiceProvider.Release<ITestService2>(ServiceName2);
        }

        [TestMethod]
        public void RegisterWithInvalidServiceClass()
        {
            Action act = () => CodeServiceProvider.Register<ITestService1, InvalidServiceClass>();
            act.Should().Throw<InvalidServiceRegistrationException>();
        }

        [TestMethod]
        public void RegisterWithNamedPerUseLifetime()
        {
            CodeServiceProvider.Register<ITestService1, NamedPerUseLifetimeService>();
            CodeServiceProvider.Register<ITestService1, NamedPerUseLifetimeService>(ServiceName1);
            CodeServiceProvider.Register<ITestService1, NamedPerUseLifetimeService>(ServiceName2);
            CodeServiceProvider.Register<ITestService2, NamedPerUseLifetimeService>();
            CodeServiceProvider.Register<ITestService2, NamedPerUseLifetimeService>(ServiceName1);
            CodeServiceProvider.Register<ITestService2, NamedPerUseLifetimeService>(ServiceName2);

            var expect = CodeServiceProvider.GetService<ITestService1>();
            var actual = CodeServiceProvider.GetService<ITestService2>();
            actual.Should().NotBeSameAs(expect);
            actual.Name.Should().Be(expect.Name);

            expect = CodeServiceProvider.GetService<ITestService1>();
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName1);
            actual.Should().NotBeSameAs(expect);
            expect.Name.Should().Be(string.Empty);
            actual.Name.Should().Be(ServiceName1);

            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName1);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName2);
            actual.Should().NotBeSameAs(expect);
            expect.Name.Should().Be(ServiceName1);
            actual.Name.Should().Be(ServiceName2);

            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName1);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName1);
            actual.Should().NotBeSameAs(expect);
            actual.Name.Should().Be(expect.Name);

            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName2);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName2);
            actual.Should().NotBeSameAs(expect);
            actual.Name.Should().Be(expect.Name);

            CodeServiceProvider.Release<ITestService1>();
            CodeServiceProvider.Release<ITestService1>(ServiceName1);
            CodeServiceProvider.Release<ITestService1>(ServiceName2);
            CodeServiceProvider.Release<ITestService2>();
            CodeServiceProvider.Release<ITestService2>(ServiceName1);
            CodeServiceProvider.Release<ITestService2>(ServiceName2);
        }

        [TestMethod]
        public void RegisterWithNamedSingleton()
        {
            CodeServiceProvider.Register<ITestService1, NamedSingletonService>(LockRecursionPolicy.NoRecursion);
            CodeServiceProvider.Register<ITestService1, NamedSingletonService>(ServiceName1);
            CodeServiceProvider.Register<ITestService1, NamedSingletonService>(ServiceName2);
            CodeServiceProvider.Register<ITestService2, NamedSingletonService>(LockRecursionPolicy.NoRecursion);
            CodeServiceProvider.Register<ITestService2, NamedSingletonService>(ServiceName1);
            CodeServiceProvider.Register<ITestService2, NamedSingletonService>(ServiceName2);

            var expect = CodeServiceProvider.GetService<ITestService1>();
            var actual = CodeServiceProvider.GetService<ITestService2>();
            actual.Should().BeSameAs(expect);
            actual.Name.Should().Be(expect.Name);

            expect = CodeServiceProvider.GetService<ITestService1>();
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName1);
            actual.Should().NotBeSameAs(expect);
            expect.Name.Should().Be(string.Empty);
            actual.Name.Should().Be(ServiceName1);

            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName1);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName2);
            actual.Should().NotBeSameAs(expect);
            expect.Name.Should().Be(ServiceName1);
            actual.Name.Should().Be(ServiceName2);

            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName1);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName1);
            actual.Should().BeSameAs(expect);
            actual.Name.Should().Be(expect.Name);

            expect = CodeServiceProvider.GetService<ITestService1>(ServiceName2);
            actual = CodeServiceProvider.GetService<ITestService2>(ServiceName2);
            actual.Should().BeSameAs(expect);
            actual.Name.Should().Be(expect.Name);

            CodeServiceProvider.Release<ITestService1>();
            CodeServiceProvider.Release<ITestService1>(ServiceName1);
            CodeServiceProvider.Release<ITestService1>(ServiceName2);
            CodeServiceProvider.Release<ITestService2>();
            CodeServiceProvider.Release<ITestService2>(ServiceName1);
            CodeServiceProvider.Release<ITestService2>(ServiceName2);
        }

        [TestMethod]
        public void RegisterWithNamedSingletonUsingLocator()
        {
            CodeServiceProvider.Register<ITestService1, NamedSingletonService>(LockRecursionPolicy.NoRecursion);
            CodeServiceProvider.Register<ITestService1, NamedSingletonService>(ServiceName1);
            CodeServiceProvider.Register<ITestService1, NamedSingletonService>(ServiceName2);
            CodeServiceProvider.Register<ITestService2, NamedSingletonService>(LockRecursionPolicy.NoRecursion);
            CodeServiceProvider.Register<ITestService2, NamedSingletonService>(ServiceName1);
            CodeServiceProvider.Register<ITestService2, NamedSingletonService>(ServiceName2);

            var expect = ServiceLocator.GetInstance<ITestService1>();
            var actual = ServiceLocator.GetInstance<ITestService2>();
            actual.Should().BeSameAs(expect);
            actual.Name.Should().Be(expect.Name);

            expect = ServiceLocator.GetInstance<ITestService1>();
            actual = ServiceLocator.GetInstance<ITestService2>(ServiceName1);
            actual.Should().NotBeSameAs(expect);
            expect.Name.Should().Be(string.Empty);
            actual.Name.Should().Be(ServiceName1);

            expect = ServiceLocator.GetInstance<ITestService1>(ServiceName1);
            actual = ServiceLocator.GetInstance<ITestService2>(ServiceName2);
            actual.Should().NotBeSameAs(expect);
            expect.Name.Should().Be(ServiceName1);
            actual.Name.Should().Be(ServiceName2);

            expect = ServiceLocator.GetInstance<ITestService1>(ServiceName1);
            actual = ServiceLocator.GetInstance<ITestService2>(ServiceName1);
            actual.Should().BeSameAs(expect);
            actual.Name.Should().Be(expect.Name);

            expect = ServiceLocator.GetInstance<ITestService1>(ServiceName2);
            actual = ServiceLocator.GetInstance<ITestService2>(ServiceName2);
            actual.Should().BeSameAs(expect);
            actual.Name.Should().Be(expect.Name);

            CodeServiceProvider.Release<ITestService1>();
            CodeServiceProvider.Release<ITestService1>(ServiceName1);
            CodeServiceProvider.Release<ITestService1>(ServiceName2);
            CodeServiceProvider.Release<ITestService2>();
            CodeServiceProvider.Release<ITestService2>(ServiceName1);
            CodeServiceProvider.Release<ITestService2>(ServiceName2);
        }

        [TestMethod]
        public void UseServiceLocatorWithSingleton()
        {
            var expect = ServiceLocator.GetInstance<AutoRegisteredSingleton>();
            var actual = CodeServiceProvider.GetService<AutoRegisteredSingleton>();
            actual.Should().BeSameAs(expect);
        }

        #region ITestService1 Interface Declaration

        private interface ITestService1 { string Name { get; } }

        #endregion // ITestService1 Interface Declaration

        #region ITestService2 Interface Declaration

        private interface ITestService2 { string Name { get; } }

        #endregion // ITestService2 Interface Declaration

        #region AutoRegisteredService Class Declaration

        [AutoRegisterService(typeof(ImplRegisteredService))]
        private class AutoRegisteredService : CodeService
        {
            protected AutoRegisteredService() { }
        }

        #endregion // AutoRegisteredService Class Declaration

        #region AutoRegisteredSingleton Class Declaration

        [AutoRegisterService]
        private sealed class AutoRegisteredSingleton : CodeServiceSingleton
        {
            [ServiceLocatorConstructor]
            private AutoRegisteredSingleton() { }
        }

        #endregion // AutoRegisteredSingleton Class Declaration

        #region ImplRegisteredService Class Declaration

        private class ImplRegisteredService : AutoRegisteredService
        {
            protected ImplRegisteredService() { }
        }

        #endregion // ImplRegisteredService Class Declaration

        #region BasicPerUseLifetimeService Class Declaration

        [UsedImplicitly]
        private sealed class BasicPerUseLifetimeService : CodeService, ITestService1, ITestService2
        {
            private BasicPerUseLifetimeService() => this.Name = Guid.NewGuid().ToString("N");

            public string Name { get; }
        }

        #endregion // BasicPerUseLifetimeService Class Declaration

        #region BasicSingletonService Class Declaration

        private sealed class BasicSingletonService : CodeServiceSingleton, ITestService1, ITestService2
        {
            private BasicSingletonService()
            {
                this.Name = Guid.NewGuid().ToString("N");
            }

            public string Name { get; }
        }

        #endregion // BasicSingletonService Class Declaration

        #region InvalidServiceClass Class Declaration

        [UsedImplicitly]
        private sealed class InvalidServiceClass : CodeService, ITestService1, ITestService2
        {
            private InvalidServiceClass(string name) => this.Name = (name?.Trim() ?? string.Empty);

            public string Name { get; }
        }

        #endregion // InvalidServiceClass Class Declaration

        #region NamedPerUseLifetimeService Class Declaration

        private sealed class NamedPerUseLifetimeService : CodeService, ITestService1, ITestService2
        {
            private NamedPerUseLifetimeService(string name) => this.Name = (name?.Trim() ?? string.Empty);

            public string Name { get; }

            [CreateServiceInstance]
            [UsedImplicitly]
            private static NamedPerUseLifetimeService CreateInstance(string name) => new NamedPerUseLifetimeService(name);
        }

        #endregion // NamedPerUseLifetimeService Class Declaration

        #region NamedSingletonService Class Declaration

        private sealed class NamedSingletonService : CodeServiceSingleton, ITestService1, ITestService2
        {
            private NamedSingletonService(string name) => this.Name = (name?.Trim() ?? string.Empty);

            public string Name { get; }

            [CreateServiceInstance]
            [UsedImplicitly]
            private static NamedSingletonService CreateInstance(string name) => new NamedSingletonService(name);
        }

        #endregion // NamedSingletonService Class Declaration

        #region SelfRegisteredService Class Declaration

        [AutoRegisterService]
        private class SelfRegisteredService : CodeService
        {
            protected SelfRegisteredService() { }
        }

        #endregion // SelfRegisteredService Class Declaration
    }
}
