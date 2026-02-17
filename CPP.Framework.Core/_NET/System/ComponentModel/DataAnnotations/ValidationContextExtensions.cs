using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;

using CPP.Framework;
using CPP.Framework.ObjectModel.Validation.Resources;

using FieldInfo = System.Reflection.FieldInfo;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Extension methods for the <see cref="ValidationContext"/> class.
    /// </summary>
    public static class ValidationContextExtensions
    {
        private const BindingFlags DefaultBindingFlags = (BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// Attempts to convert the input value to the type of the member associated with a
        /// <see cref="ValidationContext"/>.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ValidationContext"/> instance that provides context about the validation
        /// operation, such as the object and member being validated.
        /// </param>
        /// <param name="value">The value to convert.</param>
        /// <param name="sourceType">The source type of <paramref name="value"/>.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="InvalidCastException">
        /// <paramref name="value"/> cannot be converted to the destination type.
        /// </exception>
        public static object ConvertValue(this ValidationContext context, object value, Type sourceType)
        {
            ArgumentValidator.ValidateThisObj(() => context);
            return ConvertValue(context, context.MemberName, value, sourceType);
        }

        /// <summary>
        /// Attempts to convert the input value to the type of member defined by the object
        /// associated with a <see cref="ValidationContext"/>.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ValidationContext"/> instance that provides context about the validation
        /// operation, such as the object and member being validated.
        /// </param>
        /// <param name="memberName">The name of the property or field.</param>
        /// <param name="value">The value to convert.</param>
        /// <param name="sourceType">The source type of <paramref name="value"/>.</param>
        /// <returns>The converted value.</returns>
        /// <exception cref="InvalidCastException">
        /// <paramref name="value"/> cannot be converted to the destination type.
        /// </exception>
        public static object ConvertValue(this ValidationContext context, string memberName, object value, Type sourceType)
        {
            ArgumentValidator.ValidateThisObj(() => context);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => memberName);
            ArgumentValidator.ValidateNotNull(() => sourceType);

            var converted = default(object);
            var destinationType = context.GetMemberType(memberName);
            if (sourceType == destinationType) return value;

            try
            {
                var converter = TypeDescriptor.GetConverter(destinationType);
                if (value == null)
                {
                    if (destinationType.IsValueType)
                    {
                        throw new InvalidCastException();
                    }
                    if (!converter.CanConvertFrom(sourceType))
                    {
                        throw new InvalidCastException();
                    }
                    converted = null;
                }
                else
                {
                    if (converter.CanConvertFrom(value.GetType()))
                    {
                        converted = converter.ConvertFrom(value);
                    }
                    else if (value is IConvertible convertable)
                    {
                        converted = convertable.ToType(destinationType, CultureInfo.GetCultureInfo(1033));
                    }
                    else throw new InvalidCastException();
                }
            }
            catch (InvalidCastException)
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    ValidationResources.ValidationContext_Invalid_Destination_Type,
                    destinationType.FullName,
                    sourceType.FullName);
                throw new InvalidOperationException(message);
            }
            return converted;
        }

        /// <summary>
        /// Retrieve the <see cref="Type"/> of the property or field value from an object
        /// associated to the <see cref="ValidationContext"/>.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ValidationContext"/> instance that provides context about the validation
        /// operation, such as the object and member being validated.
        /// </param>
        /// <returns>A <see cref="Type"/> object, or null.</returns>
        public static Type GetMemberType(this ValidationContext context)
        {
            return GetMemberType(context, context.MemberName);
        }

        /// <summary>
        /// Retrieve the <see cref="Type"/> of the property or field value from an object
        /// associated to the <see cref="ValidationContext"/>.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ValidationContext"/> instance that provides context about the validation
        /// operation, such as the object and member being validated.
        /// </param>
        /// <param name="memberName">The name of the property or field.</param>
        /// <returns>A <see cref="Type"/> object, or null.</returns>
        public static Type GetMemberType(this ValidationContext context, string memberName)
        {
            switch (GetTargetMemberInfo(context, memberName))
            {
                case FieldInfo field: return field.FieldType;
                case PropertyInfo property: return property.PropertyType;
                case MethodInfo method: return method.ReturnType;
            }
            throw new InvalidOperationException();  // we shouldn't ever get here, but just in case...
        }

        /// <summary>
        /// Gets the <see cref="MethodInfo"/> for a target member on the object instance that also
        /// satisfies the requirements for use with validation.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ValidationContext"/> instance that provides context about the validation
        /// operation, such as the object and member being validated.
        /// </param>
        /// <param name="memberName">The name of the property or field.</param>
        /// <returns>A <see cref="MemberInfo"/> object, or null.</returns>
        private static MemberInfo GetTargetMemberInfo(ValidationContext context, string memberName)
        {
            ArgumentValidator.ValidateNotNull(() => context);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => memberName);

            var errorFormatString = default(string);
            foreach (var mi in context.ObjectType.GetMember(memberName, DefaultBindingFlags))
            {
                switch (mi)
                {
                    case FieldInfo _: goto default;
                    case PropertyInfo property:
                        {
                            if (!property.CanRead)
                            {
                                // a matching property that can't be read will always take
                                // presidence over other any other error types.
                                errorFormatString = ValidationResources.ValidationContext_Member_Is_WriteOnly;
                                break;
                            }
                        }
                        goto default;
                    case MethodInfo method:
                        {
                            if (method.IsGenericMethod || method.GetParameters().Any() || (method.ReturnType == typeof(void)))
                            {
                                // if this is the first invalid member we've encountered, then save
                                // the error message in case we need to throw an exception later.
                                if (errorFormatString == default(string))
                                {
                                    errorFormatString = ValidationResources.ValidationContext_Member_Type_Is_Invalid;
                                }
                                break;
                            }
                        }
                        goto default;
                    default: return mi;
                }
            }
            errorFormatString = (errorFormatString ?? ValidationResources.ValidationContext_Member_Is_Missing);

            // if we get to this point, then an appropriate member with the required signature
            // could not be found on the object, so throw an exception.
            var message = string.Format(
                CultureInfo.CurrentCulture,
                errorFormatString,
                memberName,
                context.ObjectType.FullName);
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Attempts to retrieve the value of the property or field from an object associated to
        /// the <see cref="ValidationContext"/>.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ValidationContext"/> instance that provides context about the validation
        /// operation, such as the object and member being validated.
        /// </param>
        /// <returns>The value of the member.</returns>
        public static object GetMemberValue(this ValidationContext context)
        {
            ArgumentValidator.ValidateThisObj(() => context);
            return GetMemberValue(context, context.MemberName);
        }

        /// <summary>
        /// Attempts to retrieve the value of the property or field from an object associated to
        /// the <see cref="ValidationContext"/>.
        /// </summary>
        /// <param name="context">
        /// A <see cref="ValidationContext"/> instance that provides context about the validation
        /// operation, such as the object and member being validated.
        /// </param>
        /// <param name="memberName">The name of the property or field.</param>
        /// <returns>The value of <paramref name="memberName"/>.</returns>
        public static object GetMemberValue(this ValidationContext context, string memberName)
        {
            ArgumentValidator.ValidateNotNull(() => context);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => memberName);

            var value = default(object);
            switch (GetTargetMemberInfo(context, memberName))
            {
                case FieldInfo field:
                    {
                        value = field.GetValue(context.ObjectInstance);
                    }
                    break;
                case MethodInfo method:
                    {
                        value = method.Invoke(context.ObjectInstance, new object[0]);
                    }
                    break;
                case PropertyInfo property:
                    {
                        value = property.GetValue(context.ObjectInstance);
                    }
                    break;
            }
            return value;
        }
    }
}
