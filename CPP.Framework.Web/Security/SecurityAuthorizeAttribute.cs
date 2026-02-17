////*******************************************************************************************////
// We had to alias the System.Web.Http library in this way in order to disambiguate two versions //
// of the System.Net.Http namespace (specifically the HttpRequestExtensions class), because both //
// exist in System.Net.Http.dll (from the framework) and the System.Web.Http.dll (from the       //
// Microsoft.AspNet.WebApi.Core nuget package). More information about how/why this was done can //
// be found at the following links:                                                              //
//                                                                                               //
//  https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/extern-alias      //
//  https://github.com/NuGet/Home/issues/4989                                                    //
//                                                                                               //
////*******************************************************************************************////
extern alias SystemWebHttp;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Security.Policies;
using CPP.Framework.Threading;

////*******************************************************************************************////
// NOTE : Since this file is trying to define a class that implements parallel features in two   //
// completely different namespaces (in this case, WebAPI and MVC), it is *EXTREMELY* important   //
// when modifying the code to use an explicit alias to any interface, class, or enum name or     //
// errors may creep in because you think you are using something from WebAPI, when really it is  //
// coming from MVC (or vice versa). Fortunately, this is probably the only kind of class where   //
// need to do something like this (fingers crossed). -- R Hoy                                    //
////*******************************************************************************************////

using IWebApiAuthorizationFilter = SystemWebHttp::System.Web.Http.Filters.IAuthorizationFilter;
using IWebApiFilter = SystemWebHttp::System.Web.Http.Filters.IFilter;
using IWebMvcAuthorizationFilter = System.Web.Mvc.IAuthorizationFilter;
using IWebMvcFilter = System.Web.Mvc.IMvcFilter;
using WebApiAllowAnonymousAttribute = SystemWebHttp::System.Web.Http.AllowAnonymousAttribute;
using WebApiHttpActionContext = SystemWebHttp::System.Web.Http.Controllers.HttpActionContext;
using WebApiHttpRequestMessage = System.Net.Http.HttpRequestMessage;
using WebApiHttpRequestMessageExtensions = SystemWebHttp::System.Net.Http.HttpRequestMessageExtensions;
using WebApiHttpResponseMessage = System.Net.Http.HttpResponseMessage;
using WebMvcAllowAnonymousAttribute = System.Web.Mvc.AllowAnonymousAttribute;
using WebMvcAuthorizationContext = System.Web.Mvc.AuthorizationContext;
using WebMvcHttpStatusCodeResult = System.Web.Mvc.HttpStatusCodeResult;
using WebMvcHttpUnauthorizedResult = System.Web.Mvc.HttpUnauthorizedResult;
using WebMvcOutputCacheAttribute = System.Web.Mvc.OutputCacheAttribute;

namespace CPP.Framework.Security
{
    /// <summary>
    /// An authorization filter that verifies the request's <see cref="IPrincipal"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         You can declare multiple of these attributes per action. You can also use
    ///         <see cref="SecurityAllowAnonymousAttribute"/> to disable authorization for a
    ///         specific action.
    ///     </para>
    ///     <para>
    ///         Please note that this class is usable for both MVC and WebAPI Controllers/Actions,
    ///         and a majority of the internal code relies on a similar implementation from both of
    ///         them (quirks and all).
    ///     </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Reviewed. Suppression is OK here.")]
    public sealed class SecurityAuthorizeAttribute : System.Web.Mvc.FilterAttribute, IWebMvcAuthorizationFilter, IWebApiAuthorizationFilter, IWebMvcFilter
    {
        private static readonly SecurityAuthorizationPolicy[] EmptyPolicyList = new SecurityAuthorizationPolicy[0];
        private readonly MultiAccessLock _syncLock = new MultiAccessLock(LockRecursionPolicy.NoRecursion);

