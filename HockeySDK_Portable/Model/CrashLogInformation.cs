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
