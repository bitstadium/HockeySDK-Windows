namespace Microsoft.HockeyApp.TestFramework
{
    using System.Threading.Tasks;
    using DataContracts;
    using Extensibility;

    /// <summary>
    /// A stub of <see cref="IContextInitializer"/>.
    /// </summary>
    internal sealed class StubContextInitializer : IContextInitializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StubContextInitializer"/> class.
        /// </summary>
        public StubContextInitializer()
        {
            this.OnInitialize = context => { };
        }

        /// <summary>
        /// Gets or sets the callback invoked by the <see cref="Initialize"/> method.
        /// </summary>
        public TelemetryContextAction OnInitialize { get; set; }

#pragma warning disable 1998
        /// <summary>
        /// Implements the <see cref="IContextInitializer.Initialize"/> method by invoking the <see cref="OnInitialize"/> callback.
        /// </summary>
        public async Task Initialize(TelemetryContext context)
        {
            this.OnInitialize(context);
        }
#pragma warning restore 1998
    }
}
