using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

using CPP.Framework.UnitTests.Testing;
using CPP.Framework.Serialization;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: AssemblyKnownType(typeof(JsonKnownTypeResolverTests.JsonExternalDerivedClass))]
[assembly: AssemblyKnownType(typeof(JsonKnownTypeResolverTests.JsonExternalClassByProxy))]

namespace CPP.Framework.Serialization
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class JsonKnownTypeResolverTests
    {
        [TestMethod]
        public void GetKnownTypes()
        {
            var actual = new HashSet<Type>();

            actual.Clear();
            actual.UnionWith(JsonKnownTypeResolver.GetKnownTypes(typeof(JsonCommonClass)));
            actual.Contains(typeof(JsonCommonClass)).Should().BeFalse();
            actual.Contains(typeof(JsonExternalDerivedClass)).Should().BeTrue();
            actual.Contains(typeof(JsonExternalClassByProxy)).Should().BeTrue();
            actual.Contains(typeof(JsonInternalDerivedClass)).Should().BeTrue();

            actual.Clear();
            actual.UnionWith(JsonKnownTypeResolver.GetKnownTypes(typeof(JsonExternalDerivedClass)));
            actual.Contains(typeof(JsonCommonClass)).Should().BeFalse();
            actual.Contains(typeof(JsonExternalDerivedClass)).Should().BeFalse();
            actual.Contains(typeof(JsonExternalClassByProxy)).Should().BeTrue();
            actual.Contains(typeof(JsonInternalDerivedClass)).Should().BeFalse();
        }

        #region Internal Helper Class Definitions

        [KnownType(nameof(GetKnownTypes))]
        [KnownType(typeof(JsonInternalDerivedClass))]
        internal class JsonCommonClass
        {
            private static IEnumerable<Type> GetKnownTypes() => JsonKnownTypeResolver.GetKnownTypes(typeof(JsonCommonClass), true);
        }

        internal class JsonExternalDerivedClass : JsonCommonClass
        {
            [JsonKnownTypeIndicator]
            public bool IsDynamicClass { get; } = true;
        }

        internal class JsonExternalClassByProxy : JsonExternalDerivedClass
        {
            public bool IsSuperDerived { get; } = true;
        }

        internal class JsonInternalDerivedClass : JsonCommonClass
        {
            [JsonKnownTypeIndicator]
            public bool IsDerivedClass { get; } = true;
        }

        #endregion // Internal Helper Class Definitions
    }
}
