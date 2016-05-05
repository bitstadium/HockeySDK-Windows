namespace Microsoft.HockeyApp.PCL
{
    
    /// <summary>
    /// static extension class for
    /// </summary>
    internal static class HockeyClientPCLExtensions
    {
        /// <summary>
        /// Configures the client with a platform helper
        /// </summary>
        /// <param name="this">The this.</param>
        /// <param name="platformHelper">The platform helper.</param>
        /// <returns></returns>
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, IHockeyPlatformHelper platformHelper)
        {
            @this.AsInternal().PlatformHelper = platformHelper;
            return @this as IHockeyClientConfigurable;
        }
    }
}