        private string _accessRights, _featureNames, _accessRoles;
        private ReadOnlyCollection<SecurityAuthorizationPolicy> _accessRolesClaims = new ReadOnlyCollection<SecurityAuthorizationPolicy>(EmptyPolicyList);
        private ReadOnlyCollection<SecurityAuthorizationPolicy> _accessRightClaims = new ReadOnlyCollection<SecurityAuthorizationPolicy>(EmptyPolicyList);
        private ReadOnlyCollection<SecurityAuthorizationPolicy> _featureNameClaims = new ReadOnlyCollection<SecurityAuthorizationPolicy>(EmptyPolicyList);
        private SecurityAuthorizationPolicy _aggregatedAccessPolicy;

        /// <summary>
        /// Gets or sets a value indicating whether or not to suppress the automatic redirect to
        /// an identity provider on authorization failures. Please note that this property will
        /// only work with the Forms authentication provider, or an authentication provider that
        /// will honor the <see cref="HttpResponse.SuppressFormsAuthenticationRedirect"/> flag.
        /// </summary>
        /// <remarks>The default value of this property is set to <c>true</c>.</remarks>
        public bool AllowAuthRedirect { get; set; } = true;

        /// <summary>
        /// Gets or sets a comma-separated list of internal access right names to validate against
        /// the identity associated with the execution context. Please note that if multiple names
        /// are provided, the security check will succeed if <b>any</b> of the rights have been
        /// granted to the identity. To verify that multiple rights have been granted at the same
        /// time, you will need to use a <see cref="SecurityAuthorizeAttribute"/> for each one.
        /// Also, if values have been set for the <see cref="FeatureNames"/> property, then at
        /// least one of the features in the list must have been enabled as well in order for the
        /// security check to succeed.
        /// </summary>
        public string AccessRights
        {
            get => _accessRights;
            set
            {
                _accessRights = (string.IsNullOrWhiteSpace(value) ? string.Empty : value);
                _accessRightClaims = new ReadOnlyCollection<SecurityAuthorizationPolicy>(GenerateSecurityPolicies(_accessRights, (ar) => SecurityAccessRightPolicy.Create(ar)).ToArray());
            }
        }

        /// <summary>
        /// Gets or sets a comma-separated list of internal security feature names to check against
        /// the identity associated with the execution context. Please note that if multiple names
        /// are provided, the security check will succeed if <b>any</b> of the features are enabled
        /// on the identity. To verify that multiple features have been enabled at the same time,
        /// use a separate <see cref="SecurityAuthorizeAttribute"/> for each feature. Also, if
        /// values have been set for the <see cref="AccessRights"/> property, then at least one of
        /// the rights in that list must have been granted as well in order for the security check
        /// to succeed.
        /// </summary>
        public string FeatureNames
        {
            get => _featureNames;
            set
            {
                _featureNames = (string.IsNullOrWhiteSpace(value) ? string.Empty : value);
                _featureNameClaims = new ReadOnlyCollection<SecurityAuthorizationPolicy>(GenerateSecurityPolicies(_featureNames, (fn) => SecurityFeatureNamePolicy.Create(fn)).ToArray());
            }
        }

        /// <inheritdoc />
        bool IWebMvcFilter.AllowMultiple => ((IWebApiFilter)this).AllowMultiple;

        /// <summary>
        /// Gets or sets a comma-separated list of internal security role names to check against
        /// the identity associated with the execution context. Please note that this property is
        /// only added for backwards compatibility. New code should use <see cref="AccessRights"/>
        /// or <see cref="FeatureNames"/> properties
        /// </summary>
        [Obsolete("Please migrate to using explicit access checks via the AccessRights and FeatureNames properties.")]
        public string Roles
        {
            get => _accessRoles;
            set
            {
                _accessRoles = (string.IsNullOrWhiteSpace(value) ? string.Empty : value);
                _accessRolesClaims = new ReadOnlyCollection<SecurityAuthorizationPolicy>(GenerateSecurityPolicies(_accessRoles, (ar) => SecurityAccessRolesPolicy.Create(ar)).ToArray());
            }
        }

