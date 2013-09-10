using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HockeyApp.Tools;

namespace HockeyApp.Views
{
    public partial class FeedbackPage : PhoneApplicationPage
    {

        public FeedbackVM VM
        {
            get { return (this.DataContext as FeedbackVM); }
            protected set { this.DataContext = value; }
        }

        public FeedbackPage()
        {
            this.VM = new FeedbackVM(FeedbackManager.Instance.AppIdentitfier);

            InitializeComponent();

            ApplicationBar = new ApplicationBar();

            ApplicationBar.Mode = ApplicationBarMode.Default;
            //TODO ?! ApplicationBar.IsMenuEnabled = true;

            ApplicationBarIconButton sendButton = new ApplicationBarIconButton();
            sendButton.IconUri = new Uri("/Assets/Send.png", UriKind.Relative);
            sendButton.Text = "Send message";
            ApplicationBar.Buttons.Add(sendButton);

            sendButton.Click += async (sender, e) => { await VM.SubmitForm(); };

            //TODO reset button ?!
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            
            base.OnBackKeyPress(e);
        }


    }
}