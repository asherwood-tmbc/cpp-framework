using System;
using System.Diagnostics.CodeAnalysis;
using System.Security;

namespace CPP.Framework
{
    /// <summary>
    /// Service wrapper class for the static members of the <see cref="TimeZoneInfo"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class TimeZoneInfoService : SingletonServiceBase
    {
        /// <summary>
        /// The default application time zone name.
        /// </summary>
        private const string DefaultTimeZone = "Pacific Standard Time";

        /// <summary>
        /// The reference to the current service implementation.
        /// </summary>
        private static readonly ServiceInstance<TimeZoneInfoService> _ServiceInstance = new ServiceInstance<TimeZoneInfoService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeZoneInfoService"/> class. 
        /// </summary>
        protected TimeZoneInfoService() { }

        /// <summary>
        /// Gets a reference to the current service implementation.
        /// </summary>
        public static TimeZoneInfoService Current => _ServiceInstance.GetInstance();

        /// <summary>
        /// Retrieves a <see cref="T:System.TimeZoneInfo"/> object from the registry based on its identifier.
        /// </summary>
        /// <param name="id">The time zone identifier, which corresponds to the <see cref="TimeZoneInfo.Id"/> property.</param>
        /// <returns>An object whose identifier is the value of the <paramref name="id"/> parameter.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="id"/> parameter is null.</exception>
        /// <exception cref="InvalidTimeZoneException">The time zone identifier was found, but the registry data is corrupted.</exception>
        /// <exception cref="OutOfMemoryException">The system does not have enough memory to hold information about the time zone.</exception>
        /// <exception cref="SecurityException">The process does not have the permissions required to read from the registry key that contains the time zone information.</exception>
        /// <exception cref="TimeZoneNotFoundException">The time zone identifier specified by <paramref name="id"/> was not found. This means that a registry key whose name matches <paramref name="id"/> does not exist, or that the key exists but does not contain any time zone data.</exception>
        public TimeZoneInfo FindSystemTimeZoneById(string id) => TimeZoneInfo.FindSystemTimeZoneById(id);

        /// <summary>
        /// Gets the default <see cref="TimeZoneInfo"/> object for the application.
        /// </summary>
        /// <returns>A <see cref="TimeZoneInfo"/> object.</returns>
        public virtual TimeZoneInfo GetDefaultTimeZoneInfo() => TimeZoneInfo.FindSystemTimeZoneById(DefaultTimeZone);
    }
}
