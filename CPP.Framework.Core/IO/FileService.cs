using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;

namespace CPP.Framework.IO
{
    /// <summary>
    /// Service wrapper class for the system <see cref="File"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FileService : SingletonServiceBase
    {
        /// <summary>
        /// The current reference to the service for the application.
        /// </summary>
        private static readonly ServiceInstance<FileService> _ServiceInstance = new ServiceInstance<FileService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class. 
        /// </summary>
        protected FileService() { }

        /// <summary>
        /// Gets the current reference to the service for the application.
        /// </summary>
        public static FileService Current => _ServiceInstance.GetInstance();

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns>
        ///     True if the caller has the required permissions and path contains the name of an 
        ///     existing file; otherwise, false. This method also returns false if path is null, an 
        ///     invalid path, or a zero-length string. If the caller does not have sufficient 
        ///     permissions to read the specified file, no exception is thrown and the method 
        ///     returns false regardless of the existence of path.
        /// </returns>
        [SecuritySafeCritical]
        public virtual bool Exists(string path) => File.Exists(path);
    }
}
