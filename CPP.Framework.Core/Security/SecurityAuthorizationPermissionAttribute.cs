using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;

using CPP.Framework.Security.Policies;

namespace CPP.Framework.Security
{
    /// <summary>
    /// Applied to a constructor, field, property, or method in order to secure access at runtime
    /// to a protected resource.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class SecurityAuthorizationPermissionAttribute : CodeAccessSecurityAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationPermissionAttribute"/>
        /// class with the specified <see cref="SecurityAction" />.
        /// </summary>
        /// <param name="action">One of the <see cref="SecurityAction" /> values. </param>
        public SecurityAuthorizationPermissionAttribute(SecurityAction action = SecurityAction.Demand) : base(action) { }

        /// <summary>
        /// Gets or sets a comma-separated list of internal access right names to validate against
        /// the identity associated with the execution context. Please note that if multiple names
        /// are provided, the security check will succeed if <b>any</b> of the rights have been
        /// granted to the identity. To verify that multiple rights have been granted at the same
        /// time, you will need to use a <see cref="SecurityAuthorizationPermissionAttribute"/> for
        /// each one. Also, if values have been set for the <see cref="FeatureNames"/> property,
        /// then at least one of the features in the list must have been enabled as well in order
        /// for the security check to succeed.
        /// </summary>
        public string AccessRights { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of internal security feature names to check against
        /// the identity associated with the execution context. Please note that if multiple names
        /// are provided, the security check will succeed if <b>any</b> of the features are enabled
        /// on the identity. To verify that multiple features have been enabled at the same time,
        /// use a separate <see cref="SecurityAuthorizationPermissionAttribute"/> for each feature.
        /// Also, if values have been set for the <see cref="AccessRights"/> property, then at
        /// least one of the rights in that list must have been granted as well in order for the
        /// security check to succeed.
        /// </summary>
        public string FeatureNames { get; set; }

        /// <summary>
        /// Creates a permission object that can then be serialized into binary form and
        /// persistently stored along with the <see cref="SecurityAction" /> in an assembly's
        /// metadata.
        /// </summary>
        /// <returns>A serializable permission object.</returns>
        public override IPermission CreatePermission()
        {
            var policies = Enumerable.Empty<SecurityClaimPolicy>()
                .Concat(GenerateSecurityPolicies(this.FeatureNames, SecurityFeatureNamePolicy.Create))
                .Concat(GenerateSecurityPolicies(this.AccessRights, SecurityAccessRightPolicy.Create));
            return new SecurityAuthorizationPermission(policies);
        }

        /// <summary>
        /// Generates <see cref="SecurityClaimPolicy"/> objects for each of the name in a comma-
        /// separated list of values assigned to a property.
        /// </summary>
        /// <param name="propertyValue">The value of the property.</param>
        /// <param name="selector">
        /// A delegate that is called for each valid identifier in <paramref name="propertyValue"/>
        /// in order to generate the necessary <see cref="SecurityClaimPolicy"/> object.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to enumerate the results.
        /// </returns>
        private static IEnumerable<SecurityClaimPolicy> GenerateSecurityPolicies(string propertyValue, Func<string, SecurityClaimPolicy> selector)
        {
            if ((propertyValue != null) && (propertyValue.Length >= 1))
            {
                var objects = propertyValue
                    .Split(',')
                    .Select(s => s.Trim())
                    .SkipNullOrWhiteSpace()
                    .Select(s => s.Trim());
                return objects.Select(selector);
            }
            return Enumerable.Empty<SecurityClaimPolicy>();
        }
    }
}
