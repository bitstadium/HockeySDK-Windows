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
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Phone.Info;
using Microsoft.Phone.Reactive;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Net.Browser;
using HockeyApp.Tools;

namespace HockeyApp
{
    public sealed class CrashHandler
    {
        private const String CrashDirectoryName = "CrashLogs";
        private const String SdkName = "HockeySDK";
        private const String SdkVersion = "1.0.1";

        private static readonly CrashHandler instance = new CrashHandler();

        private Application application = null;
        private string identifier = null;

        static CrashHandler() { }
        private CrashHandler() { }

        public static CrashHandler Instance
        {
            get
            {
                return instance;
            }
        }

        public void Configure(Application application, string identifier)
        {
            if (this.application == null)
            {
                this.application = application;
                this.identifier = identifier;

                this.application.UnhandledException += OnUnhandledException;
            }
            else
            {
                throw new InvalidOperationException("CrashHandler was already configured!");
            }
        }

        private void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs args)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(CreateHeader());
            builder.AppendLine();
            builder.Append(CreateStackTrace(args));
            SaveLog(builder.ToString());
        }

        public String CreateHeader()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Package: {0}\n", application.GetType().Namespace);
            builder.AppendFormat("Product-ID: {0}\n", GetProductID());
            builder.AppendFormat("Version: {0}\n", GetAppVersion());
            builder.AppendFormat("Windows Phone: {0}\n", Environment.OSVersion.Version.ToString());
            builder.AppendFormat("Manufacturer: {0}\n", GetDeviceManufacturer());
            builder.AppendFormat("Model: {0}\n", GetDeviceModel());
            builder.AppendFormat("Date: {0}\n", DateTime.UtcNow.ToString("o"));

            return builder.ToString();
        }

        private String CreateStackTrace(ApplicationUnhandledExceptionEventArgs args)
        {
            Exception exception = args.ExceptionObject;
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

        private void SaveLog(String log)
        {
            try
            {
                IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                if (!store.DirectoryExists(CrashDirectoryName))
                {
                    store.CreateDirectory(CrashDirectoryName);
                }

                String filename = string.Format("crash{0}.log", Guid.NewGuid());
                FileStream stream = store.CreateFile(Path.Combine(CrashDirectoryName, filename));
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
        }

        private static String GetDeviceManufacturer()
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

        private static String GetDeviceModel()
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

        // Idea based on http://bjorn.kuiper.nu/2011/10/01/wp7-notify-user-of-new-application-version/
        private static String GetAppVersion()
        {
            return getValueFromManifest("Version");
        }

        private static String GetProductID()
        {
            return getValueFromManifest("ProductID");
        }

        private static String getValueFromManifest(String key)
        {
            try
            {
                StreamReader reader = getManifestReader();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    int begin = line.IndexOf(" " + key + "=\"", StringComparison.InvariantCulture);
                    if (begin >= 0)
                    {
                        int end = line.IndexOf("\"", begin + key.Length + 3, StringComparison.InvariantCulture);
                        if (end >= 0)
                        {
                            return line.Substring(begin + key.Length + 3, end - begin - key.Length - 3);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore all exceptions
            }

            return "";
        }

        private static StreamReader getManifestReader()
        {
            Uri manifest = new Uri("WMAppManifest.xml", UriKind.Relative);
            var stream = Application.GetResourceStream(manifest);
            if (stream != null)
            {
                return new StreamReader(stream.Stream);
            }
            else
            {
                return null;
            }
        }

        public void HandleCrashes()
        {
            try
            {
                IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                if (store.DirectoryExists(CrashDirectoryName))
                {
                    string[] filenames = store.GetFileNames(CrashDirectoryName + "\\crash*.log");
                    Debugger.Log(0, "HockeyApp", filenames.ToString());

                    if (filenames.Length > 0)
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
                }
            }
            catch (Exception)
            {
                // Ignore all exceptions
            }
        }

        private void SendCrashes(IsolatedStorageFile store, string[] filenames)
        {
            foreach (String filename in filenames)
            {
                try
                {
                    Stream fileStream = store.OpenFile(Path.Combine(CrashDirectoryName, filename), FileMode.Open);
                    string log = "";
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        log = reader.ReadToEnd();
                    }
                    string body = "";
                    body += "raw=" + HttpUtility.UrlEncode(log);
                    body += "&sdk=" + SdkName;
                    body += "&sdk_version=" + SdkVersion;
                    fileStream.Close();

                    WebRequest request = WebRequestCreator.ClientHttp.Create(new Uri("https://rink.hockeyapp.net/api/2/apps/" + identifier + "/crashes"));
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.Headers[HttpRequestHeader.UserAgent] = "Hockey/WP7";

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
                    store.DeleteFile(Path.Combine(CrashDirectoryName, filename));
                }
            }
        }

        private void DeleteCrashes(IsolatedStorageFile store, string[] filenames)
        {
            foreach (String filename in filenames)
            {
                store.DeleteFile(Path.Combine(CrashDirectoryName, filename));
            }
        }
    }
}
