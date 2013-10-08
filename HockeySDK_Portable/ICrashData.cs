using System;
namespace HockeyApp
{
    public interface ICrashData
    {
        string Contact { get; set; }
        string Description { get; set; }
        string Log { get; set; }
        string SDKName { get; set; }
        string SDKVersion { get; set; }
        string UserID { get; set; }
    }
}
