using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CPP.Framework.Diagnostics.Testing
{
    /// <summary>
    /// <see cref="MemoryStream"/> that can be used for mock testing serialization.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TestMemoryStream : MemoryStream
    {
        /// <summary>
        /// Releases the unmanaged resources used by the class and optionally releases the managed 
        /// resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing) base.Dispose(disposing);
        }

        /// <summary>
        /// Manually disposes of the stream and closes it.
        /// </summary>
        public void ManualDispose() { base.Dispose(true); }
    }
}
