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
using System.Net;
using System.Text;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Phone.Info;
using Microsoft.Phone.Reactive;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Net.Browser;
using HockeyApp.Tools;
using System.Windows.Controls;

namespace HockeyApp
{

    public enum CrashInfoType {
        crash, user, contact, description
    }

    public sealed class CrashHandler
    {
        private static readonly CrashHandler instance = new CrashHandler();

        private Application application = null;
        private string identifier = null;

        private string userid = null;
        private string contactInfo = null;

        static CrashHandler() { }
        private CrashHandler() { }

        public static CrashHandler Instance
        {
            get
            {
                return instance;
            }
        }

        public void Configure(Application application, string identifier, Frame rootFrame = null)
        {
            if (this.application == null)
            {
                this.application = application;
                this.identifier = identifier;
                this.application.UnhandledException += OnUnhandledException;
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

        public void AddUserInfo(String userid, String contactInfo)
        {
            this.userid = userid;
            this.contactInfo = contactInfo;
        }

        private void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs args)
        {
            HandleException(args.ExceptionObject);
        }

        internal void HandleException(Exception e)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(CreateHeader());
            builder.AppendLine();
            builder.Append(CreateStackTrace(e));
            var crashId = SaveLog(builder.ToString(), CrashInfoType.crash);
            if (this.userid != null) { SaveLog(userid, CrashInfoType.user, crashId); }
            if (this.contactInfo != null) { SaveLog(userid, CrashInfoType.contact, crashId); }
        }


        public String CreateHeader()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Package: {0}\n", application.GetType().Namespace);
            builder.AppendFormat("Product-ID: {0}\n", ManifestHelper.GetProductID());
            builder.AppendFormat("Version: {0}\n", ManifestHelper.GetAppVersion());
            builder.AppendFormat("Windows Phone: {0}\n", Environment.OSVersion.Version.ToString());
            builder.AppendFormat("Manufacturer: {0}\n", GetDeviceManufacturer());
            builder.AppendFormat("Model: {0}\n", GetDeviceModel());
            builder.AppendFormat("Date: {0}\n", DateTime.UtcNow.ToString("o"));

            return builder.ToString();
        }

        private String CreateStackTrace(Exception exception)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append (exception.GetType ().ToString());
            builder.Append (": ");
            builder.Append(string.IsNullOrEmpty(exception.Message) ? "No reason" : exception.Message);
            builder.AppendLine ();
            builder.Append(string.IsNullOrEmpty(exception.StackTrace) ? "  at unknown location" : exception.StackTrace);

            Exception inner = exception.InnerException;
            if ((inner != null) && (!string.IsNullOrEmpty(inner.StackTrace)))
            {
                builder.AppendLine();
                builder.AppendLine("Inner Exception");
                builder.Append(inner.StackTrace);
            }

