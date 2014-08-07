using HockeyApp.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp.PCL
{
    public static class HockeyClientPCLExtensions
    {
        public static IHockeyClientConfigurable Configure(this IHockeyClient @this, IHockeyPlatformHelper platformHelper)
        {
            @this.AsInternal().PlatformHelper = platformHelper;
            return @this as IHockeyClientConfigurable;
        }
    }
}
