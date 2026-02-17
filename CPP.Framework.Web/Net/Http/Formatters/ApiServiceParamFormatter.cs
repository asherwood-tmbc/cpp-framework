using System;

namespace CPP.Framework.Net.Http.Formatters
{
    /// <summary>
    /// Provides generatic formatting for a REST WebAPI call.
    /// </summary>
    public class ApiServiceParamFormatter : HttpServiceParamFormatter
    {
        /// <summary>
        /// Formats the value for a query string parameter.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>The formatted value.</returns>
        public override string FormatValue(object value)
        {
            var converted = default(string);

            switch (value)
            {
                case string str:
                    {
                        converted = (str ?? string.Empty);
                    }
                    break;

                case DateTime date:
                    {
                        converted = date.ToString("O"); // ISO 8601 - 9999-12-31T23:59:59.9999999
                    }
                    break;

                default:
                    {
                        converted = (Convert.ToString(value) ?? string.Empty);
                    }
                    break;
            }

            return Uri.EscapeDataString(converted ?? string.Empty);
        }
    }
}
