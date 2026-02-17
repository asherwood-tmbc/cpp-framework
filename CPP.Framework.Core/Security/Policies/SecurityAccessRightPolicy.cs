using System.Security.Claims;

namespace CPP.Framework.Security.Policies
{
    /// <summary>
    /// Defines a security policy that authorizes access for a <see cref="ClaimsIdentity"/> based
    /// on whether or not a specific access right has been granted.
    /// </summary>
    public sealed class SecurityAccessRightPolicy : SecurityClaimPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAccessRightPolicy"/> class.
        /// </summary>
        /// <param name="claim">
        /// The <see cref="Claim"/> required for the authorization policy evaluation to succeed.
        /// </param>
        private SecurityAccessRightPolicy(Claim claim) : base(claim) { }

        /// <summary>
        /// Creates a new instance of the <see cref="SecurityAccessRightPolicy"/> class with a given
        /// access right name.
        /// </summary>
        /// <param name="accessRight">The internal name of the access right.</param>
        /// <returns>A <see cref="SecurityAccessRightPolicy"/> object.</returns>
        public static SecurityAccessRightPolicy Create(string accessRight)
        {
            ArgumentValidator.ValidateNotNull(() => accessRight);
            var claim = new Claim(CommonClaimTypes.AccessRight, accessRight, ClaimValueTypes.String);
            return GetOrAddCachedPolicy(claim, (clm) => new SecurityAccessRightPolicy(clm));
        }
    }
}
