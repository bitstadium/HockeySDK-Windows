namespace Microsoft.HockeyApp.TestFramework
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Extensibility.Implementation;

    internal class StubClock : IClock
    {
        public DateTimeOffset Time { get; set; }

        DateTimeOffset IClock.Time
        {
            get 
            { 
                if (this.Time == default(DateTimeOffset))
                {
                    return DateTimeOffset.Now;
                }

                return this.Time; 
            }
        }
    }
}
