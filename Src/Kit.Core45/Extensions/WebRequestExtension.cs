using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.HockeyApp.Extensions
{
    /// <summary>
    /// static extension class
    /// </summary>
    internal static class WebRequestExtension
    {
        /// <summary>
        /// Set a http header on a Web-Request. Either by setting the property or by adding to the Headers dict.
        /// </summary>
        /// <param name="request">self</param>
        /// <param name="header">header key</param>
        /// <param name="value">header value</param>
        internal static void SetHeader(this WebRequest request, string header, string value)
        {
            // Retrieve the property through reflection.
            PropertyInfo PropertyInfo = request.GetType().GetProperty(header.Replace("-", string.Empty));
            // Check if the property is available.
            if (PropertyInfo != null)
            {
                try
                {
                    // Set the value of the header.
                    PropertyInfo.SetValue(request, value, null);
                }
                catch (Exception)
                {
                    request.Headers[header] = value;
                }
            }
            else
            {
                // Set the value of the header.
                request.Headers[header] = value;
            }
        }
    }
}
