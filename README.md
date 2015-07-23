HockeySDK for Windows
=========

The official Windows SDK for the HockeyApp service. Supports .NET Framework >= 4.0 as well as Windows Phone and Windows Store Apps.

## Introduction

The HockeySDK for Windows allows users to send crash reports right from within the application.
When your app crashes, a file with basic information about the environment (device type, OS version, etc.), the reason and the stacktrace of the exception is created. 
The next time the user starts the app, he is asked to send the crash data to the developer. If he confirms the dialog, the crash log is sent to HockeyApp and then the file deleted from the device.

Furthermore it wraps the necessary api calls for sending feedback information to the platform.


## Features & Installation via Nuget
### Windows Phone 8
<pre>Nuget PM> Install-Package HockeySDK.WP</pre>

* Automatic crash reporting (store and beta apps)
* Feedback page (store and beta apps): Let your users send you feedback messages via HockeyApp 
* Automatic updates (only beta apps): Either all beta users must have developer unlocked phones or you need to either use an Enterprise Certificate to sign your beta apps. See http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj206943(v=vs.105).aspx
* Authorization using HockeyApp logins

### Windows Phone 7.5
Deprecated but still available (no Nuget package available -> use the source)

* Crash reporting and Feedback (works for beta and store apps)

### WinRT (Windows 8.1 Store Apps and Windows Phone 8.1 Store Apps)
<pre>Nuget PM> Install-Package HockeySDK.WinRT</pre>

* Automatic crash reporting
* Automatic updates (only for Windows Phone 8.1)
* Sending feedback to the developers
* Authorization using HockeyApp logins

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

Before you integrate HockeySDK into your own app, you should add the app to HockeyApp if you haven't already. Read [this faq entry](http://support.hockeyapp.net/kb/about-general-faq/how-to-create-a-new-app) on how to do this.

## Getting started 
Guides to get you started with HockeySDK can be found in the [HockeyApp Knowledge Base](http://support.hockeyapp.net/kb)

There are also [Demo Apps](https://github.com/bitstadium/HockeySDK-WindowsDemo) on GitHub.

###[HockeyApp for Windows Store Apps and Windows Phone Store Apps](http://support.hockeyapp.net/kb/client-integration-android-other-platforms/hockeyapp-for-windows-store-apps-and-windows-phone-store-apps)

###[HockeyApp for Windows Phone Silverlight Apps (8.0 and 8.1)](http://support.hockeyapp.net/kb/client-integration-android-other-platforms/hockeyapp-for-windows-phone-silverlight-apps-80-and-81)

###[HockeyApp for Windows WPF Apps](http://support.hockeyapp.net/kb/client-integration-android-other-platforms/hockeyapp-for-windows-wpf-apps)

#### Feedback in WPF
In the WPF SDK there are no UI components for the Feedback-Informations. The SDK offers methods to load Feedbacks from the server by using feedback-tokens. Feedback-tokens must be stored in the client application.
Creating a new Feedback:

1. `HockeyClient.Current.CreateFeedbackThread()` creates an new IFeedbackThread
2. `feedbackThread.PostFeedbackMessageAsync(MESSAGE, EMAIL, SUBJECT, USERNAME);` submits a new feedback message on the selected feedback-thread.
3. The FeedbackThread is created on the server with submitting the first feedback-message (keep that in mind when storing the feedback-token information)

## Using the PCL
You can use the portable library directly - e.g. when implementing your own custom-platform SDK. To get some hints look at the classes implementing 
[`IHockeyPlatformHelper`](https://github.com/bitstadium/HockeySDK-Windows/blob/develop/HockeySDK_Portable/IHockeyPlatformHelper.cs) and the extension classes e.g. [`HockeyClientWPFExtensions`](https://github.com/bitstadium/HockeySDK-Windows/blob/develop/HockeySDK_WPF/HockeyClientWPFExtensions.cs).

## Console app
There is no special SDK for console apps but you can find an example of crashhandling in Hoch.exe (Project [HockeyAppForWindowsConsole](https://github.com/bitstadium/HockeyApp-for-Windows/tree/develop/HockeyAppForWindowsConsole)).

# Support

If you have any questions, problems or suggestions, please contact us at [support@hockeyapp.net](mailto:support@hockeyapp.net).

## Contributor License

You must sign a [Contributor License Agreement](https://cla.microsoft.com/) before submitting your pull request. To complete the Contributor License Agreement (CLA), you will need to submit a request via the [form](https://cla.microsoft.com/) and then electronically sign the CLA when you receive the email containing the link to the document. You need to sign the CLA only once to cover submission to any Microsoft OSS project. 

## Release notes
All release notes can be found in the project directories

* [Hockey-SDK Portable](./HockeySDK_Portable/)
* [Hockey-SDK Portable .Net4.5](./HockeySDK_Portable45/)
* [HockeySDK WP8](./HockeySDK_WP8/)
* [HockeySDK WP7.5](./HockeySDK_WP75/)
* [HockeySDK WinRT](./HockeySDK_WinRT/)
* [HockeySDK WPF](./HockeySDK_WPF/)
* [HockeySDK WPF .Net4.5](./HockeySDK_WPF45/)
