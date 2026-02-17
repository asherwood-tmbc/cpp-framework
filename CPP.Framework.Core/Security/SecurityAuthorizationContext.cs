using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Security.Principal;

using CPP.Framework.DependencyInjection;
using CPP.Framework.DependencyInjection.Resolvers;

using JetBrains.Annotations;

namespace CPP.Framework.Security
{
    /// <summary>
    /// Represents the context for a security authorization request.
    /// </summary>
    public class SecurityAuthorizationContext
    {
        private static readonly ClaimsIdentity GenricIdentity = new ClaimsIdentity();

        private readonly object _synclock = new object();
        private ClaimsPrincipal _principal;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationContext"/> class.
        /// </summary>
        /// <param name="manager">
        /// The <see cref="SecurityAuthorizationManager"/> object that is managing the request.
        /// </param>
        /// <param name="principal">The principal being checked.</param>
        [ExcludeFromCodeCoverage]
        [ServiceLocatorConstructor]
        protected internal SecurityAuthorizationContext(SecurityAuthorizationManager manager, IPrincipal principal)
        {
            ArgumentValidator.ValidateNotNull(() => manager);
            if (principal is ClaimsPrincipal claimsPrincipal)
            {
                _principal = claimsPrincipal;
            }
            else if (principal != null)
            {
                _principal = new ClaimsPrincipal(principal);
            }
            this.Manager = manager;
        }

        /// <summary>
        /// Gets the current <see cref="ClaimsIdentity"/> for the authorization request.
        /// </summary>
        [UsedImplicitly]
        public ClaimsIdentity CurrentIdentity => ((ClaimsIdentity)this.CurrentPrincipal.Identity);

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> for the authorization request.
        /// </summary>
        public ClaimsPrincipal CurrentPrincipal => this.GetCurrentPrincipal();

        /// <summary>
        /// Gets the <see cref="SecurityAuthorizationManager"/> object for the authorization
        /// request.
        /// </summary>
        public SecurityAuthorizationManager Manager { get; }

        /// <summary>
        /// Creates a new <see cref="SecurityAuthorizationContext"/> for an authorization request.
        /// </summary>
        /// <returns>A <see cref="SecurityAuthorizationContext"/> object.</returns>
        internal static SecurityAuthorizationContext Create() => Create(null);

        /// <summary>
        /// Creates a new <see cref="SecurityAuthorizationContext"/> for an authorization request.
        /// </summary>
        /// <param name="principal">
        /// The principal to authorize, or null to use the principal assigned to the current
        /// execution context.
        /// </param>
        /// <returns>A <see cref="SecurityAuthorizationContext"/> object.</returns>
        internal static SecurityAuthorizationContext Create(IPrincipal principal)
        {
            // attempt to create an instance of the currently registered implementation of the
            // SecurityAuthorizationContext interface using the default constructor injection, but
            // only if a valid principal was not passed, which requires the caller to have already
            // registered a handler with the ServiceLocator to return the IPrincipal for the
            // current execution context in order to succeed. we are doing things this way mainly
            // to support unit testing, but also to provide compatibility with .NET Core, which no
            // longer sets the value of ClaimsIdentity.Currrent or Thread.CurrentPrincipal anymore.
            if ((principal != null) || (!ServiceLocator.TryGetInstance<SecurityAuthorizationContext>(out var context)))
            {
                // dependency resolution failed, or the caller provided a specific principal to use.
                // regardless, we have to help things along by providing an explicit dependency
                // resolver for the IPrincipal type.
                principal = (principal ?? ClaimsPrincipal.Current);
                var resolvers = new ServiceResolver[]
                {
                    new DependencyResolver<IPrincipal>(principal), 
                };
                context = ServiceLocator.GetInstance<SecurityAuthorizationContext>(resolvers);
            }
            context.Initialize();   // be sure to initialize the new context

            return context;
        }

        /// <summary>
        /// Retrieves the <see cref="IPrincipal"/> for the current execution context as a
        /// <see cref="ClaimsPrincipal"/> object.
        /// </summary>
        /// <returns>A <see cref="ClaimsPrincipal"/> object.</returns>
        [ExcludeFromCodeCoverage]
        private ClaimsPrincipal GetCurrentPrincipal()
        {
            if (_principal == null)
            {
                lock (_synclock)
                {
                    if (_principal == null)
                    {
                        if (ServiceLocator.TryGetInstance<IPrincipal>(out var principal))
                        {
                            if (!(principal is ClaimsPrincipal))
                            {
                                principal = new ClaimsPrincipal(principal);
                            }
                            _principal = ((ClaimsPrincipal)principal);
                        }
                        _principal = (_principal ?? ClaimsPrincipal.Current ?? new ClaimsPrincipal(GenricIdentity));
                    }
                }
            }
            return _principal;
        }

        /// <summary>
        /// Called by the framework when the context is first created so that it can perform any
        /// initialization of custom data. Please note that when this method is called, the
        /// <see cref="CurrentPrincipal"/> and <see cref="CurrentIdentity"/> properties should be
        /// available for use.
        /// </summary>
        protected virtual void Initialize() { } // the default implementation does nothing
    }
}
