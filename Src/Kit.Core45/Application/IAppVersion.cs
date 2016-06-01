using System;
namespace Microsoft.HockeyApp
{
    /// <summary>
    /// interface for an app version
    /// </summary>
    public interface IAppVersion
    {
        /// <summary>
        /// Public identifier of your app
        /// </summary>
        string PublicIdentifier { get; }
        /// <summary>
        /// HockeyApp internal key for your app
        /// </summary>
        string AppId { get; }
        /// <summary>
        /// Size of the App in bytes
        /// </summary>
        long Appsize { get; }
        /// <summary>
        /// App size in human readable form
        /// </summary>
        string AppSizeReadable { get; }
        /// <summary>
        /// Device family
        /// </summary>
        string DeviceFamily { get; }
        /// <summary>
        /// Internal id of the version 
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Indicates if this version is marked as mandatory
        /// </summary>
        bool Mandatory { get; }
        /// <summary>
        /// Minimum OS requirement for this version
        /// </summary>
        string MinimumOsVersion { get; }
        /// <summary>
        /// Notes
        /// </summary>
        string Notes { get; }
        /// <summary>
        /// Shortversion string
        /// </summary>
        string Shortversion { get; }
        /// <summary>
        /// Formateed string conatining shortversion and version
        /// </summary>
        string ShortversionAndVersion { get; }
        /// <summary>
        /// Timestamp of creation in unix format
        /// </summary>
        long? Timestamp { get; }
        /// <summary>
        /// Timestamp of creation as DateTime
        /// </summary>
        DateTime? TimeStamp { get; }
        /// <summary>
        /// Title of the app
        /// </summary>
        string Title { get; }
        /// <summary>
        /// Version string
        /// </summary>
        string Version { get; }
    }
}
