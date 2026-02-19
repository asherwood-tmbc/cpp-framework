using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace CPP.Framework.UnitTests.Testing
{
    [ExcludeFromCodeCoverage]
    internal static class ReflectionHelper
    {
        private const BindingFlags InternalBindingFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Creates an instance of a class regardless of its constructor visibility.
        /// </summary>
        public static T CreateInstance<T>(params object[] constructorArguments)
        {
            var types = constructorArguments.Select(arg => arg.GetType()).ToArray();
            var ctor = typeof(T).GetConstructor(InternalBindingFlags, null, types, null);
            return (T)ctor.Invoke(constructorArguments);
        }
    }
}
