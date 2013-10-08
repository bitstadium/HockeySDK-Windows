/**
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using HockeyApp.Controls;

namespace HockeyApp.Controls.NotificationBox
{
    public class NotificationBox : ItemsControl
    {
        #region Properties

        #region Title Property

        /// <summary>
        /// Gets or sets the title text to display.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        /// <value>Identifies the Title dependency property</value>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
            "Title",
            typeof(string),
            typeof(NotificationBox),
              new PropertyMetadata(default(string)));

        #endregion

        #region Message Property

        /// <summary>
        /// Gets or sets the message text to display.
        /// </summary>
        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <value>Identifies the Message dependency property</value>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
            "Message",
            typeof(string),
            typeof(NotificationBox),
              new PropertyMetadata(default(string)));

        #endregion

        #region ShowAgainOption Property

        /// <summary>
        /// Gets or sets a value for indicating if this message is to be shown again.
        /// </summary>
        public bool ShowAgainOption
        {
            get { return (bool)GetValue(ShowAgainOptionProperty); }
            set { SetValue(ShowAgainOptionProperty, value); }
        }

        /// <value>Identifies the ShowAgainOption dependency property</value>
        public static readonly DependencyProperty ShowAgainOptionProperty =
            DependencyProperty.Register(
            "ShowAgainOption",
            typeof(bool),
            typeof(NotificationBox),
              new PropertyMetadata(default(bool), ShowAgainOptionChanged));

        /// <summary>
        /// Invoked on ShowAgainOption change.
        /// </summary>
        /// <param name="d">The object that was changed</param>
        /// <param name="e">Dependency property changed event arguments</param>
        private static void ShowAgainOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var message = d as NotificationBox;
            if (message.UniqueKey != null)
            {
                Settings[message.UniqueKey] = e.NewValue;
            }
        }

        #endregion

        #region ShowAgainText Property

        /// <summary>
        /// Gets or sets the text asking if this message should be shown again.
        /// </summary>
        public string ShowAgainText
        {
            get { return (string)GetValue(ShowAgainTextProperty); }
            set { SetValue(ShowAgainTextProperty, value); }
        }

        /// <value>Identifies the ShowAgainText dependency property</value>
        public static readonly DependencyProperty ShowAgainTextProperty =
            DependencyProperty.Register(
            "ShowAgainText",
            typeof(string),
            typeof(NotificationBox),
              new PropertyMetadata(default(string)));

        #endregion

        #region ShowAgainVisibility Property

        /// <summary>
        /// Gets or sets a value indicating if the show again message is visible or not.
        /// </summary>
        public Visibility ShowAgainVisibility
        {
            get { return (Visibility)GetValue(ShowAgainVisibilityProperty); }
            set { SetValue(ShowAgainVisibilityProperty, value); }
        }

        /// <value>Identifies the ShowAgainVisibility dependency property</value>
        public static readonly DependencyProperty ShowAgainVisibilityProperty =
            DependencyProperty.Register(
            "ShowAgainVisibility",
            typeof(Visibility),
            typeof(NotificationBox),
              new PropertyMetadata(default(Visibility)));

        #endregion

        internal string UniqueKey { get; set; }

        private static IsolatedStorageSettings Settings
        {
            get { return IsolatedStorageSettings.ApplicationSettings; }
        }

        #endregion

        #region Ctor

        public NotificationBox()
        {
            DefaultStyleKey = typeof(NotificationBox);
        }

        #endregion

        #region Overrides

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new NotificationBoxItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is NotificationBoxItem;
        }

        #endregion
    }
}
