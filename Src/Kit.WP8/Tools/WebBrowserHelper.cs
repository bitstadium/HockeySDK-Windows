namespace Microsoft.HockeyApp.Tools
{
    using System;
    using System.IO;
    using System.Text;
    using System.Windows;

    public static class WebBrowserHelper
    {
        private static string cssStyles;

        internal static string CssStyles
        {
            get
            {
                if (cssStyles == null) {
                    cssStyles = new StreamReader(Application.GetResourceStream(new Uri("/" + WebBrowserHelper.AssemblyNameWithoutExtension + ";component/Assets/wp8releasenotes.css", UriKind.Relative)).Stream).ReadToEnd();
                }

                return cssStyles;
            }
        }

        public static string WrapContent(string content)
        {
            string theme = ((Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible) ? "light" : "dark";
            var builder = new StringBuilder();
            builder.Append("<html><head><meta name='viewport' content='width=device-width;user-scalable=no' /><style type='text/css'>");
            builder.Append(WebBrowserHelper.CssStyles);
            builder.Append("</style></head><body class=" + theme + " >");
            builder.Append(content);
            builder.Append("</body></html>");
            return builder.ToString();
        }

        private static string assemblyNameWithoutExtension;

        public static string AssemblyNameWithoutExtension
        {
            get
            {
                if (string.IsNullOrEmpty(assemblyNameWithoutExtension))
                {
                    assemblyNameWithoutExtension = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
                }
                
                return assemblyNameWithoutExtension;
            }
        }
    }
}
