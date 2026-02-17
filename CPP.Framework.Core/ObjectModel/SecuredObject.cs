using System;
using System.Security.Principal;
using System.Threading;

using CPP.Framework.Security;
using CPP.Framework.Security.Policies;

namespace CPP.Framework.ObjectModel
{
    /// <summary>
    /// Abstract base class for all objects that can be secured at runtime using a custom access
    /// policy.
    /// </summary>
    public abstract class SecuredObject : ISupportsObjectAccessPolicy
    {
        /// <inheritdoc />
        SecurityAuthorizationPolicy ISupportsObjectAccessPolicy.GetAccessPolicy(SecurityAuthorizationContext context) => this.GetAccessPolicy(context);

        /// <summary>
        /// Retrieves the access policy for the current instance.
        /// </summary>
        /// <param name="context">The context for the access check request.</param>
        /// <returns>An <see cref="SecurityAuthorizationPolicy"/> object.</returns>
        /// <exception cref="NotImplementedException">
        /// The current object has not defined any custom access policies.
        /// </exception>
        protected internal virtual SecurityAuthorizationPolicy GetAccessPolicy(SecurityAuthorizationContext context) => null;
    }
}
