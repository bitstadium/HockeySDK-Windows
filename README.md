HockeySDK for Windows
=========

The official Windows SDK for the HockeyApp service. Supports .NET Framework >= 4.0 as well as Windows Phone 8.

## Introduction

The HockeySDK for Windows allows users to send crash reports right from within the application.
When your app crashes, a file with basic information about the environment (device type, OS version, etc.), the reason and the stacktrace of the exception is created. 
The next time the user starts the app, he is asked to send the crash data to the developer. If he confirms the dialog, the crash log is sent to HockeyApp and then the file deleted from the device.

Furthermore it wraps the necessary api calls for sending feedback information to the platform.

## Features & Installation via Nuget
If you want to use the nuget packages make sure you have prerelease filter activated.

### Windows Phone 8
<pre>Nuget PM> Install-Package HockeySDK.WP</pre>

* Automatic crash reporting (store and beta apps)
* Feedback page (store and beta apps): Let your users send you feedback messages via HockeyApp 
* Automatic updates (only beta apps): Either all beta users must have developer unlocked phones or you need to either use an Enterprise Certificate to sign your beta apps. See http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj206943(v=vs.105).aspx

### Windows Phone 7.5
Deprecated but still available (no Nuget package available -> use the source)

* Crash reporting and Feedback (works for beta and store apps)

### WinRT (Windows Store Apps)
<pre>Nuget PM> Install-Package HockeySDK.WinRT</pre>

* Automatic crash reporting
* Sending feedback to the developers

### WPF
<pre>Nuget PM> Install-Package HockeySDK.WPF</pre>

* Automatic crash reporting
* Sending feedback to the developers

### Portable lib (core component) 
<pre>Nuget PM> Install-Package HockeySDK.Core</pre>
A basic API wrapper for the HockeyApp API that supports

* Handling of crashes
* Submitting of crashes to the HockeyApp server
* Checking for newest app version  
* Sending feedback to the developers

## Requirements

Before you integrate HockeySDK into your own app, you should add the app to HockeyApp if you haven't already. Read http://support.hockeyapp.net/kb/about-general-faq/how-to-create-a-new-app on how to do this.

## Getting started with WP8

### Configure the CrashHandler

1. Open your App.xaml.cs
* Search for the "App" constructor method and add the following line after InitializePhoneApplication() :<pre>CrashHandler.Instance.Configure(this, YOUR_APP_ID, RootFrame);</pre>
* Replace YOUR_APP_ID with the App ID of your app on HockeyApp. See http://support.hockeyapp.net/kb/about-general-faq/how-to-find-the-app-id
* Open the root page of your app (or the page in which you want to check for new crashes), for example MainPage.xaml.cs.
* Search the constructor of the class, e.g. "MainPage", and add the following line at the bottom:<pre>HockeyApp.CrashHandler.Instance.HandleCrashes();</pre>If you want to send crashes without user interaction, use this line instead:<pre>HockeyApp.CrashHandler.Instance.HandleCrashes(true);</pre>
There is also an Async Version of that method if you use async in your framework. 

Now every time when an unhandled exception occurs, the exception stacktrace is logged. At the next start, you should be asked to send the crash data (or crashes are sent automatically if you configured this).

### Link the feedback page
Use the following line in an apropriate place (like a click-handler for a Feedback-Button)
<pre>FeedbackManager.Instance.NavigateToFeedbackUI(NavigationService);</pre>
if you don't have access to NavigationService in your ViewModels (like when using Caliburn.Micro) you should use the following URI with your navigation framework <pre>new Uri("/HockeyApp;component/Views/FeedbackPage.xaml", UriKind.Relative)</pre>

### Use automatic updates
Use the next line right after the call to HandleCrashes() to check for updates and present them to the user automatically
<pre>UpdateManager.RunUpdateCheck(Constants.HockeyAppAppId);</pre>

## Getting started with Windows 8.1 (Windows RT)
1. Open your App.xaml.cs and add the following to the OnLaunched method before the `Window.Current.Activate();`.
2. Before calling any methods you have to configure the sdk via `HockeyApp.HockeyClientWinRT.Instance.Configure(YOUR_APP_ID, YOUR_APP_VERSION)`
3. You can also pass in a variety of other parameters such as: `HockeyApp.HockeyClientWinRT.Instance.Configure(YOUR_APP_ID, YOUR_APP_VERSION, USERNAME, CONTACT_INFO, DESCRIPTION_HANDLER, API_BASE)`
4. The parameters provided to the configure method are:
* YOUR_APP_ID and YOUR_APP_VERSION are not optional
* USERNAME and CONTACT_INFO is for optional submitting an logged on username and contactinfo
* DESCRIPTION_HANDLER is a lambda for submitting additional information like an event log
* The default API_BASE is https://rink.hockeyapp.net/api/2/ and can be overwritten
3. After the sdk is configured, all non handled exceptions are caught by the sdk. 
4. Crashdata is send using `HockeyApp.HockeyClientWinRT.Instance.SendCrashesNowAsync();`
* The user can be asked, if the crashed should be sent using `HockeyApp.HockeyClientWinRT.Instance.CrashesAvailable` and `HockeyApp.HockeyClientWinRT.Instance.CrashesAvailableCount`
* Crashed can be deleted using `HockeyApp.HockeyClientWinRT.Instance.DeleteAllCrashes()`

