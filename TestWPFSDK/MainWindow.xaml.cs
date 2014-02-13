using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestWPFSDK
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.ThrowDiv0Exception();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Process.Start(HockeyApp.HockeyClientWPF.Instance.GetPathToHockeyCrashes);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ThrowDiv0Exception();
            }
            catch (Exception ex)
            {
                throw new Exception("Outer Message", ex);
            }
        }

        private async void Button_Click_4(object sender, RoutedEventArgs e)
        {
            await HockeyApp.HockeyClientWPF.Instance.SendCrashesNowAsync();
        }


        private void ThrowDiv0Exception(){
            int i = 1;
            i--;
            int y = i / i;
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            List<Exception> exceptions = new List<Exception>();
            for (int i = 1; i < 10; i++)
            {
                try
                {
                    this.ThrowDiv0Exception();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException("Some Exceptions were thrown..", exceptions);
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            System.GC.Collect();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            try
            {
                Button_Click_3(sender, e);
            }
            catch (Exception ex)
            {
                throw new Exception("Most Outer Message", ex);
            }
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            try
            {
                Button_Click_7(sender, e);
            }
            catch (Exception ex)
            {
                throw new Exception("Most Most Outer Message", ex);
            }
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            List<Exception> exceptions = new List<Exception>();
            for (int i = 1; i < 10; i++)
            {
                try
                {
                    Button_Click_7(sender, e);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException("Some Exceptions were thrown..", exceptions);
        }
    }
}
