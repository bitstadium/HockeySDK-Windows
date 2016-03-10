namespace Microsoft.HockeyApp
{
    using System;

    /// <summary>
    /// Interface used during initial fluent configuration of HockeyClient
    /// </summary>
    public interface IHockeyClientConfigurable { }

    /// <summary>
    /// Extensions for fluent configuration
    /// </summary>
    public static class HockeyClientConfigurationExtensions
    {

        /// <summary>
        /// Use this if you're using an on-premise version of HockeyApp. Default is: https://rink.hockeyapp.net
        /// </summary>
        /// <param name="this"></param>
        /// <param name="hockeyApiDomain"></param>
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
        /// Set the user Id and emal/contact information of the current user if known. This is sent to HockeyApp with crashes.
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
    }
    
}