### Use Feedback
In the SDK there are no UI components for the Feedback-Informations. The SDK offers methods to load Feedbacks from the server by using feedback-tokens. Feedback-tokens
must stored in the client application.
Creating a new Feedback:

1. `HockeyApp.HockeyClientWinRT.Instance.CreateFeedbackThread()` creates an new IFeedbackThread
2. `feedbackThread.PostFeedbackMessageAsync(MESSAGE, EMAIL, SUBJECT, USERNAME);` submits a new feedback message on the selected feedback-thread.
3. The FeedbackThread is created on the server with submitting the first feedback-message (keep that in mind when storing the feedback-token information)

## Getting started with WPF

### Configure the CrashHandler
1. Open your App.xaml.cs and override the OnStartup-Method. If your are using frameworks like Caliburn.Micro use the caliburn-bootstrapper.
2. Before calling any methods you have to configure the sdk via `HockeyApp.HockeyClientWPF.Instance.Configure(YOUR_APP_ID, YOUR_APP_VERSION, USERNAME, CONTACT_INFO, DESCRIPTION_HANDLER, API_BASE)`
* YOUR_APP_ID and YOUR_APP_VERSION are not optional
* USERNAME and CONTACT_INFO is for optional submitting an logged on username and contactinfo
* DESCRIPTION_HANDLER is a lambda for submitting additional information like an event log
* The default API_BASE is https://rink.hockeyapp.net/api/2/ and can be overwritten
3. After the sdk is configured, all non handled exceptions are caught by the sdk. Exception information are written to the filesystem (%APPDATA%)
4. Crashdata is send using <pre>HockeyApp.HockeyClientWPF.Instance.SendCrashesNowAsync();</pre>
* The user can be asked, if the crashed should be sent using `HockeyApp.HockeyClientWPF.Instance.CrashesAvailable` and `HockeyApp.HockeyClientWPF.Instance.CrashesAvailableCount`
* Crashed can be deleted using `HockeyApp.HockeyClientWPF.Instance.DeleteAllCrashes()`

### Use Feedback
In the WPF SDK there are no UI components for the Feedback-Informations. The SDK offers methods to load Feedbacks from the server by using feedback-tokens. Feedback-tokens
must stored in the client application.
Creating a new Feedback:

1. `HockeyApp.HockeyClientWPF.Instance.CreateFeedbackThread()` creates an new IFeedbackThread
2. `feedbackThread.PostFeedbackMessageAsync(MESSAGE, EMAIL, SUBJECT, USERNAME);` submits a new feedback message on the selected feedback-thread.
3. The FeedbackThread is created on the server with submitting the first feedback-message (keep that in mind when storing the feedback-token information)

## Using the PCL
Without using the WPF or WP-SDKs you can use the portable library directly - e.g. when implementing an own SDK.

1. First you have to configure the portable using `HockeyClient.Configure(YOUR_APP_ID,YOUR_VERSION,API_BASE,USERNAME,USER_CONTACT_INFO,DESCRIPTION)` or
`HockeyClient.ConfigureInternal(YOUR_APP_ID,YOUR_VERSION,API_BASE,USERNAME,USER_CONTACT_INFO, USER_AGENT, SDK_NAME, SDK_VERSION, DESCRIPTION);`
2. Exception have to be caught in your own SDK. Using `HockeyClient.Instance.CreateCrashData(EXCEPTION,CRASHLOGINFO);` 
you can serialize the CrashData using `ICrashData.Serialize(Stream outputstream);`
3. When you want to post all crashes to the server, `HockeyClient.Deserialize(Stream inputStream)` offers you a deserializing functionality. 
4. With the deserialized ICrashData you can post the crash information using `ICrashData.Task SendDataAsync();`
 
## Console app
There is no special SDK for console apps, because feedback information is not used in console apps. The crashhandler has to be implemented yourself. Please find an example in Hoch.exe (HockeyUploaderConsole).



## Support

If you have any questions, problems or suggestions, please contact us at [support@hockeyapp.net](mailto:support@hockeyapp.net).

##Release notes
All release notes can be found in the project directories

* [Hockey-SDK Portable](./HockeySDK_Portable/)
* [Hockey-SDK Portable .Net4.5](./HockeySDK_Portable45/)
* [HockeySDK WP8](./HockeySDK_WP8/)
* [HockeySDK WP7.5](./HockeySDK_WP75/)
* [HockeySDK WinRT](./HockeySDK_WinRT/)
* [HockeySDK WPF](./HockeySDK_WPF/)
* [HockeySDK WPF .Net4.5](./HockeySDK_WPF45/)
