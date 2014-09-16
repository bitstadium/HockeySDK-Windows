using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace HockeyApp.Tools
{
    public class StringColonConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string res = (string)value;
            if(!String.IsNullOrEmpty(res) && !res.TrimEnd().EndsWith(":")){
                return res.TrimEnd() + ":";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
