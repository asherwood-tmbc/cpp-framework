using System.Security.Principal;

namespace CPP.Framework.Security.Policies
{
    /// <summary>
    /// Implemented by classes to indicate that they support an object-level security access policy.
    /// </summary>
    public interface ISupportsObjectAccessPolicy
    {
        /// <summary>
        /// Retrieves the access policy for the current instance.
        /// </summary>
        /// <param name="context">The context for the access check request.</param>
        /// <returns>An <see cref="SecurityAuthorizationPolicy"/> object.</returns>
        SecurityAuthorizationPolicy GetAccessPolicy(SecurityAuthorizationContext context);
    }
}
