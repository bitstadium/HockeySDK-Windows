using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.UI.Xaml;

namespace HockeyApp.Tools
{
    public class ScreenResolution
    {

        public static double HeightWithoutScale { get { return Window.Current.Bounds.Height; } }
        public static double WidthWithoutScale { get { return Window.Current.Bounds.Width; } }

        public static double Height { get { return Window.Current.Bounds.Height * DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel; } }
        public static double Width { get { return Window.Current.Bounds.Width * DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel; } }

    }
}
