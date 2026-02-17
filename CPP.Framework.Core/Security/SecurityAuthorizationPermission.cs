using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;

using CPP.Framework.Security.Policies;

using JetBrains.Annotations;

namespace CPP.Framework.Security
{
    /// <summary>
    /// Describes a set of required security claims used to protect access to a secured resource.
    /// </summary>
    [Serializable]
    public sealed class SecurityAuthorizationPermission : IPermission, IUnrestrictedPermission
    {
        private const string ClassTypeAttributeName = "class";
        private const string ClassVersAttributeName = "version";
        private const string SecurityElementName = "IPermission";

        private const string AuthorityAttributeName = "issuer";
        private const string ClaimObjectElementName = "claim";
        private const string ClaimTypeAttributeName = "claimType";
        private const string ClaimDataAttributeName = "value";
        private const string ValueTypeAttributeName = "valueType";

        private static readonly string EncodedAssemblyQualifiedClassName;
        private static readonly Version ClassVersion10 = new Version(1, 0);
        private static readonly Version CurrentVersion = ClassVersion10;

        private static readonly ClaimByTypeComparer _ClaimComparer = new ClaimByTypeComparer();

        /// <summary>
        /// Initializes static members of the <see cref="SecurityAuthorizationPermission"/> class. 
        /// </summary>
        static SecurityAuthorizationPermission()
        {
            var sb = new StringBuilder(typeof(SecurityAuthorizationPermission).AssemblyQualifiedName);
            sb.Remove('"', '\'');
            EncodedAssemblyQualifiedClassName = sb.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationPermission"/> class.
        /// </summary>
        /// <param name="state">One of the <see cref="PermissionState"/> values.</param>
        [ExcludeFromCodeCoverage]
        [UsedImplicitly]
        public SecurityAuthorizationPermission(PermissionState state)
        {
            this.Claims = CreateSortedClaimsCollection(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationPermission"/> class.
        /// </summary>
        /// <param name="policy">
        /// The <see cref="SecurityClaimPolicy"/> to validate against the principal.
        /// </param>
        public SecurityAuthorizationPermission(SecurityClaimPolicy policy)
        {
            ArgumentValidator.ValidateNotNull(() => policy);
            this.Claims = new ReadOnlyCollection<Claim>(new[] { policy.Claim });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationPermission"/> class.
        /// </summary>
        /// <param name="policies">
        /// An <see cref="IEnumerable{T}"/> object that can be enumerated to get the list of
        /// policies to validate against the principal.
        /// </param>
        internal SecurityAuthorizationPermission(IEnumerable<SecurityClaimPolicy> policies)
        {
            ArgumentValidator.ValidateNotNull(() => policies);
            this.Claims = CreateSortedClaimsCollection(policies.Select(pol => pol.Claim));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationPermission"/> class.
        /// </summary>
        /// <param name="claims">
        /// An <see cref="IEnumerable{T}"/> object that can be used to generate the list of claims
        /// to validate against the principal.
        /// </param>
        [ExcludeFromCodeCoverage]
        private SecurityAuthorizationPermission(IEnumerable<Claim> claims)
        {
            this.Claims = CreateSortedClaimsCollection(claims);
        }

        /// <summary>
        /// Gets the list of claims to validate against the principal.
        /// </summary>
        internal ReadOnlyCollection<Claim> Claims { get; private set; }

        /// <summary>
        /// Checks whether or not the <see cref="IPrincipal"/> for the current execution context
        /// matches a given <see cref="SecurityAuthorizationPolicy"/>.
        /// </summary>
        /// <param name="policy">
        /// The <see cref="SecurityAuthorizationPolicy"/> to validate against the principal.
        /// </param>
        /// <returns><c>True</c> if the principal is authorized; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="policy"/> is null.</exception>
        [UsedImplicitly]
        public static bool CheckAccess(SecurityAuthorizationPolicy policy)
        {
            var context = SecurityAuthorizationContext.Create();
            return CheckAccess(policy, context);
        }

        /// <summary>
        /// Checks whether or not an <see cref="IPrincipal"/> matches a given
        /// <see cref="SecurityAuthorizationPolicy"/>.
        /// </summary>
        /// <param name="policy">
        /// The <see cref="SecurityAuthorizationPolicy"/> to validate against the principal.
        /// </param>
        /// <param name="principal">
        /// The principal to check. If this value is null, then the principal for the current
        /// execution context is used instead.
        /// </param>
        /// <returns><c>True</c> if the principal is authorized; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="policy"/> is null.</exception>
        [UsedImplicitly]
        public static bool CheckAccess(SecurityAuthorizationPolicy policy, IPrincipal principal)
        {
            var context = SecurityAuthorizationContext.Create(principal);
            return CheckAccess(policy, context);
        }

        /// <summary>
        /// Checks whether or not an <see cref="IPrincipal"/> matches a given
        /// <see cref="SecurityAuthorizationPolicy"/>.
        /// </summary>
        /// <param name="policy">
        /// The <see cref="SecurityAuthorizationPolicy"/> to validate against the principal.
        /// </param>
        /// <param name="context">
        /// The <see cref="SecurityAuthorizationContext"/> for the authorization request.
        /// </param>
        /// <returns><c>True</c> if the principal is authorized; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        ///     <para><paramref name="policy"/> is null.</para>
        ///     <para>-or-</para>
        ///     <para><paramref name="context"/> is null.</para>
        /// </exception>
        private static bool CheckAccess(SecurityAuthorizationPolicy policy, SecurityAuthorizationContext context)
        {
            ArgumentValidator.ValidateNotNull(() => policy);
            ArgumentValidator.ValidateNotNull(() => context);
            return policy.CheckAccess(context);
        }

        /// <summary>
        /// Checks whether or not a set of <see cref="Claims"/> have been assigned to a given
        /// principal. Please note that if <paramref name="criteria"/> contains more than one type
        /// of claim (i.e. the <see cref="Claim.Type"/> properties do not match), this method will
        /// separate the claims by type and evaluate each group independently. This means that one
        /// claim for each type will need to be found in the principal's claims list in order for
        /// the authentication check to succeed, not just any one claim in the input list.
        /// </summary>
        /// <param name="criteria">
        /// An <see cref="IEnumerable{T}"/> of <see cref="Claim"/> objects to search for in the
        /// given principal.
        /// </param>
        /// <param name="context">
        /// The <see cref="SecurityAuthorizationContext"/> object for the authorization request,
        /// which also includes the principal to search for claims.
        /// </param>
        /// <returns>
        /// <c>True</c> if the required claims from <paramref name="criteria"/> were found;
        /// otherwise, <c>false</c>.
        /// </returns>
        [UsedImplicitly]
        private static bool CheckClaims(IEnumerable<Claim> criteria, SecurityAuthorizationContext context)
        {
            var claimType = default(string);
            var allowed = false;
            foreach (var claim in criteria.SkipNull())
            {
                // check if the current claim is part of a new group (which is only possible if the
                // "all" flag has not been set).
                if (!string.Equals(claim.Type, claimType, StringComparison.OrdinalIgnoreCase))
                {
                    // if authorization didn't success for the the previous claim group, then we
                    // should stop processing any more claims.
                    if (!allowed && (claimType != null)) break;

                    // otherwise, initialize the authorization flag for the new claim group.
                    allowed = context.Manager.CheckAccess(claim, context);
                    claimType = claim.Type;
                }
                else if (!allowed)
                {
                    // only update the allowed flag for the current group if the authorization
                    // check for the previous claim did not succeed. this has the end result of
                    // only requiring one claim from each group to pass, but at the same time
                    // requiring at least one claim from each group for the entire check to succeed.
                    if (!allowed) allowed = context.Manager.CheckAccess(claim, context);
                }
            }
            return allowed;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public IPermission Copy() => new SecurityAuthorizationPermission(this.Claims);

        /// <summary>
        /// Creates a <see cref="ReadOnlyCollection{T}"/> of claims that are sorted by the
        /// claim type.
        /// </summary>
        /// <param name="claims">
        /// An <see cref="IEnumerable{T}"/> object that can be enumerated to provide the claims for
        /// the new collection. If this value is null, then the returned collection will contain no
        /// elements, and an exception is not thrown.
        /// </param>
        /// <returns>A <see cref="ReadOnlyCollection{T}"/> object.</returns>
        private static ReadOnlyCollection<Claim> CreateSortedClaimsCollection(IEnumerable<Claim> claims)
        {
            var sequence = claims.SkipNull().ToList();
            sequence.Sort(_ClaimComparer);
            return new ReadOnlyCollection<Claim>(sequence);
        }

        /// <summary>
        /// Throws a <see cref="SecurityException"/> if the principal for the current execution
        /// context does not match the security policy of the permission, or if the identity
        /// assigned to the principal has not been authenticated.
        /// </summary>
        /// <exception cref="SecurityAuthenticationException">
        /// The identity for the principal has not been authenticated.
        /// </exception>
        /// <exception cref="SecurityAuthenticationException">
        /// The principal could not be authorized.
        /// </exception>
        public void Demand() => this.Demand((IPrincipal)null, true);

        /// <summary>
        /// Throws a <see cref="SecurityException"/> if the principal for the current execution
        /// context does not match the security policy of the permission, or optionally, if the
        /// identity assigned to the principal has not been authenticated.
        /// </summary>
        /// <param name="authenticatedOnly">
        /// <c>True</c> if the identity assigned to the principal must be authenticated; otherwise,
        /// <c>false</c>. The allows the caller to verify any conditions that must be checked prior
        /// to authorizing the principal's identity (i.e. pre-authorization environment checks).
        /// </param>
        /// <exception cref="SecurityAuthenticationException">
        /// <paramref name="authenticatedOnly"/> is true, and the identity for the principal has
        /// not been authenticated.
        /// </exception>
        /// <exception cref="SecurityAuthenticationException">
        /// The principal could not be authorized.
        /// </exception>
        [ExcludeFromCodeCoverage]
        [UsedImplicitly]
        public void Demand(bool authenticatedOnly) => this.Demand((IPrincipal)null, authenticatedOnly);

        /// <summary>
        /// Throws a <see cref="SecurityException"/> if a given principal does not match the
        /// security policy of the permission, or if the identity assigned to the principal has not
        /// been authenticated.
        /// </summary>
        /// <param name="target">
        /// The <see cref="IPrincipal"/> to validate against. If this value is null, then the
        /// default principal for the current execution context is used instead.
        /// </param>
        /// <exception cref="SecurityAuthenticationException">
        /// The identity for the principal has not been authenticated.
        /// </exception>
        /// <exception cref="SecurityAuthenticationException">
        /// The principal could not be authorized.
        /// </exception>
        [ExcludeFromCodeCoverage]
        [UsedImplicitly]
        public void Demand(IPrincipal target) => this.Demand(target, true);

        /// <summary>
        /// Throws a <see cref="SecurityException"/> if a given principal does not match the
        /// security policy of the permission, or optionally, if the identity assigned to the
        /// principal has not been authenticated.
        /// </summary>
        /// <param name="target">
        /// The <see cref="IPrincipal"/> to validate against. If this value is null, then the
        /// default principal for the current execution context is used instead.
        /// </param>
        /// <param name="authenticatedOnly">
        /// <c>True</c> if the identity assigned to the principal must be authenticated; otherwise,
        /// <c>false</c>. The allows the caller to verify any conditions that must be checked prior
        /// to authorizing the user's identity (i.e. pre-authorization environment checks).
        /// </param>
        /// <exception cref="SecurityAuthenticationException">
        /// <paramref name="authenticatedOnly"/> is true, and the identity for the principal has
        /// not been authenticated.
        /// </exception>
        /// <exception cref="SecurityAuthenticationException">
        /// The principal could not be authorized.
        /// </exception>
        [UsedImplicitly]
        public void Demand(IPrincipal target, bool authenticatedOnly)
        {
            var context = SecurityAuthorizationContext.Create(target);
            if (authenticatedOnly && (!context.CurrentIdentity.IsAuthenticated))
            {
                throw new SecurityAuthenticationException();
            }
            if (!CheckClaims(this.Claims, context))
            {
                throw new SecurityAuthorizationException();
            }
        }

        /// <summary>
        /// Throws a <see cref="SecurityException"/> if the principal for the current execution
        /// context does not match a given <see cref="SecurityAuthorizationPolicy"/>, or if the
        /// identity assigned to the principal has not been authenticated.
        /// </summary>
        /// <param name="policy">
        /// The <see cref="SecurityAuthorizationPolicy"/> to validate against the principal. If the
        /// policy is null, then the method will only verify whether the principal is authenticated
        /// or not.
        /// </param>
        /// <exception cref="SecurityException">
        /// <paramref name="policy"/> could not be validated, or the principal is not authenticated.
        /// </exception>
        [ExcludeFromCodeCoverage]
        [UsedImplicitly]
        public static void Demand(SecurityAuthorizationPolicy policy) => Demand(policy, null, true);

        /// <summary>
        /// Throws a <see cref="SecurityException"/> if the principal for the current execution
        /// context does not match a given <see cref="SecurityAuthorizationPolicy"/>, or optionally
        /// if the identity assigned to the principal has not been authenticated.
        /// </summary>
        /// <param name="policy">
        /// The <see cref="SecurityAuthorizationPolicy"/> to validate against the principal. If the
        /// policy is null, then the method will only verify whether the principal is authenticated
        /// or not (but only if <paramref name="authenticatedOnly"/> is <c>true</c>).
        /// </param>
        /// <param name="authenticatedOnly">
        /// <c>True</c> if the principal must be authentication prior to checking for the claim;
        /// otherwise, <c>false</c>.
        /// </param>
        /// <exception cref="SecurityException">
        /// <paramref name="policy"/> could not be validated, or the principal is not authenticated.
        /// </exception>
        [ExcludeFromCodeCoverage]
        [UsedImplicitly]
        public static void Demand(SecurityAuthorizationPolicy policy, bool authenticatedOnly) => Demand(policy, null, authenticatedOnly);

        /// <summary>
        /// Throws a <see cref="SecurityException"/> if a given <see cref="IPrincipal"/> does not
        /// match a given <see cref="SecurityAuthorizationPolicy"/>, or if the identity assigned to
        /// the principal has not been authenticated.
        /// </summary>
        /// <param name="policy">
        /// The <see cref="SecurityAuthorizationPolicy"/> to validate against the principal. If the
        /// policy is null, then the method will only verify whether the principal is authenticated
        /// or not.
        /// </param>
        /// <param name="target">
        /// The principal to check. If this value is null, then the principal for the current
        /// execution context is used instead.
        /// </param>
        /// <exception cref="SecurityException">
        /// <paramref name="policy"/> could not be validated, or the principal is not authenticated.
        /// </exception>
        [ExcludeFromCodeCoverage]
        [UsedImplicitly]
        public static void Demand(SecurityAuthorizationPolicy policy, IPrincipal target) => Demand(policy, target, true);

        /// <summary>
        /// Throws a <see cref="SecurityException"/> if a given <see cref="IPrincipal"/> does not
        /// match a given <see cref="SecurityAuthorizationPolicy"/>, or optionally if the identity
        /// assigned to the principal has not been authenticated.
        /// </summary>
        /// <param name="policy">
        /// The <see cref="SecurityAuthorizationPolicy"/> to validate against the principal. If the
        /// policy is null, then the method will only verify whether the principal is authenticated
        /// or not (but only if <paramref name="authenticatedOnly"/> is <c>true</c>).
        /// </param>
        /// <param name="target">
        /// The principal to check. If this value is null, then the principal for the current
        /// execution context is used instead.
        /// </param>
        /// <param name="authenticatedOnly">
        /// <c>True</c> if the principal must be authentication prior to checking for the claim;
        /// otherwise, <c>false</c>.
        /// </param>
        /// <exception cref="SecurityException">
        /// <paramref name="policy"/> could not be validated, or the principal is not authenticated.
        /// </exception>
        [UsedImplicitly]
        public static void Demand(SecurityAuthorizationPolicy policy, IPrincipal target, bool authenticatedOnly)
        {
            var context = SecurityAuthorizationContext.Create(target);
            if (authenticatedOnly && (!context.CurrentIdentity.IsAuthenticated))
            {
                throw new SecurityAuthenticationException();
            }
            if (!SecurityAuthorizationPermission.CheckAccess(policy, context))
            {
                throw new SecurityAuthorizationException();
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public void FromXml(SecurityElement element)
        {
            ArgumentValidator.ValidateNotNull(() => element);
            if (element.Tag != SecurityElementName)
            {
                throw new InvalidOperationException(ErrorStrings.InvalidPermissionSecurityXml);
            }

            var className = ValidateAttribute(element, ClassTypeAttributeName, EncodedAssemblyQualifiedClassName);
            if (className != EncodedAssemblyQualifiedClassName)
            {
                var message = string.Format(
                    ErrorStrings.InvalidPermissionClassXml,
                    className,
                    EncodedAssemblyQualifiedClassName);
                throw new InvalidOperationException(message);
            }
            var version = ValidateAttribute(element, ClassVersAttributeName, ClassVersion10);

            if (version >= ClassVersion10)
            {
                var claims = new HashSet<Claim>(_ClaimComparer);
                foreach (var child in element.Children.OfType<SecurityElement>())
                {
                    if (child.Tag != ClaimObjectElementName) continue;
                    var authority = ValidateAttribute(child, AuthorityAttributeName, ClaimsIdentity.DefaultIssuer);
                    var claimType = ValidateAttribute(child, ClaimTypeAttributeName);
                    var claimData = ValidateAttribute(child, ClaimDataAttributeName);
                    var valueType = ValidateAttribute(child, ValueTypeAttributeName, ClaimValueTypes.String);
                    claims.Add(new Claim(claimType, claimData, valueType, authority));
                }
                this.Claims = CreateSortedClaimsCollection(claims);
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public IPermission Intersect(IPermission target)
        {
            if (this.TryConvertPermission(target, out var permission))
            {
                var claims = new HashSet<Claim>(permission.Claims, _ClaimComparer);
                claims.ExceptWith(this.Claims);
                return new SecurityAuthorizationPermission(claims);
            }
            return null;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public bool IsSubsetOf(IPermission target)
        {
            if (this.TryConvertPermission(target, out var permission))
            {
                foreach (var claim in permission.Claims)
                {
                    if (!this.Claims.Contains(claim)) return false;
                }
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public bool IsUnrestricted() => true;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        [ExcludeFromCodeCoverage]
        public override string ToString() => this.ToXml().ToString();

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public SecurityElement ToXml()
        {
            var element = new SecurityElement(SecurityElementName)
            {
                Attributes = new Hashtable
                {
                    [ClassTypeAttributeName] = EncodedAssemblyQualifiedClassName,
                    [ClassVersAttributeName] = CurrentVersion.ToString(),
                },
            };
            foreach (var claim in this.Claims)
            {
                var child = new SecurityElement(ClaimObjectElementName)
                {
                    Attributes = new Hashtable
                    {
                        [AuthorityAttributeName] = claim.Issuer,
                        [ClaimTypeAttributeName] = claim.Type,
                        [ClaimDataAttributeName] = claim.Value,
                        [ValueTypeAttributeName] = claim.ValueType,
                    }
                };
                element.AddChild(child);
            }
            return element;
        }

        /// <summary>
        /// Attempts to convert an <see cref="IPermission"/> object reference to a
        /// <see cref="SecurityAuthorizationPermission"/> reference.
        /// </summary>
        /// <param name="target">The target permission reference to convert.</param>
        /// <param name="permssion">
        /// An output value that receives the converted permission on success.
        /// </param>
        /// <returns>
        /// <c>True</c> if <paramref name="target"/> was successfully converted; otherwise,
        /// <c>false</c>.
        /// </returns>
        private bool TryConvertPermission(IPermission target, out SecurityAuthorizationPermission permssion)
        {
            permssion = (target as SecurityAuthorizationPermission);
            return (permssion != null);
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public IPermission Union(IPermission target)
        {
            if (this.TryConvertPermission(target, out var permission))
            {
                var claims = new HashSet<Claim>(permission.Claims, _ClaimComparer);
                claims.UnionWith(this.Claims);
                return new SecurityAuthorizationPermission(claims);
            }
            return null;
        }

        /// <summary>
        /// Reads an attribute from the element, and then validates either that the value is not
        /// null, or that a default has been provided.
        /// </summary>
        /// <param name="element">The element that contains the attributes to read.</param>
        /// <param name="name">The name of the attribute to read.</param>
        /// <param name="default">An optional default value to use if the attribute is not set.</param>
        /// <returns>
        /// A string that contains either the value of the attribute, or <paramref name="@default"/>.
        /// </returns>
        [ExcludeFromCodeCoverage]
        private static string ValidateAttribute(SecurityElement element, string name, string @default = null)
        {
            var value = (element.Attribute(name) ?? @default);
            if (value == null)
            {
                var message = string.Format(
                    ErrorStrings.InvalidPermissionClaimXml,
                    name);
                throw new InvalidOperationException(message);
            }
            return value;
        }

        /// <summary>
        /// Reads an attribute from the element, and then validates either that the value is not
        /// null, or that a default has been provided.
        /// </summary>
        /// <param name="element">The element that contains the attributes to read.</param>
        /// <param name="name">The name of the attribute to read.</param>
        /// <param name="default">An optional default value to use if the attribute is not set.</param>
        /// <returns>
        /// A string that contains either the value of the attribute, or <paramref name="@default"/>.
        /// </returns>
        [ExcludeFromCodeCoverage]
        private static Version ValidateAttribute(SecurityElement element, string name, Version @default)
        {
            try
            {
                return new Version(ValidateAttribute(element, name));
            }
            catch (InvalidOperationException)
            {
                if (@default == null) throw;
            }
            return @default;
        }
    }
}
