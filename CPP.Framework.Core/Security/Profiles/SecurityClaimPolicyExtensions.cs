using System.Security.Claims;

using CPP.Framework.ComponentModel;

// ReSharper disable once CheckNamespace
namespace CPP.Framework.Security.Policies
{
    /// <summary>
    /// Extension methods for the <see cref="SecurityClaimPolicy"/> class.
    /// </summary>
    public static class SecurityClaimPolicyExtensions
    {
        /// <summary>
        ///  Gets the value associated with a <see cref="SecurityClaimPolicy"/> object.
        /// </summary>
        /// <param name="policy">The policy object to query.</param>
        /// <returns>A <see cref="string"/> that contains the value.</returns>
        public static string GetClaimValue(this SecurityClaimPolicy policy)
        {
            var accessor = DynamicPropertyAccessor.GetInstance(policy.GetType());
            return accessor.GetValue<Claim>(policy, "Claim").Value;
        }
    }
}
