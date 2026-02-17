using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

using JetBrains.Annotations;

namespace CPP.Framework.Security
{
    /// <summary>
    /// Thrown during an authorization check when an identity assigned to the principal has not
    /// been authenticated
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class SecurityAuthenticationException : SecurityException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthenticationException"/> class.
        /// </summary>
        [UsedImplicitly]
        public SecurityAuthenticationException() : base(ErrorStrings.InvalidIdentityAuthentication) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthenticationException"/> class.
        /// </summary>
        /// <param name="innerException">The exception that caused the current exception.</param>
        [UsedImplicitly]
        public SecurityAuthenticationException(Exception innerException) : base(ErrorStrings.InvalidIdentityAuthentication, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthenticationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the reason for the exception.</param>
        protected SecurityAuthenticationException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthenticationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the reason for the exception.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        protected SecurityAuthenticationException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthenticationException" /> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="info "/> is <see langword=" null"/>.
        /// </exception>
        [SecuritySafeCritical]
        protected SecurityAuthenticationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
