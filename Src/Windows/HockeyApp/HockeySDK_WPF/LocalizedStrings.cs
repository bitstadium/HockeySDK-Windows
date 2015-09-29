using System;
using System.Collections;
using System.Collections.Generic;
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
    internal class LocalizedStrings
    {
        private static HockeySDKStrings _localizedResources = new HockeySDKStrings();

        private static dynamic customResourceWrapper = new ResourceWrapper(HockeySDKStrings.ResourceManager);

        public static ResourceManager CustomResourceManager
        {
            get
            {
                return customResourceWrapper.CustomResourceManager as ResourceManager;
            }
            set
            {
                customResourceWrapper.CustomResourceManager = value;
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

    internal class ResourceWrapper : DynamicObject
    {
        private ResourceManager customResMan;
        public ResourceManager CustomResourceManager
        {
            get { return customResMan; }
            set { customResMan = value; }
        }

        private ResourceManager internalResMan;
        public ResourceManager InternalResourceManager
        {
            get { return internalResMan; }
            set { internalResMan = value; }
        }

        public ResourceWrapper(ResourceManager internalResMan)
        {
            this.internalResMan = internalResMan;
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
                string value = null;
                if (customResMan != null)
                {
                    value = customResMan.GetString(index);
                }
                if (String.IsNullOrEmpty(value))
                {
                    value = internalResMan.GetString(index);
                }
                if (String.IsNullOrEmpty(value))
                {
                    value = index + "_i18n";
                }
                return value;
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