![Test Status](https://mseng.visualstudio.com/DefaultCollection/_apis/public/build/definitions/96a62c4a-58c2-4dbb-94b6-5979ebc7f2af/2471/badge)
![Build Status](https://mseng.visualstudio.com/DefaultCollection/_apis/public/build/definitions/96a62c4a-58c2-4dbb-94b6-5979ebc7f2af/2560/badge)

HockeySDK for Windows
=========

The official Windows SDK for the http://www.hockeyapp.com.

## Feature Support
1. Crash Reporting
2. Beta Distribution
3. Feedback
4. Authentication
5. Custom Events Telemetry
6. Usage Metrics Reporting

## Platform Support
1. Windows 10 (Universal Windows Platform (UWP))
2. Windows 8.1 (WinRT)
3. Windows Phone 8.1 (Silverlight)
4. Windows Phone 8.0
5. WPF 4.5

## Onboarding Instructions 
1. Add nuget package: 

| Platform | Nuget Package Manager Console Command |
| --- | --- |
| Windows 10 (UWP) | Install-Package HockeySDK.UWP -Pre |
| Windows 8.1 (WinRT) | Install-Package HockeySDK.WINRT -Pre |
| Windows Phone 8.1 (Silverlight), Windows Phone 8.0 (Silverlight) | Install-Package HockeySDK.WP -Pre |
| WPF 4.5 | Install-Package HockeySDK.WPF -Pre |

2. In App.xaml.cs file add the following line: <pre>using Microsoft.HockeyApp;</pre> in usage declaration section.
3. In App.xaml.cs file add the following line: <pre>Microsoft.HockeyApp.HockeyClient.Current.Configure(“Your_App_ID”);</pre> in App class constructor.
4. If you are using HockeySDK.WinRT or HockeySDK.WP, in App.xaml.cs add the following line at the end of the <i>async void Application_Launching(object sender, LaunchingEventArgs e)</i>
   <pre>await HockeyClient.Current.SendCrashesAsync(/* sendWithoutAsking: true */);</pre>
5. Enable Internet(Client) Capability in package manifest.

## Demo Applications
https://github.com/bitstadium/HockeySDK-WindowsDemo

## Documentation
https://support.hockeyapp.net/kb/client-integration-windows-and-windows-phone
 

## Contributor License
You must sign a [Contributor License Agreement](https://cla.microsoft.com/) before submitting your pull request. To complete the Contributor License Agreement (CLA), you will need to submit a request via the [form](https://cla.microsoft.com/) and then electronically sign the CLA when you receive the email containing the link to the document. You need to sign the CLA only once to cover submission to any Microsoft OSS project. 

## Support
If you have any questions, problems or suggestions, please contact us at [support@hockeyapp.net](mailto:support@hockeyapp.net).