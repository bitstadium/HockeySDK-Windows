namespace Microsoft.HockeyApp.Extensibility.Implementation.Tracing
{
    using Services;
    using System.Diagnostics;

    internal sealed class CoreEventSource
    {
        internal bool Enabled { get; set; }

        private CoreEventSource()
        {
        }

        public static CoreEventSource Log
        {
            get { return Nested.Instance; }
        }

        public void LogVerbose(string msg)
        {
            if (Enabled)
            {
                WriteLine("HockeySDK Info: " + msg);
            }
        }
        
        public void LogError(string msg)
        {
            if (Enabled)
            {
                WriteLine("HockeySDK Error: " + msg);
            }
        }
       
        private void WriteLine(string message)
        {
            Debug.WriteLine(message);
        }        

        /// <summary>
        /// We are using Singleton with nested class in order to have lazy singleton initialization to prevent memory issue described in a bug #566011.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1409:RemoveUnnecessaryCode", Justification = "Reviewed.")]
        private class Nested
        {
            internal static readonly CoreEventSource Instance = new CoreEventSource();

            /// <summary>
            /// Initializes static members of the <see cref="Nested" /> class. 
            /// Explicit static constructor to tell C# compiler not to mark type.
            /// </summary>
            static Nested()
            {
            }
        }
    }
}
