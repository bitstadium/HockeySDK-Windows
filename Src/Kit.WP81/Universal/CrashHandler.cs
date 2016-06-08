/*
 * The MIT License
 * Copyright (c) 2014 Codenauts UG (haftungsbeschränkt). All rights reserved.
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE. 
 */

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using Microsoft.HockeyApp.Tools;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using Microsoft.HockeyApp.Exceptions;
using Microsoft.HockeyApp.Model;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Windows.UI.Popups;

namespace Microsoft.HockeyApp
{

    /// <summary>
    /// Providing Crash-Handling functionality with HockeyApp in your App
    /// </summary>
    internal partial class CrashHandler
    {

        private ILog logger = HockeyLogManager.GetLog(typeof(CrashHandler));

        protected static CrashHandler _instance;

        static CrashHandler() { _instance = new CrashHandler(); }

        internal static CrashHandler Current
        {
            get { return _instance; }
        }

        /// <summary>
        /// Handle saved crashes async. Checks if new error traces are available and notifies the user if he wants to send them.
        /// </summary>
        /// <param name="sendAutomatically">suppress the notification box and try to send crashes in the background.</param>
        /// <returns>Awaitable Task. Result is true if crashes have been sent.</returns>
        public async Task<bool> HandleCrashesAsync(Boolean sendAutomatically = false)
        {
            var helper = HockeyClient.Current.AsInternal().PlatformHelper;

            if (NetworkInterface.GetIsNetworkAvailable())
            {
                if (await HockeyClient.Current.AsInternal().AnyCrashesAvailableAsync())
                {

                    if (sendAutomatically)
                    {
                        await HockeyClient.Current.AsInternal().SendCrashesAndDeleteAfterwardsAsync();
                        return true;
                    }
                    else
                    {
                        if (await AskUserForAgreementAsync())
                        {
                            await HockeyClient.Current.AsInternal().SendCrashesAndDeleteAfterwardsAsync();
                            return true;
                        }
                        else
                        {
                            await HockeyClient.Current.AsInternal().DeleteAllCrashesAsync();
                            return false;
                        };
                    }
                }
            }
            return false;
        }

        protected virtual async Task<bool> AskUserForAgreementAsync()
        {
            var messageDialog = new MessageDialog(LocalizedStrings.LocalizedResources.SendCrashQuestion, LocalizedStrings.LocalizedResources.CrashData);

            // Add commands and set their command ids
            messageDialog.Commands.Add(new UICommand(LocalizedStrings.LocalizedResources.Send, null, true));
            messageDialog.Commands.Add(new UICommand(LocalizedStrings.LocalizedResources.Delete, null, false));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 1;

            // Show the message dialog and get the event that was invoked via the async operator
            var commandChosen = await messageDialog.ShowAsync();

            if (commandChosen == null) {
                return false;
            }

            return (Boolean)commandChosen.Id;
        }
    }
}
