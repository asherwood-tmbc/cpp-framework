using System;
using System.Runtime.InteropServices;

namespace CPP.Framework
{
    /// <summary>
    /// Service class used to generate unique identifiers for objects.
    /// </summary>
    public class GuidGeneratorService : SingletonServiceBase
    {
        /// <summary>
        /// The reference to the current service instance for the application.
        /// </summary>
        private static readonly ServiceInstance<GuidGeneratorService> _ServiceInstance = new ServiceInstance<GuidGeneratorService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GuidGeneratorService"/> class. 
        /// </summary>
        protected GuidGeneratorService() { }

        /// <summary>
        /// Gets a reference to the current instance for the application.
        /// </summary>
        public static GuidGeneratorService Current => _ServiceInstance.GetInstance();

        /// <summary>
        /// Creates a new, sequential <see cref="Guid"/> based on the local machine's MAC address.
        /// </summary>
        /// <param name="rguid">
        /// An output value that receives the generated <see cref="Guid"/> on success.
        /// </param>
        /// <returns>A WIN32 result code.</returns>
        [DllImport("rpcrt4.dll", SetLastError = true)]
        private static extern int UuidCreateSequential(out Guid rguid);

        /// <summary>
        /// Generates a new unique identifier value.
        /// </summary>
        /// <returns>A <see cref="Guid"/> value.</returns>
        public virtual Guid NewGuid() => this.NewGuid(false);

        /// <summary>
        /// Generates a new unique identifier value.
        /// </summary>
        /// <param name="sequential">
        /// True to generate a sequential <see cref="Guid"/>, or false to generate a standard 
        /// <c>Guid</c>.
        /// </param>
        /// <returns>A <see cref="Guid"/> value.</returns>
        public virtual Guid NewGuid(bool sequential)
        {
            if (sequential)
            {
                UuidCreateSequential(out var rguid);
                return rguid;
            }
            return Guid.NewGuid();
        }

        /// <summary>
        /// Generates a new unique identifier value for a target object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="target">The target object.</param>
        /// <returns>A <see cref="Guid"/> value.</returns>
        public virtual Guid NewGuid<T>(T target) => this.NewGuid();

        /// <summary>
        /// Generates a new unique identifier value for a target object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="target">The target object.</param>
        /// <param name="sequential">
        /// True to generate a sequential <see cref="Guid"/>, or false to generate a standard 
        /// <c>Guid</c>.
        /// </param>
        /// <returns>A <see cref="Guid"/> value.</returns>
        public virtual Guid NewGuid<T>(T target, bool sequential) => this.NewGuid(sequential);

        /// <summary>
        /// Generates a new unique identifier that can be used for indexed column values in MS SQL
        /// with a lower probability of producing high page fragmentation.
        /// </summary>
        /// <returns>A sequential <see cref="Guid"/> value.</returns>
        public virtual Guid SqlGuid()
        {
            /***
             * the NEWSEQUENTIALID() MS SQL function uses the UuidCreateSequential Win32 API to
             * generate the value (which is also what the GuidGeneratorService uses), but then
             * afterwards it changes the byte order for the members of the GUID from big-endian
             * to little-endian before returning the value, which we must also do before we can
             * use it as a UNIQUEIDENTIFIER. For reference, this is how the GUID structure is
             * defined by the Win32 API (from rpc.h):
             *
             *      typedef struct _GUID
             *      {
             *          DWORD Data1;    // UInt32, 4 Bytes
             *          WORD  Data2;    // UInt16, 2 Bytes
             *          WORD  Data3;    // UInt16, 2 Bytes
             *          BYTE  Data4[8]; // Byte Order is Irrelevant Here
             *      } GUID;
             *
             * More Info: https://blogs.msdn.microsoft.com/dbrowne/2012/07/03/how-to-generate-sequential-guids-for-sql-server-in-net/
             ***/

            var buffer = this.NewGuid(true).ToByteArray();
            Array.Reverse(buffer, 0, 4);
            Array.Reverse(buffer, 4, 2);
            Array.Reverse(buffer, 6, 2);
            return new Guid(buffer);
        }

        /// <summary>
        /// Generates a new unique identifier for a target object that can be used indexed column
        /// values in MS SQL with a lower probability of producing high page fragmentation.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="target">The target entity that will be using the id.</param>
        /// <returns>A sequential <see cref="Guid"/> value.</returns>
        public virtual Guid SqlGuid<T>(T target) where T : class => this.SqlGuid();
    }
}
