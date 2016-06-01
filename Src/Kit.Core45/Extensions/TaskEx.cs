using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp.Extensions
{
    /// <summary>
    /// TaskEx class to have a consistens interface with .net4.0
    /// </summary>
    internal class TaskEx
    {

        /// <summary>
        /// wrapper for Task.Run()
        /// </summary>
        /// <typeparam name="T">result type</typeparam>
        /// <param name="func">funct to run asyncronously</param>
        /// <returns></returns>
        public static async Task<T> Run<T>(Func<T> func)
        {
            return await Task.Run<T>(func);
        }
    }
}
