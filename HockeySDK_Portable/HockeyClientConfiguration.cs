using HockeyApp.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp
{
    public interface IHockeyClientConfigurable { }

    public static class HockeyClientConfigurationExtensions
    {

        //TODO implement
        public static IHockeyClientConfigurable SetApiBase(this IHockeyClientConfigurable @this, string anApiBaseString)
        {

            return @this;
        }

        public static IHockeyClientConfigurable SetExceptionDescriptionLoader(this IHockeyClientConfigurable @this, Func<Exception, string> descriptionLoader = null)
        {
            @this.AsInternal().DescriptionLoader = descriptionLoader;
            return @this;
        }

        public static IHockeyClientConfigurable SetContactInfo(this IHockeyClientConfigurable @this, string user, string email)
        {
            
            return @this;
        }

        public static IHockeyClient UpdateContactInfo(this IHockeyClient @this, string user, string email)
        {
            
            return @this;
        }
    }
    
}
