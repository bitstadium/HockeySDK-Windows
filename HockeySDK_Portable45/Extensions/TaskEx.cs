using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HockeyApp.Extensions
{
    public class TaskEx
    {

        public static async Task<T> Run<T>(Func<T> func)
        {
            return await Task.Run<T>(func);
        }

    }
}
