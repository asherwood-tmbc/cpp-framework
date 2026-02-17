using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework.Services
{
    /// <summary>
    /// Abstract base class for all objects that provide code services to an application.
    /// </summary>
    public abstract class CodeService : ICodeService, IDisposable
    {
        /// <summary>
        /// Finalizes an instance of the <see cref="CodeService"/> class. 
        /// </summary>
        ~CodeService() => this.Dispose(false);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        [ExcludeFromCodeCoverage]
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting 
        /// unmanaged resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>True</c> if the object is being disposed explicitly (e.g. from a <c>using</c> block), 
        /// or <c>false</c> if it is being disposed from the finalized.</param>
        protected virtual void Dispose(bool disposing) { }
    }
}