            return builder.ToString().Trim();
        }

        private Guid SaveLog(String log, CrashInfoType infoType, Guid? id = null)
        {
            var crashId = id ?? Guid.NewGuid();
            try
            {
                IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                if (!store.DirectoryExists(Constants.CrashDirectoryName))
                {
                    store.CreateDirectory(Constants.CrashDirectoryName);
                }
                
                String filename = string.Format("{0}{1}.log", infoType.ToString(), crashId);
                FileStream stream = store.CreateFile(Path.Combine(Constants.CrashDirectoryName, filename));
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(log);
                }
                stream.Close();
            }
            catch
            {
                // Ignore all exceptions
            }
            return crashId;
        }

        public void HandleCrashes()
        {
            HandleCrashes(false);
        }

        public void HandleCrashes(Boolean sendAutomatically)
        {
            try
            {
                IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                if (store.DirectoryExists(Constants.CrashDirectoryName))
                {
                    string[] filenames = store.GetFileNames(Constants.CrashDirectoryName + "\\crash*.log");
                    Debugger.Log(0, "HockeyApp", filenames.ToString());

                    if (filenames.Length > 0)
                    {
                        if (sendAutomatically)
                        {
                            SendCrashes(store, filenames);
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

        private void ShowNotificationToSend(IsolatedStorageFile store, string[] filenames)
        {
            Scheduler.Dispatcher.Schedule(() =>
            {
                NotificationTool.Show(
                    "Crash Data",
                    "The app quit unexpectedly. Would you like to send information about this to the developer?",
                    new NotificationAction("Send", () =>
                    {
                        SendCrashes(store, filenames);
                    }),
                    new NotificationAction("Delete", () =>
                    {
                        DeleteCrashes(store, filenames);
                    })
                );
            });
        }

        private String GetFileContentsIfExists(IsolatedStorageFile store, string filename)
        {
            String content = null;
            if (store.FileExists(Path.Combine(Constants.CrashDirectoryName, filename)))
            {
                Stream fileStream = store.OpenFile(Path.Combine(Constants.CrashDirectoryName, filename), FileMode.Open);
                using (StreamReader reader = new StreamReader(fileStream))
                {
                     content = reader.ReadToEnd();
                } 
                fileStream.Close();
            }
            return content;
        }

        private void SendCrashes(IsolatedStorageFile store, string[] filenames)
        {
            foreach (String filename in filenames)
            {
                try
                {
                    string log = GetFileContentsIfExists(store, filename) ?? "";
                    string user = GetFileContentsIfExists(store, filename.Replace(CrashInfoType.crash.ToString(), CrashInfoType.user.ToString()));
                    string contact = GetFileContentsIfExists(store, filename.Replace(CrashInfoType.crash.ToString(), CrashInfoType.contact.ToString())); ;
                    string description = GetFileContentsIfExists(store, filename.Replace(CrashInfoType.crash.ToString(), CrashInfoType.description.ToString())); ;

                    string body = "";
                    body += "raw=" + HttpUtility.UrlEncode(log);
                    if (user != null)
                    {
                        body += "&userID=" + HttpUtility.UrlEncode(user);
                    }
                    if (contact != null)
                    {
                        body += "&contact=" + HttpUtility.UrlEncode(contact);
                    }
                    if (description != null)
                    {
                        body += "&description=" + HttpUtility.UrlEncode(description);
                    }
                    body += "&sdk=" + Constants.SdkName;
                    body += "&sdk_version=" + Constants.SdkVersion;

                    WebRequest request = WebRequestCreator.ClientHttp.Create(new Uri(Constants.ApiBase + "apps/" + identifier + "/crashes"));
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Headers[HttpRequestHeader.UserAgent] = Constants.UserAgentString;

                    request.BeginGetRequestStream(requestResult =>
                    {
                        try
                        {
                            Stream stream = request.EndGetRequestStream(requestResult);
                            byte[] byteArray = Encoding.UTF8.GetBytes(body);
                            stream.Write(byteArray, 0, body.Length);
                            stream.Close();

                            request.BeginGetResponse(responseResult =>
                            {
                                Boolean deleteCrashes = true;
                                try
                                {
                                    request.EndGetResponse(responseResult);
                                }
                                catch (WebException e)
                                {
                                    if ((e.Status == WebExceptionStatus.ConnectFailure) ||
                                        (e.Status == WebExceptionStatus.ReceiveFailure) ||
                                        (e.Status == WebExceptionStatus.SendFailure) ||
                                        (e.Status == WebExceptionStatus.Timeout) ||
                                        (e.Status == WebExceptionStatus.UnknownError))
                                    {
                                        deleteCrashes = false;
                                    }
                                }
                                catch (Exception)
                                {
                                }
                                finally
                                {
                                    if (deleteCrashes)
                                    {
                                        DeleteCrashes(store, filenames);
                                    }
                                }
                            }, null);
                        }
                        catch (Exception)
                        {
                        }
                    }, null);
                }
                catch (Exception)
                {
                    store.DeleteFile(Path.Combine(Constants.CrashDirectoryName, filename));
                }
            }
        }

        private void DeleteCrashes(IsolatedStorageFile store, string[] filenames)
        {
            foreach (String filename in filenames)
            {
                store.DeleteFile(Path.Combine(Constants.CrashDirectoryName, filename));
            }
        }

        internal static String GetDeviceManufacturer()
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

        internal static String GetDeviceModel()
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
