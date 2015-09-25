@ECHO OFF
del *.nupkg
.\nuget.exe pack .\HockeySDK.WINRT80.nuspec
pause