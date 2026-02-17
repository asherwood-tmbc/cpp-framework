using System;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace CPP.Framework.Security.Policies
{
    /// <summary>
    /// Abstract base class for all security policy objects that authorize access for a
    /// <see cref="ClaimsIdentity"/> based on whether or not a specific <see cref="Claim"/> has
    /// been added.
    /// </summary>
    public abstract class SecurityClaimPolicy : SecurityAuthorizationPolicy, IEquatable<SecurityClaimPolicy>
    {
        private static readonly ConcurrentDictionary<Claim, SecurityClaimPolicy> _ClaimPolicyCache = new ConcurrentDictionary<Claim, SecurityClaimPolicy>(new ClaimByTypeComparer());

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityClaimPolicy"/> class.
        /// </summary>
        /// <param name="claim">
        /// The <see cref="Claim"/> required for the authorization policy evaluation to succeed.
        /// </param>
        protected SecurityClaimPolicy(Claim claim)
        {
            ArgumentValidator.ValidateNotNull(() => claim);
            this.Claim = claim;
            this.HashCode = GenerateHashCodeValue(claim);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityClaimPolicy"/> class.
        /// </summary>
        /// <param name="type">The type of the claim to assign to the policy.</param>
        /// <param name="value">The value of the claim to assign to the policy.</param>
        protected SecurityClaimPolicy(string type, string value) : this(type, value, ClaimValueTypes.String) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityClaimPolicy"/> class.
        /// </summary>
        /// <param name="type">The type of the claim to assign to the policy.</param>
        /// <param name="value">The value of the claim to assign to the policy.</param>
        /// <param name="valueType">
        /// The data type of <paramref name="value"/> (usually from <see cref="ClaimValueTypes"/>).
        /// </param>
        protected SecurityClaimPolicy(string type, string value, string valueType)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => type);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => value);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => valueType);
            this.Claim = new Claim(type, value, valueType);
            this.HashCode = GenerateHashCodeValue(this.Claim);
        }

        /// <summary>
        /// Gets the <see cref="Claim"/> required for the authorization policy evaluation to
        /// succeed.
        /// </summary>
        internal Claim Claim { get; }

        /// <summary>
        /// Gets the unique id for the value of the current instance. This value also serves as the
        /// pre-calculated hash for the current object.
        /// </summary>
        private int HashCode { get; }

        /// <summary>
        /// Checks whether or not the identity assigned to principal has been granted the claim
        /// associated with the current security policy object.
        /// </summary>
        /// <param name="context">
        /// The <see cref="SecurityAuthorizationContext"/> object for the authorization request.
        /// </param>
        /// <returns>
        /// <c>True</c> if the identity is authorized as defined by the current security policy
        /// object; otherwise, <c>false</c>.
        /// </returns>
        protected internal override bool CheckAccess(SecurityAuthorizationContext context)
        {
            return context.Manager.CheckAccess(this.Claim, context);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <c>True</c> if the current object is equal to the <paramref name="other" /> parameter;
        /// otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(SecurityClaimPolicy other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (ReferenceEquals(null, other)) return false;
            return (this.HashCode == other.HashCode);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => this.Equals(obj as SecurityClaimPolicy);

        /// <summary>
        /// Generates a hash code value for a <see cref="Claim"/>.
        /// </summary>
        /// <param name="claim">The claim for which to generate a hash code.</param>
        /// <returns>A hash code value.</returns>
        private static int GenerateHashCodeValue(Claim claim) => ($"{claim.Type.ToLower()}({claim.Value})").GetHashCode();

        /// <inheritdoc />
        public override int GetHashCode() => this.HashCode;

        /// <summary>
        /// Gets or adds the value of a cached <see cref="SecurityClaimPolicy"/>.
        /// </summary>
        /// <typeparam name="TPolicy">The type of the policy class.</typeparam>
        /// <param name="claim">The claim assigned to the policy.</param>
        /// <param name="factory">A delegate that is called if a new policy needs to be created.</param>
        /// <returns>The cached policy object.</returns>
        protected static TPolicy GetOrAddCachedPolicy<TPolicy>(Claim claim, Func<Claim, TPolicy> factory)
            where TPolicy : SecurityClaimPolicy
        {
            return (TPolicy)_ClaimPolicyCache.GetOrAdd(claim, factory);
        }

        /// <summary>Returns a value that indicates whether the values of two <see cref="T:CPP.Framework.Security.SecurityAccessObject" /> objects are equal.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(SecurityClaimPolicy left, SecurityClaimPolicy right) => (Equals(left, right));

        /// <summary>Returns a value that indicates whether two <see cref="T:CPP.Framework.Security.SecurityAccessObject" /> objects have different values.</summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(SecurityClaimPolicy left, SecurityClaimPolicy right) => (!Equals(left, right));
    }
}
