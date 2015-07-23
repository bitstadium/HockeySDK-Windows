namespace Microsoft.ApplicationInsights.Extensibility
{
    using System.IO;
    using System.IO.IsolatedStorage;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Various utilities.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Reads the serialized context from persistent storage, or will create a new context if none exits.
        /// </summary>
        /// <param name="fileName">The file to read from storage.</param>
        /// <returns>The fallback context we will be using.</returns>
        public static TType ReadSerializedContext<TType>(string fileName) where TType : IFallbackContext, new()
        {
            // get a reference to the persitent store
            using (IsolatedStorageFile persistentStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // if the file exits, attempt to read/deserialize it. If we fail, we'll just regen it.
                bool regenerateContext = true;
                if (persistentStore.FileExists(fileName) == true)
                {
                    try
                    {
                        using (IsolatedStorageFileStream stream = persistentStore.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                        {
                            XDocument document = XDocument.Load(stream);
                            TType temp = new TType();
                            if (temp.Deserialize(document.Root) == true)
                            {
                                regenerateContext = false;
                                return temp;
                            }
                        }
                    }
                    catch (IsolatedStorageException)
                    {
                        // TODO: swallow?
                    }
                    catch (FileNotFoundException)
                    {
                        // TODO: swallow?
                    }
                    catch (XmlException)
                    {
                        // TODO: swallow?
                    }
                }

                // if we're here we will need to regen our context
                if (regenerateContext == true)
                {
                    // create the XML document first
                    XDocument document = new XDocument();
                    document.Add(new XElement(XName.Get(typeof(TType).Name)));

                    // initialize the new set of settings and serialize to the document root
                    TType temp = new TType();
                    temp.Initialize();
                    temp.Serialize(document.Root);

                    // write to persistent storage
                    try
                    {
                        using (IsolatedStorageFileStream stream = persistentStore.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            stream.SetLength(0);
                            document.Save(stream);
                            stream.Flush(true);
                            return temp;
                        }
                    }
                    catch (IsolatedStorageException)
                    {
                        // TODO: swallow?
                    }
                    catch (FileNotFoundException)
                    {
                        // TODO: swallow?
                    }
                }
            }

            TType defaultReturn = new TType();
            defaultReturn.Initialize();
            return defaultReturn;
        }
    }
}
