using System.Security.Claims;

namespace CPP.Framework.Security
{
    /// <summary>
    /// Defines constants for the common claim types that can be assigned to a subject for any code
    /// base or application. This class cannot be inherited.
    /// </summary>
    public class CommonClaimTypes
    {
        /// <summary>
        /// The URI for a claim that specifies a permission granted to an entity.
        /// </summary>
        public const string AccessRight = "http://schemas.cpp.com/ws/2018/08/identity/claims/accessRight";

        /// <summary>
        /// The URI for a claim that specifies the email address of an entity.
        /// </summary>
        public const string Email = ClaimTypes.Email;

        /// <summary>
        /// The URI for a claim that specifies an application feature accessible to an entity.
        /// </summary>
        public const string FeatureName = "http://schemas.cpp.com/ws/2018/08/identity/claims/featureName";

        /// <summary>
        /// The URI for a claim that specifies the name of an entity (i.e. the username).
        /// </summary>
        public const string Name = ClaimTypes.Name;

        /// <summary>
        /// The URI for a claim that specifies the role of an entity.
        /// </summary>
        public const string Role = ClaimTypes.Role;
    }
}
