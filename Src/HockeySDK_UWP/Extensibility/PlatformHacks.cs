namespace Microsoft.HockeyApp.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using global::Windows.Devices.Enumeration.Pnp;

    internal static class PlatformHacks
    {
        private const string DeviceDriverKey = "{A8B865DD-2E3D-4094-AD97-E593A70C75D6}";
        private const string DeviceDriverVersionKey = PlatformHacks.DeviceDriverKey + ",3";
        private const string DeviceDriverProviderKey = PlatformHacks.DeviceDriverKey + ",9";
        private const string RootContainer = "{00000000-0000-0000-FFFF-FFFFFFFFFFFF}";
        private const string RootContainerQuery = "System.Devices.ContainerId:=\"" + PlatformHacks.RootContainer + "\"";

        public static async Task<string> GetWindowsVersion()
        {
            string[] requestedProperties = new string[]
                                               {
                                                   PlatformHacks.DeviceDriverVersionKey, 
                                                   PlatformHacks.DeviceDriverProviderKey
                                               };

            PnpObjectCollection pnpObjects = await PnpObject.FindAllAsync(PnpObjectType.Device, requestedProperties, PlatformHacks.RootContainerQuery);

            string guessedVersion = pnpObjects.Select(item => new ProviderVersionPair
                                                                  {
                                                                      Provider = (string)GetValueOrDefault(item.Properties, PlatformHacks.DeviceDriverProviderKey),
                                                                      Version = (string)GetValueOrDefault(item.Properties, PlatformHacks.DeviceDriverVersionKey)
                                                                  })
                                              .Where(item => string.IsNullOrEmpty(item.Version) == false)
                                              .Where(item => string.Compare(item.Provider, "Microsoft", StringComparison.Ordinal) == 0)
                                              .GroupBy(item => item.Version)
                                              .OrderByDescending(item => item.Count())
                                              .Select(item => item.Key)
                                              .First();

            return guessedVersion;
        }

        private static TValue GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }

        private class ProviderVersionPair
        {
            public string Provider { get; set; }

            public string Version { get; set; }
        }
    }
}