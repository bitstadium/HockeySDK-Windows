using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.HockeyApp.Model
{
    /// <summary>
    /// representing additional meta info to a crashlog
    /// </summary>
    public struct CrashLogInformation
    {
        /// <summary>
        /// name of app package
        /// </summary>
        public string PackageName;
        /// <summary>
        /// version of app
        /// </summary>
        public string Version;
        /// <summary>
        /// os
        /// </summary>
        public string OperatingSystem;
        /// <summary>
        /// device manufacturer
        /// </summary>
        public string Manufacturer;
        /// <summary>
        /// device model
        /// </summary>
        public string Model;
        /// <summary>
        /// product id of app
        /// </summary>
        public string ProductID;
        /// <summary>
        /// windows phone version
        /// </summary>
        public string WindowsPhone;
        /// <summary>
        /// windows version
        /// </summary>
        public string Windows;

        /// <summary>
        /// concatenate info to key-value string 
        /// </summary>
        /// <returns>data as string</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Text.StringBuilder.AppendFormat(System.String,System.Object[])")]
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (!String.IsNullOrWhiteSpace(this.PackageName)){
                builder.AppendFormat("Package: {0}\n", this.PackageName);
            }
            if (!String.IsNullOrWhiteSpace(this.Version))
            {
                builder.AppendFormat("Version: {0}\n", this.Version);
            }
            if (!String.IsNullOrWhiteSpace(this.OperatingSystem))
            {
                builder.AppendFormat("OS: {0}\n", this.OperatingSystem);
            }
            if (!String.IsNullOrWhiteSpace(this.WindowsPhone))
            {
                builder.AppendFormat("Windows Phone: {0}\n", this.WindowsPhone);
            }
            if (!String.IsNullOrWhiteSpace(this.Windows))
            {
                builder.AppendFormat("Windows: {0}\n", this.Windows);
            }
            if (!String.IsNullOrWhiteSpace(this.Manufacturer))
            {
                builder.AppendFormat("Manufacturer: {0}\n", this.Manufacturer);
            }
            if (!String.IsNullOrWhiteSpace(this.Model))
            {
                builder.AppendFormat("Model: {0}\n", this.Model);
            }
            
            builder.AppendFormat("Date: {0}\n", DateTime.UtcNow.ToString("o"));

            if (!String.IsNullOrWhiteSpace(this.ProductID))
            {
                builder.AppendFormat("Product-ID: {0}\n", this.ProductID);
            }
            return builder.ToString();
        }
        
    }
}
