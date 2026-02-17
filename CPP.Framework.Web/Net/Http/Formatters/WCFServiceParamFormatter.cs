using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace CPP.Framework.Net.Http.Formatters
{
    /// <summary>
    /// Formats the query string parameters for an <see cref="HttpServiceClient"/> according to the
    /// OData type system specification supported by the WCF Data Services 5.6.4 libraries.
    /// (http://www.odata.org/documentation/odata-version-2-0/overview/#AbstractTypeSystem).
    /// </summary>
    public class WCFServiceParamFormatter : HttpServiceParamFormatter
    {
        /// <summary>
        /// The map of data types to their formats,
        /// </summary>
        private static readonly ReadOnlyDictionary<Type, string> _ParamFormatMap = new ReadOnlyDictionary<Type, string>(new Dictionary<Type, string>
        {
            [typeof(bool)]           = "{0}",
            [typeof(byte)]           = "{0:D}",
            [typeof(DateTime)]       = "datetime'{0:yyyy-mm-ddThh:MM:ss.fffffff}'",
            [typeof(DateTimeOffset)] = "{0:O}",
            [typeof(decimal)]        = "{0:F}M",
            [typeof(double)]         = "{0:E2}d",
            [typeof(Guid)]           = "guid'{0:D}'",
            [typeof(short)]          = "{0}",
            [typeof(int)]            = "{0}",
            [typeof(long)]           = "{0}L",
            [typeof(float)]          = "{0:F}f",
        });

        /// <summary>
        /// Formats the value for a query string parameter.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>The formatted value.</returns>
        public override string FormatValue(object value)
        {
            var format = ((string)null);
            if (ReferenceEquals(null, value))
            {
                format = string.Empty;
            }
            else if (!_ParamFormatMap.TryGetValue(value.GetType(), out format))
            {
                if (value is byte[] bytes)
                {
                    format = "X'{0}'";
                    value = bytes.Aggregate(new StringBuilder(), (s, b) => s.AppendFormat("{0:D}", b)).ToString();
                }
                else
                {
                    format = "'{0}'";
                    value = Uri.EscapeDataString(Convert.ToString(value).Replace("'", @"''"));
                }
                return string.Format(format, value);
            }
            else if ((value != null) && typeof(bool).IsInstanceOfType(value))
            {
                return value.ToString().ToLowerInvariant();
            }
            return string.Format(format, value);
        }
    }
}
