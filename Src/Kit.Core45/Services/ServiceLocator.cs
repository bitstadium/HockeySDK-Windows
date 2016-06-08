namespace Microsoft.HockeyApp
{
    using Services;
    using System;
    using System.Collections.Generic;

    internal static class ServiceLocator
    {
        private static readonly IDictionary<Type, object> servcies;

        static ServiceLocator()
        {
            servcies = new Dictionary<Type, object>();
            AddService<IHttpService>(new HttpService());
        }

        internal static void AddService<T>(object adaptee)
        {
            servcies[typeof(T)] = adaptee;
        }

        internal static T GetService<T>()
        {
            return (T)servcies[typeof(T)];
        }
    }
}
