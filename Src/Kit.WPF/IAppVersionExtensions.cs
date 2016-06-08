using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.HockeyApp
{
    /// <summary>
    /// Interface for app version.
    /// </summary>
    public static class IAppVersionExtensions
    {
        //better ideas welcome... Need something like an extension property
        private static Dictionary<string, string> _downloadFilesnames = new Dictionary<string, string>();

#pragma warning disable 1998
        /// <summary>
        /// Downloads the new version to a temporary folder and provides the generated filename.
        /// The progress delegate returns, if the download process should be canceled. Return false, if not.
        /// </summary>
        /// <param name="this">The this.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="completed">callbakc when download is completed.</param>
        /// <returns>
        /// Filename of the msi-file
        /// </returns>
        public static async Task<string> DownloadMsi(this IAppVersion @this, Func<DownloadProgressInformation, bool> progress = null, Action completed = null)
        {
            var tmpFilenameWithPath = Path.GetTempFileName();

            var msiFilenameWithPath = Path.GetFileNameWithoutExtension(tmpFilenameWithPath);
            msiFilenameWithPath += ".msi";
            msiFilenameWithPath = Path.Combine(Path.GetDirectoryName(tmpFilenameWithPath), msiFilenameWithPath);
            File.Move(tmpFilenameWithPath, msiFilenameWithPath);


            var uri = new Uri(HockeyClient.Current.AsInternal().ApiBaseVersion2 + "apps/" + HockeyClient.Current.AsInternal().AppIdentifier + "/app_versions/" + @this.Id + ".msi");
            WebClient wc = new WebClient();

            if (progress != null)
            {
                wc.DownloadProgressChanged += (a, b) =>
                {
                    if (progress(new DownloadProgressInformation(b.ProgressPercentage, b.BytesReceived, b.TotalBytesToReceive)))
                    {
                        wc.CancelAsync();
                    };
                };
            }
            if(completed != null){
                wc.DownloadFileCompleted += (a,b) => completed();
            }

            wc.DownloadFileAsync(uri, msiFilenameWithPath);
            
            if (_downloadFilesnames.ContainsKey(@this.AppId))
            {
                _downloadFilesnames[@this.AppId] = msiFilenameWithPath;
            }
            else
            {
                _downloadFilesnames.Add(@this.AppId, msiFilenameWithPath);
            }
            return msiFilenameWithPath;
        }
#pragma warning restore  1998

        /// <summary>
        /// Installs the app. If not downloaded, the download will start implicitly.
        /// After starting the install process a call to Environment.Exit is done.
        /// </summary>
        /// <param name="this"></param>
        public static void InstallVersion(this IAppVersion @this)
        {
            if (!_downloadFilesnames.ContainsKey(@this.AppId))
            {
                throw new Exception("Download the msi first!");
            }
            var msiFilename = _downloadFilesnames[@this.AppId];
            Process.Start(msiFilename);
            Environment.Exit(0);
        }

    }
}
