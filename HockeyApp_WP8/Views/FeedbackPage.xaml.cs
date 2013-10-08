using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HockeyApp.ViewModels;
using System.Windows.Input;

namespace HockeyApp.Views
{
    public partial class FeedbackPage : PhoneApplicationPage
    {

        public FeedbackVM VM
        {
            get { return (this.DataContext as FeedbackVM); }
            protected set { this.DataContext = value; }
        }

        ApplicationBarIconButton sendButton = new ApplicationBarIconButton();
        ApplicationBarIconButton answerButton = new ApplicationBarIconButton();

        protected void ShowFormAppBar() {
            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.Add(sendButton);
        }

        protected void ShowListAppBar() {
            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.Add(answerButton);
            if (VM.Messages.Any())
            {
                MessageList.UpdateLayout();
                MessageList.ScrollIntoView(VM.Messages.Last());
            }
        }
        
        public FeedbackPage()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;

            sendButton.IconUri = new Uri("/HockeyApp.Content/Send.png", UriKind.Relative);
            sendButton.Text = "Send message";
            sendButton.Click += async (sender, e) =>
            {
                object focusObj = FocusManager.GetFocusedElement();
                if (focusObj != null && focusObj is TextBox)
                {
                    var binding = (focusObj as TextBox).GetBindingExpression(TextBox.TextProperty);
                    binding.UpdateSource();
                }
                await VM.SubmitForm();
            };

            answerButton.IconUri = new Uri("/HockeyApp.Content/Reply.png", UriKind.Relative);
            answerButton.Text = "Answer";
            answerButton.Click += (sender, e) =>
            {
                VM.SwitchToMessageForm();
            };

            Action<bool> showFormAppBarAction = (switchToForm) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    if (switchToForm)
                    {
                        ShowFormAppBar();
                    }
                    else
                    {
                        ShowListAppBar();
                    }
                });
            };
            showFormAppBarAction(false);

            this.VM = new FeedbackVM(showFormAppBarAction);

            InitializeComponent();
            
            /*
            ApplicationBar.IsMenuEnabled = true;
            ApplicationBarMenuItem menuItem1 = new ApplicationBarMenuItem();
            menuItem1.Text = "menu item 1";
            ApplicationBar.MenuItems.Add(menuItem1);
             */
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (!VM.IsMessageListVisible && VM.IsThreadActive)
            {
                VM.SwitchToMessageList();
                e.Cancel = true;
            }
            base.OnBackKeyPress(e);
        }
    }
}