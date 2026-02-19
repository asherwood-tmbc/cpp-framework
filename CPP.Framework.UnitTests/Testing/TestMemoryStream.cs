using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace CPP.Framework.UnitTests.Testing
{
    [ExcludeFromCodeCoverage]
    public class TestMemoryStream : MemoryStream
    {
        protected override void Dispose(bool disposing)
        {
            if (!disposing) base.Dispose(disposing);
        }

        public void ManualDispose() { base.Dispose(true); }
    }
}
