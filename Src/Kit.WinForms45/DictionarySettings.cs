using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// Dictionary based app settings that HockeyApp.Core needs to store user id and session data
    /// </summary>
    sealed class DictionarySettings
    {
        private static DictionarySettings _current;
        private static object _loadSyncObject = new object();

        /// <summary>
        /// Settings instance for this AppDomain
        /// </summary>
        public static DictionarySettings Current
        {
            get
            {
                lock (_loadSyncObject)
                {
                    return _current ?? (_current = new DictionarySettings());
                }
            }
        }

        private const string FILE_NAME = "settings.json";
        private const string FOLDER_NAME = "HockeyApp";

        private readonly IDictionary<string, object> _local;
        private readonly IDictionary<string, object> _roaming;

        private DictionarySettings()
        {
            _local = Load(Path.Combine(Application.LocalUserAppDataPath, FOLDER_NAME));
            _roaming = Load(Path.Combine(Application.UserAppDataPath, FOLDER_NAME));

            Application.ApplicationExit += (o, e) => Save();
        }

        /// <summary>
        /// User's local HockeyApp settings for this application
        /// </summary>
        public IDictionary<string, object> LocalSettings => _local;

        /// <summary>
        /// User's roaming HockeyApp settings for this Application
        /// </summary>
        public IDictionary<string, object> RoamingSettings => _roaming;

        /// <summary>
        /// Saves both local and roaming settings to disk
        /// </summary>
        public void Save()
        {
            Save(_local, Path.Combine(Application.LocalUserAppDataPath, FOLDER_NAME));
            Save(_roaming, Path.Combine(Application.UserAppDataPath, FOLDER_NAME));
        }

        private static void Save(IDictionary<string, object> settings, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var serializer = new DataContractJsonSerializer(typeof(ConcurrentDictionary<string, object>), new DataContractJsonSerializerSettings()
            {
                UseSimpleDictionaryFormat = true,
                KnownTypes = new Type[] { typeof(DateTimeOffset) },
                EmitTypeInformation = EmitTypeInformation.AsNeeded,
                RootName = "root"
            });

            using (var writer = File.CreateText(Path.Combine(path, FILE_NAME)))
            {
                serializer.WriteObject(writer.BaseStream, settings);
            }
        }

        private static IDictionary<string, object> Load(string path)
        {
            var fullPath = Path.Combine(path, FILE_NAME);
            if (File.Exists(fullPath))
            {
                try
                {
                    var serializer = new DataContractJsonSerializer(typeof(ConcurrentDictionary<string, object>), new DataContractJsonSerializerSettings()
                    {
                        UseSimpleDictionaryFormat = true,
                        KnownTypes = new Type[] { typeof(DateTimeOffset) },
                        EmitTypeInformation = EmitTypeInformation.AsNeeded,
                        RootName = "root"
                    });

                    using (var stream = File.OpenRead(fullPath))
                    {
                        var d = serializer.ReadObject(stream);

                        return d as IDictionary<string, object>;
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error deserializing settings: " + e.Message);
                    try
                    {
                        File.Delete(fullPath);
                    }
                    catch { }
                }
            }

            return new ConcurrentDictionary<string, object>(); ;
        }
    }
}
