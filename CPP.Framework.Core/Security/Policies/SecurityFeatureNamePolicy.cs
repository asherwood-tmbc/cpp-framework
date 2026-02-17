using System.Security.Claims;

namespace CPP.Framework.Security.Policies
{
    /// <summary>
    /// Defines a security policy that authorizes access for a <see cref="ClaimsIdentity"/> based
    /// on whether or not a specific application feature has been enabled.
    /// </summary>
    public sealed class SecurityFeatureNamePolicy : SecurityClaimPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityFeatureNamePolicy"/> class.
        /// </summary>
        /// <param name="claim">
        /// The <see cref="Claim"/> required for the authorization policy evaluation to succeed.
        /// </param>
        private SecurityFeatureNamePolicy(Claim claim) : base(claim) { }

        /// <summary>
        /// Creates a new instance of the <see cref="SecurityFeatureNamePolicy"/> class with a given
        /// feature name.
        /// </summary>
        /// <param name="featureName">The name of the application feature.</param>
        /// <returns>A <see cref="SecurityFeatureNamePolicy"/> object.</returns>
        public static SecurityFeatureNamePolicy Create(string featureName)
        {
            ArgumentValidator.ValidateNotNull(() => featureName);
            var claim = new Claim(CommonClaimTypes.FeatureName, featureName, ClaimValueTypes.String);
            return GetOrAddCachedPolicy(claim, (clm) => new SecurityFeatureNamePolicy(clm));
        }
    }
}
