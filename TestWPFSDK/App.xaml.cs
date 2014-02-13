using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace TestWPFSDK
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            HockeyApp.HockeyClientWPF.Instance.Configure(
                TestWPFSDK.Properties.Settings.Default.MyAppID,
                version.ToString(),
                null,
                null,
                (ex) =>
                {
                    return "";
                }, keepRunning:true);



            base.OnStartup(e);
        }
    }
}
