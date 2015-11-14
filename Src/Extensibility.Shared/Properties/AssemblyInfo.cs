using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if !UWP // ToDo: We need to remove this file once we remove Extensibility assemblies.
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyCopyright("Copyright © Microsoft. All Rights Reserved.")]
[assembly: ComVisible(false)]
#endif

[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Extensibility.Wrt81.Wpa81.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Extensibility.Wp80.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Extensibility.Wrt81.Win81.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Kit" + AssemblyInfo.PublicKey)]
