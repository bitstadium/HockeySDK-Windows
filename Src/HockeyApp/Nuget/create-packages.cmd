@ECHO OFF
del *.nupkg
.\nuget.exe pack .\HockeySDK.Core.nuspec
.\nuget.exe pack .\HockeySDK.WPF.nuspec
.\nuget.exe pack .\HockeySDK.WINFORMS.nuspec
.\nuget.exe pack .\HockeySDK.WP.nuspec
.\nuget.exe pack .\HockeySDK.WINRT.nuspec
pause