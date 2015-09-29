/*
 * Origin:
 * http://wpassets.codeplex.com/SourceControl/changeset/view/86675
 * 
 * Microsoft Public License (Ms-PL)
 * This license governs use of the accompanying software. If you use the
 * software, you accept this license. If you do not accept the license, do
 * not use the software.
 *
 * 1. Definitions
 *
 * The terms "reproduce," "reproduction," "derivative works," and
 * "distribution" have the same meaning here as under U.S. copyright law.
 *
 * A "contribution" is the original software, or any additions or changes
 * to the software.
 *
 * A "contributor" is any person that distributes its contribution under
 * this license.
 *
 * "Licensed patents" are a contributor's patent claims that read directly
 * on its contribution.
 *
 * 2. Grant of Rights
 *
 * (A) Copyright Grant- Subject to the terms of this license, including the
 * license conditions and limitations in section 3, each contributor grants
 * you a non-exclusive, worldwide, royalty-free copyright license to reproduce
 * its contribution, prepare derivative works of its contribution, and
 * distribute its contribution or any derivative works that you create.
 *
 * (B) Patent Grant- Subject to the terms of this license, including the
 * license conditions and limitations in section 3, each contributor grants
 * you a non-exclusive, worldwide, royalty-free license under its licensed
 * patents to make, have made, use, sell, offer for sale, import, and/or
 * otherwise dispose of its contribution in the software or derivative works
 * of the contribution in the software.
 *
 * 3. Conditions and Limitations
 *
 * (A) No Trademark License - This license does not grant you rights to use
 * any contributors' name, logo, or trademarks.
 *
 * (B) If you bring a patent claim against any contributor over patents that
 * you claim are infringed by the software, your patent license from such
 * contributor to the software ends automatically.
 *
 * (C) If you distribute any portion of the software, you must retain all
 * copyright, patent, trademark, and attribution notices that are present
 * in the software.
 *
 * (D) If you distribute any portion of the software in source code form,
 * you may do so only under this license by including a complete copy of
 * this license with your distribution. If you distribute any portion of
 * the software in compiled or object code form, you may only do so under
 * a license that complies with this license.
 *
 * (E) The software is licensed "as-is." You bear the risk of using it. The
 * contributors give no express warranties, guarantees or conditions. You
 * may have additional consumer rights under your local laws which this
 * license cannot change. To the extent permitted under your local laws,
 * the contributors exclude the implied warranties of merchantability,
 * fitness for a particular purpose and non-infringement.
 */

using System;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using HockeyApp.Controls.NotificationBox;

namespace HockeyApp.Tools
{
    public class NotificationTool
    {
        #region Consts

        public const double NotificationWidth = 480;
        public const double NotificationHeight = 800;

        #endregion

        #region Properties

        private static Popup Popup { get; set; }
        private static bool AppBarVisibility { get; set; }

        private static NotificationBox Notification
        {
            get
            {
                if (Popup == null)
                {
                    return null;
                }

                return Popup.Child as NotificationBox;
            }
        }

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

        private static IsolatedStorageSettings Settings
        {
            get { return IsolatedStorageSettings.ApplicationSettings; }
        }

        private static string UniqueKey { get; set; }

        private static Action<bool> SuppressionCallback { get; set; }

        #endregion

        #region Utilities

        /// <summary>
        /// Displays a notification box with title, message and custom actions.
        /// </summary>
        /// <param name="title">The title of this message.</param>
        /// <param name="message">The message body text.</param>
        /// <param name="actions">A collection of actions.</param>
        public static void Show(string title, string message, params NotificationAction[] actions)
        {
            if (IsShown)
            {
                ClosePopup();
            }

            Popup = new Popup
            {
                Child = CreateNotificationBox(title, message, actions)
            };

            OpenPopup();
        }

