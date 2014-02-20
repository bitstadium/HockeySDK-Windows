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

    public enum FeedbackViewState
    {
        MessageForm, MessageList, ImageEdit, ImageShow, Unknown
    }

    public partial class FeedbackPage : PhoneApplicationPage
   {

        internal FeedbackPageVM VM
        {
            get { return (this.DataContext as FeedbackPageVM); }
            private set { this.DataContext = value; }
        }

        internal FeedbackViewState CurrentViewState = FeedbackViewState.Unknown;

        internal FeedbackMessageFormControl formControl;
        internal FeedbackMessageListControl listControl;
        internal FeedbackImageControl imageControl;

        internal List<FeedbackViewState> lastActiveViewStates = new List<FeedbackViewState>();

        public FeedbackPage()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;

            Action<FeedbackViewState> switchViewStateAction = (newViewState) =>
            {
                SwitchToViewState(newViewState);
            };
            this.VM = new FeedbackPageVM(switchViewStateAction);

            this.formControl = new FeedbackMessageFormControl(this);
            this.listControl = new FeedbackMessageListControl(this);
            this.imageControl = new FeedbackImageControl(this);

            initializeAppBarIcons();
            
            InitializeComponent();
        }

        private void SwitchToViewState(FeedbackViewState newViewState, bool isBackOperation = false)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                switch (newViewState)
                {
                    case FeedbackViewState.MessageForm:
                        ShowFormAppBar();
                        this.FeedbackPageContent.Content = formControl;
                        break;
                    case FeedbackViewState.MessageList:
                        ShowListAppBar();
                        this.FeedbackPageContent.Content = listControl;
                        break;
                    case FeedbackViewState.ImageEdit:
                        imageControl.VM = this.VM.CurrentImageVM;
                        ShowDrawAppBar();
                        this.FeedbackPageContent.Content = imageControl;
                        break;
                    case FeedbackViewState.ImageShow:
                        imageControl.VM = this.VM.CurrentImageVM;
                        ShowDrawAppBar();
                        this.FeedbackPageContent.Content = imageControl;
                        break;
                    case FeedbackViewState.Unknown:
                        NavigationService.GoBack();
                        break;
                    default:
                        break;
                }
                if (!isBackOperation)
                {
                    lastActiveViewStates.Add(CurrentViewState);
                }
                CurrentViewState = newViewState;
            });
        }

        internal void NavigateBack()
        {
            if (!lastActiveViewStates.Any() ||FeedbackViewState.Unknown.Equals(lastActiveViewStates.Last()))
            {
                NavigationService.GoBack();
            }
            else
            {
                var state = lastActiveViewStates.Last();
                lastActiveViewStates.RemoveAt(lastActiveViewStates.Count - 1);
                SwitchToViewState(state, true);
            }
        }

        #region AppBar

        ApplicationBarIconButton sendButton = new ApplicationBarIconButton();
        ApplicationBarIconButton attachButton = new ApplicationBarIconButton();
        ApplicationBarIconButton cancelButton = new ApplicationBarIconButton();

        ApplicationBarIconButton answerButton = new ApplicationBarIconButton();

        ApplicationBarMenuItem menuItemOk = new ApplicationBarMenuItem();
        ApplicationBarMenuItem menuItemClose = new ApplicationBarMenuItem();
        ApplicationBarMenuItem menuItemReset = new ApplicationBarMenuItem();
        ApplicationBarMenuItem menuItemDelete = new ApplicationBarMenuItem();

        private void initializeAppBarIcons()
        {
            sendButton.IconUri = new Uri("/HockeyAppContent/Send.png", UriKind.Relative);
            sendButton.Text = LocalizedStrings.LocalizedResources.Send;
            sendButton.Click += async (sender, e) =>
            {
                await this.formControl.SendButtonClicked();
            };

            answerButton.IconUri = new Uri("/HockeyAppContent/Reply.png", UriKind.Relative);
            answerButton.Text = LocalizedStrings.LocalizedResources.Reply;
            answerButton.Click += (sender, e) =>
            {
                VM.SwitchToMessageForm();
            };

            cancelButton.IconUri = new Uri("/HockeyAppContent/Cancel.png", UriKind.Relative);
            cancelButton.Text = LocalizedStrings.LocalizedResources.Cancel;
            cancelButton.Click += (sender, e) =>
            {
                this.formControl.VM.ClearForm();
                this.NavigateBack();
            };

            attachButton.IconUri = new Uri("/HockeyAppContent/Attach.png", UriKind.Relative);
            attachButton.Text = LocalizedStrings.LocalizedResources.AttachImg;
            attachButton.Click += (sender, e) =>
            {
                this.formControl.AttachButtonClicked();
            };

            menuItemOk.Text = LocalizedStrings.LocalizedResources.ImgOk;
            menuItemOk.Click += (sender, e) =>
            {
                this.imageControl.OkButtonClicked();
            };

            menuItemClose.Text = LocalizedStrings.LocalizedResources.ImgClose;
            menuItemClose.Click += (sender, e) =>
            {
                this.imageControl.OkButtonClicked();
            };

            menuItemReset.Text = LocalizedStrings.LocalizedResources.ImgReset;
            menuItemReset.Click += (sender, e) =>
            {
                this.imageControl.ResetButtonClicked();
            };

            menuItemDelete.Text = LocalizedStrings.LocalizedResources.ImgDelete;
            menuItemDelete.Click += (sender, e) =>
            {
                this.imageControl.DeleteButtonClicked();
            };

        }

        protected void ShowDrawAppBar()
        {
            SystemTray.IsVisible = false;
            ApplicationBar.Opacity = 0;
            ApplicationBar.Buttons.Clear();
            ApplicationBar.MenuItems.Clear();
            
            if(this.VM.CurrentImageVM.IsEditable) {
                ApplicationBar.MenuItems.Add(menuItemDelete);
                ApplicationBar.MenuItems.Add(menuItemReset);
                ApplicationBar.MenuItems.Add(menuItemOk);
            }
            else
            {
                ApplicationBar.MenuItems.Add(menuItemClose);
            }
            ApplicationBar.IsMenuEnabled = true;
        }

        protected void ShowFormAppBar()
        {
            SystemTray.IsVisible = true;
            ApplicationBar.Opacity = 1;
            ApplicationBar.Buttons.Clear();
            ApplicationBar.IsMenuEnabled = false;
            ApplicationBar.Buttons.Add(sendButton);
            ApplicationBar.Buttons.Add(attachButton);
            ApplicationBar.Buttons.Add(cancelButton);
        }

        protected void ShowListAppBar()
        {
            SystemTray.IsVisible = true;
            ApplicationBar.Opacity = 1;
            ApplicationBar.Buttons.Clear();
            ApplicationBar.IsMenuEnabled = false;
            ApplicationBar.Buttons.Add(answerButton);
            if (VM.Messages.Any())
            {
                listControl.MessageList.UpdateLayout();
                listControl.MessageList.ScrollIntoView(VM.Messages.Last());
            }
        }

        #endregion

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (!FeedbackViewState.MessageList.Equals(CurrentViewState))
            {
                e.Cancel = true;
                NavigateBack();

                /* immer glecih zurück
                if (!FeedbackViewState.MessageForm.Equals(CurrentViewState) || (MessageBox.Show("Discard your message?", "Feedback", MessageBoxButton.OKCancel) == MessageBoxResult.OK))
                {
                    
                } */
            }
            
            base.OnBackKeyPress(e);
        }

   }
}