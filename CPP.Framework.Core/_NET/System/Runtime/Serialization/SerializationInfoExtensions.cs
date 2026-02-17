using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using CPP.Framework;

// ReSharper disable once CheckNamespace
namespace System.Runtime.Serialization
{
    /// <summary>
    /// Provides extension methods for the system <see cref="ISerializable"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SerializationInfoExtensions
    {
        /// <summary>
        /// Loads a property value from a serialization stream.
        /// </summary>
        /// <typeparam name="TObject">The type of the serializable object.</typeparam>
        /// <typeparam name="TTarget">The type of the property value.</typeparam>
        /// <param name="info">The <see cref="SerializationInfo"/> that contains the serialized values for <paramref name="source"/>.</param>
        /// <param name="source">The object being deserialized.</param>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being loaded.</param>
        /// <returns>True if the property value was loaded successfully; otherwise, false if the default was used.</returns>
        public static bool LoadProperty<TObject, TTarget>(this SerializationInfo info, TObject source, Expression<Func<TObject, TTarget>> expression)
            where TObject : class, ISerializable
        {
            return info.LoadProperty(source, expression, default(TTarget));
        }

        /// <summary>
        /// Loads a property value from a serialization stream.
        /// </summary>
        /// <typeparam name="TObject">The type of the serializable object.</typeparam>
        /// <typeparam name="TTarget">The type of the property value.</typeparam>
        /// <param name="info">The <see cref="SerializationInfo"/> that contains the serialized values for <paramref name="source"/>.</param>
        /// <param name="source">The object being deserialized.</param>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being loaded.</param>
        /// <param name="defaultValue">The default value to use if the property value is not found in the stream.</param>
        /// <returns>True if the property value was loaded successfully; otherwise, false if the default was used.</returns>
        public static bool LoadProperty<TObject, TTarget>(this SerializationInfo info, TObject source, Expression<Func<TObject, TTarget>> expression, TTarget defaultValue)
            where TObject : class, ISerializable
        {
            ArgumentValidator.ValidateThisObj(() => info);
            ArgumentValidator.ValidateNotNull(() => source);
            ArgumentValidator.ValidateNotNull(() => expression);

            var member = expression.GetMemberInfo();
            var result = false; // assume failure
            var value = defaultValue;
            try
            {
                value = ((TTarget)info.GetValue(member.Name, typeof(TTarget)));
                result = true;
            }
            catch (SerializationException)
            {
                return false; // property value not found
            }

            member.SetPropertyValue(source, value);
            return result;
        }

        /// <summary>
        /// Saves a property value to a serialization stream.
        /// </summary>
        /// <typeparam name="TObject">The type of the serializable object.</typeparam>
        /// <typeparam name="TTarget">The type of the property value.</typeparam>
        /// <param name="info">The <see cref="SerializationInfo"/> that contains the serialized values for <paramref name="source"/>.</param>
        /// <param name="source">The object being serialized.</param>
        /// <param name="expression">An <see cref="Expression{TDelegate}"/> that evaluates to the property being saved.</param>
        public static void SaveProperty<TObject, TTarget>(this SerializationInfo info, TObject source, Expression<Func<TObject, TTarget>> expression)
            where TObject : class, ISerializable
        {
            ArgumentValidator.ValidateThisObj(() => info);
            ArgumentValidator.ValidateNotNull(() => source);
            ArgumentValidator.ValidateNotNull(() => expression);
            var member = expression.GetMemberInfo();
            info.AddValue(member.Name, expression.Compile()(source));
        }
    }
}