        /// <summary>
        /// Displays a notification box with title, message and custom actions.
        /// In addition a message asking if this message should be shown again next time.
        /// </summary>
        /// <param name="title">The title of this message.</param>
        /// <param name="message">The message body text.</param>        
        /// <param name="showAgainText">The text asking if this message should be shown again.</param>
        /// <param name="forceShowAgain">Value indicating whether to force message display in case that the user suppressed this message, </param>
        /// <param name="suppression">Callback for indicating whether message suppressed or not..</param>
        /// <param name="uniqueKey">Unique key representing a specific message identity.</param>
        /// <param name="actions">A collection of actions.</param>
        public static void ShowAgain(string title, string message, string showAgainText, bool forceShowAgain, Action<bool> suppression, string uniqueKey, params NotificationAction[] actions)
        {
            if (IsShown)
            {
                ClosePopup();
            }

            bool showAgain;
            if (!Settings.TryGetValue(uniqueKey, out showAgain))
            {
                showAgain = true;
                Settings[uniqueKey] = showAgain;
            }

            if (showAgain || forceShowAgain)
            {
                Popup = new Popup
                {
                    Child = CreateNotificationBox(title, message, actions, Visibility.Visible, showAgain, showAgainText, uniqueKey)
                };

                SuppressionCallback = suppression;
                UniqueKey = uniqueKey;
                OpenPopup();
            }
        }

        /// <summary>
        /// Displays a notification box with title, message and custom actions.
        /// In addition a message asking if this message should be shown again next time.
        /// </summary>
        /// <param name="title">The title of this message.</param>
        /// <param name="message">The message body text.</param>        
        /// <param name="forceShowAgain">Value indicating whether to force message display in case that the user suppressed this message, </param>
        /// <param name="suppression">Callback for indicating whether message suppressed or not..</param>
        /// <param name="uniqueKey">Unique key representing a specific message identity.</param>
        /// <param name="commands">A collection of actions.</param>
        public static void ShowAgain(string title, string message, bool forceShowAgain, Action<bool> suppression, string uniqueKey, params NotificationAction[] commands)
        {
            ShowAgain(title, message, "Show this message again", forceShowAgain, suppression, uniqueKey, commands);
        }

        internal static void Close()
        {
            if (SuppressionCallback != null)
            {
                SuppressionCallback(!Notification.ShowAgainOption);
            }

            ClosePopup();
        }

        #endregion

        private static PhoneApplicationFrame RootFrame
        {
            get
            {
                try
                {
                    return Application.Current.RootVisual as PhoneApplicationFrame;
                }
                catch
                {
                    return null;
                }
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

        private static void HandleBackKeyAndAppBar()
        {
            RootFrame.BackKeyPress += parentPage_BackKeyPress;
            if (CurrentPage.ApplicationBar != null)
            {
                AppBarVisibility = CurrentPage.ApplicationBar.IsVisible;
                CurrentPage.ApplicationBar.IsVisible = false;
            }
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

        private static NotificationBox CreateNotificationBox(string title, string message, NotificationAction[] commands, Visibility showAgainVisibility = Visibility.Collapsed, bool showAgain = true, string showAgainText = null, string uniqueKey = null)
        {
            var notificationBox = new NotificationBox
            {
                Width = NotificationWidth,
                Height = NotificationHeight,
                Title = title,
                Message = message,
                ShowAgainOption = showAgain,
                ShowAgainText = showAgainText,
                ShowAgainVisibility = showAgainVisibility,
                UniqueKey = uniqueKey
            };

            foreach (var action in commands)
            {
                var notificationBoxItem = new NotificationBoxItem
                {
                    Content = action.Content,
                    Command = action.Command,
                    ContentTemplate = TryFindTemplate(action.ContentTemplateKey)
                };

                notificationBox.Items.Add(notificationBoxItem);
            }

            return notificationBox;
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
                HandleBackKeyAndAppBar();
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

            if (CurrentPage.ApplicationBar != null)
            {
                CurrentPage.ApplicationBar.IsVisible = AppBarVisibility;
            }
        }
    }
}
