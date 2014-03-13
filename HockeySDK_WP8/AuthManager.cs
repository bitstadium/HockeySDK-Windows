using HockeyApp.Model;
using Microsoft.Phone.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using HockeyApp.Tools;

namespace HockeyApp
{
    public enum TokenValidationPolicy { Never, EveryLogin, OnNewVersion }
    public enum AuthenticationMode { Authorize, Identify }
    public enum AuthValidationMode { Strict, Graceful }

    public class AuthManager
    {
        private static readonly AuthManager instance = new AuthManager();

        static AuthManager() { }
        private AuthManager() { }

        public static AuthManager Instance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        /// Store data encrypted in the isolatedStorage
        /// </summary>
        /// <param name="dataIdentifier">identifier for the data to write</param>
        /// <param name="data"></param>
        public void StoreStringProtected(string dataIdentifier, string data)
        {
            byte[] protectedData = ProtectedData.Protect(Encoding.UTF8.GetBytes(data), Encoding.UTF8.GetBytes("hockeyAppIsCool"));

            IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream writestream = new IsolatedStorageFileStream(dataIdentifier + ".crypt", FileMode.Create, FileAccess.Write, file);

            Stream writer = new StreamWriter(writestream).BaseStream;
            writer.Write(protectedData, 0, protectedData.Length);
            writer.Close();
            writestream.Close();
        }

        /// <summary>
        /// Retreive encrypted Data from the isolated storage
        /// </summary>
        /// <param name="dataIdentifier">identifier for the data to read</param>
        /// <returns>Decrypted string or null if no value has been stored for the given dataIdentifier</returns>
        public string RetrieveProtectedString(string dataIdentifier)
        {
            IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication();
            IsolatedStorageFileStream readstream = null;
            try
            {
                readstream = new IsolatedStorageFileStream(dataIdentifier + ".crypt", FileMode.Open, FileAccess.Read, file);
            }
            catch (IsolatedStorageException)
            {
                return null;
            }

            Stream reader = new StreamReader(readstream).BaseStream;
            byte[] protectedData = new byte[reader.Length];

            reader.Read(protectedData, 0, protectedData.Length);
            reader.Close();
            readstream.Close();

            byte[] dataBytes = ProtectedData.Unprotect(protectedData, Encoding.UTF8.GetBytes("hockeyAppIsCool"));
            return Encoding.UTF8.GetString(dataBytes, 0, dataBytes.Length);
        }

        private Uri _failRedirect;
        internal Uri FailRedirect
        {
            get { return _failRedirect; }
            set { _failRedirect = value; }
        }


        private Uri _successRedirect;
        internal Uri SuccessRedirect
        {
            get { return _successRedirect; }
            set { _successRedirect = value; }
        }

        private IAuthStatus authStatus = null;
        internal IAuthStatus CurrentAuthStatus
        {
            get { return authStatus; }
            set
            {
                authStatus = value;
                if (authStatus as AuthStatus != null && authStatus.IsIdentified)
                {
                    StoreStringProtected(Constants.AuthStatusKey, (authStatus as AuthStatus).SerializeToString());
                    IsolatedStorageSettings.ApplicationSettings.SetValue(Constants.AuthLastAuthorizedVersionKey, ManifestHelper.GetAppVersion());
                    IsolatedStorageSettings.ApplicationSettings.Save();
                }
            }
        }

        /// <summary>
        /// Manage user login against hockeyapp service, will show a login popup if no valid token is available, or data is missing (email, pw)
        /// HockeyClient needs to be configured before calling this method (normally done by configuring a crahshandler in the App() constructor)
        /// </summary>
        public void DoUserAuthentication(NavigationService navigationService, Uri successRedirect, Uri failRedirect, AuthenticationMode authMode = AuthenticationMode.Authorize,
            TokenValidationPolicy tokenValidationPolicy = TokenValidationPolicy.EveryLogin, AuthValidationMode authValidaationMode = AuthValidationMode.Graceful,
            string email = null, string appSecret = null)
        {
            if (AuthenticationMode.Identify.Equals(authMode) && String.IsNullOrEmpty(appSecret))
            {
                throw new ApplicationException(LocalizedStrings.LocalizedResources.Authentication_AppSecretMissing);
            }
            this.SuccessRedirect = successRedirect;
            this.FailRedirect = failRedirect;

            bool needsLogin = TokenValidationPolicy.EveryLogin.Equals(tokenValidationPolicy);

            if(!needsLogin && TokenValidationPolicy.OnNewVersion.Equals(tokenValidationPolicy)) {
                string lastAuthorizedVersion = IsolatedStorageSettings.ApplicationSettings.GetValue(Constants.AuthLastAuthorizedVersionKey) as String;
                needsLogin = (lastAuthorizedVersion == null) || (new Version(lastAuthorizedVersion) >= new Version(ManifestHelper.GetAppVersion()));
            }

            if (needsLogin)
            {
                navigationService.Navigate(new Uri("/HockeyApp;component/Views/LoginPage.xaml?authmode=" + HttpUtility.UrlEncode(authMode.ToString())
                                                                + "&appsecret=" + HttpUtility.UrlEncode(appSecret)
                                                                + "&email=" + HttpUtility.UrlEncode(email ?? "")
                                                                + "&validationmode=" + HttpUtility.UrlEncode(authValidaationMode.ToString() ?? ""), UriKind.Relative));
            }
        }

        /// <summary>
        /// Starts the Web-Authentication-Flow of hockeyapp.
        /// Prerequisite is to register your app for a URI-association as describe here:
        /// http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj206987%28v=vs.105%29.aspx#BKMK_URIassociations
        /// Make sure to add the following line to your registered protocols (inside the <Extension>-Tag):
        /// <Protocol Name="hockeyAuth" NavUriFragment="encodedLaunchUri=%s" TaskID="_default" />
        /// In your UriMapperBase- Implementation in MapURI() call UpdateManager.HandleLoginURI(uri)
        /// </summary>
        /*public void InitiateWebAuthentication()
        {
            var webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(HockeyClient.Instance.ApiBaseVersion3 + "identity/TODO", UriKind.Absolute);
            webBrowserTask.Show();
        }

        public void HandleLoginURI(Uri uri)
        {
            //TODO
        }*/

        /*
         public override Uri MapUri(Uri uri)
        {
            tempUri = System.Net.HttpUtility.UrlDecode(uri.ToString());

            // URI association launch for contoso.
            if (tempUri.Contains("contoso:ShowProducts?CategoryID="))
            {
                // Get the category ID (after "CategoryID=").
                int categoryIdIndex = tempUri.IndexOf("CategoryID=") + 11;
                string categoryId = tempUri.Substring(categoryIdIndex);

                // Map the show products request to ShowProducts.xaml
                return new Uri("/ShowProducts.xaml?CategoryID=" + categoryId, UriKind.Relative);
            }

            // Otherwise perform normal launch.
            return uri;
            }
        }
        */

    }
}
