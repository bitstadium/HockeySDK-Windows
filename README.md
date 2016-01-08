Unit tests:
![Build Status](https://mseng.visualstudio.com/DefaultCollection/_apis/public/build/definitions/96a62c4a-58c2-4dbb-94b6-5979ebc7f2af/2471/badge)

Signed Build:
![Build Status](https://mseng.visualstudio.com/DefaultCollection/_apis/public/build/definitions/96a62c4a-58c2-4dbb-94b6-5979ebc7f2af/2560/badge)

HockeySDK for Windows
=========

The official Windows SDK for the HockeyApp service. Supports Universal Windows 10 Applications.

## Introduction

The HockeySDK for Windows allows users to send crash reports right from within the application.
When your app crashes, a file with basic information about the environment (device type, OS version, etc.), the reason and the stacktrace of the exception is created. 
The next time the user starts the app, the crash log is sent to HockeyApp and then the file deleted from the device.


## Features
### Universal Windows 10 Applications
* Automatic crash reporting

### Windows Phone 8.1, WinRT, WPF
Deprecated but still available (no Nuget package available -> use the source)


## Onboarding instructions 
1. Add nuget package: <pre>Install-Package HockeySDK.UWP</pre>
2. Open your App.xaml.cs file and add the using directive for HockeyApp at the top: <pre>using HockeyApp</pre>
3. In the App constructor add the following line: <pre>HockeyClient.Current.Configure(“Your_App_ID”)</pre>
4. Enable Internet(Client) Capability in package manifest

## Contributing
### Build process (available for Microsoft Employees only)
1. Run https://mseng.visualstudio.com/DefaultCollection/AppInsights/_build#favDefinitionId=2471&_a=completed in order to perform an automated build with unit tests.
2. Run https://mseng.visualstudio.com/DefaultCollection/AppInsights/_build#favDefinitionId=2560&_a=completed in order to create signed binaries.
3. Signed build output: \\vscsstor\CIDrops\HockeyApp\HockeySDK-Windows-Private\strongly-signed\

### Contributor License
You must sign a [Contributor License Agreement](https://cla.microsoft.com/) before submitting your pull request. To complete the Contributor License Agreement (CLA), you will need to submit a request via the [form](https://cla.microsoft.com/) and then electronically sign the CLA when you receive the email containing the link to the document. You need to sign the CLA only once to cover submission to any Microsoft OSS project. 

## Support

If you have any questions, problems or suggestions, please contact us at [support@hockeyapp.net](mailto:support@hockeyapp.net).