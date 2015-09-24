using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace HockeyApp.Tools
{
    internal class AppxManifest
    {

        private AppxManifest() {}

        private static AppxManifest _instance;
        public static AppxManifest Current
        {
            get { return _instance; }
        }

        internal static void InitializeManifest()
        {
            if (_instance == null)
            {
                _instance = new AppxManifest();
                var x = Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("AppxManifest.xml").AsTask().Result;
                var stream = x.OpenReadAsync().AsTask().Result;
                var root = XElement.Load(XmlReader.Create(stream.AsStreamForRead()));
                _instance.Package = new InternalPackage(root);
            }
        }

        public IPackage Package { get; private set; }

        private class InternalPackage :IPackage
        {
            public InternalPackage(XElement element)
            {
                if (element.Element(XName.Get("Identity", element.GetDefaultNamespace().NamespaceName)) != null)
                {
                    this.Identity = new InternalIdentity(element.Element(XName.Get("Identity", element.GetDefaultNamespace().NamespaceName)));
                }
                if(element.Element(XName.Get("PhoneIdentity", "http://schemas.microsoft.com/appx/2014/phone/manifest")) != null) {
                    this.PhoneIdentity = new PhoneIdentity(element.Element(XName.Get("PhoneIdentity", "http://schemas.microsoft.com/appx/2014/phone/manifest")));
                }

                this.Properties = new Dictionary<string, string>();
                var xProps = element.Element(XName.Get("Properties", element.GetDefaultNamespace().NamespaceName));
                if (xProps != null)
                {
                    foreach (XElement current in xProps.Elements())
                    {
                        this.Properties.Add(current.Name.LocalName, current.Value);
                    }
                }
            }
            public IIdentity Identity { get; private set; }
            public IPhoneIdentity PhoneIdentity { get; private set; }
            public Dictionary<string, string> Properties { get; private set; }
        }

        private class PhoneIdentity : IPhoneIdentity
        {
            public PhoneIdentity(XElement element)
            {
                this.PhoneProductId = element.Attribute("PhoneProductId").Value;
                this.PhoneProductId = element.Attribute("PhonePublisherId").Value;
            }
            public string PhoneProductId { get; private set; }
            public string PhonePublisherId { get; private set; }
        }

        private class InternalIdentity : IIdentity
        {
            public InternalIdentity(XElement element)
            {
                if (element != null)
                {
                    this.Name = element.Attribute("Name").Value;
                    this.Publisher = element.Attribute("Publisher").Value;
                    this.Version = element.Attribute("Version").Value;
                    this.ProcessorArchitecture = element.Attribute("ProcessorArchitecture").Value;
                }
            }
            public string Name { get; private set; }
            public string Publisher { get; private set; }
            public string Version { get; private set; }
            public string ProcessorArchitecture { get; private set; }
        }
    }

    internal interface IPhoneIdentity
    {
        string PhoneProductId { get; }
        string PhonePublisherId { get; }
    }
    internal interface IPackage
    {
        IIdentity Identity { get; }
        IPhoneIdentity PhoneIdentity { get; }
        Dictionary<string, string> Properties { get; }
    }

    internal interface IIdentity
    {
        string Name { get; }
        string ProcessorArchitecture { get; }
        string Publisher { get; }
        string Version { get; }
    }
    
}
