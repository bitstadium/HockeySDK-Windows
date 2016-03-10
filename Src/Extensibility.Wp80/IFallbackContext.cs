namespace Microsoft.HockeyApp.Extensibility
{
    using System.Xml.Linq;

    /// <summary>
    /// The interface for all fallback contexts.
    /// </summary>
    internal interface IFallbackContext
    {
        /// <summary>
        /// Initializes this instance with a set of new properties to serve as context.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Serializes the current instance to the passed in root element.
        /// </summary>
        /// <param name="rootElement">The root element to serialize to.</param>
        void Serialize(XElement rootElement);

        /// <summary>
        /// Deserializes the passed in root element to the current instance.
        /// </summary>
        /// <param name="rootElement">The root element to deserialize.</param>
        /// <returns>True if deserialization was successful, false otherwise.</returns>
        bool Deserialize(XElement rootElement);
    }
}