        /// <summary>
        /// Validates the authorization for the user associated with an <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="principal">The <see cref="IPrincipal"/> to validate against.</param>
        /// <exception cref="SecurityAuthenticationException">
        /// <paramref name="principal"/> is not authenticated (i.e. not logged in).
        /// </exception>
        /// <exception cref="SecurityAuthorizationException">
        /// <paramref name="principal"/> is authenticated, but is not authorized.
        /// </exception>
        private void AuthorizeCore(IPrincipal principal)
        {
            if (principal?.Identity?.IsAuthenticated ?? false)
            {
                var policy = this.GetAggregatePolicy();
                if (policy == null) return;
                SecurityAuthorizationPermission.Demand(policy, principal, true);
            }
            else throw new SecurityAuthenticationException();
        }

        /// <summary>
        /// Gets a reference to the <see cref="HttpResponseBase"/> object for the active HTTP
        /// request.
        /// </summary>
        /// <returns>
        /// An <see cref="HttpResponseBase"/> object, or null if the response is not available.
        /// </returns>
        private HttpResponseBase GetActiveResponse()
        {
            if (!ServiceLocator.TryGetInstance<HttpResponseBase>(out var response))
            {
                if (HostingEnvironment.IsHosted && (HttpContext.Current?.Response != null))
                {
                    response = new HttpResponseWrapper(HttpContext.Current.Response);
                }
            }
            return response;
        }
        
        /// <summary>
        /// Aggregates the policies in multiple sets into a single security policy.
        /// </summary>
        /// <returns>A <see cref="SecurityAuthorizationPolicy"/> object.</returns>
        private SecurityAuthorizationPolicy GetAggregatePolicy()
        {
            using (_syncLock.GetReaderAccess())
            {
                if (_aggregatedAccessPolicy != null) return _aggregatedAccessPolicy;
            }
            using (_syncLock.GetWriterAccess())
            {
                if (_aggregatedAccessPolicy == null)
                {
                    var policy = default(SecurityAuthorizationPolicy);
                    var collection = new[]
                    {
                        _accessRightClaims,
                        _featureNameClaims,
                        _accessRolesClaims,
                    };
                    foreach (var policySet in collection.Where(ps => (ps?.Count >= 1)))
                    {
                        var temp = policySet.Aggregate((set, pol) => (set | pol));
                        policy = ((policy == null) ? temp : (policy & temp));
                    }
                    _aggregatedAccessPolicy = policy;
                }
                return _aggregatedAccessPolicy;
            }
        }

        /// <summary>
        /// Updates the status of the flag to redirect the client to the identity provider on an
        /// authorization or authentication failure for a given HTTP request.
        /// </summary>
        /// <param name="response">
        /// The <see cref="HttpResponseBase"/> object for the request, if available. If this value
        /// is null, then the method will attempt to retrieve it from the current HTTP context.
        /// </param>
        private void UpdateAuthRedirectFlag(HttpResponseBase response)
        {
            if (!this.AllowAuthRedirect && (response != null))
            {
                response.SuppressFormsAuthenticationRedirect = true;
            }
        }

        #region WebMvc Authorization Filter Implementation        

        /// <summary>
        /// Generates <see cref="SecurityClaimPolicy"/> objects for each of the name in a comma-
        /// separated list of values assigned to a property.
        /// </summary>
        /// <param name="propertyValue">The value of the property.</param>
        /// <param name="selector">
        /// A delegate that is called for each valid identifier in <paramref name="propertyValue"/>
        /// in order to generate the necessary <see cref="SecurityClaimPolicy"/> object.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to enumerate the results.
        /// </returns>
        private static IEnumerable<SecurityAuthorizationPolicy> GenerateSecurityPolicies(string propertyValue, Func<string, SecurityClaimPolicy> selector)
        {
            if ((propertyValue != null) && (propertyValue.Length >= 1))
            {
                var objects = propertyValue
                    .Split(',')
                    .Select(s => s.Trim())
                    .SkipNullOrWhiteSpace()
                    .Select(s => s.Trim());
                return objects.Select(selector);
            }
            return Enumerable.Empty<SecurityClaimPolicy>();
        }

