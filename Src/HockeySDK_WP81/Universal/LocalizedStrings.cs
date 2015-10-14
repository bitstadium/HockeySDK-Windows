using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Microsoft.HockeyApp
{
    public class LocalizedStrings
    {

        private static ResourceWrapper _localizedResources = null;

        public static dynamic LocalizedResources { 
            get {
                if (_localizedResources == null)
                {
                    _localizedResources = new ResourceWrapper();
                }
                return _localizedResources;
            }
        }
    }

    public class ResourceWrapper : DynamicObject
    {
        private ResourceLoader _customResLoader;
        internal ResourceLoader CustomResourceLoader
        {
            get { return _customResLoader; }
            set { _customResLoader = value; }
        }

        private ResourceLoader _internalResLoader;
        internal ResourceLoader InternalResourceLoader
        {
            get { return _internalResLoader; }
            set { _internalResLoader = value; }
        }

        internal ResourceWrapper()
        {
            InternalResourceLoader = ResourceLoader.GetForViewIndependentUse("HockeyApp/Resources");
            try
            {   //try to load HockeyApp.resw if available in project!
                CustomResourceLoader = ResourceLoader.GetForViewIndependentUse("HockeyApp");
            }
            catch (Exception) { }
        }

        public object this[string index]
        {
            get
            {
                string value = null;
                if (_customResLoader != null)
                {
                    value = _customResLoader.GetString(index);
                }
                if (String.IsNullOrEmpty(value))
                {
                    value = _internalResLoader.GetString(index);
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
