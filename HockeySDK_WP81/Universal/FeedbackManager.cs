using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
using HockeyApp.Model;
using HockeyApp.Tools;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using HockeyApp.ViewModels;


namespace HockeyApp
{
    /// <summary>
    /// Provides the methods to integrate HockeyApp Feedback in your app
    /// </summary>
    internal partial class FeedbackManager
    {

        private ILog logger = HockeyLogManager.GetLog(typeof(FeedbackManager));

        #region singleton

        protected static FeedbackManager _instance = new FeedbackManager();

        internal static FeedbackManager Current
        {
            get { return _instance; }
        }

        #endregion

        private string _initUserName;
        internal string InitialUsername
        {
            get { return _initUserName ?? HockeyClient.Current.AsInternal().UserID; ; }
            set { _initUserName = value; }
        }

        private string _initEmail;

        public string InitialEmail
        {
            get { return _initEmail ?? HockeyClient.Current.AsInternal().ContactInformation; }
            set { _initEmail = value; }
        }
        

        private const string FeedbackThreadTokensSettingsKey = "HockeyAppFeedbackThreadTokens";
        private const char FeedbackThreadTokensSeparator = '|';

        private IHockeyPlatformHelper platformHelper = HockeyClient.Current.AsInternal().PlatformHelper;

        private List<FeedbackThreadVM> _openthreadVMs = new List<FeedbackThreadVM>();
        internal IEnumerable<FeedbackThreadVM> OpenFeedbackThreadVMs
        {
            get { return _openthreadVMs; }
        }

        internal void AddFeedbackThread(FeedbackThreadVM fbThreadVM)
        {
            this._openthreadVMs.Add(fbThreadVM);
        }

        internal async Task<IEnumerable<FeedbackThreadVM>> LoadFeedbackThreadsAsync()
        {
            var tokens = platformHelper.GetSettingValue(FeedbackThreadTokensSettingsKey) ?? "";
            List<string> tokenList = tokens.Split(FeedbackThreadTokensSeparator).ToList();
            List<FeedbackThreadVM> vms = new List<FeedbackThreadVM>();
            foreach (string token in tokenList.Where(t => !String.IsNullOrEmpty(t)))
            {
                var thread = await HockeyClient.Current.AsInternal().OpenFeedbackThreadAsync(token);
                if (thread != null)
                {
                    vms.Add(new FeedbackThreadVM(thread));
                }
            }
            if (!vms.Any()) { vms.Add(new FeedbackThreadVM(FeedbackThread.CreateInstance())); }
            this._openthreadVMs = vms;
            return vms;
        }

        internal void SaveFeedbackThreadTokens()
        {
            var threads = this.OpenFeedbackThreadVMs.Where(t => !t.FeedbackThread.IsNewThread);
            if(threads.Any()) {
                platformHelper.SetSettingValue(FeedbackThreadTokensSettingsKey,
                    threads.Select(t => t.FeedbackThread.Token).Aggregate((a, b) =>  a + FeedbackThreadTokensSeparator + b));
            }
        }

        internal async Task RefreshFeedbackThreadVMAsync(FeedbackThreadVM threadVM)
        {
            if (!threadVM.FeedbackThread.IsNewThread)
            {
                try
                {
                    threadVM.FeedbackThread = (await HockeyClient.Current.AsInternal().OpenFeedbackThreadAsync(threadVM.FeedbackThread.Token)) ?? FeedbackThread.CreateInstance();
                }
                catch (Exception e)
                {
                    logger.Error(e);
                }
            }
           
        }


    }
}