        /// <summary>
        /// Called when authorization is required.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        void IWebMvcAuthorizationFilter.OnAuthorization(WebMvcAuthorizationContext filterContext)
        {
            ArgumentValidator.ValidateNotNull(() => filterContext);
            if (WebMvcOutputCacheAttribute.IsChildActionCacheActive(filterContext))
            {
            }
            var action = filterContext.ActionDescriptor;

            // check if the controller action has also been marked as anonymous.
            var anonymous = false;
            if ((anonymous = action.IsDefined(typeof(SecurityAllowAnonymousAttribute), true)) == false)
            {
                var controller = action.ControllerDescriptor;
                anonymous = controller.IsDefined(typeof(SecurityAllowAnonymousAttribute), true);
            }
            if (anonymous) return;

            if ((anonymous = action.IsDefined(typeof(WebMvcAllowAnonymousAttribute), true)) == false)
            {
                var controller = action.ControllerDescriptor;
                anonymous = controller.IsDefined(typeof(WebMvcAllowAnonymousAttribute), true);
            }
            if (anonymous) return;

            // otherwise, check the authentication/authorization and add a handler for the cache
            // validation callback.
            try
            {
                this.AuthorizeCore(filterContext.HttpContext?.User);
                var cache = filterContext.HttpContext?.Response.Cache;
                if (cache != null)
                {
                    cache.SetProxyMaxAge(new TimeSpan(0));
                    cache.AddValidationCallback(OnValidateHttpCache, null);
                }
            }
            catch (SecurityAuthenticationException)
            {
                this.UpdateAuthRedirectFlag(filterContext.HttpContext?.Response);
                filterContext.Result = new WebMvcHttpUnauthorizedResult();
            }
            catch (SecurityAuthorizationException)
            {
                this.UpdateAuthRedirectFlag(filterContext.HttpContext?.Response);
                filterContext.Result = new WebMvcHttpStatusCodeResult(HttpStatusCode.Forbidden);
            }
        }

        /// <summary>
        /// Called by the MVC framework to validate whether or not a cached response from a
        /// controller action is still valid.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the request.</param>
        /// <param name="data">
        /// The context data associated with the cache entry (which is always null in our case).
        /// </param>
        /// <param name="validationStatus">
        /// An output parameter that receives the validation status for the cache entry.
        /// </param>
        private void OnValidateHttpCache(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            ArgumentValidator.ValidateNotNull(() => context);
            var wrapper = new HttpContextWrapper(context);

            try
            {
                this.AuthorizeCore(wrapper.User);
                validationStatus = HttpValidationStatus.Valid;
            }
            catch (SecurityException)
            {
                this.UpdateAuthRedirectFlag(wrapper.Response);
                validationStatus = HttpValidationStatus.IgnoreThisRequest;
            }
        }

        #endregion // WebMvc Authorization Filter Implementation

        #region WebApi Authorization Filter Implementation

        /// <summary>
        /// Gets a value indicating whether more than one instance of the indicated attribute can
        /// be specified for a single program element.
        /// </summary>
        /// <returns>
        /// <c>True</c> if more than one instance is allowed to be specified; otherwise,
        /// <c>false</c>. The default is false.
        /// </returns>
        bool IWebApiFilter.AllowMultiple => this.AllowMultiple;

        /// <summary>
        /// Creates an error response message for a request.
        /// </summary>
        /// <param name="request">The request to create the response for.</param>
        /// <param name="statusCode">The error status code of the response.</param>
        /// <param name="message">The error message for the response.</param>
        /// <returns>An <see cref="WebApiHttpResponseMessage"/> object.</returns>
        private static WebApiHttpResponseMessage CreateErrorResponse(WebApiHttpRequestMessage request, HttpStatusCode statusCode, string message)
        {
            return WebApiHttpRequestMessageExtensions.CreateErrorResponse(request, statusCode, message);
        }

