using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace HockeyApp
{
    /// <summary>
    /// Helper class to reaed values from the WPManifest
    /// </summary>
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
                XElement appxml = System.Xml.Linq.XElement.Load("WMAppManifest.xml");
                var appElement = (from manifestData in appxml.Descendants("App") select manifestData).SingleOrDefault();
                return appElement.Attribute(key).Value;
            }
            catch (Exception)
            {
                // Ignore all exceptions
            }

            return "";
        }
    }
}
