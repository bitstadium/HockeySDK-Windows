namespace Microsoft.HockeyApp.Extensibility
{
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// The reader is platform specific and applies to Windows Phone Silverlight applications only.
    /// </summary>
    internal partial class ComponentContextReader : IComponentContextReader
    {
        /// <summary>
        /// The name for this component.
        /// </summary>
        private string name;

        /// <summary>
        /// The version for this component.
        /// </summary>
        private string version;

        /// <summary>
        /// Gets the manifest's root level element.
        /// </summary>
        internal virtual XElement Manifest { get; private set; }

        /// <summary>
        /// Initializes the current instance with respect to the platform specific implementation.
        /// </summary>
        public void Initialize()
        {
            const string ManifestFileName = "WMAppManifest.xml";
            try
            {
                XDocument document = XDocument.Load(ManifestFileName);
                this.Manifest = document.Root;
            }
            catch (FileNotFoundException)
            {
                // if something went horribly wrong, the file won't be there ...
            }
            catch (XmlException)
            {
                // ... or the XML document is corrupt
            }
        }

        /// <summary>
        /// Gets the name for this application.
        /// </summary>
        /// <returns>The name of the application if found, Unknown otherwise.</returns>
        public string GetName()
        {
            if (this.name != null)
            {
                return this.name;
            }

            XElement root = this.Manifest;
            if (root == null)
            {
                return this.name = ComponentContextReader.UnknownComponentVersion;
            }

            XElement applicationElement = root.Element(XName.Get("App", string.Empty));
            if (applicationElement == null)
            {
                return this.name = ComponentContextReader.UnknownComponentVersion;
            }

            XAttribute versionAttribute = applicationElement.Attribute(XName.Get("Title"));
            if (versionAttribute == null)
            {
                return this.name = ComponentContextReader.UnknownComponentVersion;
            }

            string version = versionAttribute.Value;
            if (string.IsNullOrWhiteSpace(version) == false)
            {
                return this.name = version;
            }

            return this.name = ComponentContextReader.UnknownComponentVersion;
        }

        /// <summary>
        /// Gets the version for the current application. If the version cannot be found, we will return the passed in default.
        /// </summary>
        /// <returns>The extracted data.</returns>
        public string GetVersion()
        {
            if (this.version != null)
            {
                return this.version;
            }

            XElement root = this.Manifest;
            if (root == null)
            {
                return this.version = ComponentContextReader.UnknownComponentVersion;
            }

            XElement applicationElement = root.Element(XName.Get("App", string.Empty));
            if (applicationElement == null)
            {
                return this.version = ComponentContextReader.UnknownComponentVersion;
            }

            XAttribute versionAttribute = applicationElement.Attribute(XName.Get("Version"));
            if (versionAttribute == null)
            {
                return this.version = ComponentContextReader.UnknownComponentVersion;
            }

            string temp = versionAttribute.Value;
            if (string.IsNullOrWhiteSpace(temp) == false)
            {
                return this.version = temp;
            }

            return this.version = ComponentContextReader.UnknownComponentVersion;
        }
    }
}