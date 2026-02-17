using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace CPP.Framework.Net.Http
{
    /// <summary>
    /// Thrown when a request fails for an HTTP service call.
    /// </summary>
    /// <typeparam name="TCode">The type of the failure status code.</typeparam>
    [ExcludeFromCodeCoverage]
    public abstract class HttpServiceRequestFailedException<TCode> : HttpServiceRequestFailedException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServiceRequestFailedException{TCode}"/> class. 
        /// </summary>
        /// <param name="errorCode">
        /// The error code for the failure.
        /// </param>
        /// <param name="message">
        /// The error message for the failure.
        /// </param>
        protected HttpServiceRequestFailedException(TCode errorCode, string message) : base(message)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServiceRequestFailedException{TCode}"/> class. 
        /// </summary>
        /// <param name="errorCode">
        /// The error code for the failure.
        /// </param>
        /// <param name="message">
        /// The error message for the failure.
        /// </param>
        /// <param name="innerException">
        /// The exception that caused the current exception.
        /// </param>
        protected HttpServiceRequestFailedException(TCode errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpServiceRequestFailedException{TCode}"/> class. 
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> that holds the serialized object data.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/> that contains contextual information about the source or destination.
        /// </param>
        protected HttpServiceRequestFailedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            info.LoadProperty(this, (x => x.ErrorCode));
        }

        /// <summary>
        /// Gets the error code for the failure.
        /// </summary>
        public TCode ErrorCode { get; }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="SerializationInfo"/> with 
        /// information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is a null reference.</exception>
        /// <PermissionSet>
        ///     <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*"/>
        ///     <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter"/>
        /// </PermissionSet>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SaveProperty(this, (x => x.ErrorCode));
            base.GetObjectData(info, context);
        }
    }
}
