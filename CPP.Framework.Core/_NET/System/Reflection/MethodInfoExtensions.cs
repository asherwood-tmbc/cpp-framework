using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CPP.Framework;

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    /// <summary>
    /// Defines extension methods for the <see cref="MethodInfo" /> class.
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>
        /// Verifies whether or not a declare generic argument or parameter <see cref="Type"/>
        /// matches an expected type or open generic type.
        /// </summary>
        /// <param name="declared">The declared type.</param>
        /// <param name="expected">The expected type.</param>
        /// <param name="collected">
        /// An optional <see cref="HashSet{T}"/> that receives any match generic type arguments
        /// from the matched declared type for an open generic expected type.
        /// </param>
        /// <returns>
        /// True if <paramref name="declared"/> matches <paramref name="expected"/>; otherwise, 
        /// false.
        /// </returns>
        private static bool ArgumentTypeEquals(Type declared, Type expected, HashSet<Type> collected)
        {
            if ((expected == null) || (expected == typeof(void)))
            {
                return (declared.IsGenericParameter || (declared == typeof(void)));
            }
            if (expected.IsGenericType && expected.ContainsGenericParameters)
            {
                if (!declared.IsGenericType) return false;
                if (declared.GetGenericTypeDefinition() != expected) return false;
                return CollectGenericArguments(declared, expected, collected);
            }
            return (declared == expected);
        }

        /// <summary>
        /// Verifies whether or not an generic argument list or parameter list matches and expected
        /// signature.
        /// </summary>
        /// <param name="sequence">The declared type list to verify.</param>
        /// <param name="expected">
        /// An array of <see cref="Type" /> objects representing the number, order, and type of the 
        /// parameters for the method to get, or <see cref="F:System.Type.EmptyTypes" /> to match a 
        /// empty argument list.
        /// </param>
        /// <param name="collected">
        /// An optional <see cref="HashSet{T}"/> that collects any match generic type arguments
        /// from the matched declared types for any expected open generic type.
        /// </param>
        /// <param name="processed">
        /// An optional <see cref="HashSet{T}"/> that receives matched generic type parameters when
        /// validating the generic arguments list.
        /// </param>
        /// <returns>True if the declared types match the expected types; otherwise, false.</returns>
        private static bool ArgumentListEquals(IEnumerable sequence, IEnumerable<Type> expected, HashSet<Type> collected, HashSet<Type> processed)
        {
            var declared = (sequence as IEnumerable<Type>);
            if ((declared == null) && (sequence is IEnumerable<ParameterInfo> @params))
            {
                declared = @params.Select(pi => pi.ParameterType);
            }
            if (declared == null) throw new ArgumentNullException(nameof(sequence));
            processed?.Clear(); // always start fresh with the processed list

            using (var signature = (declared ?? Enumerable.Empty<Type>()).GetEnumerator())
            using (var arguments = (expected ?? Enumerable.Empty<Type>()).GetEnumerator())
            {
                while (signature.MoveNext())
                {
                    if (!arguments.MoveNext()) return false;
                    var x = signature.Current;
                    var y = arguments.Current;
                    if (!ArgumentTypeEquals(x, y, collected)) return false;
                    if ((processed != null) && (x?.IsGenericParameter ?? false)) processed.Add(x);
                }
                return (!arguments.MoveNext()); // should be no more arguments left to compare
            }
        }

        /// <summary>
        /// Collects the generic argument definitions for the expected type that have been matched
        /// against the corresponding declared type.
        /// </summary>
        /// <param name="declared">The declared type.</param>
        /// <param name="expected">The expected type.</param>
        /// <param name="collected">
        /// An optional <see cref="HashSet{T}"/> that receives any match generic type arguments
        /// from the matched declared type for an open generic expected type.
        /// </param>
        /// <returns>
        /// True if items were added to <paramref name="collected"/>; otherwise, false.
        /// </returns>
        private static bool CollectGenericArguments(Type declared, Type expected, HashSet<Type> collected)
        {
            if (collected == null) return true;
            if (declared.IsGenericParameter && expected.IsGenericParameter)
            {
                collected.Add(declared);
                return true;
            }
            if (declared.IsGenericType && expected.IsGenericType)
            {
                var signature = declared.GetGenericArguments();
                var arguments = expected.GetGenericArguments();
                if (signature.Length != arguments.Length) return false;

                for (var i = 0; i < signature.Length; i++)
                {
                    if (!CollectGenericArguments(signature[i], arguments[i], collected)) return false;
                }
                return true;
            }
            return ((!declared.IsGenericType) && (!expected.IsGenericType));
        }

        /// <summary>
        /// Tests whether or not a <see cref="MethodInfo"/> matches a given signature.
        /// </summary>
        /// <param name="mi">The <see cref="MethodInfo"/> to verify.</param>
        /// <param name="parameters">
        /// An array of <see cref="Type" /> objects representing the number, order, and type of the 
        /// parameters for the method to get, or <see cref="F:System.Type.EmptyTypes" /> to match a 
        /// empty argument list.
        /// </param>
        /// <returns>
        /// True if <paramref name="mi"/> matches the signature provided; otherwise, false.
        /// </returns>
        public static bool SignatureEquals(this MethodInfo mi, IEnumerable<Type> parameters)
        {
            return SignatureEquals(mi, parameters, typeof(void));
        }

        /// <summary>
        /// Tests whether or not a <see cref="MethodInfo"/> matches a given signature.
        /// </summary>
        /// <param name="mi">The <see cref="MethodInfo"/> to verify.</param>
        /// <param name="parameters">
        /// An array of <see cref="Type" /> objects representing the number, order, and type of the 
        /// parameters for the method to get, or <see cref="F:System.Type.EmptyTypes" /> to match a 
        /// empty argument list.
        /// </param>
        /// <param name="returnType">The expected type of the method's return value.</param>
        /// <returns>
        /// True if <paramref name="mi"/> matches the signature provided; otherwise, false.
        /// </returns>
        public static bool SignatureEquals(this MethodInfo mi, IEnumerable<Type> parameters, Type returnType)
        {
            ArgumentValidator.ValidateThisObj(() => mi);

            returnType = (returnType ?? typeof(void));
            var collected = new HashSet<Type>();

            if (!ArgumentListEquals(mi.GetParameters(), parameters, collected, null)) return false;
            if (!ArgumentTypeEquals(mi.ReturnType, returnType, collected)) return false;

            var processed = default(HashSet<Type>);
            var type = default(Type);
            while ((collected.Count >= 1) && (type != typeof(object)))
            {
                var declared = default(IEnumerable<Type>);
                var expected = default(IEnumerable<Type>);
                processed = (processed ?? new HashSet<Type>());

                var local = type;
                expected = ((type == null)
                    ? collected.Where(pi => (pi.DeclaringMethod == mi))
                    : collected.Where(pi => (pi.DeclaringType == local)))
                    .OrderBy(pi => pi.GenericParameterPosition);
                declared = (type?.GetGenericArguments() ?? mi.GetGenericArguments());

                if (!ArgumentListEquals(declared, expected, null, processed)) return false;
                collected.ExceptWith(processed);    // remove the processed arguments
                type = ((type == null) ? mi.DeclaringType : type.BaseType);
            }
            return (collected.Count == 0);
        }

        /// <summary>
        /// Selects the first method in a sequence with a given signature.
        /// </summary>
        /// <param name="sequence">
        /// An <see cref="IEnumerable{T}"/> object that references a sequence of
        /// <see cref="MethodInfo"/> objects to check against the signature provided.
        /// </param>
        /// <param name="parameters">
        /// An array of <see cref="Type" /> objects representing the number, order, and type of the 
        /// parameters for the method to get, or <see cref="F:System.Type.EmptyTypes" /> to match a 
        /// empty argument list.
        /// </param>
        /// <returns>
        /// A <see cref="MethodInfo"/> object, or null if no matching signature was found within
        /// <paramref name="sequence"/>.
        /// </returns>
        public static MethodInfo WithSignature(this IEnumerable<MethodInfo> sequence, IEnumerable<Type> parameters)
        {
            return WithSignature(sequence, parameters, typeof(void));
        }

        /// <summary>
        /// Selects the first method in a sequence with a given signature.
        /// </summary>
        /// <param name="sequence">
        /// An <see cref="IEnumerable{T}"/> object that references a sequence of
        /// <see cref="MethodInfo"/> objects to check against the signature provided.
        /// </param>
        /// <param name="parameters">
        /// An array of <see cref="Type" /> objects representing the number, order, and type of the 
        /// parameters for the method to get, or <see cref="F:System.Type.EmptyTypes" /> to match a 
        /// empty argument list.
        /// </param>
        /// <param name="returnType">The expected type of the method's return value.</param>
        /// <returns>
        /// A <see cref="MethodInfo"/> object, or null if no matching signature was found within
        /// <paramref name="sequence"/>.
        /// </returns>
        public static MethodInfo WithSignature(this IEnumerable<MethodInfo> sequence, IEnumerable<Type> parameters, Type returnType)
        {
            var found = sequence
                .Where(mi => (mi != null))
                .Where(mi => mi.SignatureEquals(parameters, returnType))
                .FirstOrDefault();
            return found;
        }
    }
}
