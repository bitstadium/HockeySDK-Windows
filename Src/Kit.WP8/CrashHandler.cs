/*
 * The MIT License
 * Copyright (c) 2012 Codenauts UG (haftungsbeschränkt). All rights reserved.
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
using Microsoft.Phone.Info;
using Microsoft.Phone.Reactive;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using Microsoft.HockeyApp.Tools;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using Microsoft.HockeyApp.Exceptions;
using Microsoft.HockeyApp.Model;
using Microsoft.HockeyApp;
using System.Globalization;

namespace Microsoft.HockeyApp
{

    /// <summary>
    /// Providing Crash-Handling functionality with HockeyApp in your app.  Don't use directly. Use HockeyClient.Current - extension methods
    /// </summary>
    public class CrashHandler
    {
        private static readonly CrashHandler _instance = new CrashHandler();

        private ILog logger = HockeyLogManager.GetLog(typeof(CrashHandler));

        private CrashLogInformation _crashLogInfo;
        private Application _application = null;
        
        static CrashHandler() { }
        private CrashHandler() {}

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        [Obsolete]
        public static CrashHandler Instance
        {
            get
            {
                return _instance;
            }
        }

        internal static CrashHandler Current { get { return _instance; } }

        internal Application Application
        {
            get { return this._application; }
            set { this._application = value; }
        }

        [Obsolete]
        private void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs args)
        {
            HandleException(args.ExceptionObject);
        }

        internal void HandleException(Exception unhandledException)
        {
            CrashLogInformation crashInfo = this._crashLogInfo;
            //TODO refactor in next version
            if (this._crashLogInfo.PackageName == null) { this._crashLogInfo = HockeyClient.Current.AsInternal().PrefilledCrashLogInfo; }
            ICrashData cd = HockeyClient.Current.AsInternal().CreateCrashData(unhandledException, this._crashLogInfo);

            var crashId = Guid.NewGuid();
            try
            {
                IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                if (!store.DirectoryExists(Constants.CrashDirectoryName))
                {
                    store.CreateDirectory(Constants.CrashDirectoryName);
                }

                String filename = string.Format(CultureInfo.InvariantCulture, "{0}{1}.log", Constants.CrashFilePrefix, crashId);
                using (FileStream stream = store.CreateFile(Path.Combine(Constants.CrashDirectoryName, filename)))
                {
                    cd.Serialize(stream);
                }
            }
            catch (Exception ex)
            {
                HockeyClient.Current.AsInternal().HandleInternalUnhandledException(ex);
            }
        }


        /// <summary>
        /// Handle saved crashes. Checks if new error traces are available and notifies the user if he wants to send them.
        /// </summary>
        /// <param name="sendAutomatically">suppress the notification box and try to send crashes in the background.</param>
        public void HandleCrashes(Boolean sendAutomatically = false)
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                    //TODO remove in next major version
                    MoveOldCrashlogsIfNeeded(store);
                    if (store.DirectoryExists(Constants.CrashDirectoryName))
                    {
                        string[] filenames = store.GetFileNames(Path.Combine(Constants.CrashDirectoryName, Constants.CrashFilePrefix + "*.log"));
                        if (filenames.Length > 0)
                        {
                            Debugger.Log(0, "Microsoft.HockeyApp", filenames.Aggregate((a, b) => a + " | " + b).ToString());
                            if (sendAutomatically)
                            {
                                var __ = SendCrashesAsync(store, filenames);
                            }
                            else
                            {
                                ShowNotificationToSend(store, filenames);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    HockeyClient.Current.AsInternal().HandleInternalUnhandledException(e);
                }
            }
        }

        public void MoveOldCrashlogsIfNeeded(IsolatedStorageFile store)
        {
            try
            {
                if (store.DirectoryExists(Constants.OldCrashDirectoryName))
                {
                    var files = store.GetFileNames(Path.Combine(Constants.OldCrashDirectoryName, Constants.CrashFilePrefix + "*.log"));
                    if (files.Length > 0)
                    {
                        if (!store.DirectoryExists(Constants.CrashDirectoryName))
                        {
                            store.CreateDirectory(Constants.CrashDirectoryName);
                        }
                        foreach (var fileName in files)
                        {
                            store.MoveFile(Path.Combine(Constants.OldCrashDirectoryName, Path.GetFileName(fileName)), Path.Combine(Constants.CrashDirectoryName, Path.GetFileName(fileName)));
                        }
                        if (store.GetFileNames(Path.Combine(Constants.OldCrashDirectoryName, Constants.CrashFilePrefix + "*.*")).Length == 0)
                        {
                            store.DeleteDirectory(Constants.OldCrashDirectoryName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                HockeyClient.Current.AsInternal().HandleInternalUnhandledException(e);
            }
        }

        /// <summary>
        /// Handle saved crashes async. Checks if new error traces are available and notifies the user if he wants to send them.
        /// </summary>
        /// <param name="sendAutomatically">suppress the notification box and try to send crashes in the background.</param>
        /// <returns>Awaitable Task. Result is true if crashes have been sent.</returns>
        public async Task<bool> HandleCrashesAsync(Boolean sendAutomatically = false)
        {
            //TODO refactor for use with platformhelper
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                try
                {
                    IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                    if (store.DirectoryExists(Constants.CrashDirectoryName))
                    {
                        string[] filenames = store.GetFileNames(Path.Combine(Constants.CrashDirectoryName, Constants.CrashFilePrefix + "*.log"));
                        if (filenames.Length > 0)
                        {
                            Debugger.Log(0, "Microsoft.HockeyApp", filenames.Aggregate((a, b) => a + " | " + b).ToString());
                            if (sendAutomatically)
                            {
                                await SendCrashesAsync(store, filenames);
                                return true;
                            }
                            else
                            {
                                return await ShowNotificationToSend(store, filenames);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    HockeyClient.Current.AsInternal().HandleInternalUnhandledException(e);
                }
            }
            return false;
        }

        private Task<bool> ShowNotificationToSend(IsolatedStorageFile store, string[] filenames)
        {
            var tcs = new TaskCompletionSource<bool>();
            Scheduler.Dispatcher.Schedule(() =>
            {
                NotificationTool.Show(
                    LocalizedStrings.LocalizedResources.CrashData,
                    LocalizedStrings.LocalizedResources.SendCrashQuestion,
                    new NotificationAction(LocalizedStrings.LocalizedResources.Send, (Action)(async () =>
                    {
                        tcs.TrySetResult(true);
                        await SendCrashesAsync(store, filenames);
                    })),
                    new NotificationAction(LocalizedStrings.LocalizedResources.Delete, (Action) (() =>
                    {
                        tcs.TrySetResult(false);
                        foreach (string filename in filenames)
                        {
                            try
                            {
                                store.DeleteFile(Path.Combine(Constants.CrashDirectoryName, filename));
                            }
                            catch (Exception e)
                            {
                                HockeyClient.Current.AsInternal().HandleInternalUnhandledException(e);
                            }
                        }
                    }))
                );
            });
            return tcs.Task;
        }

        private async Task SendCrashesAsync(IsolatedStorageFile store, string[] filenames)
        {
            foreach (String filename in filenames)
            {
                if (store.FileExists(Path.Combine(Constants.CrashDirectoryName, filename)))
                {
                    ICrashData cd;
                    try
                    {
                        using (Stream fileStream = store.OpenFile(Path.Combine(Constants.CrashDirectoryName, filename), FileMode.Open))
                        {
                            cd = HockeyClient.Current.AsInternal().Deserialize(fileStream);
                        }
                        await cd.SendDataAsync();
                    }
                    catch (WebTransferException e)
                    {
                        //try again on next run
                        HockeyClient.Current.AsInternal().HandleInternalUnhandledException(e);
                        return;
                    }
                    store.DeleteFile(Path.Combine(Constants.CrashDirectoryName, filename));
                }
            }
        }

        [Obsolete]
        public static String GetDeviceManufacturer()
        {
            object manufacturer;
            if (DeviceExtendedProperties.TryGetValue("DeviceManufacturer", out manufacturer))
            {
                return manufacturer.ToString();
            }
            else
            {
                return "Unknown";
            }
        }

        [Obsolete]
        public static String GetDeviceModel()
        {
            object model;
            if (DeviceExtendedProperties.TryGetValue("DeviceName", out model))
            {
                return model.ToString();
            }
            else
            {
                return "Unknown";
            }
        }
    }
}
