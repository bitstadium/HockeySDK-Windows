using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace Microsoft.HockeyApp.Services
{
    sealed class WmiService
    {
        public IEnumerable<string> GetManagementProperties(string path, string property)
        {
            try
            {
                return from x in new ManagementObjectSearcher($"SELECT {property} FROM {path}").Get().Cast<ManagementObject>()
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
            try
            {
                var name = GetManagementProperties(path, property).FirstOrDefault();
                return name != null ? name.ToString() : "Unknown";
            }
            catch
            {
            }
            return null;
        }
    }
}
