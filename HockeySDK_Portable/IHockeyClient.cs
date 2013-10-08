using System;
namespace HockeyApp
{
    public interface IHockeyClient
    {

        string ApiBase { get; }
        string UserAgentString { get; set; }

        string SdkName { get; }
        string SdkVersion { get; }

        string AppIdentifier { get; }
        string VersionInformation { get; }
        string UserID { get; }
        string ContactInformation { get; }

        
        IFeedbackThread CreateNewFeedbackThread();
        System.Threading.Tasks.Task<IFeedbackThread> OpenFeedbackThreadAsync(string threadToken);
        System.Threading.Tasks.Task<HockeyApp.Model.AppVersion> GetLatestAppVersion();
        System.Threading.Tasks.Task PostCrashAsync(string log, string userID, string contactInformation, string description);
        
    }
}
