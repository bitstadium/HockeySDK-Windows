using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Globalization;

namespace Microsoft.HockeyApp.Services.Device
{
    /// <summary>
    /// Provides basic access to Wmi properties
    /// </summary>
    internal sealed class WmiService
    {
        /// <summary>
        /// Gets the property values for the given management query.
        /// </summary>
        /// <typeparam name="T">The generic type parameter</typeparam>
        /// <param name="path">The management path.</param>
        /// <param name="property">The management property.</param>
        /// <returns>The collection of found property values</returns>
        public IEnumerable<T> GetManagementProperties<T>(string path, string property)
        {
            try
            {
                var statement = string.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1}", property, path);
                var managementObjects = new ManagementObjectSearcher(statement).Get().Cast<ManagementObject>();
                return managementObjects.Select(x => x.GetPropertyValue(property)).Where(x => x != null).Cast<T>();
            }
            catch
            {
                return Enumerable.Empty<T>();
            }
        }

        /// <summary>
        /// Gets the property value for the given management query.
        /// </summary>
        /// <typeparam name="T">The generic type parameter</typeparam>
        /// <param name="path">The management path.</param>
        /// <param name="property">The management property.</param>
        /// <returns>The property value, or null if no value was found</returns>
        public T GetManagementProperty<T>(string path, string property)
        {
            return GetManagementProperties<T>(path, property).FirstOrDefault();
        }
    }
}