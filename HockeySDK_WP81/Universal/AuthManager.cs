using HockeyApp.Model;
using HockeyApp.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Resources.Core;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace HockeyApp
{

    /// <summary>
    /// Define how often an already obtained user-token should be validated with credentials
    /// </summary>
    public enum TokenValidationPolicy
    {
        /// <summary>
        /// The never
        /// </summary>
        Never,
        /// <summary>
        /// The every login
        /// </summary>
        EveryLogin,
        /// <summary>
        /// The on new version
        /// </summary>
        OnNewVersion
    }

    /// <summary>
    /// If validation of aan already obtained token is not possible (usually network related errors) deny acces (strict) or allow anyways
    /// </summary>
    public enum AuthValidationMode
    {
        /// <summary>
        /// The strict
        /// </summary>
        Strict,
        /// <summary>
        /// The graceful
        /// </summary>
        Graceful
    }

    /// <summary>
    /// Auhtorize with HockeyApp account email and password or only identify if email belongs to a valid HockeyApp account for this app.
    /// </summary>
    public enum AuthenticationMode
    {
        /// <summary>
        /// The authorize
        /// </summary>
        Authorize,
        /// <summary>
        /// The identify
        /// </summary>
        Identify
    }

    /// <summary>
    /// Class for authentication of users with hockeyapp credentials
    /// </summary>
    internal partial class AuthManager
    {
        private ILog logger = HockeyLogManager.GetLog(typeof(AuthManager));

        protected static AuthManager _instance = new AuthManager();
        internal Frame Frame { get { return Window.Current.Content as Frame; } }

        internal static AuthManager Current { get { return AuthManager._instance; } }

        /// <summary>
        /// Store data encrypted in the isolatedStorage
        /// </summary>
        /// <param name="dataIdentifier">identifier for the data to write</param>
        /// <param name="data">the data to store</param>
        internal async Task StoreStringProtectedAsync(string dataIdentifier, string data)
        {
            DataProtectionProvider Provider = new DataProtectionProvider("LOCAL=user");

            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8);

            // Encrypt the message.
            IBuffer buffProtected = await Provider.ProtectAsync(buffMsg);

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(dataIdentifier + ".crypt", CreationCollisionOption.ReplaceExisting);
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                await stream.WriteAsync(buffProtected.ToArray(),0,(int)buffProtected.Length);
            }
        }

        /// <summary>
        /// Retreive encrypted Data from the isolated storage
        /// </summary>
        /// <param name="dataIdentifier">identifier for the data to read</param>
        /// <returns>Decrypted string or null if no value has been stored for the given dataIdentifier</returns>
        internal async Task<string> RetrieveProtectedStringAsync(string dataIdentifier)
        {
            DataProtectionProvider Provider = new DataProtectionProvider("LOCAL=user");
            IBuffer buffer;

            try
            {
                buffer = await FileIO.ReadBufferAsync(await ApplicationData.Current.LocalFolder.GetFileAsync(dataIdentifier + ".crypt"));
            }
            catch (Exception)
            {
                return null;
            }
            
            IBuffer buffUnprotected = await Provider.UnprotectAsync(buffer);
            return CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffUnprotected);
        }

        private Type _successRedirectPageType;
        internal Type SuccessRedirectPageType
        {
            get { return _successRedirectPageType; }
            set { _successRedirectPageType = value; }
        }

        private Action _successAction;
        internal Action SuccessAction
        {
            get { return _successAction; }
            set { _successAction = value; }
        }

        protected IAuthStatus _authStatus = null;
        internal async Task UpdateAuthStatusAsync(IAuthStatus newStatus)
        {
            if (_authStatus as AuthStatus != null && _authStatus.IsIdentified)
            {
                await StoreStringProtectedAsync(ConstantsUniversal.AuthStatusKey, (_authStatus as AuthStatus).SerializeToString());
                ApplicationData.Current.LocalSettings.Values.SetValue(ConstantsUniversal.AuthLastAuthorizedVersionKey, HockeyClient.Current.AsInternal().VersionInfo);
            }
            _authStatus = newStatus;
        }

        /// <summary>
        /// Authenticate a user against the hockeyapp service. Will show a login popup if no valid token is available.
        /// HockeyClient needs to be configured before calling this method, which is automatically done internally when you configure a CrashHandler in the App() constructor.
        /// </summary>
        /// <param name="navigationService">the navigation service - needed to navigate to the login-page. Just use this.NavigationService from your view.</param>
        /// <param name="successRedirect">The URI for the page to redirect to after successfull login</param>
        /// <param name="authMode">[optional] (default: Authorize) AuthMode (Identify uses only the email-adresse, Authorize email and password)</param>
        /// <param name="tokenValidationPolicy">[optional] (default: EveryLogin) Policy for revalidation (every login or only after updates)</param>
        /// <param name="authValidationMode">[optional] (default: Graceful) Mode for token-Validation (Strict needs a network-connection on every login)</param>
        /// <param name="email">[optional] inititalize email of the user</param>
        /// <param name="appSecret">[optional] HockeyApp AppSecret of your App. only needed for AuthMode.Identify</param>
        internal void AuthenticateUser(AuthenticationMode authMode = AuthenticationMode.Authorize,
            TokenValidationPolicy tokenValidationPolicy = TokenValidationPolicy.EveryLogin, AuthValidationMode authValidationMode = AuthValidationMode.Graceful,
            string email = null, string appSecret = null)
        {
            if (AuthenticationMode.Identify.Equals(authMode) && String.IsNullOrEmpty(appSecret))
            {
                throw new Exception("Internal error: AppSecret must be provided when Identify function is used"); //ResourceManager.Current.MainResourceMap
            }
            bool needsLogin = TokenValidationPolicy.EveryLogin.Equals(tokenValidationPolicy);

            if(!needsLogin && TokenValidationPolicy.OnNewVersion.Equals(tokenValidationPolicy)) {
                string lastAuthorizedVersion = ApplicationData.Current.LocalSettings.Values.GetValue(ConstantsUniversal.AuthLastAuthorizedVersionKey) as String;
                needsLogin = (lastAuthorizedVersion == null) || (new Version(lastAuthorizedVersion) < new Version(AppxManifest.Current.Package.Identity.Version));
            }

            if (needsLogin)
            {
                this.ShowLoginScreen(authMode, appSecret, email, authValidationMode);
            }
            else
            {
                this.ExecuteSuccessRedirectOrAction();
            }
        }

        internal async void ExecuteSuccessRedirectOrAction()
        {
            if (this.SuccessAction != null)
            {
                await CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(this.SuccessAction));
            }
            if (this.SuccessRedirectPageType != null)
            {
                this.Frame.Navigate(this.SuccessRedirectPageType);
            }
        }
       
        /// <summary>
        /// Removes the user token from the phone store, so on the next call to AuthenticateUser the Loginscreen is shown.
        /// Effectively this serves as a logout from your app. In most cases you want to call AuthenticateUser() immediatley after RemoveUserToken
        /// </summary>
        internal async Task RemoveUserTokenAsync()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(ConstantsUniversal.AuthStatusKey + ".crypt");
                await file.DeleteAsync();
            }
            catch (FileNotFoundException)
            {
               //file doesn't exist
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
         
        }*/

    }
}
