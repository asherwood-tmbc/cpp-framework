using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Resources;
using CPP.Framework.Globalization;
using CPP.Framework.Threading;

namespace CPP.Framework.Resources
{
    /// <summary>
    /// Provides access to string resources within a single file group.
    /// </summary>
    public abstract class ResourceContainer
    {
        /// <summary>
        /// The <see cref="MultiAccessLock"/> used to synchronize access to the object across
        /// multiple threads.
        /// </summary>
        private readonly MultiAccessLock _syncLock = new MultiAccessLock();

        /// <summary>
        /// The reference to the <see cref="ResourceManager"/> object that manages the resources.
        /// </summary>
        private ResourceManager _resourceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceContainer"/> class. 
        /// </summary>
        /// <param name="groupName">
        /// The name of the base resource file group for the container.
        /// </param>
        protected ResourceContainer(string groupName)
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => groupName);
            this.GroupName = groupName;
        }

        /// <summary>
        /// Gets the name of the base resource file group for the container.
        /// </summary>
        public string GroupName { get; private set; }

        /// <summary>
        /// Gets the resource manager for the container.
        /// </summary>
        /// <returns>A <see cref="ResourceManager"/> instance.</returns>
        private ResourceManager GetResourceManager()
        {
            using (_syncLock.GetReaderAccess())
            {
                if (_resourceManager != null) return _resourceManager;
            }
            using (_syncLock.GetWriterAccess())
            {
                if (_resourceManager == null)
                {
                    var typeInfo = this.GetType();
                    var baseName = string.Join(".", new { typeInfo.Namespace, this.GroupName, });
                    _resourceManager = new ResourceManager(baseName, typeInfo.Assembly);
                }
                return _resourceManager;
            }
        }

        /// <summary>
        /// Gets the <see cref="CultureInfo"/> for the current resource container.
        /// </summary>
        /// <returns>A <see cref="CultureInfo"/> instance.</returns>
        protected virtual CultureInfo GetResourceCultureInfo() => CultureInfoService.Current.ResourceCulture;

        /// <summary>
        /// Loads a string from the resource file.
        /// </summary>
        /// <param name="expression">An expression that evaluates to the container property for the string to load.</param>
        /// <param name="fallback">An optional fallback string to use if the value cannot be loaded.</param>
        /// <returns>The resource string value.</returns>
        protected virtual string GetResourceString(Expression<Func<string>> expression, string fallback = "")
        {
            ArgumentValidator.ValidateNotNull(() => expression);
            var propertyName = expression.GetMemberName();
            return this.GetResourceString(propertyName, fallback);
        }

        /// <summary>
        /// Loads a string from the resource file.
        /// </summary>
        /// <param name="resourceName">The name of the resource string to load.</param>
        /// <param name="fallback">A optional fallback string to use if the value cannot be loaded.</param>
        /// <returns>The resource string value.</returns>
        protected virtual string GetResourceString(string resourceName, string fallback = "")
        {
            ArgumentValidator.ValidateNotNullOrWhiteSpace(() => resourceName);
            fallback = (fallback ?? string.Empty);
            try
            {
                var culture = this.GetResourceCultureInfo();
                return this.GetResourceManager().GetString(resourceName, culture);
            }
            catch (MissingManifestResourceException)
            {
                return fallback;
            }
            catch (MissingSatelliteAssemblyException)
            {
                return fallback;
            }
        }
    }
}
