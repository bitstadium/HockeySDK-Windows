using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Microsoft.HockeyApp.Tools
{
    internal static class UIExtensions
    {
        internal static async Task ScrollToEnd(this ListBox @this)
        {
            if (@this.Items.Any())
            {
               await CoreWindow.GetForCurrentThread().Dispatcher.RunIdleAsync((o) => @this.ScrollIntoView(@this.Items.Last()));
            }
        }

    }
}
