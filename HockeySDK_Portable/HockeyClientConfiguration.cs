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

        /// <summary>
        /// Use this if you're using an on-premise version of HockeyApp. Default is: https://rink.hockeyapp.net
        /// </summary>
        /// <param name="this"></param>
        /// <param name="anApiDomain"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable SetApiDomain(this IHockeyClientConfigurable @this, string hockeyApiDomain)
        {
            @this.AsInternal().ApiDomain = hockeyApiDomain;
            return @this;
        }

        /// <summary>
        /// The provided func is called in case of an exception and the returned string is added as additional description of the exception.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="descriptionLoader"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable SetExceptionDescriptionLoader(this IHockeyClientConfigurable @this, Func<Exception, string> descriptionLoader)
        {
            @this.AsInternal().DescriptionLoader = descriptionLoader;
            return @this;
        }

        /// <summary>
        /// The provided func is called in case of an exception and the returned string is added as additional description of the exception.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="descriptionLoader"></param>
        /// <returns></returns>
        public static IHockeyClient UpdateExceptionDescriptionLoader(this IHockeyClient @this, Func<Exception, string> descriptionLoader)
        {
            @this.AsInternal().DescriptionLoader = descriptionLoader;
            return @this;
        }

        /// <summary>
        /// Set the user Id and email/contact information of the current user if known. This is sent to HockeyApp with crashes.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static IHockeyClientConfigurable SetContactInfo(this IHockeyClientConfigurable @this, string user, string email)
        {
            @this.AsInternal().UserID = user;
            @this.AsInternal().ContactInformation = email;
            return @this;
        }

        /// <summary>
        /// Set the user Id and email/contact information of the current user if known. This is sent to HockeyApp with crashes.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static IHockeyClient UpdateContactInfo(this IHockeyClient @this, string user, string email)
        {
            @this.AsInternal().UserID = user;
            @this.AsInternal().ContactInformation = email;
            return @this;
        }

        /// <summary>
        /// Enqueue exception and extra description to be sent to HockeyApp. Useful for debugging handled exception.
        /// </summary>
        /// <param name="this"></param>
        /// <param name="ex"></param>
        /// <param name="extraDescription">[optional] Override the description provided by Exception Description Loader</param>
        /// <param name="descriptionCombine">[optional] Resolve the conflict when both <paramref name="extraDescription"/> and Exception Description Loader are set.</param>
        /// <returns></returns>
        public static async Task<IHockeyClient> SendExceptionAsync(this IHockeyClient @this, Exception ex, string extraDescription = null, 
            Func<string, string, string> descriptionCombine = null)
        {
            await @this.AsInternal().HandleExceptionAsync(ex, extraDescription, descriptionCombine);
            return @this;
        }
    }
    
}
