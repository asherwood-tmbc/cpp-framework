using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CPP.Framework.DependencyInjection;
using CPP.Framework.Net.Http.Formatters;

using Newtonsoft.Json;

namespace CPP.Framework.Net.Http
{
    /// <summary>
    /// Abstract base class for all objects that access a SiteCore REST API over HTTP using JSON.
    /// </summary>
    public abstract class HttpServiceClient : IDisposable
    {
        /// <summary>
        /// The <see cref="HashSet{T}"/> of URI schemes that are allowed for the service location.
        /// </summary>
        private static readonly HashSet<string> _AllowedSchemeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Uri.UriSchemeHttp,
            Uri.UriSchemeHttps,
        };

        /// <summary>
        /// The <see cref="HttpServiceParamFormatter"/> value to used format the query string
        /// parameters for any GET API calls.
        /// </summary>
        private readonly HttpServiceParamFormatter _formatter;

        /// <summary>
        /// The underlying <see cref="HttpClient"/> used to send and receive the REST calls.
        /// </summary>
        private HttpClient _client;

        /// <summary>
        /// The flag the indicates whether or not the object has been disposed.
        /// </summary>
        private int _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServiceClient"/> class.
        /// </summary>
        /// <param name="serviceUri">The absolute <see cref="Uri"/> location of the service.</param>
        protected HttpServiceClient(Uri serviceUri) : this(serviceUri, new ApiServiceParamFormatter()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServiceClient"/> class.
        /// </summary>
        /// <param name="serviceUri">The absolute <see cref="Uri"/> location of the service.</param>
        /// <param name="formatter">
        /// The <see cref="HttpServiceParamFormatter"/> value to used format the query string
        /// parameters for any GET API calls.
        /// </param>
        protected HttpServiceClient(Uri serviceUri, HttpServiceParamFormatter formatter)
        {
            ArgumentValidator.ValidateNotNull(() => serviceUri);
            ArgumentValidator.ValidateNotNull(() => formatter);

            if (!serviceUri.IsAbsoluteUri)
            {
                throw new ArgumentException(ErrorStrings.InvalidEndPointAddressKind, nameof(serviceUri));
            }
            if (!_AllowedSchemeSet.Contains(serviceUri.Scheme))
            {
                throw new ArgumentException(ErrorStrings.InvalidEndPointAddressScheme, nameof(serviceUri));
            }
            this.BaseServiceUri = new Uri(serviceUri.GetLeftPart(UriPartial.Authority));
            this.FullServiceUri = serviceUri;

            _formatter = formatter;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="HttpServiceClient"/> class. 
        /// </summary>
        ~HttpServiceClient() => this.Dispose(false);

        /// <summary>
        /// Gets the underlying <see cref="HttpClient"/> to use for the REST calls.
        /// </summary>
        protected HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    if (_disposed == 1)
                    {
                        throw new ObjectDisposedException(this.GetType().FullName);
                    }
                    var client = new HttpClient(this.CreateMessageHandler())
                    {
                        BaseAddress = new Uri(this.FullServiceUri.GetLeftPart(UriPartial.Authority)),
                    };
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    _client = client;
                }
                return _client;
            }
        }

        /// <summary>
        /// Gets the scheme, authority, and port segments of the <see cref="FullServiceUri"/>.
        /// </summary>
        protected Uri BaseServiceUri { get; }

        /// <summary>
        /// Gets the absolute <see cref="Uri"/> location of the service.
        /// </summary>
        protected Uri FullServiceUri { get; }

        /// <summary>
        /// Creates an instance of an <see cref="HttpMessageHandler"/> object, which is used to 
        /// manage sending the underlying <see cref="HttpRequestMessage"/> for each service call.
        /// </summary>
        /// <returns>A <see cref="HttpMessageHandler"/> instance.</returns>
        /// <remarks>
        /// Testing code (such as a unit test) can register a custom <see cref="HttpMessageHandler"/>
        /// type with the <see cref="ServiceLocator"/> if the test method needs to break a network
        /// dependency on the HTTP call, as opposed to intercepting <see cref="HttpClient"/> calls.
        /// </remarks>
        protected internal virtual HttpMessageHandler CreateMessageHandler()
        {
            if (!ServiceLocator.TryGetInstance<HttpMessageHandler>(out var handler))
            {
                handler = new HttpClientHandler();
            }
            return handler;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// True if the object is being explicitly disposed; otherwise, false if the object is 
        /// being finalized.
        /// </param>
        private void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                try
                {
                    this.OnDispose(disposing);
                }
                catch
                {
                    /* ignored */
                }

                var client = Interlocked.Exchange(ref _client, null);
                if (disposing)
                {
                    client?.Dispose();
                }
            }
        }

        /// <summary>
        /// Formats an object for use as a query string value.
        /// </summary>
        /// <param name="value">The object to format.</param>
        /// <returns>A string value.</returns>
        protected string FormatQueryStringValue(object value) => _formatter.FormatValue(value);

        /// <summary>
        /// Generates a <see cref="Uri"/> for a service request method.
        /// </summary>
        /// <param name="actionName">The name of the service action method.</param>
        /// <returns>A relative <see cref="Uri"/> value.</returns>
        protected Uri GenerateRequestUri(string actionName)
        {
            return this.GenerateRequestUri(actionName, null);
        }

        /// <summary>
        /// Generates a <see cref="Uri"/> for a service request method.
        /// </summary>
        /// <param name="actionName">The name of the service action method.</param>
        /// <param name="queryParams">
        /// An <see cref="IDictionary{TKey, TValue}"/> of string/object pairs, or an anonymous type
        /// containing property/value pairs that represent the request query string arguments.
        /// </param>
        /// <returns>A relative <see cref="Uri"/> value.</returns>
        protected Uri GenerateRequestUri(string actionName, object queryParams)
        {
            // first, build the path to the service method.
            var path = new StringBuilder(this.FullServiceUri.AbsolutePath);
            if (!string.IsNullOrWhiteSpace(actionName))
            {
                if ((path.Length >= 1) && (path[path.Length - 1] != '/'))
                {
                    path.Append('/');
                }
                path.Append(actionName.Trim());
            }

            // next, format any query string arguments, if provided.
            if (!(queryParams is IDictionary<string, object> dictionary))
            {
                dictionary = new Dictionary<string, object>();
                if (queryParams != null)
                {
                    foreach (var property in TypeDescriptor.GetProperties(queryParams).OfType<PropertyDescriptor>())
                    {
                        var name = property.Name;
                        if (property.Attributes[typeof(JsonIgnoreAttribute)] != null)
                        {
                            continue;
                        }
                        if (property.Attributes[typeof(JsonPropertyAttribute)] is JsonPropertyAttribute attribute)
                        {
                            name = attribute.PropertyName;
                        }
                        dictionary[name] = property.GetValue(queryParams);
                    }
                }
            }

            var queryString = new StringBuilder();
            foreach (var property in dictionary)
            {
                var name = this.FormatQueryStringValue(property.Key);
                var data = this.FormatQueryStringValue(property.Value);

                if (queryString.Length >= 1)
                {
                    queryString.Append('&');
                }
                queryString.Append($"{name}={data}");
            }

            // finally, use a UriBuilder to format the whole thing, and then return the result.
            var builder = new UriBuilder(this.BaseServiceUri)
            {
                Path = path.ToString(),
                Query = queryString.ToString(),
            };
            return new Uri(builder.Uri.PathAndQuery, UriKind.Relative);
        }

        /// <summary>
        /// Called by the base class to perform application-defined tasks associated with freeing, 
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// True if the object is being explicitly disposed; otherwise, false if the object is 
        /// being finalized.
        /// </param>
        protected internal virtual void OnDispose(bool disposing) { /* nothing to do here */ }

        /// <summary>
        /// Sends a request to the service to execute an action method.
        /// </summary>
        /// <param name="method">
        ///     The HTTP method to use for the request.
        /// </param>
        /// <param name="actionArgs">
        ///     An object that contains the additional parameters to pass to the service action method.
        /// </param>
        /// <param name="actionName">The name of the service action method.</param>
        /// <returns>An <see cref="HttpResponseMessage"/> object that contains the response.</returns>
        protected HttpResponseMessage Send(HttpMethod method, object actionArgs, [CallerMemberName] string actionName = null)
        {
            return Task.Run(async () => await this.SendAsync(method, actionArgs, actionName)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a request to the service to execute an action method, and unwraps the response as
        /// as JSON model.
        /// </summary>
        /// <typeparam name="TResult">The type of the response result.</typeparam>
        /// <param name="method">
        ///     The HTTP method to use for the request.
        /// </param>
        /// <param name="actionArgs">
        ///     An object that contains the additional parameters to pass to the service action method.
        /// </param>
        /// <param name="actionName">The name of the service action method.</param>
        /// <returns>The result model deserialized from the response.</returns>
        protected TResult Send<TResult>(HttpMethod method, object actionArgs, [CallerMemberName] string actionName = null)
            where TResult : class, new()
        {
            return Task.Run(async () => await this.SendAsync<TResult>(method, actionArgs, actionName)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a request to the service to execute an action method.
        /// </summary>
        /// <param name="method">
        ///     The HTTP method to use for the request.
        /// </param>
        /// <param name="actionArgs">
        ///     An object that contains the additional parameters to pass to the service action method.
        /// </param>
        /// <param name="actionName">The name of the service action method.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        protected async Task<HttpResponseMessage> SendAsync(HttpMethod method, object actionArgs, [CallerMemberName] string actionName = null)
        {
            return await this.SendAsync(method, actionArgs, CancellationToken.None, actionName);
        }

        /// <summary>
        /// Sends a request to the service to execute an action method.
        /// </summary>
        /// <param name="method">
        ///     The HTTP method to use for the request.
        /// </param>
        /// <param name="actionArgs">
        ///     An object that contains the additional parameters to pass to the service action method.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of 
        /// cancellation.
        /// </param>
        /// <param name="actionName">The name of the service action method.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        protected async Task<HttpResponseMessage> SendAsync(HttpMethod method, object actionArgs, CancellationToken cancellationToken, [CallerMemberName] string actionName = null)
        {
            ArgumentValidator.ValidateNotNull(() => actionName);
            if (method == HttpMethod.Get)
            {
                var requestUri = this.GenerateRequestUri(actionName, actionArgs);
                return await this.Client.GetAsync(requestUri, cancellationToken);
            }
            if (method == HttpMethod.Post)
            {
                var requestUri = this.GenerateRequestUri(actionName);
                return await this.Client.PostAsJsonAsync(requestUri, actionArgs, cancellationToken);
            }
            if (method == HttpMethod.Put)
            {
                var requestUri = this.GenerateRequestUri(actionName);
                return await this.Client.PutAsJsonAsync(requestUri, actionArgs, cancellationToken);
            }
            if (method == HttpMethod.Delete)
            {
                var requestUri = this.GenerateRequestUri(actionName, actionArgs);
                return await this.Client.DeleteAsync(requestUri, cancellationToken);
            }
            throw new ArgumentException(ErrorStrings.InvalidHttpRequestMethod, nameof(method));
        }

        /// <summary>
        /// Sends a request to the service to execute an action method.
        /// </summary>
        /// <typeparam name="TResult">The type of the result model.</typeparam>
        /// <param name="method">
        ///     The HTTP method to use for the request.
        /// </param>
        /// <param name="actionArgs">
        ///     An object that contains the additional parameters to pass to the service action method.
        /// </param>
        /// <param name="actionName">The name of the service action method.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        protected async Task<TResult> SendAsync<TResult>(HttpMethod method, object actionArgs, [CallerMemberName] string actionName = null)
            where TResult : class, new()
        {
            return await this.SendAsync<TResult>(method, actionArgs, CancellationToken.None, actionName);
        }

        /// <summary>
        /// Sends a request to the service to execute an action method.
        /// </summary>
        /// <typeparam name="TResult">The type of the result model.</typeparam>
        /// <param name="method">
        ///     The HTTP method to use for the request.
        /// </param>
        /// <param name="actionArgs">
        ///     An object that contains the additional parameters to pass to the service action method.
        /// </param>
        /// <param name="cancellationToken">
        /// A cancellation token that can be used by other objects or threads to receive notice of 
        /// cancellation.
        /// </param>
        /// <param name="actionName">The name of the service action method.</param>
        /// <returns>A <see cref="Task"/> object representing the asynchronous operation.</returns>
        protected async Task<TResult> SendAsync<TResult>(HttpMethod method, object actionArgs, CancellationToken cancellationToken, [CallerMemberName] string actionName = null)
            where TResult : class, new()
        {
            using (var response = await this.SendAsync(method, actionArgs, cancellationToken, actionName))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<TResult>(cancellationToken);
            }
        }
    }
}
