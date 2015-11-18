using Microsoft.HockeyApp.Common;
using Microsoft.HockeyApp.Model;
using Microsoft.HockeyApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.HockeyApp.ViewModels
{
    public partial class FeedbackThreadVM
    {

        public string ThreadInfo { get { return (this.Subject ?? "").ToUpper(); } }

        private void SetCommands()
        {
            this.ReplyCommand = new RelayCommand(() =>
            {
                (Window.Current.Content as Frame).Navigate(typeof(FeedbackFormPage));
            });

            this.ReloadCommand = new RelayCommand(async () =>
            {
                await FeedbackManager.Current.RefreshFeedbackThreadVMAsync(this);
            });
        }

        #region Commands

        public ICommand ReplyCommand { get; private set; }
        public ICommand ReloadCommand { get; private set; }

        #endregion


    }
}
