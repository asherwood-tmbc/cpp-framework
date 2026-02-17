using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

using CPP.Framework.Diagnostics.Testing;
using CPP.Framework.Serialization;

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
            Verify.IsFalse(actual.Contains(typeof(JsonCommonClass)));
            Verify.IsTrue(actual.Contains(typeof(JsonExternalDerivedClass)));
            Verify.IsTrue(actual.Contains(typeof(JsonExternalClassByProxy)));
            Verify.IsTrue(actual.Contains(typeof(JsonInternalDerivedClass)));

            actual.Clear();
            actual.UnionWith(JsonKnownTypeResolver.GetKnownTypes(typeof(JsonExternalDerivedClass)));
            Verify.IsFalse(actual.Contains(typeof(JsonCommonClass)));
            Verify.IsFalse(actual.Contains(typeof(JsonExternalDerivedClass)));
            Verify.IsTrue(actual.Contains(typeof(JsonExternalClassByProxy)));
            Verify.IsFalse(actual.Contains(typeof(JsonInternalDerivedClass)));
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
