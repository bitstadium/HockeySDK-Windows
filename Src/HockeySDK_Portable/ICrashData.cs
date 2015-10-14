using System;
using System.IO;
using System.Threading.Tasks;
namespace Microsoft.HockeyApp
{
    /// <summary>
    /// interface for crashlog data
    /// </summary>
    public interface ICrashData
    {

        /// <summary>
        /// optional, string with contact information, limited to 255 chars
        /// </summary>
        string Contact { get; set; }

        /// <summary>
        /// optional, file with optional information, e.g. excerpts from the system log
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// required, file with the crash log - Note: The maximum allowed file size is 200kB!
        /// </summary>
        string Log { get; set; }

        /// <summary>
        /// Name of the used SDK
        /// </summary>
        string SDKName { get; set; }

        /// <summary>
        /// Version of the used SDK
        /// </summary>
        string SDKVersion { get; set; }

        /// <summary>
        /// optional, string with a user or device ID, limited to 255 chars
        /// </summary>
        string UserID { get; set; }


        /// <summary>
        /// Serializes the crashdata to a stream.
        /// </summary>
        /// <param name="outputStream"></param>
        void Serialize(Stream outputStream);

        /// <summary>
        /// Post the crash to the HockeyApp-Platform
        /// </summary>
        /// <returns></returns>
        Task SendDataAsync();
        
    }
}
