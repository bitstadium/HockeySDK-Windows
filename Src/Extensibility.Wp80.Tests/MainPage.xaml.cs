namespace Microsoft.ApplicationInsights.Windows.Wp80.Tests
{
    using System.Threading;

    using Microsoft.Phone.Controls;
    using Microsoft.VisualStudio.TestPlatform.Core;
    using Microsoft.VisualStudio.TestPlatform.TestExecutor;

    using vstest_executionengine_platformbridge;

    /// <summary>
    /// The main page.
    /// </summary>
    public partial class MainPage : 
        PhoneApplicationPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            var wrapper = new TestExecutorServiceWrapper();
            new Thread(new ServiceMain((param0, param1) => wrapper.SendMessage((ContractName)param0, param1)).Run).Start();
        }
    }
}