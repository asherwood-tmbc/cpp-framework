using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security;

using JetBrains.Annotations;

namespace CPP.Framework.Security
{
    /// <summary>
    /// Thrown during an authorization check when the identity assigned to the principal is not
    /// authorized to access the requested resource.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [Serializable]
    public class SecurityAuthorizationException : SecurityException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationException"/> class.
        /// </summary>
        [UsedImplicitly]
        public SecurityAuthorizationException() : base(ErrorStrings.IdentityAuthorizationFailed) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationException"/> class.
        /// </summary>
        /// <param name="inner">The exception that caused the current exception.</param>
        [UsedImplicitly]
        public SecurityAuthorizationException(Exception inner) : base(ErrorStrings.InvalidIdentityAuthentication, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the reason for the exception.</param>
        protected SecurityAuthorizationException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the reason for the exception.</param>
        /// <param name="innerException">The exception that caused the current exception.</param>
        protected SecurityAuthorizationException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityAuthorizationException" /> class with
        /// serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="info "/> is <see langword=" null"/>.
        /// </exception>
        [SecuritySafeCritical]
        protected SecurityAuthorizationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
