using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyCopyright("Copyright © Microsoft. All Rights Reserved.")]

[assembly: ComVisible(false)]

[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Core.Net35.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Core.Net40.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Core.Net46.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Core.Wrt81.Win81.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Core.Wp80.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Core.Wrt81.Wpa81.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Kit.Tests" + AssemblyInfo.PublicKey)]

[assembly: InternalsVisibleTo("Microsoft.HockeyApp.TestFramework" + AssemblyInfo.PublicKey)]

[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Extensibility" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Windows.NuGet.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Extensibility.Wp80.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Extensibility.Wrt81.Wpa81.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Extensibility.Wrt81.Win81.Tests" + AssemblyInfo.PublicKey)]

[assembly: InternalsVisibleTo("Microsoft.HockeyApp.WindowsChannel" + AssemblyInfo.PublicKey)]

[assembly: InternalsVisibleTo("Microsoft.HockeyApp.PersistenceChannel" + AssemblyInfo.PublicKey)]

[assembly: InternalsVisibleTo("Microsoft.HockeyApp.PersistenceChannel.Net40.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.PersistenceChannel.Net45.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.PersistenceChannel.Wrt81.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.PersistenceChannel.Wp80.Tests" + AssemblyInfo.PublicKey)]
[assembly: InternalsVisibleTo("Microsoft.HockeyApp.Kit" + AssemblyInfo.PublicKey)]

// This is for RDD
#if PUBLIC_RELEASE
[assembly: InternalsVisibleTo("Microsoft.EnterpriseManagement.OperationsManager.Apm.RuntimeDiscovery, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
#endif

internal static class AssemblyInfo
{
#if PUBLIC_RELEASE
    // Public key; assemblies are delay signed.
    public const string PublicKey = ", PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9";
    public const string MoqPublicKey = ", PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7";
#else
    // Internal key; assemblies are fully signed.
    public const string PublicKey = "";
    public const string MoqPublicKey = "";
#endif
}