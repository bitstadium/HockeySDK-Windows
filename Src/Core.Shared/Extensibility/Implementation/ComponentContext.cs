namespace Microsoft.HockeyApp.Extensibility.Implementation
{
    using System.Collections.Generic;
    using DataContracts;
    using Extensibility.Implementation.External;

    /// <summary>
    /// Encapsulates information describing an Application Insights component.
    /// </summary>
    /// <remarks>
    /// This class matches the "Application" schema concept. We are intentionally calling it "Component" for consistency 
    /// with terminology used by our portal and services and to encourage standardization of terminology within our 
    /// organization. Once a consensus is reached, we will change type and property names to match.
    /// </remarks>
    internal sealed class ComponentContext : IJsonSerializable
    {
        private readonly IDictionary<string, string> tags;

        internal ComponentContext(IDictionary<string, string> tags)
        {
            this.tags = tags;
        }

        /// <summary>
        /// Gets or sets the application version.
        /// </summary>
        public string Version
        {
            get { return this.tags.GetTagValueOrNull(ContextTagKeys.Keys.ApplicationVersion); }
            set { this.tags.SetStringValueOrRemove(ContextTagKeys.Keys.ApplicationVersion, value); }
        }

        /// <summary>
        ///  Gets or sets bundle/package identifier in reverse domain name notation.
        /// </summary>
        public string ApplicationId
        {
            get { return this.tags.GetTagValueOrNull(ContextTagKeys.Keys.ApplicationId); }
            set { this.tags.SetStringValueOrRemove(ContextTagKeys.Keys.ApplicationId, value); }
        }

        void IJsonSerializable.Serialize(IJsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteProperty("version", this.Version);
            writer.WriteEndObject();
        }
    }
}
