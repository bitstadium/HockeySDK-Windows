using System;
using System.Collections;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace HockeyApp
{
    /// <summary>
    /// Provides access to string resources.
    /// </summary>
    public class LocalizedStrings
    {
        private static HockeyAppStrings _localizedResources = new HockeyAppStrings();

        private static dynamic customResourceWrapper = new ResourceWrapper(HockeyAppStrings.ResourceManager);
        private static ResourceManager customResourceManager = null;

        public static ResourceManager CustomResourceManager
        {
            get
            {
                return customResourceManager;
            }
            set
            {
                customResourceManager = value;
                customResourceWrapper = new ResourceWrapper(value, HockeyAppStrings.ResourceManager);
            }
        }

        public static dynamic LocalizedResources
        {
            get
            {
                return customResourceWrapper;
            }
        }
    }
    
    public class ResourceWrapper : DynamicObject
    {
        ResourceManager customResMan;
        ResourceManager defaultResMan;

        public ResourceWrapper(ResourceManager defaultResMan)
        {
            this.defaultResMan = defaultResMan;
            this.customResMan = null;
        }

        public ResourceWrapper(ResourceManager customResMan, ResourceManager defaultResMan)
        {
            this.defaultResMan = defaultResMan;
            this.customResMan = customResMan;
        }

        /// <summary>
        /// The indexer is needed to be able to use indexer-syntax in XAML to data bind
        /// </summary>
        /// <param name="index">The name of the property.</param>
        /// <returns>The value of the property, or null if the property doesn't exist.</returns>
        public object this[string index]
        {
            get
            {
                if (customResMan != null)
                {
                    return customResMan.GetString(index) ?? defaultResMan.GetString(index);
                }
                else
                {
                    return defaultResMan.GetString(index);
                }
            }
        }

        //For dynamic use in code
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this[binder.Name];
            return result != null;
        }
    }
}