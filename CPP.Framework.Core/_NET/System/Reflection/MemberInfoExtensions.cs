using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CPP.Framework;

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    /// <summary>
    /// Provides extension methods for the system <see cref="MethodInfo"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Gets the value for a property or field for an object.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> for the property or field declaration.</param>
        /// <param name="instance">The object instance for which to retrieve the value (which can be null for static properties).</param>
        /// <returns>The property or field value.</returns>
        public static object GetPropertyValue(this MemberInfo methodInfo, object instance)
        {
            object value = null;
            switch (methodInfo.MemberType)
            {
                case MemberTypes.Field:
                    {
                        value = ((FieldInfo)methodInfo).GetValue(instance);
                    }
                    break;
                case MemberTypes.Property:
                    {
                        value = ((PropertyInfo)methodInfo).GetValue(instance);
                    }
                    break;
                default: throw new ArgumentException(ErrorStrings.InvalidPropertyOrFieldMemberInfo);
            }
            return value;
        }

        /// <summary>
        /// Gets the value for a property or field for an object.
        /// </summary>
        /// <typeparam name="TValue">The expected type of the property or field value.</typeparam>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> for the property or field declaration.</param>
        /// <param name="instance">The object instance for which to retrieve the value (which can be null for static properties).</param>
        /// <returns>The property or field value.</returns>
        public static TValue GetPropertyValue<TValue>(this MemberInfo methodInfo, object instance)
        {
            return ((TValue)methodInfo.GetPropertyValue(instance));
        }

        /// <summary>
        /// Determines whether or not the member has a custom attribute applied.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member to check.</param>
        /// <returns>True if the attribute has been applied; otherwise, false.</returns>
        public static bool HasCustomAttribute<TAttribute>(this MemberInfo memberInfo)
            where TAttribute : Attribute
        {
            return memberInfo.HasCustomAttribute(typeof(TAttribute), false);
        }

        /// <summary>
        /// Determines whether or not the member has a custom attribute applied.
        /// </summary>
        /// <typeparam name="TAttribute">The type of the attribute.</typeparam>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member to check.</param>
        /// <param name="inherit">True to included inherited attributes; otherwise, false.</param>
        /// <returns>True if the attribute has been applied; otherwise, false.</returns>
        public static bool HasCustomAttribute<TAttribute>(this MemberInfo memberInfo, bool inherit)
            where TAttribute : Attribute
        {
            return memberInfo.HasCustomAttribute(typeof(TAttribute), inherit);
        }

        /// <summary>
        /// Determines whether or not the member has a custom attribute applied.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member to check.</param>
        /// <param name="attributeType">The type of the attribute.</param>
        /// <returns>True if the attribute has been applied; otherwise, false.</returns>
        public static bool HasCustomAttribute(this MemberInfo memberInfo, Type attributeType)
        {
            return memberInfo.HasCustomAttribute(attributeType, false);
        }

        /// <summary>
        /// Determines whether or not the member has a custom attribute applied.
        /// </summary>
        /// <param name="memberInfo">The <see cref="MemberInfo"/> of the member to check.</param>
        /// <param name="attributeType">The type of the attribute.</param>
        /// <param name="inherit">True to included inherited attributes; otherwise, false.</param>
        /// <returns>True if the attribute has been applied; otherwise, false.</returns>
        public static bool HasCustomAttribute(this MemberInfo memberInfo, Type attributeType, bool inherit)
        {
            return memberInfo.GetCustomAttributes(attributeType, inherit).Any();
        }

        /// <summary>
        /// Sets the value of a property or field for an object.
        /// </summary>
        /// <typeparam name="TValue">The expected type of the property or field value.</typeparam>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> for the property or field declaration.</param>
        /// <param name="instance">The object instance for which to retrieve the value (which can be null for static properties).</param>
        /// <param name="value">The value to set.</param>
        public static void SetPropertyValue<TValue>(this MemberInfo methodInfo, object instance, TValue value)
        {
            switch (methodInfo.MemberType)
            {
                case MemberTypes.Field:
                    {
                        ((FieldInfo)methodInfo).SetValue(instance, value);
                    }
                    break;
                case MemberTypes.Property:
                    {
                        ((PropertyInfo)methodInfo).SetValue(instance, value);
                    }
                    break;
                default: throw new ArgumentException(ErrorStrings.InvalidPropertyOrFieldMemberInfo);
            }
        }
    }
}
