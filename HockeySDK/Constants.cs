using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp
{
    internal class Constants
    {
        internal const string CrashDirectoryName = "CrashLogs";
        internal const string SdkName = "HockeySDK";
        internal const string SdkVersion = "1.1.0";
        internal const string ApiBase = "https://rink.hockeyapp.net/api/2/";
        internal const string UserAgentString = "Hockey/WP8";

        internal const string FeedbackThreadKey = "HockeyAppFeedback_ThreadId";
        internal const string FeedbackEmailKey = "HockeyAppFeedback_Email";
        internal const string FeedbackUsernameKey = "HockeyAppFeedback_Username";
        internal const string ContentTypeUrlEncoded = "application/x-www-form-urlencoded"; 
    }
}
