using System.Security.Claims;

namespace CPP.Framework.Security.Policies
{
    /// <summary>
    /// Defines a security policy that authorizes access for a <see cref="ClaimsIdentity"/> based
    /// on whether or not a specific account role has been granted.
    /// </summary>
    public class SecurityAccessRolesPolicy : SecurityClaimPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAccessRolesPolicy"/> class.
        /// </summary>
        /// <param name="claim">
        /// The <see cref="Claim"/> required for the authorization policy evaluation to succeed.
        /// </param>
        private SecurityAccessRolesPolicy(Claim claim) : base(claim) { }

        /// <summary>
        /// Creates a new instance of the <see cref="SecurityAccessRightPolicy"/> class with a given
        /// access right name.
        /// </summary>
        /// <param name="accessRight">The internal name of the access right.</param>
        /// <returns>A <see cref="SecurityAccessRightPolicy"/> object.</returns>
        public static SecurityAccessRolesPolicy Create(string accessRight)
        {
            ArgumentValidator.ValidateNotNull(() => accessRight);
            var claim = new Claim(CommonClaimTypes.Role, accessRight, ClaimValueTypes.String);
            return GetOrAddCachedPolicy(claim, (clm) => new SecurityAccessRolesPolicy(clm));
        }
    }
}
