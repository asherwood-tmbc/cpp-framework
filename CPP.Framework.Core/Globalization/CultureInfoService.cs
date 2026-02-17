using System.Globalization;
using System.Threading;

namespace CPP.Framework.Globalization
{
    /// <summary>
    /// Provides access to the static members of the system <see cref="CultureInfo"/> class.
    /// </summary>
    public class CultureInfoService : SingletonServiceBase
    {
        /// <summary>
        /// The current instance of the service for the application.
        /// </summary>
        private static readonly ServiceInstance<CultureInfoService> _ServiceInstance = new ServiceInstance<CultureInfoService>();

        /// <summary>
        /// The resource <see cref="CultureInfo"/> value for the current thread.
        /// </summary>
        private readonly ThreadLocal<CultureInfo> _resourceCulture;

        /// <summary>
        /// Initializes a new instance of the <see cref="CultureInfoService"/> class. 
        /// </summary>
        protected CultureInfoService()
        {
            _resourceCulture = new ThreadLocal<CultureInfo>(() => this.CurrentUICulture);
        }

        /// <summary>
        /// Gets the current instance of the service for the application.
        /// </summary>
        public static CultureInfoService Current { get { return _ServiceInstance.GetInstance(); } }

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo" /> object that represents the culture used by 
        /// the current thread.
        /// </summary>
        public virtual CultureInfo CurrentCulture => CultureInfo.CurrentCulture;

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> object that represents the current user 
        /// interface culture used by the Resource Manager to look up culture-specific resources at 
        /// run time.
        /// </summary>
        public virtual CultureInfo CurrentUICulture => CultureInfo.CurrentUICulture;

        /// <summary>
        /// Gets or sets the default culture for threads in the current application domain.
        /// </summary>
        public virtual CultureInfo DefaultThreadCurrentCulture
        {
            get => CultureInfo.DefaultThreadCurrentCulture;
            set => CultureInfo.DefaultThreadCurrentCulture = value;
        }

        /// <summary>
        /// Gets or sets the default UI culture for threads in the current application domain.
        /// </summary>
        public virtual CultureInfo DefaultThreadCurrentUICulture
        {
            get => CultureInfo.DefaultThreadCurrentUICulture;
            set => CultureInfo.DefaultThreadCurrentUICulture = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="CultureInfo"/> object that overrides the culture used by 
        /// the Resource Manager to look up culture-specific resources at run time for the current 
        /// thread.
        /// </summary>
        public virtual CultureInfo ResourceCulture
        {
            get => _resourceCulture.Value;
            set => _resourceCulture.Value = (value ?? this.CurrentUICulture);
        }

        /// <summary>
        /// Creates a <see cref="CultureInfo"/> that represents the specific culture that is 
        /// associated with the specified name.
        /// </summary>
        /// <param name="name">A predefined <see cref="CultureInfo"/> name or the name of an existing <see cref="CultureInfo"/> object. <paramref name="name"/> is not case-sensitive.</param>
        /// <returns>
        ///     <para>A <see cref="CultureInfo"/> object that represents:</para>
        ///     <para>The invariant culture, if <paramref name="name"/> is an empty string ("").</para>
        ///     <para>-or-</para>
        ///     <para>The specific culture associated with <paramref name="name"/>, if <paramref name="name"/> is a neutral culture.</para>
        ///     <para>-or-</para>
        ///     <para>The culture specified by <paramref name="name"/>, if <paramref name="name"/> is already a specific culture.</para>
        /// </returns>
        public virtual CultureInfo CreateSpecificCulture(string name) => CultureInfo.CreateSpecificCulture(name);

        /// <summary>
        /// Retrieves a cached, read-only instance of a culture by using the specified culture 
        /// identifier.
        /// </summary>
        /// <param name="culture">A locale identifier (LCID).</param>
        /// <returns>A read-only <see cref="CultureInfo"/> object.</returns>
        public virtual CultureInfo GetCultureInfo(int culture) => CultureInfo.GetCultureInfo(culture);

        /// <summary>
        /// Retrieves a cached, read-only instance of a culture by using the specified culture 
        /// identifier.
        /// </summary>
        /// <param name="name">The name of the culture. <paramref name="name"/> is not case-sensitive.</param>
        /// <returns>A read-only <see cref="CultureInfo"/> object.</returns>
        public virtual CultureInfo GetCultureInfo(string name) => CultureInfo.GetCultureInfo(name);

        /// <summary>
        /// Retrieves a cached, read-only instance of a culture by using the specified culture 
        /// identifier.
        /// </summary>
        /// <param name="name">The name of the culture. <paramref name="name"/> is not case-sensitive.</param>
        /// <param name="altName">The name of a culture that supplies the <see cref="TextInfo"/> and <see cref="CompareInfo"/> objects used to initialize <paramref name="name"/>. <paramref name="altName"/> is not case-sensitive.</param>
        /// <returns>A read-only <see cref="CultureInfo"/> object.</returns>
        public virtual CultureInfo GetCultureInfo(string name, string altName) => CultureInfo.GetCultureInfo(name, altName);

        /// <summary>
        /// Gets the default <see cref="CultureInfo"/> object that represents the culture used by
        /// the Resource Manager to look up culture-specific resources at run time.
        /// </summary>
        /// <returns>A <see cref="CultureInfo"/> object.</returns>
        protected virtual CultureInfo GetDefaultResourceCulture() => this.CurrentUICulture;
    }
}
