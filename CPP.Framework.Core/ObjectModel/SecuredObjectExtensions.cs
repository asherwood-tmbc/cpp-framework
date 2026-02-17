using System.Security.Claims;
using System.Security.Principal;

using CPP.Framework.ObjectModel.Validation;
using CPP.Framework.Security;

// ReSharper disable once CheckNamespace
namespace CPP.Framework.ObjectModel
{
    /// <summary>
    /// Extension methods for the <see cref="SecuredObject"/> class.
    /// </summary>
    public static class SecuredObjectExtensions
    {
        /// <summary>
        /// Checks whether or not the identity for a user has permission to access to the data in
        /// the <see cref="SecuredObject"/> instance.
        /// </summary>
        /// <param name="instance">The instance to test.</param>
        /// <param name="identity">The identity to use to evaluate the permissions.</param>
        /// <returns><b>True</b> if the user has access; otherwise, <b>false</b>.</returns>
        public static bool CheckAccess(this SecuredObject instance, ClaimsIdentity identity)
        {
            ArgumentValidator.ValidateThisObj(() => instance);
            ArgumentValidator.ValidateNotNull(() => identity);
            return ObjectValidator.CheckAccess(instance, new ClaimsPrincipal(identity));
        }

        /// <summary>
        /// Checks whether or not the identity for a user has permission to access to the data in
        /// the <see cref="SecuredObject"/> instance.
        /// </summary>
        /// <param name="instance">The instance to test.</param>
        /// <param name="principal">The principal to use to evaluate the permissions.</param>
        /// <returns><b>True</b> if the user has access; otherwise, <b>false</b>.</returns>
        public static bool CheckAccess(this SecuredObject instance, IPrincipal principal)
        {
            ArgumentValidator.ValidateThisObj(() => instance);
            ArgumentValidator.ValidateNotNull(() => principal);
            return ObjectValidator.CheckAccess(instance, principal);
        }

        /// <summary>
        /// Checks whether or not the identity for a user has permission to access to the data in
        /// the <see cref="SecuredObject"/> instance.
        /// </summary>
        /// <param name="instance">The instance to test.</param>
        /// <param name="identity">The identity to use to evaluate the permissions.</param>
        /// <exception cref="SecurityAuthorizationException">
        /// The current user does not have sufficient permissions to access the object.
        /// </exception>
        public static void Demand(this SecuredObject instance, ClaimsIdentity identity)
        {
            ArgumentValidator.ValidateThisObj(() => instance);
            ArgumentValidator.ValidateNotNull(() => identity);
            ObjectValidator.DemandAccess(instance, new ClaimsPrincipal(identity));
        }

        /// <summary>
        /// Checks whether or not the identity for a user has permission to access to the data in
        /// the <see cref="SecuredObject"/> instance.
        /// </summary>
        /// <param name="instance">The instance to test.</param>
        /// <param name="principal">The principal to use to evaluate the permissions.</param>
        /// <exception cref="SecurityAuthorizationException">
        /// The current user does not have sufficient permissions to access the object.
        /// </exception>
        public static void Demand(this SecuredObject instance, IPrincipal principal)
        {
            ArgumentValidator.ValidateThisObj(() => instance);
            ArgumentValidator.ValidateNotNull(() => principal);
            ObjectValidator.DemandAccess(instance, principal);
        }
    }
}