        /// <summary>
        /// Executes the authorization filter to synchronize.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="cancellationToken">The cancellation token associated with the filter.</param>
        /// <param name="continuation">The continuation.</param>
        /// <returns>The authorization filter to synchronize.</returns>
        Task<WebApiHttpResponseMessage> IWebApiAuthorizationFilter.ExecuteAuthorizationFilterAsync(
            WebApiHttpActionContext actionContext,
            CancellationToken cancellationToken,
            Func<Task<WebApiHttpResponseMessage>> continuation)
        {
            ArgumentValidator.ValidateNotNull(() => actionContext);
            ArgumentValidator.ValidateNotNull(() => continuation);
            return ExecuteAuthorizationFilterAsyncCore(actionContext, cancellationToken, continuation);
        }

        /// <summary>
        /// Executes the authorization filter to synchronize.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="cancellationToken">The cancellation token associated with the filter.</param>
        /// <param name="continuation">The continuation.</param>
        /// <returns>The authorization filter to synchronize.</returns>
        private async Task<WebApiHttpResponseMessage> ExecuteAuthorizationFilterAsyncCore(
            WebApiHttpActionContext actionContext,
            CancellationToken cancellationToken,
            Func<Task<WebApiHttpResponseMessage>> continuation)
        {
            await OnAuthorizationAsync(actionContext, cancellationToken);
            return (actionContext.Response ?? await continuation());
        }

        /// <summary>
        /// Called when authorization is required.
        /// </summary>
        /// <param name="actionContext">The context for the request to authorize.</param>
        private void OnAuthorization(WebApiHttpActionContext actionContext)
        {
            ArgumentValidator.ValidateNotNull(() => actionContext);
            var action = actionContext.ActionDescriptor;

            // check if the controller action has also been marked as anonymous.
            var anonymous = false;
            if ((anonymous = action.GetCustomAttributes<SecurityAllowAnonymousAttribute>().Any()) == false)
            {
                var controller = actionContext.ControllerContext.ControllerDescriptor;
                anonymous = controller.GetCustomAttributes<SecurityAllowAnonymousAttribute>().Any();
            }
            if (anonymous) return;

            if ((anonymous = action.GetCustomAttributes<WebApiAllowAnonymousAttribute>().Any()) == false)
            {
                var controller = actionContext.ControllerContext.ControllerDescriptor;
                anonymous = controller.GetCustomAttributes<WebApiAllowAnonymousAttribute>().Any();
            }
            if (anonymous) return;

            // otherwise, check the authentication/authorization for the request.
            var request = actionContext.ControllerContext.Request;
            try
            {
                var principal = actionContext.ControllerContext?.RequestContext?.Principal;
                this.AuthorizeCore(principal);
            }
            catch (SecurityAuthenticationException ex)
            {
                var response = this.GetActiveResponse();
                this.UpdateAuthRedirectFlag(response);
                actionContext.Response = CreateErrorResponse(request, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (SecurityAuthorizationException ex)
            {
                var response = this.GetActiveResponse();
                this.UpdateAuthRedirectFlag(response);
                actionContext.Response = CreateErrorResponse(request, HttpStatusCode.Forbidden, ex.Message);
            }
        }

        /// <summary>
        /// Adapter method used to executes the authorization filter in a task-friendly manner.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="cancellationToken">The cancellation token associated with the filter.</param>
        /// <returns>A <see cref="Task"/> object.</returns>
        // ReSharper disable once UnusedParameter.Local
        private Task OnAuthorizationAsync(WebApiHttpActionContext actionContext, CancellationToken cancellationToken)
        {
            try
            {
                OnAuthorization(actionContext);
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }
            return TaskHelpers.Completed();
        }

        #endregion // WebApi Authorization Filter Implementation
    }
}
