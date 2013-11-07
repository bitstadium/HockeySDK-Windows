using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HockeyApp
{
    public class ManifestHelper
    {

        private static readonly ManifestHelper instance = new ManifestHelper();
        static ManifestHelper() { }

        private ManifestHelper() { 
        }
        
        public static ManifestHelper Instance
        {
            get
            {
                return instance;
            }
        }

        // Idea based on http://bjorn.kuiper.nu/2011/10/01/wp7-notify-user-of-new-application-version/
        public static String GetAppVersion()
        {
            return Instance.GetValueFromManifest("Version");
        }

        public static String GetProductID()
        {
            return Instance.GetValueFromManifest("ProductID");
        }

        internal String GetValueFromManifest(String key)
        {
            try
            {
                StreamReader reader = getManifestReader();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    int begin = line.IndexOf(" " + key + "=\"", StringComparison.InvariantCulture);
                    if (begin >= 0)
                    {
                        int end = line.IndexOf("\"", begin + key.Length + 3, StringComparison.InvariantCulture);
                        if (end >= 0)
                        {
                            return line.Substring(begin + key.Length + 3, end - begin - key.Length - 3);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore all exceptions
            }

            return "";
        }

        internal static StreamReader getManifestReader()
        {
            Uri manifest = new Uri("WMAppManifest.xml", UriKind.Relative);
            var stream = Application.GetResourceStream(manifest);
            if (stream != null)
            {
                return new StreamReader(stream.Stream);
            }
            else
            {
                return null;
            }
        }


    }
}
