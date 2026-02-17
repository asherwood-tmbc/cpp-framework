using System.Security.Claims;

namespace CPP.Framework.Security.Policies
{
    /// <summary>
    /// Defines a security policy that authorizes access for a <see cref="ClaimsIdentity"/> based
    /// on the evaluation of two existing <see cref="SecurityAuthorizationPolicy"/> objects.
    /// </summary>
    internal sealed class SecurityAuthorizationPolicyGroup : SecurityAuthorizationPolicy
    {
        private readonly LogicalOperator _operator;
        private readonly SecurityAuthorizationPolicy _policy, _second;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationPolicyGroup"/> class.
        /// </summary>
        /// <param name="policy">The first policy object in the group.</param>
        /// <param name="second">The other policy object in the group.</param>
        /// <param name="operator">
        /// A <see cref="LogicalOperator"/> value that indicates how the results of
        /// <paramref name="policy"/> and <paramref name="second"/> should be combined to generate a
        /// single result.
        /// </param>
        private SecurityAuthorizationPolicyGroup(SecurityAuthorizationPolicy policy, SecurityAuthorizationPolicy second, LogicalOperator @operator)
        {
            _policy = policy;
            _operator = @operator;
            _second = second;
        }

        /// <summary>
        /// Defines the available operators that indicate how the policies in the group should be
        /// locally combined.
        /// </summary>
        private enum LogicalOperator { All, Any, Not, }

        /// <summary>
        /// Creates a new <see cref="SecurityAuthorizationPolicy"/> that is the logical conjunction of two
        /// existing policy objects (i.e. <paramref name="first"/> AND <paramref name="other"/>).
        /// </summary>
        /// <param name="first">The policy object on the left side of the operator.</param>
        /// <param name="other">The policy object on the right side of the operator.</param>
        /// <returns>A <see cref="SecurityAuthorizationPolicy"/> object.</returns>
        internal static SecurityAuthorizationPolicyGroup All(SecurityAuthorizationPolicy first, SecurityAuthorizationPolicy other)
        {
            return new SecurityAuthorizationPolicyGroup(first, other, LogicalOperator.All);
        }

        /// <summary>
        /// Creates a new <see cref="SecurityAuthorizationPolicy"/> that is the logical disjunction of
        /// two existing policy objects (i.e. <paramref name="first"/> OR <paramref name="other"/>).
        /// </summary>
        /// <param name="first">The policy object on the left side of the operator.</param>
        /// <param name="other">The policy object on the right side of the operator.</param>
        /// <returns>A <see cref="SecurityAuthorizationPolicy"/> object.</returns>
        internal static SecurityAuthorizationPolicyGroup Any(SecurityAuthorizationPolicy first, SecurityAuthorizationPolicy other)
        {
            return new SecurityAuthorizationPolicyGroup(first, other, LogicalOperator.Any);
        }

        /// <summary>
        /// Checks whether or not the identity assigned to a principal is authorized to access a
        /// protected resource.
        /// resource.
        /// </summary>
        /// <param name="context">
        /// The <see cref="SecurityAuthorizationContext"/> object for the authorization request.
        /// </param>
        /// <returns><c>True</c> if the identity is authorized; otherwise, <c>false</c>.</returns>
        protected internal override bool CheckAccess(SecurityAuthorizationContext context)
        {
            var authorized = false; // assume failure
            switch (_operator)
            {
                case LogicalOperator.All:
                    {
                        authorized =
                            (_policy?.CheckAccess(context) ?? true) &&
                            (_second?.CheckAccess(context) ?? true);
                    }
                    break;

                case LogicalOperator.Any:
                    {
                        authorized =
                            (_policy?.CheckAccess(context) ?? true) ||
                            (_second?.CheckAccess(context) ?? true);
                    }
                    break;

                case LogicalOperator.Not:
                    {
                        authorized = (!(_policy?.CheckAccess(context) ?? true));
                    }
                    break;
            }
            return authorized;
        }

        /// <summary>
        /// Creates a new <see cref="SecurityAuthorizationPolicy"/> that is the logical negation of an
        /// existing policy (i.e. NOT <paramref name="policy"/>).
        /// </summary>
        /// <param name="policy">The access policy to negate.</param>
        /// <returns>A <see cref="SecurityAuthorizationPolicy"/> object.</returns>
        internal static SecurityAuthorizationPolicyGroup Not(SecurityAuthorizationPolicy policy)
        {
            return new SecurityAuthorizationPolicyGroup(policy, null, LogicalOperator.Not);
        }
    }
}
