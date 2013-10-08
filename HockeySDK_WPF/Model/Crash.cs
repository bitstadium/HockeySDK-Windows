using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace HockeyApp.Model
{

    internal enum CrashInfoType
    {
        crash, user, contact, description
    }
    internal class Crash
    {
        #region static
        private static string[] GetCrashFiles()
        {
            return Directory.GetFiles(Constants.GetPathToHockeyCrashes(), "crash*.log");
        }
        internal static bool CrashesAvailable { get { return GetCrashFiles().Length > 0; } }
        internal static int CrashesAvailableCount { get { return GetCrashFiles().Length; } }


        private static string GetFileContentsIfExists(string filename)
        {
            String content = null;
            if (File.Exists(filename))
            {
                StreamReader sr = File.OpenText(filename);
                content = sr.ReadToEnd();
                sr.Close();
            }
            return content;
        }


        internal static IEnumerable<Crash> GetCrashes()
        {
            List<Crash> retVal = new List<Crash>();
            foreach (string filename in GetCrashFiles())
            {
                Crash crash = new Crash();
                crash.Log = GetFileContentsIfExists(filename) ?? "";
                crash.UserID = GetFileContentsIfExists(filename.Replace(CrashInfoType.crash.ToString(), CrashInfoType.user.ToString()));
                crash.ContactInformation = GetFileContentsIfExists(filename.Replace(CrashInfoType.crash.ToString(), CrashInfoType.contact.ToString())); 
                crash.Description = GetFileContentsIfExists(filename.Replace(CrashInfoType.crash.ToString(), CrashInfoType.description.ToString()));
                retVal.Add(crash);
            }
            return retVal;
        }
        #endregion

        #region ctor
        private ILog _logger = HockeyLogManager.GetLog(typeof(Crash));
        private Crash(){}
        internal Crash(Exception ex, Func<Exception, string> descriptionLoader)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(CreateHeader());
            builder.AppendLine();
            builder.Append(HockeyHelper.CreateStackTrace(ex));

            this.Log = builder.ToString();

            this.UserID = HockeyClient.Instance.UserID;
            this.ContactInformation = HockeyClient.Instance.ContactInformation;
            if (descriptionLoader != null)
            {
                try
                {
                    this.Description = descriptionLoader(ex);
                }
                catch (Exception) { }
            }
        }
        #endregion

        #region Serialize
        private String CreateHeader()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Package: {0}\n", Application.Current.GetType().Namespace);
            builder.AppendFormat("Version: {0}\n", HockeyClient.Instance.VersionInformation);
            builder.AppendFormat("OS: {0}\n", Environment.OSVersion);
            builder.AppendFormat("Manufacturer: {0}\n", "");
            builder.AppendFormat("Model: {0}\n", "");
            builder.AppendFormat("Date: {0}\n", DateTime.UtcNow.ToString("o"));

            return builder.ToString();
        }

        internal void Save()
        {
            var crashId = Guid.NewGuid();
            if (!String.IsNullOrWhiteSpace(this.Log)) { saveFile(CrashInfoType.crash, this.Log); }
            if (!string.IsNullOrWhiteSpace(this.UserID)) { saveFile(CrashInfoType.user, this.UserID); }
            if (!string.IsNullOrWhiteSpace(this.ContactInformation)) { saveFile(CrashInfoType.contact, this.ContactInformation); }
            if (!string.IsNullOrWhiteSpace(this.Description)) { this.saveFile(CrashInfoType.description, this.Description); }
        }

        private void saveFile(CrashInfoType infoType, string information)
        {
            try
            {
                String filename = string.Format("{0}{1}.log", infoType.ToString(), this.crashID);
                FileStream stream = File.Create(Path.Combine(Constants.GetPathToHockeyCrashes(), filename));
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(information);
                }
                stream.Close();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }
        #endregion

        internal void Delete()
        {
            this.delete(CrashInfoType.crash);
            this.delete(CrashInfoType.user);
            this.delete(CrashInfoType.contact);
            this.delete(CrashInfoType.description);
        }

        private void delete(CrashInfoType infoType){
            String filename = string.Format("{0}{1}.log", infoType.ToString(), this.crashID);
            File.Create(Path.Combine(Constants.GetPathToHockeyCrashes(), filename));
        }

        #region props
        internal string crashID{get;set;}
        internal string Log { get; set; }
        internal string UserID{get;set;}
        internal string ContactInformation{get;set;}
        internal string Description{get;set; }
        #endregion

    }
}
