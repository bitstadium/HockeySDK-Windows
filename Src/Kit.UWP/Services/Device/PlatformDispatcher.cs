namespace Microsoft.HockeyApp.Services.Device
{
    using System;
    using System.Threading.Tasks;

    using Extensibility.Implementation.Tracing;

    using global::Windows.ApplicationModel.Core;
    using global::Windows.UI.Core;

    internal static class PlatformDispatcher
    {
        public static Task RunAsync(Action action, CoreDispatcher dispatcher = null)
        {
            if (action == null)
            {
                return Task.FromResult((object)null);
            }

            try
            {
                dispatcher = dispatcher ?? CoreApplication.MainView.CoreWindow.Dispatcher;

                return dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal,
                    new DispatchedHandler(action)).AsTask()
                    .ContinueWith(
                    t =>
                    {
                        if (t.IsFaulted && t.Exception != null)
                        {
                            CoreEventSource.Log.LogVerbose(
                                "Got an exception from an action using the UI dispatcher: " +
                                t.Exception.ToString());
                        }
                    });
            }
            catch (Exception exception)
            {
                CoreEventSource.Log.LogVerbose("Got an exception from an action using the UI dispatcher: " + exception);
                return Task.FromResult((object)null);
            }
        }
    }
}
