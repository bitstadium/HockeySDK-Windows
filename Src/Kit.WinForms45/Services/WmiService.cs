using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Globalization;

namespace Microsoft.HockeyApp.Services
{
    sealed class WmiService
    {
        public IEnumerable<string> GetManagementProperties(string path, string property)
        {
            try
            {
                var select = string.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1}", property, path);
                return from x in new ManagementObjectSearcher(@select).Get().Cast<ManagementObject>()
                       let p = x.GetPropertyValue(property)
                       where p != null
                       select p.ToString();
            }
            catch
            {
            }
            return Enumerable.Empty<string>();
        }

        public string GetManagementProperty(string path, string property)
        {
            return GetManagementProperties(path, property).FirstOrDefault() ?? "Unknown";
        }
    }
}
