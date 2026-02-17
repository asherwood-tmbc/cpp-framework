using System;
using System.Diagnostics.CodeAnalysis;

namespace CPP.Framework
{
    /// <summary>
    /// Service wrapper class for the system <see cref="DateTime"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DateTimeService : SingletonServiceBase
    {
        /// <summary>
        /// The reference to the current service instance for the application.
        /// </summary>
        private static readonly ServiceInstance<DateTimeService> _ServiceInstance = new ServiceInstance<DateTimeService>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeService"/> class. 
        /// </summary>
        protected DateTimeService() { }

        /// <summary>
        /// Gets a reference to the current service instance for the application.
        /// </summary>
        public static DateTimeService Current => _ServiceInstance.GetInstance();

        /// <summary>
        /// Gets a <see cref="DateTime"/> object that is set to the current date and time on this
        /// computer, expressed as the local time zone.
        /// </summary>
        public virtual DateTime Now => DateTime.Now;

        /// <summary>
        /// Gets a <see cref="DateTime"/> object that is set to the current date and time on this
        /// computer, expressed as the Coordinated Universal Time (UTC).
        /// </summary>
        public virtual DateTime UtcNow => DateTime.UtcNow;
    }
}
