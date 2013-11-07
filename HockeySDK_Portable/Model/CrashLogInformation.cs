using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HockeyApp.Model
{
    public struct CrashLogInformation
    {
        public string PackageName;
        public string Version;
        public string OperatingSystem;
        public string Manufacturer;
        public string Model;
        public string ProductID;
        public string WindowsPhone;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("Package: {0}\n", this.PackageName);
            builder.AppendFormat("Version: {0}\n", this.Version);
            builder.AppendFormat("OS: {0}\n", this.OperatingSystem);
            builder.AppendFormat("Windows Phone: {0}\n", this.WindowsPhone);
            builder.AppendFormat("Manufacturer: {0}\n", this.Manufacturer);
            builder.AppendFormat("Model: {0}\n", this.Model);
            builder.AppendFormat("Date: {0}\n", DateTime.UtcNow.ToString("o"));
            builder.AppendFormat("Product-ID: {0}\n", this.ProductID);
            return builder.ToString();
        }
        
    }
}
