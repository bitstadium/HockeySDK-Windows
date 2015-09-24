using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HockeyApp.Tools
{
    internal class DynamicNavigationParameters : DynamicObject, IDynamicMetaObjectProvider
    {
        object Instance;
        Type InstanceType;

        List<PropertyInfo> InstancePropertyInfo
        {
            get
            {
                if (_InstancePropertyInfo == null && Instance != null)                
                    _InstancePropertyInfo = Instance.GetType().GetRuntimeProperties().Where(pi => pi.GetMethod.IsPublic).ToList();
                return _InstancePropertyInfo;                
            }
        }
        List<PropertyInfo> _InstancePropertyInfo;

        public Dictionary<string,object> Properties = new Dictionary<string, object>();

        public DynamicNavigationParameters() 
        {
            Initialize(this);            
        }

        public DynamicNavigationParameters(object instance)
        {
            Initialize(instance);
        }


        protected virtual void Initialize(object instance)
        {
            Instance = instance;
            if (instance != null)
                InstanceType = instance.GetType();           
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;

            if (Properties.Keys.Contains(binder.Name))
            {
                result = Properties[binder.Name];
                return true;
            }

            if (Instance != null)
            {
                try
                {
                    return GetProperty(Instance, binder.Name, out result);                    
                }
                catch { }
            }

            result = null;
            return true; //immer true zurückgeben - im zweifelsfall ist es immer null
        }


        public override bool TrySetMember(SetMemberBinder binder, object value)
        {

            // first check to see if there's a native property to set
            if (Instance != null)
            {
                try
                {
                    bool result = SetProperty(Instance, binder.Name, value);
                    if (result)
                        return true;
                }
                catch { }
            }
            
            Properties[binder.Name] = value;
            return true;
        }

        protected bool GetProperty(object instance, string name, out object result)
        {
            if (instance == null)
                instance = this;

            var mi = InstanceType.GetRuntimeProperty(name);
            if (mi != null)
            {

                result = mi.GetValue(instance, null);
                return true;
            }

            result = null;
            return true;                
        }

        protected bool SetProperty(object instance, string name, object value)
        {
            if (instance == null)
                instance = this;

            var mi = InstanceType.GetRuntimeProperty(name);
            if (mi != null)
            {
                mi.SetValue(Instance, value, null);
                return true;
            }
            return false;                
        }

        public object this[string key]
        {
            get
            {
                try
                {
                    return Properties[key];
                }
                catch (KeyNotFoundException)
                {
                    object result = null;
                    if (GetProperty(Instance, key, out result))
                        return result;
                    return null;
                }
            }
            set
            {
                if (Properties.ContainsKey(key))
                {
                    Properties[key] = value;
                    return;
                }

                var mi = InstanceType.GetRuntimeProperty(key);
                if (mi != null)
                    SetProperty(Instance, key, value);
                else
                    Properties[key] = value;
            }
        }

        public IEnumerable<KeyValuePair<string,object>> GetProperties(bool includeInstanceProperties = false)
        {
            if (includeInstanceProperties && Instance != null)
            {
                foreach (var prop in this.InstancePropertyInfo)
                    yield return new KeyValuePair<string, object>(prop.Name, prop.GetValue(Instance, null));
            }

            foreach (var key in this.Properties.Keys)
               yield return new KeyValuePair<string, object>(key, this.Properties[key]);

        }

        public bool Contains(KeyValuePair<string, object> item, bool includeInstanceProperties = false)
        {
            bool res = Properties.ContainsKey(item.Key);
            if (res)
                return true;

            if (includeInstanceProperties && Instance != null)
            {
                foreach (var prop in this.InstancePropertyInfo)
                {
                    if (prop.Name == item.Key)
                        return true;
                }
            }

            return false;
        }        
    }
}