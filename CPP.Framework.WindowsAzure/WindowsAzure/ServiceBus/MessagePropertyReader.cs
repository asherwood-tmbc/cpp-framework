using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

using CPP.Framework.Diagnostics;

using Microsoft.ServiceBus.Messaging;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    /// <summary>
    /// Helper class used to read the <see cref="BrokeredMessage"/> properties for message
    /// content model class.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    internal abstract class MessagePropertyReader
    {
        private const BindingFlags DefaultBindingFlags = (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        private static readonly ConcurrentDictionary<Type, MessagePropertyReader> _CachedModelMap = new ConcurrentDictionary<Type, MessagePropertyReader>();
        private static readonly Regex _PropertyFormatRegex = new Regex(@"^(?:{([\w_]+)})$", RegexOptions.Compiled);

        /// <summary>
        /// Gets the <see cref="MessagePropertyReader"/> for a given message content model class.
        /// </summary>
        /// <typeparam name="TContent">The type of the model.</typeparam>
        /// <returns>A <see cref="MessagePropertyReader"/> object.</returns>
        internal static MessagePropertyReader GetPropertyReader<TContent>()
        {
            var reader = _CachedModelMap.GetOrAdd(
                (typeof(TContent)),
                (type) => new MessagePropertyReader<TContent>(GetPropertyMetadata<TContent>()));
            return reader;
        }

        /// <summary>
        /// Generates a cached delegate for a propert defined by a message content model class.
        /// </summary>
        /// <typeparam name="TContent">The type of the message content model.</typeparam>
        /// <param name="propertyInfo">The property for which to generate a delegate.</param>
        /// <returns>A <see cref="Func{T, TResult}"/> delegate.</returns>
        protected static Func<TContent, object> GeneratePropertyDelegate<TContent>(PropertyInfo propertyInfo)
        {
            // (obj) => (object)obj./Property/
            var inputParam = Expression.Parameter(typeof(TContent), "obj");
            var queryValue = Expression.Property(inputParam, propertyInfo);
            var lambdaBody = Expression.Convert(queryValue, typeof(object));
            return Expression.Lambda<Func<TContent, object>>(lambdaBody, inputParam).Compile();
        }

        /// <summary>
        /// Gets the property metadata for a message content model.
        /// </summary>
        /// <typeparam name="TContent">The type of the model.</typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> object that contains the metadata.</returns>
        protected static IEnumerable<MessagePropertyMetadata<TContent>> GetPropertyMetadata<TContent>()
        {
            var sequence = typeof(TContent)
                .GetCustomAttributes(typeof(AzureMessagePropertyAttribute), true)
                .OfType<AzureMessagePropertyAttribute>();
            foreach (var attribute in sequence)
            {
                var match = _PropertyFormatRegex.Match(attribute.Value);
                var name = attribute.Name;

                if (match.Success)
                {
                    // only value types and string are supported (but not structs)
                    var propertyInfo = typeof(TContent).GetProperty(match.Groups[1].Value, DefaultBindingFlags);
                    var propertyType = propertyInfo?.PropertyType;
                    if ((propertyType != null) && ((propertyType.IsPrimitive) || (propertyType == typeof(string)) || (attribute.FormatString != null)))
                    {
                        yield return MessagePropertyMetadata<TContent>.Create(name, GeneratePropertyDelegate<TContent>(propertyInfo), attribute.FormatString, attribute.LowerCase);
                        continue;
                    }
                    if (propertyType != null)
                    {
                        var message = string.Format(
                            ErrorStrings.InvalidBrokeredMessagePropertyType,
                            typeof(TContent).FullName,
                            attribute.Name);
                        Journal.CreateSource<MessagePropertyReader>().WriteWarning(message);
                        continue;
                    }
                }
                yield return MessagePropertyMetadata<TContent>.Create(name, ((obj) => attribute.Value), null, false);
            }
        }

        /// <summary>
        /// Reads the <see cref="BrokeredMessage"/> properties from a message content model.
        /// </summary>
        /// <param name="content">The content class for the message model.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> object that contains the properties.</returns>
        internal abstract IEnumerable<MessageProperty> GetPropertyValues(object content);
    }
}
