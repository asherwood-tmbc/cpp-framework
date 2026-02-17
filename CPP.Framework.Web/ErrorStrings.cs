namespace CPP.Framework
{
    /// <summary>
    /// Provides the message text for exceptions thrown by the library.
    /// </summary>
    internal static class ErrorStrings
    {
        /// <summary>
        /// Order must be greater than or equal to -1.
        /// </summary>
        internal const string FilterAttributeOrderOutOfRange = "Order must be greater than or equal to -1.";

        /// <summary>
        /// The address value must be an absolute URI.
        /// </summary>
        internal const string InvalidEndPointAddressKind = "The endpoint address must be an absolute URI.";

        /// <summary>
        /// The URI scheme for the endpoint address is invlaid, or is not supported.
        /// </summary>
        internal const string InvalidEndPointAddressScheme = "The URI scheme for the endpoint address is invlaid, or is not supported.";

        /// <summary>
        /// The HTTP method provided is not supported for the request.
        /// </summary>
        internal const string InvalidHttpRequestMethod = "The HTTP method provided is not valid for the service request.";
    }
}
