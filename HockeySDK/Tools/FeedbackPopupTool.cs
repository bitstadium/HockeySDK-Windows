using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Linq;
using HockeyApp.Views;
using System.Collections.Generic;
using Microsoft.Phone.Controls;
using System.ComponentModel;
using HockeyApp.Model;
using System.IO.IsolatedStorage;

namespace HockeyApp.Tools
{
    public class FeedbackPopupTool
    {
        #region Properties

        private static Popup Popup { get; set; }

        /// <summary>
        /// Gets value indicating whether a message is shown to the user.
        /// </summary>
        public static bool IsShown
        {
            get
            {
                return Popup != null && Popup.IsOpen;
            }
        }

        #endregion

        #region Utilities


        internal static void ShowPopup(FeedbackVM feedbackHelper)
        {
            if (IsShown)
            {
                ClosePopup();
            }

            Popup = new Popup
            {
                Child = CreateFeedbackControl(feedbackHelper)
            };

            OpenPopup();
        }

        internal static void Close()
        {
            ClosePopup();
        }

        #endregion

        private static PhoneApplicationFrame RootFrame
        {
            get
            {
                return Application.Current.RootVisual as PhoneApplicationFrame;
            }
        }

        private static PhoneApplicationPage CurrentPage
        {
            get
            {
                PhoneApplicationPage currentPage = null;
                if (RootFrame != null)
                {
                    currentPage = RootFrame.Content as PhoneApplicationPage;
                }
                return currentPage;
            }
        }

        private static void SafeShow(Dispatcher dispatcher, Action showAction)
        {
            if (RootFrame != null)
            {
                showAction();
            }
            else
            {
                dispatcher.BeginInvoke(showAction);
            }
        }

        private static void HandleBackKey()
        {
            RootFrame.BackKeyPress += parentPage_BackKeyPress;
        }

        private static void parentPage_BackKeyPress(object sender, CancelEventArgs e)
        {
            RootFrame.BackKeyPress -= parentPage_BackKeyPress;
            if (IsShown)
            {
                ClosePopup();
                e.Cancel = true;
            }
        }

        private static FeedbackControl CreateFeedbackControl(FeedbackVM feedbackHelper)
        {
            var feedbackControl = new FeedbackControl(feedbackHelper)
            {
                Height =  Application.Current.Host.Content.ActualHeight,
                Width =  Application.Current.Host.Content.ActualWidth,
            };

            return feedbackControl;
        }

        private static DataTemplate TryFindTemplate(object templateKey)
        {
            DataTemplate template = null;
            if (templateKey != null && Application.Current.Resources.Contains(templateKey))
            {
                template = Application.Current.Resources[templateKey] as DataTemplate;
            }
            return template;
        }

        private static void OpenPopup()
        {
            SafeShow(Popup.Dispatcher, () =>
            {
                HandleBackKey();
                Popup.IsOpen = true;
            });
        }

        private static void ClosePopup()
        {
            if (Popup != null)
            {
                Popup.IsOpen = false;
                Popup = null;
            }
        }

    }
}
