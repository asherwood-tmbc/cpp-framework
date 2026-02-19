using System;
using System.Diagnostics.CodeAnalysis;
using CPP.Framework.DependencyInjection;

namespace CPP.Framework.UnitTests.Testing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [ExcludeFromCodeCoverage]
    public class StubServiceRegistrationAttribute : Attribute
    {
        public StubServiceRegistrationAttribute(Type interfaceType) : this(interfaceType, null) { }

        public StubServiceRegistrationAttribute(Type interfaceType, string registrationName)
        {
            ArgumentValidator.ValidateNotNull(() => interfaceType);
            this.InterfaceType = interfaceType;
            this.RegistrationName = (registrationName ?? String.Empty).Trim();
        }

        public Type InterfaceType { get; private set; }
        public string RegistrationName { get; private set; }
    }
}
