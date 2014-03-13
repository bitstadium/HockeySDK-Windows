using HockeyApp.Model;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HockeyApp.Tools
{
    public static class UtilExtensions
    {

        public static bool IsValidEmail(this string str)
        {
            if (str == null) { return false; }
            const String pattern =
                @"^([0-9a-zA-Z]" + //Start with a digit or alphabate
                @"([\+\-_\.][0-9a-zA-Z]+)*" + // No continues or ending +-_. chars in email
                @")+" +
                @"@(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]*\.)+[a-zA-Z0-9]{2,17})$";

            return Regex.IsMatch(str, pattern);
        }

        public static bool IsEmpty(this string str)
        {
            return str == null || str.Trim().Length == 0;
        }

        public static bool SetValue(this IsolatedStorageSettings settings, string key, object value)
        {
            if (settings.Contains(key))
            {
                settings[key] = value;
                return false;
            }
            else
            {
                settings.Add(key, value);
                return true;
            }
        }

        public static bool RemoveValue(this IsolatedStorageSettings settings, string key)
        {
            if (settings.Contains(key))
            {
                settings.Remove(key);
                return true;
            }
            return false;
        }
        
        public static object GetValue(this IsolatedStorageSettings settings, string key)
        {
            if (settings.Contains(key))
            {
                return settings[key];
            }
            return null;
        }

    }
}
