using System;
using System.Security.Claims;
using System.Security.Principal;

using CPP.Framework.DependencyInjection;
using CPP.Framework.Services;

namespace CPP.Framework.Security
{
    /// <summary>
    /// Manages access permission checks for one or more secured resources.
    /// </summary>
    [AutoRegisterService]
    public class SecurityAuthorizationManager : CodeServiceSingleton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationManager"/> class.
        /// </summary>
        [ServiceLocatorConstructor]
        protected SecurityAuthorizationManager() { }

        /// <summary>
        /// Checks whether or not the identity assigned to an <see cref="IPrincipal"/> has been
        /// assigned a given claim.
        /// </summary>
        /// <param name="target">
        /// The claim to search for. If this value is null, no exception will be thrown, and the
        /// authorization request will simply return success. However, if the value of the claim
        /// is an empty string, the method will check for a matching claim of the same type, but
        /// does not try to match the value.
        /// </param>
        /// <param name="context">
        /// The <see cref="SecurityAuthorizationContext"/> object for the authorization request.
        /// </param>
        /// <returns><c>True</c> if the principal is authorized; otherwise, <c>false</c>.</returns>
        /// <remarks>
        ///     <para>
        ///         The default implementation only verifies that the identity has been
        ///         authenticated (if required by the context), and that <paramref name="target"/>
        ///         exists in the claims list (if the value is not <see langword="null" />). If the
        ///         application requires more extensive validation, then you will need to define a
        ///         class derived from <see cref="SecurityAuthorizationContext"/>, as in the
        ///         example below.
        ///     </para>
        ///     <code>
        ///         public class MyAuthorizationManager : SecurityAuthorizationManager
        ///         {
        ///             public override bool CheckAccess(string accessRight, SecurityAuthorizationContext context)
        ///             {
        ///                 if (base.CheckAccess(accessRight, context))
        ///                 {
        ///                     // TODO : Perform extra validation here.
        ///                 }
        ///                 return false;
        ///             }
        ///         }
        ///     </code>
        ///     <para>
        ///         You can then register it with the <see cref="CodeServiceProvider"/> in your
        ///         application startup using the following code:
        ///     </para>
        ///     <code>
        ///         CodeServiceProvider.Register&lt;SecurityAuthorizationManager, MyAuthorizationManager&gt;();
        ///     </code>
        /// </remarks>
        protected internal virtual bool CheckAccess(Claim target, SecurityAuthorizationContext context)
        {
            var exists = context.CurrentIdentity.HasClaim(
                (claim) =>
                    {
                        var match = string.Equals(claim.Type, target.Type, StringComparison.OrdinalIgnoreCase);
                        if (match && (!string.IsNullOrWhiteSpace(target.Value)))
                        {
                            match = string.Equals(claim.Value, target.Value, StringComparison.Ordinal);
                        }
                        return match;
                    });
            return exists;
        }
    }
}
