using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Microsoft.ServiceBus.Messaging;

namespace CPP.Framework.WindowsAzure.ServiceBus
{
    /// <summary>
    /// Override of the <see cref="MessagePropertyReader"/> for a specific message content
    /// model class.
    /// </summary>
    /// <typeparam name="TContent">The type of the model.</typeparam>
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "Reviewed. Suppression is OK here.")]
    internal sealed class MessagePropertyReader<TContent> : MessagePropertyReader
    {
        private readonly ReadOnlyCollection<MessagePropertyMetadata<TContent>> _metadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagePropertyReader{TContent}"/> class.
        /// </summary>
        /// <param name="metadata">
        /// The property metadata to use for reading the <see cref="BrokeredMessage"/> properties
        /// from the content model.
        /// </param>
        internal MessagePropertyReader(IEnumerable<MessagePropertyMetadata<TContent>> metadata)
        {
            _metadata = new ReadOnlyCollection<MessagePropertyMetadata<TContent>>(metadata.ToList());
        }

        /// <inheritdoc />
        internal override IEnumerable<MessageProperty> GetPropertyValues(object model)
        {
            if (model is TContent instance)
            {
                foreach (var property in _metadata)
                {
                    var value = property.Getter(instance);
                    if (value == null) continue;

                    if (property.Format != null)
                    {
                        var formatted = string.Format(property.Format, value);
                        if (property.LowerCase)
                        {
                            formatted = formatted.ToLowerInvariant();
                        }
                        value = formatted;
                    }
                    var name = property.Name;
                    yield return new MessageProperty(name, value);
                }
            }
        }
    }
}
