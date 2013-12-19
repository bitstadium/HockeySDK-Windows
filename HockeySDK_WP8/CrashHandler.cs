/**
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
using System.Text;
using System.Windows;
using Microsoft.Phone.Info;
using Microsoft.Phone.Reactive;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using HockeyApp.Tools;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using HockeyApp.Exceptions;
using HockeyApp.Model;

namespace HockeyApp
{

    public enum CrashInfoType {
        crash, user, contact, description
    }

    public sealed class CrashHandler
    {
        private static readonly CrashHandler instance = new CrashHandler();

        private Application application = null;
        
        static CrashHandler() { }
        private CrashHandler() { }

        public static CrashHandler Instance
        {
            get
            {
                return instance;
            }
        }

        internal Application Application { get { return this.application; } }

        /// <summary>
        /// Configures the hockey crashhandler
        /// </summary>
        /// <param name="application">The WP Application</param>
        /// <param name="identifier">public identifier (App Identifier) of your app at HockeyApp</param>
        /// <param name="rootFrame">[optional] The rootframe of your app.</param>
        /// <param name="descriptionLoader">[optional] Func to load an additional description (e.g. log-info) when an exception occurs</param>
        /// <param name="apiBase">[optional] base URL of HockeyApp server. Only needed for private HockeyApp installations.</param>
        /// <param name="userId">[optional] Id/Name of current user of your app. Sent with crash information. Can also be set later with AddUserInfo(..)</param>
        /// <param name="contactInformation">[optional] Contact info of current user of your app. Sent with crash information. Can also be set later with AddUserInfo(..)</param>
        public void Configure(Application application, 
            string identifier, 
            Frame rootFrame = null, 
            Func<Exception,string> descriptionLoader = null, 
            string apiBase = null, 
            string userId = null, 
            string contactInformation = null)
        {
            if (this.application == null)
            {
                this.application = application;
                this.application.UnhandledException += OnUnhandledException;
                HockeyClient.ConfigureInternal(identifier,
                    ManifestHelper.GetAppVersion(),
                    userID: userId,
                    contactInformation: contactInformation, 
                    userAgentName: Constants.UserAgentString,
                    sdkName: Constants.SdkName, 
                    sdkVersion: Constants.SdkVersion,
                    descriptionLoader: descriptionLoader);
                if (rootFrame != null)
                {
                    //Idea based on http://www.markermetro.com/2013/01/technical/handling-unhandled-exceptions-with-asyncawait-on-windows-8-and-windows-phone-8/
                    AsyncSynchronizationContext.RegisterForFrame(rootFrame, this);
                }
            }
            else
            {
                throw new InvalidOperationException("CrashHandler was already configured!");
            }
        }

        /// <summary>
        /// Add infos about current user. Sent with error-traces to the hockeyapp server
        /// </summary>
        /// <param name="userid">id of the user</param>
        /// <param name="contactInfo">contact info like an email adress</param>
        public void AddUserInfo(String userid, String contactInfo)
        {
            HockeyClient.Instance.UserID = userid;
            HockeyClient.Instance.ContactInformation = contactInfo;
        }

        private void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs args)
        {
            HandleException(args.ExceptionObject);
        }

        internal void HandleException(Exception e)
        {
            StringBuilder logInfoBuilder = new StringBuilder();
            logInfoBuilder.AppendFormat("Package: {0}\n", application.GetType().Namespace);
            logInfoBuilder.AppendFormat("Version: {0}\n", HockeyClient.Instance.VersionInfo);
            logInfoBuilder.AppendFormat("Windows Phone: {0}\n", Environment.OSVersion.Version.ToString());
            logInfoBuilder.AppendFormat("Manufacturer: {0}\n", GetDeviceManufacturer());
            logInfoBuilder.AppendFormat("Model: {0}\n", GetDeviceModel());
            logInfoBuilder.AppendFormat("Date: {0}\n", DateTime.UtcNow.ToString("o"));
            logInfoBuilder.AppendFormat("Product-ID: {0}\n", ManifestHelper.GetProductID());

            ICrashData cd = HockeyClient.Instance.CreateCrashData(e, logInfoBuilder.ToString());

            var crashId = Guid.NewGuid();
            try
            {
                IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                if (!store.DirectoryExists(Constants.CrashDirectoryName))
                {
                    store.CreateDirectory(Constants.CrashDirectoryName);
                }

                String filename = string.Format("{0}{1}.log", Constants.CrashFilePrefix, crashId);
                FileStream stream = store.CreateFile(Path.Combine(Constants.CrashDirectoryName, filename));
                cd.Serialize(stream);
                stream.Close();
            }
            catch
            {
                // Ignore all exceptions
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
                    if (store.DirectoryExists(Constants.CrashDirectoryName))
                    {
                        string[] filenames = store.GetFileNames(Path.Combine(Constants.CrashDirectoryName, Constants.CrashFilePrefix + "*.log"));
                        if (filenames.Length > 0)
                        {
                            Debugger.Log(0, "HockeyApp", filenames.Aggregate((a, b) => a + " | " + b).ToString());
                            if (sendAutomatically)
                            {
                                SendCrashesAsync(store, filenames);
                            }
                            else
                            {
                                ShowNotificationToSend(store, filenames);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore all exceptions
                }
            }
        }

        public async Task<bool> HandleCrashesAsync(Boolean sendAutomatically = false)
        {
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
                            Debugger.Log(0, "HockeyApp", filenames.Aggregate((a, b) => a + " | " + b).ToString());
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
                catch 
                {
                    // Ignore all exceptions
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
                            catch 
                            {
                                //TODO logging auf WP? 
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
                try
                {
                    if (store.FileExists(Path.Combine(Constants.CrashDirectoryName, filename)))
                    {
                        Stream fileStream = store.OpenFile(Path.Combine(Constants.CrashDirectoryName, filename), FileMode.Open);
                        ICrashData cd = HockeyClient.Instance.Deserialize(fileStream);
                        fileStream.Close();
                        await cd.SendDataAsync();
                        store.DeleteFile(Path.Combine(Constants.CrashDirectoryName, filename));
                    }
                }
                catch (WebTransferException)
                {

                }
                catch (Exception)
                {

                }
            }
        }

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
