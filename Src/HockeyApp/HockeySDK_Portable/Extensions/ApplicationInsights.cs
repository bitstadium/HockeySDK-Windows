namespace HockeyApp.Extensions
{
    using System;

    public class ApplicationInsights
    {
        public static void ConfigureApplicationInsights(string applicationInsightsInstrumentationKey)
        {
            try
            {

#if NET_4_5
                var assembly = System.Reflection.Assembly.Load(new System.Reflection.AssemblyName("Microsoft.ApplicationInsights"));
#else
                var assembly = System.Reflection.Assembly.Load("Microsoft.ApplicationInsights");
#endif
                if (assembly != null)
                {
                    var telemetryConfigurationType = assembly.GetType("Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration");
                    if (telemetryConfigurationType != null)
                    {
                        var activeProperty = telemetryConfigurationType.GetProperty("Active");
                        if (activeProperty != null)
                        {
                            var activeObj = activeProperty.GetValue(telemetryConfigurationType, null);
                            if (activeObj != null)
                            {
                                var activeType = activeObj.GetType();
                                if (activeType != null)
                                {
                                    var instrumentationKeyProperty = activeType.GetProperty("InstrumentationKey");
                                    if (instrumentationKeyProperty != null)
                                    {
                                        instrumentationKeyProperty.SetValue(activeObj, applicationInsightsInstrumentationKey, null);
                                    }
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception)
            {
            }
        }
    }
}