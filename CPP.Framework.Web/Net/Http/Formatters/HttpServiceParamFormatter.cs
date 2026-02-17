namespace CPP.Framework.Net.Http.Formatters
{
    /// <summary>
    /// Abstract base class for all objects that are used to format parameter values for the query
    /// string string to an <see cref="HttpServiceClient"/> API call.
    /// </summary>
    public abstract class HttpServiceParamFormatter
    {
        /// <summary>
        /// Formats the value for a query string parameter.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <returns>The formatted value.</returns>
        public abstract string FormatValue(object value);
    }
}
