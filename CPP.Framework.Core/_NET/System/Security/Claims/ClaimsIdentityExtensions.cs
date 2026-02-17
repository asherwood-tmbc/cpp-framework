using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

using CPP.Framework;
using CPP.Framework.Security;
using CPP.Framework.Security.Policies;

using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Security.Claims
{
    /// <summary>
    /// Extensions methods for the <see cref="ClaimsIdentity"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [UsedImplicitly]
    public static class ClaimsIdentityExtensions
    {
        /// <summary>
        /// Assigns a new claim value to a <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">The value for the claim.</param>
        /// <returns>
        /// <c>True</c> if the claim was added; otherwise, <c>false</c> if the claim already exists.
        /// </returns>
        [UsedImplicitly]
        public static bool AddClaim(this ClaimsIdentity identity, string claimType, bool value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            var strval = value.ToString(CultureInfo.InvariantCulture);
            if (!identity.HasClaim(claimType, strval))
            {
                identity.AddClaim(new Claim(claimType, strval, ClaimValueTypes.Boolean));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Assigns a new claim value to a <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">The value for the claim.</param>
        /// <returns>
        /// <c>True</c> if the claim was added; otherwise, <c>false</c> if the claim already exists.
        /// </returns>
        [UsedImplicitly]
        public static bool AddClaim(this ClaimsIdentity identity, string claimType, DateTime value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            var strval = value.ToString("yyyy-MM-dd\\THH:mm:ss\\Z", CultureInfo.InvariantCulture);
            if (!identity.HasClaim(claimType, strval))
            {
                identity.AddClaim(new Claim(claimType, strval, ClaimValueTypes.DateTime));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Assigns a new claim value to a <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">The value for the claim.</param>
        /// <returns>
        /// <c>True</c> if the claim was added; otherwise, <c>false</c> if the claim already exists.
        /// </returns>
        [UsedImplicitly]
        public static bool AddClaim(this ClaimsIdentity identity, string claimType, double value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            var strval = value.ToString("F", CultureInfo.InvariantCulture);
            if (!identity.HasClaim(claimType, strval))
            {
                identity.AddClaim(new Claim(claimType, strval, ClaimValueTypes.Double));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Assigns a new claim value to a <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">The value for the claim.</param>
        /// <returns>
        /// <c>True</c> if the claim was added; otherwise, <c>false</c> if the claim already exists.
        /// </returns>
        [UsedImplicitly]
        public static bool AddClaim(this ClaimsIdentity identity, string claimType, Guid value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            var strval = value.ToString("N", CultureInfo.InvariantCulture);
            if (!identity.HasClaim(claimType, strval))
            {
                identity.AddClaim(new Claim(claimType, strval, ClaimValueTypes.String));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Assigns a new claim value to a <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">The value for the claim.</param>
        /// <returns>
        /// <c>True</c> if the claim was added; otherwise, <c>false</c> if the claim already exists.
        /// </returns>
        [UsedImplicitly]
        public static bool AddClaim(this ClaimsIdentity identity, string claimType, int value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            var strval = value.ToString("D", CultureInfo.InvariantCulture);
            if (!identity.HasClaim(claimType, strval))
            {
                identity.AddClaim(new Claim(claimType, strval, ClaimValueTypes.Integer32));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Assigns a new claim value to a <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">The value for the claim.</param>
        /// <returns>
        /// <c>True</c> if the claim was added; otherwise, <c>false</c> if the claim already exists.
        /// </returns>
        [UsedImplicitly]
        public static bool AddClaim(this ClaimsIdentity identity, string claimType, long value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            var strval = value.ToString("D", CultureInfo.InvariantCulture);
            if (!identity.HasClaim(claimType, strval))
            {
                identity.AddClaim(new Claim(claimType, strval, ClaimValueTypes.Integer64));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Assigns a new claim value to a <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">The value for the claim.</param>
        /// <returns>
        /// <c>True</c> if the claim was added; otherwise, <c>false</c> if the claim already exists.
        /// </returns>
        [UsedImplicitly]
        public static bool AddClaim(this ClaimsIdentity identity, string claimType, string value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => value);
            if (!identity.HasClaim(claimType, value))
            {
                identity.AddClaim(new Claim(claimType, value, ClaimValueTypes.String));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Assigns a new claim value to a <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">The value for the claim.</param>
        /// <returns>
        /// <c>True</c> if the claim was added; otherwise, <c>false</c> if the claim already exists.
        /// </returns>
        [UsedImplicitly]
        public static bool AddClaim(this ClaimsIdentity identity, string claimType, uint value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            var strval = value.ToString("D", CultureInfo.InvariantCulture);
            if (!identity.HasClaim(claimType, strval))
            {
                identity.AddClaim(new Claim(claimType, strval, ClaimValueTypes.UInteger32));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Assigns a new claim value to a <see cref="ClaimsIdentity"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">The value for the claim.</param>
        /// <returns>
        /// <c>True</c> if the claim was added; otherwise, <c>false</c> if the claim already exists.
        /// </returns>
        [UsedImplicitly]
        public static bool AddClaim(this ClaimsIdentity identity, string claimType, ulong value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            var strval = value.ToString("D", CultureInfo.InvariantCulture);
            if (!identity.HasClaim(claimType, strval))
            {
                identity.AddClaim(new Claim(claimType, strval, ClaimValueTypes.UInteger64));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether or not the current identity is authorized based on a given
        /// <see cref="SecurityAuthorizationPolicy"/>.
        /// </summary>
        /// <param name="identity">The identity to check.</param>
        /// <param name="policy">The policy to validate against the identity.</param>
        /// <returns>
        /// <c>True</c> if <paramref name="identity"/> is authorized; otherwise, <c>false</c>.
        /// </returns>
        [UsedImplicitly]
        public static bool CheckAccess(this ClaimsIdentity identity, SecurityAuthorizationPolicy policy)
        {
            ArgumentValidator.ValidateNotNull(() => policy);
            var principal = new ClaimsPrincipal(identity);
            var context = SecurityAuthorizationContext.Create(principal);
            return policy.CheckAccess(context);
        }

        /// <summary>
        /// Gets the value(s) for a given claim type.
        /// </summary>
        /// <typeparam name="TValue">The type of the claim value.</typeparam>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="converter">
        /// A delegate to call for each claim value to convert it to the output type.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the retrieved
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        private static IEnumerable<TValue> GetClaimValuesAs<TValue>(ClaimsIdentity identity, string claimType, Func<string, TValue> converter)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            ArgumentValidator.ValidateNotNull(() => converter);

            var sequence = identity.FindAll(claimType)
                .Select(claim => claim.Value)
                .Select(converter);
            foreach (var claim in sequence) yield return claim;
        }

        /// <summary>
        /// Gets the value(s) for a given claim type as a <see langword="bool"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static IEnumerable<bool> GetClaimValuesAsBoolean(this ClaimsIdentity identity, string claimType) => GetClaimValuesAs(identity, claimType, bool.Parse);

        /// <summary>
        /// Gets the value(s) for a given claim type as a <see langword="DateTime"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static IEnumerable<DateTime> GetClaimValuesAsDateTime(this ClaimsIdentity identity, string claimType) => GetClaimValuesAs(identity, claimType, DateTime.Parse);

        /// <summary>
        /// Gets the value(s) for a given claim type as a <see langword="double"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static IEnumerable<double> GetClaimValuesAsDouble(this ClaimsIdentity identity, string claimType) => GetClaimValuesAs(identity, claimType, double.Parse);

        /// <summary>
        /// Gets the value(s) for a given claim type as a <see cref="Guid"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static IEnumerable<Guid> GetClaimValuesAsGuid(this ClaimsIdentity identity, string claimType) => GetClaimValuesAs(identity, claimType, Guid.Parse);

        /// <summary>
        /// Gets the value(s) for a given claim type as an <see langword="int"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static IEnumerable<int> GetClaimValuesAsInt32(this ClaimsIdentity identity, string claimType) => GetClaimValuesAs(identity, claimType, int.Parse);

        /// <summary>
        /// Gets the value(s) for a given claim type as a <see langword="long"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static IEnumerable<long> GetClaimValuesAsInt64(this ClaimsIdentity identity, string claimType) => GetClaimValuesAs(identity, claimType, long.Parse);

        /// <summary>
        /// Gets the value(s) for a given claim type as a <see langword="string"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static IEnumerable<string> GetClaimValuesAsString(this ClaimsIdentity identity, string claimType) => GetClaimValuesAs(identity, claimType, s => s);

        /// <summary>
        /// Gets the value(s) for a given claim type as a <see langword="uint"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static IEnumerable<uint> GetClaimValuesAsUInt32(this ClaimsIdentity identity, string claimType) => GetClaimValuesAs(identity, claimType, uint.Parse);

        /// <summary>
        /// Gets the value(s) for a given claim type as a <see langword="ulong"/>.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static IEnumerable<ulong> GetClaimValuesAsUInt64(this ClaimsIdentity identity, string claimType) => GetClaimValuesAs(identity, claimType, ulong.Parse);

        /// <summary>
        /// Retrieves the single value of a claim as a <see cref="bool"/>. Please note that this
        /// method will also throw an exception if there is more than one value assigned for the
        /// same claim type.
        /// </summary>
        /// <param name="identity">The identity that contains the claims to search.</param>
        /// <param name="claimType">The claim type of the claim to retrieve.</param>
        /// <returns>The claim value.</returns>
        [UsedImplicitly]
        public static bool GetSingleClaimValueAsBoolean(this ClaimsIdentity identity, string claimType)
        {
            if (!TryGetSingleClaimValueAsBoolean(identity, claimType, out var value))
            {
                throw new SecurityClaimNotFoundException(claimType);
            }
            return value;
        }

        /// <summary>
        /// Retrieves the single value of a claim as a <see cref="DateTime"/>. Please note that
        /// this method will also throw an exception if there is more than one value assigned for
        /// the same claim type.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static DateTime GetSingleClaimValueAsDateTime(this ClaimsIdentity identity, string claimType)
        {
            if (!TryGetSingleClaimValueAsDateTime(identity, claimType, out var value))
            {
                throw new SecurityClaimNotFoundException(claimType);
            }
            return value;
        }

        /// <summary>
        /// Retrieves the single value of a claim as a <see cref="double"/>. Please note that this
        /// method will also throw an exception if there is more than one value assigned for the
        /// same claim type.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static double GetSingleClaimValueAsDouble(this ClaimsIdentity identity, string claimType)
        {
            if (!TryGetSingleClaimValueAsDouble(identity, claimType, out var value))
            {
                throw new SecurityClaimNotFoundException(claimType);
            }
            return value;
        }

        /// <summary>
        /// Retrieves the single value of a claim as a <see cref="Guid"/>. Please note that this
        /// method will also throw an exception if there is more than one value assigned for the
        /// same claim type.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static Guid GetSingleClaimValueAsGuid(this ClaimsIdentity identity, string claimType)
        {
            if (!TryGetSingleClaimValueAsGuid(identity, claimType, out var value))
            {
                throw new SecurityClaimNotFoundException(claimType);
            }
            return value;
        }

        /// <summary>
        /// Retrieves the single value of a claim as a <see cref="int"/>. Please note that this
        /// method will also throw an exception if there is more than one value assigned for the
        /// same claim type.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static int GetSingleClaimValueAsInt32(this ClaimsIdentity identity, string claimType)
        {
            if (!TryGetSingleClaimValueAsInt32(identity, claimType, out var value))
            {
                throw new SecurityClaimNotFoundException(claimType);
            }
            return value;
        }

        /// <summary>
        /// Retrieves the single value of a claim as a <see cref="long"/>. Please note that this
        /// method will also throw an exception if there is more than one value assigned for the
        /// same claim type.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static long GetSingleClaimValueAsInt64(this ClaimsIdentity identity, string claimType)
        {
            if (!TryGetSingleClaimValueAsInt64(identity, claimType, out var value))
            {
                throw new SecurityClaimNotFoundException(claimType);
            }
            return value;
        }

        /// <summary>
        /// Retrieves the single value of a claim as a <see cref="string"/>. Please note that this
        /// method will also throw an exception if there is more than one value assigned for the
        /// same claim type.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static string GetSingleClaimValueAsString(this ClaimsIdentity identity, string claimType)
        {
            if (!TryGetSingleClaimValueAsString(identity, claimType, out var value))
            {
                throw new SecurityClaimNotFoundException(claimType);
            }
            return value;
        }

        /// <summary>
        /// Retrieves the single value of a claim as a <see cref="uint"/>. Please note that this
        /// method will also throw an exception if there is more than one value assigned for the
        /// same claim type.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static uint GetSingleClaimValueAsUInt32(this ClaimsIdentity identity, string claimType)
        {
            if (!TryGetSingleClaimValueAsUInt32(identity, claimType, out var value))
            {
                throw new SecurityClaimNotFoundException(claimType);
            }
            return value;
        }

        /// <summary>
        /// Retrieves the single value of a claim as a <see cref="ulong"/>. Please note that this
        /// method will also throw an exception if there is more than one value assigned for the
        /// same claim type.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static ulong GetSingleClaimValueAsUInt64(this ClaimsIdentity identity, string claimType)
        {
            if (!TryGetSingleClaimValueAsUInt64(identity, claimType, out var value))
            {
                throw new SecurityClaimNotFoundException(claimType);
            }
            return value;
        }

        /// <summary>
        /// Grants the claim associated with a <see cref="SecurityClaimPolicy"/> object.
        /// </summary>
        /// <param name="identity">The identity to grant the access right.</param>
        /// <param name="policy">The <see cref="SecurityClaimPolicy"/> for the claim to grant.</param>
        /// <returns>A reference to <paramref name="identity"/>.</returns>
        [UsedImplicitly]
        public static ClaimsIdentity Grant(this ClaimsIdentity identity, SecurityClaimPolicy policy)
        {
            var claim = policy.Claim;
            if (!identity.HasClaim(claim.Type, claim.Value)) identity.AddClaim(claim);
            return identity;
        }

        /// <summary>
        /// Removes all instances of a claim from an identity.
        /// </summary>
        /// <param name="identity">The identity that contains the claims to remove.</param>
        /// <param name="claimType">The claim type of the claim to remove.</param>
        [UsedImplicitly]
        public static void RemoveAll(this ClaimsIdentity identity, string claimType)
        {
            var existing = identity.FindAll(claimType).ToArray();
            Array.ForEach(existing, clm => identity.TryRemoveClaim(clm));
        }

        /// <summary>
        /// Revokes the claim associated with a <see cref="SecurityClaimPolicy"/> object.
        /// </summary>
        /// <param name="identity">The identity to grant the claim.</param>
        /// <param name="policy">The <see cref="SecurityClaimPolicy"/> for the claim to revoke.</param>
        /// <returns>A reference to <paramref name="identity"/>.</returns>
        [UsedImplicitly]
        public static ClaimsIdentity Revoke(this ClaimsIdentity identity, SecurityClaimPolicy policy)
        {
            identity.FindAll(policy.Claim.Type)
                .Where(c => c.Value == policy.Claim.Value)
                .All(identity.TryRemoveClaim);

            return identity;
        }

        /// <summary>
        /// Sets the value of a claim as a <see cref="bool"/>.
        /// </summary>
        /// <param name="identity">The identity that contains the claims to search.</param>
        /// <param name="claimType">The claim type of the claim to set.</param>
        /// <param name="value">
        /// The value to set for the claim. If the value is null, then the claim is simply removed.
        /// </param>
        [UsedImplicitly]
        public static void SetSingleClaimValue(this ClaimsIdentity identity, string claimType, bool? value)
        {
            identity.RemoveAll(claimType);
            if (!value.HasValue) return;
            identity.AddClaim(claimType, value.Value);
        }

        /// <summary>
        /// Sets the value of a claim as a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="identity">The identity that contains the claims to search.</param>
        /// <param name="claimType">The claim type of the claim to set.</param>
        /// <param name="value">
        /// The value to set for the claim. If the value is null, then the claim is simply removed.
        /// </param>
        [UsedImplicitly]
        public static void SetSingleClaimValue(this ClaimsIdentity identity, string claimType, DateTime? value)
        {
            identity.RemoveAll(claimType);
            if (!value.HasValue) return;
            identity.AddClaim(claimType, value.Value);
        }

        /// <summary>
        /// Sets the value of a claim as a <see cref="double"/>.
        /// </summary>
        /// <param name="identity">The identity that contains the claims to search.</param>
        /// <param name="claimType">The claim type of the claim to set.</param>
        /// <param name="value">
        /// The value to set for the claim. If the value is null, then the claim is simply removed.
        /// </param>
        [UsedImplicitly]
        public static void SetSingleClaimValue(this ClaimsIdentity identity, string claimType, double? value)
        {
            identity.RemoveAll(claimType);
            if (!value.HasValue) return;
            identity.AddClaim(claimType, value.Value);
        }

        /// <summary>
        /// Sets the value of a claim as a <see cref="Guid"/>.
        /// </summary>
        /// <param name="identity">The identity that contains the claims to search.</param>
        /// <param name="claimType">The claim type of the claim to set.</param>
        /// <param name="value">
        /// The value to set for the claim. If the value is null, then the claim is simply removed.
        /// </param>
        [UsedImplicitly]
        public static void SetSingleClaimValue(this ClaimsIdentity identity, string claimType, Guid? value)
        {
            identity.RemoveAll(claimType);
            if (!value.HasValue) return;
            identity.AddClaim(claimType, value.Value);
        }

        /// <summary>
        /// Sets the value of a claim as a <see cref="int"/>.
        /// </summary>
        /// <param name="identity">The identity that contains the claims to search.</param>
        /// <param name="claimType">The claim type of the claim to set.</param>
        /// <param name="value">
        /// The value to set for the claim. If the value is null, then the claim is simply removed.
        /// </param>
        [UsedImplicitly]
        public static void SetSingleClaimValue(this ClaimsIdentity identity, string claimType, int? value)
        {
            identity.RemoveAll(claimType);
            if (!value.HasValue) return;
            identity.AddClaim(claimType, value.Value);
        }

        /// <summary>
        /// Sets the value of a claim as a <see cref="long"/>.
        /// </summary>
        /// <param name="identity">The identity that contains the claims to search.</param>
        /// <param name="claimType">The claim type of the claim to set.</param>
        /// <param name="value">
        /// The value to set for the claim. If the value is null, then the claim is simply removed.
        /// </param>
        [UsedImplicitly]
        public static void SetSingleClaimValue(this ClaimsIdentity identity, string claimType, long? value)
        {
            identity.RemoveAll(claimType);
            if (!value.HasValue) return;
            identity.AddClaim(claimType, value.Value);
        }

        /// <summary>
        /// Sets the value of a claim as a <see langword="string"/>.
        /// </summary>
        /// <param name="identity">The identity that contains the claims to search.</param>
        /// <param name="claimType">The claim type of the claim to set.</param>
        /// <param name="value">
        /// The value to set for the claim. If the value is null, then the claim is simply removed.
        /// </param>
        [UsedImplicitly]
        public static void SetSingleClaimValue(this ClaimsIdentity identity, string claimType, string value)
        {
            identity.RemoveAll(claimType);
            if (string.IsNullOrWhiteSpace(value)) return;
            identity.AddClaim(claimType, value);
        }

        /// <summary>
        /// Sets the value of a claim as a <see cref="uint"/>.
        /// </summary>
        /// <param name="identity">The identity that contains the claims to search.</param>
        /// <param name="claimType">The claim type of the claim to set.</param>
        /// <param name="value">
        /// The value to set for the claim. If the value is null, then the claim is simply removed.
        /// </param>
        [UsedImplicitly]
        public static void SetSingleClaimValue(this ClaimsIdentity identity, string claimType, uint? value)
        {
            identity.RemoveAll(claimType);
            if (!value.HasValue) return;
            identity.AddClaim(claimType, value.Value);
        }

        /// <summary>
        /// Sets the value of a claim as a <see cref="ulong"/>.
        /// </summary>
        /// <param name="identity">The identity that contains the claims to search.</param>
        /// <param name="claimType">The claim type of the claim to set.</param>
        /// <param name="value">
        /// The value to set for the claim. If the value is null, then the claim is simply removed.
        /// </param>
        [UsedImplicitly]
        public static void SetSingleClaimValue(this ClaimsIdentity identity, string claimType, ulong? value)
        {
            identity.RemoveAll(claimType);
            if (!value.HasValue) return;
            identity.AddClaim(claimType, value.Value);
        }

        /// <summary>
        /// Gets the single value for a given claim type. Please note that this function will still
        /// return false if there's more than one claim with the same type assigned to the identity.
        /// </summary>
        /// <typeparam name="TValue">The type of the claim value.</typeparam>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="converter">
        /// A delegate to call for each claim value to convert it to the output type.
        /// </param>
        /// <param name="value">An output variable that receives the value on success.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the retrieved
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        private static bool TryGetSingleClaimValueAs<TValue>(ClaimsIdentity identity, string claimType, Func<string, TValue> converter, out TValue value)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => claimType);
            ArgumentValidator.ValidateNotNull(() => converter);

            value = default(TValue);
            try
            {
                var sequence = identity.FindAll(claimType)
                    .Select(claim => claim.Value)
                    .Select(converter);
                value = sequence.Single();
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;   // either the sequemce contains no elements, or contains more than one.
            }
        }

        /// <summary>
        /// Gets the single value for a given claim type as a <see langword="bool"/>. Please note
        /// that this function will still return false if there's more than one claim with the same
        /// type assigned to the identity.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">An output variable that receives the value on success.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static bool TryGetSingleClaimValueAsBoolean(this ClaimsIdentity identity, string claimType, out bool value)
        {
            return TryGetSingleClaimValueAs(identity, claimType, bool.Parse, out value);
        }

        /// <summary>
        /// Gets the single value for a given claim type as a <see langword="DateTime"/>. Please
        /// note that this function will still return false if there's more than one claim with the
        /// same type assigned to the identity.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">An output variable that receives the value on success.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static bool TryGetSingleClaimValueAsDateTime(this ClaimsIdentity identity, string claimType, out DateTime value)
        {
            DateTime ParseValueAsUTC(string s)
            {
                return DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal);
            }
            return TryGetSingleClaimValueAs(identity, claimType, ParseValueAsUTC, out value);
        }

        /// <summary>
        /// Gets the single value for a given claim type as a <see langword="double"/>. Please
        /// note that this function will still return false if there's more than one claim with the
        /// same type assigned to the identity.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">An output variable that receives the value on success.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static bool TryGetSingleClaimValueAsDouble(this ClaimsIdentity identity, string claimType, out double value)
        {
            return TryGetSingleClaimValueAs(identity, claimType, double.Parse, out value);
        }

        /// <summary>
        /// Gets the single value for a given claim type as a <see langword="Guid"/>. Please note
        /// that this function will still return false if there's more than one claim with the same
        /// type assigned to the identity.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">An output variable that receives the value on success.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static bool TryGetSingleClaimValueAsGuid(this ClaimsIdentity identity, string claimType, out Guid value)
        {
            return TryGetSingleClaimValueAs(identity, claimType, Guid.Parse, out value);
        }

        /// <summary>
        /// Gets the single value for a given claim type as a <see langword="int"/>. Please note
        /// that this function will still return false if there's more than one claim with the same
        /// type assigned to the identity.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">An output variable that receives the value on success.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static bool TryGetSingleClaimValueAsInt32(this ClaimsIdentity identity, string claimType, out int value)
        {
            return TryGetSingleClaimValueAs(identity, claimType, int.Parse, out value);
        }

        /// <summary>
        /// Gets the single value for a given claim type as a <see langword="long"/>. Please note
        /// that this function will still return false if there's more than one claim with the same
        /// type assigned to the identity.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">An output variable that receives the value on success.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static bool TryGetSingleClaimValueAsInt64(this ClaimsIdentity identity, string claimType, out long value)
        {
            return TryGetSingleClaimValueAs(identity, claimType, long.Parse, out value);
        }

        /// <summary>
        /// Gets the single value for a given claim type as a <see langword="string"/>. Please note
        /// that this function will still return false if there's more than one claim with the same
        /// type assigned to the identity.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">An output variable that receives the value on success.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static bool TryGetSingleClaimValueAsString(this ClaimsIdentity identity, string claimType, out string value)
        {
            return TryGetSingleClaimValueAs(identity, claimType, (s => s), out value);
        }

        /// <summary>
        /// Gets the single value for a given claim type as a <see langword="uint"/>. Please note
        /// that this function will still return false if there's more than one claim with the same
        /// type assigned to the identity.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">An output variable that receives the value on success.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static bool TryGetSingleClaimValueAsUInt32(this ClaimsIdentity identity, string claimType, out uint value)
        {
            return TryGetSingleClaimValueAs(identity, claimType, uint.Parse, out value);
        }

        /// <summary>
        /// Gets the single value for a given claim type as a <see langword="ulong"/>. Please note
        /// that this function will still return false if there's more than one claim with the same
        /// type assigned to the identity.
        /// </summary>
        /// <param name="identity">
        /// The <see cref="ClaimsIdentity"/> that contains the claims to search.
        /// </param>
        /// <param name="claimType">
        /// A <see cref="ClaimTypes"/> or <see cref="CommonClaimTypes"/> constant value, or a
        /// custom claim type string.
        /// </param>
        /// <param name="value">An output variable that receives the value on success.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> object that can be used to iterate over the available
        /// claim values.
        /// </returns>
        [UsedImplicitly]
        public static bool TryGetSingleClaimValueAsUInt64(this ClaimsIdentity identity, string claimType, out ulong value)
        {
            return TryGetSingleClaimValueAs(identity, claimType, ulong.Parse, out value);
        }
    }
}
