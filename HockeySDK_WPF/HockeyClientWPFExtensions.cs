using HockeyApp.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp
{
    public static class HockeyClientWPFExtensions
    {
        internal static IHockeyClientInternal AsInternal(this IHockeyClient @this)
        {
            return (IHockeyClientInternal)@this;
        }

        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, string identifier)
        {
            @this.AsInternal().PlatformHelper = new HockeyPlatformHelperWPF();
            @this.AsInternal().AppIdentifier = identifier;
            @this.AsInternal().SdkName = Constants.SDKNAME;
            @this.AsInternal().SdkVersion = Constants.SDKVERSION;
            @this.AsInternal().UserAgentString = Constants.USER_AGENT_STRING;

            return (IHockeyClientConfigurable)@this;
        }


    }
}
