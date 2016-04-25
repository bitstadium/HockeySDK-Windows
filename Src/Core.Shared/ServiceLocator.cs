namespace Microsoft.HockeyApp
{
    using System;
    using System.Collections.Generic;

    public static class ServiceLocator
    {
        private static readonly IDictionary<Type, object> servcies;

        static ServiceLocator()
        {
            servcies = new Dictionary<Type, object>();
        }

        public static void AddService<T>(object adaptee)
        {
            if (!servcies.ContainsKey(typeof(T)))
            {
                servcies.Add(typeof(T), adaptee);
            }
        }

        public static T GetService<T>()
        {
            return (T)servcies[typeof(T)];
        }
    }
}
