using System;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Claims;

using JetBrains.Annotations;

namespace CPP.Framework.Security
{
    /// <summary>
    /// Thrown when attempting to retrieve the value of a <see cref="Claim"/> that is not available
    /// for a <see cref="ClaimsIdentity"/>.
    /// </summary>
    [Serializable]
    public class SecurityClaimNotFoundException : SecurityException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityClaimNotFoundException"/> class.
        /// </summary>
        /// <param name="claimType">The type of the claim that was not found.</param>
        public SecurityClaimNotFoundException(string claimType) : base(FormatMessage(claimType)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityClaimNotFoundException"/> class.
        /// </summary>
        /// <param name="claimType">The type of the claim that was not found.</param>
        /// <param name="innerException">
        /// The <see cref="Exception"/> that caused the current exception.
        /// </param>
        public SecurityClaimNotFoundException(string claimType, Exception innerException) : base(FormatMessage(claimType), innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityClaimNotFoundException" /> class
        /// with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo" /> that holds the serialized object data about the
        /// exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext" /> that contains contextual information about the
        /// source or destination.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="info" /> parameter is <see langword="null" />.
        /// </exception>
        /// <exception cref="SerializationException">
        /// The class name is <see langword="null" /> or <see cref="Exception.HResult" /> is zero.
        /// </exception>
        [UsedImplicitly]
        protected SecurityClaimNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.ClaimType = info.GetString(nameof(ClaimType));
        }

        /// <summary>
        /// Gets the type of the claim that was not found.
        /// </summary>
        public string ClaimType { get; }

        /// <summary>
        /// Formats the message for the exception.
        /// </summary>
        /// <param name="claimType">The type of the claim that was not found.</param>
        /// <returns>A string that contains the formatted value.</returns>
        private static string FormatMessage(string claimType)
        {
            var message = string.Format(
                ErrorStrings.SecurityClaimTypeNotFound,
                claimType);
            return message;
        }

        /// <summary>When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.</summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is a null reference (Nothing in Visual Basic). </exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(ClaimType), this.ClaimType, typeof(string));
            base.GetObjectData(info, context);
        }
    }
}
