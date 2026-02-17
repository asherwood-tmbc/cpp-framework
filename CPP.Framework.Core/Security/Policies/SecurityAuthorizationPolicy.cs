using System.Security.Claims;

namespace CPP.Framework.Security.Policies
{
    /// <summary>
    /// Abstract base class for all security policy objects that are used to authorize access for a
    /// <see cref="ClaimsIdentity"/> based a specific condition.
    /// </summary>
    public abstract class SecurityAuthorizationPolicy
    {
        /// <summary>
        /// Checks whether or not the identity assigned to a principal is authorized to access a
        /// protected resource.
        /// resource.
        /// </summary>
        /// <param name="context">
        /// The <see cref="SecurityAuthorizationContext"/> object for the authorization request.
        /// </param>
        /// <returns>
        /// <c>True</c> if the identity is authorized as defined by the current security policy
        /// object; otherwise, <c>false</c>.
        /// </returns>
        protected internal abstract bool CheckAccess(SecurityAuthorizationContext context);

        /// <summary>
        /// Creates a new <see cref="SecurityAuthorizationPolicy"/> that is the logical conjunction
        /// of two existing policy objects (i.e. <paramref name="left"/> AND <paramref name="right"/>).
        /// </summary>
        /// <param name="left">The policy object on the left side of the operator.</param>
        /// <param name="right">The policy object on the right side of the operator.</param>
        /// <returns>A <see cref="SecurityAuthorizationPolicy"/> object.</returns>
        public static SecurityAuthorizationPolicy operator &(SecurityAuthorizationPolicy left, SecurityAuthorizationPolicy right)
        {
            return SecurityAuthorizationPolicyGroup.All(left, right);
        }

        /// <summary>
        /// Creates a new <see cref="SecurityAuthorizationPolicy"/> that is the logical disjunction
        /// of two existing policy objects (i.e. <paramref name="left"/> OR <paramref name="right"/>).
        /// </summary>
        /// <param name="left">The policy object on the left side of the operator.</param>
        /// <param name="right">The policy object on the right side of the operator.</param>
        /// <returns>A <see cref="SecurityAuthorizationPolicy"/> object.</returns>
        public static SecurityAuthorizationPolicy operator |(SecurityAuthorizationPolicy left, SecurityAuthorizationPolicy right)
        {
            return SecurityAuthorizationPolicyGroup.Any(left, right);
        }

        /// <summary>
        /// Creates a new <see cref="SecurityAuthorizationPolicy"/> that is the logical negation of
        /// an existing policy (i.e. NOT <paramref name="policy"/>).
        /// </summary>
        /// <param name="policy">The access policy to negate.</param>
        /// <returns>A <see cref="SecurityAuthorizationPolicy"/> object.</returns>
        public static SecurityAuthorizationPolicy operator !(SecurityAuthorizationPolicy policy)
        {
            return SecurityAuthorizationPolicyGroup.Not(policy);
        }
    }
}
