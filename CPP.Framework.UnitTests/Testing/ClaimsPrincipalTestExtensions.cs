using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using CPP.Framework.DependencyInjection;
using CPP.Framework.Security;
using CPP.Framework.Security.Policies;

namespace CPP.Framework.UnitTests.Testing
{
    [ExcludeFromCodeCoverage]
    internal static class ClaimsPrincipalTestExtensions
    {
        public static ClaimsPrincipal CreatePrincipal(string authenticationType)
        {
            var identity = string.IsNullOrWhiteSpace(authenticationType)
                ? new ClaimsIdentity()
                : new ClaimsIdentity(authenticationType);
            return new ClaimsPrincipal(identity);
        }

        public static ClaimsPrincipal GrantAccessRight(this ClaimsPrincipal principal, string accessRight)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => accessRight);
            return principal.GrantClaim(CommonClaimTypes.AccessRight, accessRight);
        }

        public static ClaimsPrincipal GrantFeatureName(this ClaimsPrincipal principal, string featureName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => featureName);
            return principal.GrantClaim(CommonClaimTypes.FeatureName, featureName);
        }

        public static ClaimsPrincipal GrantClaim(this ClaimsPrincipal principal, string claimType, string value, string valueType = ClaimValueTypes.String)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => value);

            var claim = new Claim(claimType, value, (valueType ?? ClaimValueTypes.String));
            ((ClaimsIdentity)principal.Identity).AddClaim(claim);
            return principal;
        }

        public static ClaimsPrincipal GrantUserName(this ClaimsPrincipal principal, string userName)
        {
            ArgumentValidator.ValidateNotNull(() => principal);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => userName);

            var identity = (ClaimsIdentity)principal.Identity;
            var existing = identity.Claims
                .Where(clm => clm.Type == ClaimTypes.Name)
                .ToList();
            existing.ForEach(clm => identity.TryRemoveClaim(clm));
            identity.AddClaim(new Claim(identity.NameClaimType, userName));
            return principal;
        }

        public static ClaimsIdentity GrantAccessRight(this ClaimsIdentity identity, string accessRight)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => accessRight);
            return identity.Grant(SecurityAccessRightPolicy.Create(accessRight));
        }

        public static ClaimsIdentity RevokeAccessRight(this ClaimsIdentity identity, string accessRight)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => accessRight);
            return identity.Revoke(SecurityAccessRightPolicy.Create(accessRight));
        }
    }
}
