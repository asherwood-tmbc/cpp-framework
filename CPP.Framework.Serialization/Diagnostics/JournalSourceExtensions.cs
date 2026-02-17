using System;

using CPP.Framework.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CPP.Framework.Diagnostics
{
    /// <summary>
    /// Extension methods for the <see cref="JournalSource"/> class.
    /// </summary>
    public static class JournalSourceExtensions
    {
        /// <summary>
        /// Writes the property values of a model as telemetry data to a <see cref="JournalSource"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="source">The <see cref="JournalSource"/> to write to.</param>
        /// <param name="model">The model instance to write.</param>
        /// <returns>The <paramref name="source"/> value.</returns>
        public static JournalSource WriteTelemetryModel<TModel>(this JournalSource source, TModel model)
        {
            return WriteTelemetryModel(source, model, -1);
        }

        /// <summary>
        /// Writes the property values of a model as telemetry data to a <see cref="JournalSource"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <param name="source">The <see cref="JournalSource"/> to write to.</param>
        /// <param name="model">The model instance to write.</param>
        /// <param name="maxDepth">
        /// The maximum number of levels to recurse in the objet graph for <paramref name="model"/>.
        /// </param>
        /// <returns>The <paramref name="source"/> value.</returns>
        public static JournalSource WriteTelemetryModel<TModel>(this JournalSource source, TModel model, int maxDepth)
        {
            // ensure that the maximum depth is not set to zero, in order to avoid any confusion 
            // between what the caller expects, versus how the recursion algorithm actually works,
            // which is that zero means to halt recursion and exit.
            if (maxDepth == 0) maxDepth = 1;
            try
            {
                if ((source != null) && (model != null))
                {
                    var settings = new JsonSerializerSettings
                    {
                        ContractResolver = new ConfidentialContractResolver(),
                    };
                    var serializer = JsonSerializer.Create(settings);
                    var jsonObject = JObject.FromObject(model, serializer);
                    var objectName = model.GetType().Name;
                    WalkObjectNodeTree(source, objectName, jsonObject, maxDepth);
                }
            }
            catch (Exception ex)
            {
                // since there is not much we can do in this situation (since we are in the middle
                // of a logging call), all we can do is add a notation to the telemetry values and
                // swallow the error.
                source?.WriteTelemetryValue("JournalSourceExtensions", ex.Message);
            }
            return source;
        }

        /// <summary>
        /// Writes the property values for a node tree as telemetry to a <see cref="JournalSource"/>
        /// object.
        /// </summary>
        /// <param name="source">The <see cref="JournalSource"/> object.</param>
        /// <param name="parentPath">The path to the parent object or property.</param>
        /// <param name="node">The current node in the tree to process.</param>
        /// <param name="maxDepth">The maximum depth in the object graph to traverse.</param>
        private static void WalkObjectNodeTree(JournalSource source, string parentPath, JToken node, int maxDepth)
        {
            switch (node.Type)
            {
                case JTokenType.Object:
                    {
                        // if we've already hit the maximum depth allowed, then stop recursing, but
                        // still log something for the value.
                        if (maxDepth != 0)
                        {
                            if (node is JObject jObject)
                            {
                                foreach (var property in jObject.Properties())
                                {
                                    var childPath = $"{parentPath}-{property.Name}";
                                    WalkObjectNodeTree(source, childPath, property.Value, maxDepth);
                                }
                            }
                        }
                        else source.WriteTelemetryValue(parentPath, "(object)");
                    }
                    break;

                case JTokenType.Array:
                    {
                        var index = 0;
                        foreach (var element in node.Children())
                        {
                            var childPath = $"{parentPath}[{index++}]";
                            WalkObjectNodeTree(source, childPath, element, (maxDepth - 1));
                        }
                    }
                    break;

                default:
                    {
                        var value = node.ToString();
                        source.WriteTelemetryValue(parentPath, value);
                    }
                    break;
            }
        }
    }
}
