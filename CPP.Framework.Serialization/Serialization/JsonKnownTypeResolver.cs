using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using CPP.Framework.Threading;
using Newtonsoft.Json.Linq;

namespace CPP.Framework.Serialization
{
    /// <summary>
    /// Helper class used to resolve the materialized type from a JSON string.
    /// </summary>
    public class JsonKnownTypeResolver
    {
        /// <summary>
        /// The default scope flags for search for members through reflection.
        /// </summary>
        private const BindingFlags DefaultSearchAccess = (BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// The cached list of <see cref="AssemblyKnownTypeAttribute"/> values for all of the
        /// assemblies loaded in the current <see cref="AppDomain"/>.
        /// </summary>
        private static readonly Lazy<AssemblyKnownTypeAttribute[]> _AssemblyKnownTypes = new Lazy<AssemblyKnownTypeAttribute[]>(
            () =>
            {
                var attributes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(asm => asm.GetCustomAttributes(typeof(AssemblyKnownTypeAttribute), false))
                    .OfType<AssemblyKnownTypeAttribute>();
                return attributes.ToArray();
            },
            LazyThreadSafetyMode.PublicationOnly);

        /// <summary>
        /// The global map of types to their known type resolvers.
        /// </summary>
        private static readonly Dictionary<Type, JsonKnownTypeResolver> _GlobalResolverMap = new Dictionary<Type, JsonKnownTypeResolver>();

        /// <summary>
        /// The reader/writer lock used to synchronize acces to the <see cref="_knownTypeMap"/>
        /// variable.
        /// </summary>
        private static readonly MultiAccessLock _SyncLock = new MultiAccessLock(LockRecursionPolicy.SupportsRecursion);
 
        /// <summary>
        /// Contains a map of known type indicator property names to their corresponding
        /// <see cref="JsonKnownTypeResolver"/> instances.
        /// </summary>
        private readonly ReadOnlyDictionary<string, JsonKnownTypeResolver> _knownTypeMap;
 
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonKnownTypeResolver"/> class. 
        /// </summary>
        /// <param name="baseType">
        /// The base type associated with the resolver.
        /// </param>
        protected JsonKnownTypeResolver(Type baseType)
        {
            var propertyInfo = (baseType.GetProperties(BindingFlags.Instance | BindingFlags.DeclaredOnly | DefaultSearchAccess)
                .Where(p => (p.HasCustomAttribute<JsonKnownTypeIndicatorAttribute>()))
                .FirstOrDefault());
            this.BaseType = baseType;
            this.PropertyName = ((propertyInfo == null) ? string.Empty : propertyInfo.Name);

            var map = new Dictionary<string, JsonKnownTypeResolver>(StringComparer.Ordinal);
            var set = new HashSet<Type>();
            foreach (var knownType in GetKnownTypes(baseType))
            {
                if (!set.Add(knownType)) continue;  // processing the same type twice will make bad things happen--don't do it
                if (baseType != knownType.BaseType) continue;

                var resolver = GetOrAddGlobalResolver(knownType, true);
                if (map.ContainsKey(resolver.PropertyName))
                {
                    throw new DuplicateKnownTypeException(knownType, propertyInfo);
                }
                map[resolver.PropertyName] = resolver;
            }
            _knownTypeMap = new ReadOnlyDictionary<string, JsonKnownTypeResolver>(map);
        }

        /// <summary>
        /// Gets the base type associated with the resolver.
        /// </summary>
        public Type BaseType { get; }

        /// <summary>
        /// Gets the name of the indicator property for the base type, if available.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Creates a <see cref="JsonKnownTypeResolver"/> for a given type.
        /// </summary>
        /// <param name="baseType">The base type of the resolver.</param>
        /// <returns>A <see cref="JsonKnownTypeResolver"/> object.</returns>
        public static JsonKnownTypeResolver Create(Type baseType)
        {
            return GetOrAddGlobalResolver(baseType, false);
        }

        /// <summary>
        /// Retrieves a the list of known types for the base type.
        /// </summary>
        /// <param name="baseType">The <see cref="Type"/> object for the base type.</param>
        /// <param name="ignoreKnownTypeMethods">
        /// True to ignore <see cref="Type"/> values returned from the static methods defined by a 
        /// <see cref="KnownTypeAttribute"/>, which prevents a situation where a stack overflow can 
        /// occur if the method is called from within the body of such a static method. Passing a
        /// value for this parameter is optional, and the default is false.
        /// </param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that can be used to iterate over the types.</returns>
        public static IEnumerable<Type> GetKnownTypes(Type baseType, bool ignoreKnownTypeMethods = false)
        {
            ArgumentValidator.ValidateNotNull(() => baseType);

            // look for all types known to the class.
            var attributes = baseType
                .GetCustomAttributes<KnownTypeAttribute>(false)
                .ToList();
            foreach (var knownType in attributes)
            {
                if (!string.IsNullOrWhiteSpace(knownType.MethodName))
                {
                    if (ignoreKnownTypeMethods) continue; // let's not blow the stack, if requested.

                    var methodInfo = baseType.GetMethod(knownType.MethodName, (BindingFlags.Static | DefaultSearchAccess));
                    if (methodInfo == null) continue;

                    if (methodInfo.Invoke(null, new object[0]) is IEnumerable<Type> enumerator)
                    {
                        foreach (var type in enumerator) yield return type;
                    }
                    continue;
                }
                if (knownType.Type != null) yield return knownType.Type;
            }

            // look for all types known to the loaded assemblies.
            foreach (var attribute in _AssemblyKnownTypes.Value)
            {
                var candidate = attribute.Type;
                if (!baseType.IsAssignableFrom(attribute.Type)) continue;
                if (baseType == candidate) continue;    // never include yourself in the list
                if (!candidate.IsClass) continue;
                if (candidate.IsAbstract) continue;
                if (candidate.IsGenericType && (!candidate.IsConstructedGenericType)) continue;
                yield return attribute.Type;
            }
        }

        /// <summary>
        /// Gets the <see cref="JsonKnownTypeResolver"/> for a given type.
        /// </summary>
        /// <param name="baseType">The type assocated with the resolver.</param>
        /// <param name="validate">True to verify whether or not the indicator property for the resolver is valid; otherwise, false.</param>
        /// <returns>A <see cref="JsonKnownTypeResolver"/> object.</returns>
        private static JsonKnownTypeResolver GetOrAddGlobalResolver(Type baseType, bool validate)
        {
            JsonKnownTypeResolver resolver = null;
            using (_SyncLock.GetReaderAccess())
            {
                if (_GlobalResolverMap.TryGetValue(baseType, out resolver)) return resolver;
            }
            using (_SyncLock.GetWriterAccess())
            {
                if (!_GlobalResolverMap.TryGetValue(baseType, out resolver))
                {
                    resolver = new JsonKnownTypeResolver(baseType);
                    if (validate && string.IsNullOrWhiteSpace(resolver.PropertyName))
                    {
                        throw new InvalidKnownTypeException(baseType);
                    }
                    _GlobalResolverMap[baseType] = resolver;
                }
                return resolver;
            }
        }

        /// <summary>
        /// Attempts to resolve the type of the destination class for a given JSON object.
        /// </summary>
        /// <param name="jsonObject">The JSON object.</param>
        /// <returns>A <see cref="Type"/> object.</returns>
        public Type ResolveType(JObject jsonObject)
        {
            if (string.IsNullOrWhiteSpace(this.PropertyName) || (jsonObject[this.PropertyName] != null))
            {
                foreach (var entry in _knownTypeMap)
                {
                    if (jsonObject[entry.Key] == null) continue;
                    var candidate = entry.Value.ResolveType(jsonObject);
                    if (candidate != null) return candidate;
                }
                return this.BaseType;
            }
            return null;
        }
    }
}
