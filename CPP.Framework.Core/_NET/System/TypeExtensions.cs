using System.Collections.Generic;
using System.Linq;
using CPP.Framework;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// Extension methods for the <see cref="Type"/> class.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Represents a set of simple built-in types that are either not marked as 
        /// <see cref="Type.IsPrimitive"/>, or are not value types.
        /// </summary>
        private static readonly HashSet<Type> ExtendedSimpleTypeSet = new HashSet<Type>
        {
            typeof(string),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(decimal),
            typeof(Guid),
            typeof(object),
            typeof(TimeSpan),
        };

        /// <summary>
        /// Returns a sequence of custom attributes applied to this member and identified by Type.
        /// </summary>
        /// <param name="typeInfo">The type member to search.</param>
        /// <param name="attributeType">The type of the target attribute for which to search.</param>
        /// <param name="inherit">True to include attributes inherited from base types; otherwise, false.</param>
        /// <param name="includeInterfaces">True to include attributes applied to interfaces implemented by the type; otherwise, false.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that can be used to iterate over the attributes.</returns>
        public static IEnumerable<Attribute> GetCustomAttributes(this Type typeInfo, Type attributeType, bool inherit, bool includeInterfaces)
        {
            ArgumentValidator.ValidateThisObj(() => typeInfo);

            // return the attributes applied directly to the member first.
            var iterator = Attribute.GetCustomAttributes(typeInfo, attributeType, inherit);
            foreach (var attribute in iterator) yield return attribute;
            if (!includeInterfaces) yield break;  // if we aren't including interface attributes, we're done

            // now check for the attributes applied to interfaces. we have to do it this way 
            // because unlike C/C++, interfaces are *implemented* in the CLR, not inherited (to 
            // avoid issues that can arise from allowing multiple inheritance).
            IEnumerable<Attribute> GetInterfaceAttributes(Type candidate)
            {
                foreach (var ti in candidate.GetDirectInterfaces())
                {
                    foreach (var attribute in Attribute.GetCustomAttributes(ti, attributeType, inherit))
                    {
                        yield return attribute;
                    }
                    foreach (var baseType in candidate.GetDirectInterfaces())
                    {
                        foreach (var attribute in GetInterfaceAttributes(baseType)) yield return attribute;
                    }
                }
            }
            foreach (var attribute in GetInterfaceAttributes(typeInfo)) yield return attribute;
        }

        /// <summary>
        /// Returns a sequence of custom attributes applied to this member and identified by Type.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="typeInfo">The type member to search.</param>
        /// <param name="inherit">True to include attributes inherited from base types; otherwise, false.</param>
        /// <param name="includeInterfaces">True to include attributes applied to interfaces implemented by the type; otherwise, false.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that can be used to iterate over the attributes.</returns>
        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this Type typeInfo, bool inherit, bool includeInterfaces)
            where TAttribute : Attribute
        {
            return typeInfo.GetCustomAttributes(typeof(TAttribute), inherit, includeInterfaces).OfType<TAttribute>();
        }

        /// <summary>
        /// Returns a list interfaces that are uniquely implemented by a <see cref="Type"/> (i.e.
        /// interfaces that can only be implemented directly by the type, and not because they are
        /// being implemented indirectly because they are the base of another interface).
        /// </summary>
        /// <param name="typeInfo">The type member to search.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that can be used to iterate over the types.</returns>
        public static IEnumerable<Type> GetDirectInterfaces(this Type typeInfo)
        {
            var interfaces = typeInfo.GetInterfaces()
                .Except(typeInfo.BaseType?.GetInterfaces() ?? Enumerable.Empty<Type>())
                .Except(typeInfo.GetInterfaces().SelectMany(ti => ti.GetInterfaces()));
            foreach (var ti in interfaces) yield return ti;
        }

        /// <summary>
        /// Determines whether or not a <see cref="Type"/> object represents a built-in .NET type.
        /// </summary>
        /// <param name="typeInfo">The type object to check.</param>
        /// <returns>True if the type info if for a built-in type; otherwise, false.</returns>
        public static bool IsBuiltInType(this Type typeInfo)
        {
            return ((typeInfo != null) && (typeInfo.IsPrimitive || ExtendedSimpleTypeSet.Contains(typeInfo)));
        }
    }
}
