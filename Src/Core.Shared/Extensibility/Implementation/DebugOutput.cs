// <copyright file="DebugOutput.cs" company="Microsoft">
// Copyright © Microsoft. All Rights Reserved.
// </copyright>

#define DEBUG

namespace Microsoft.HockeyApp.Extensibility.Implementation
{
    using System.Diagnostics;
    using Extensibility;

    internal class DebugOutput : IDebugOutput
    {
        public void WriteLine(string message)
        {
            Debug.WriteLine(message);
        }

        public bool IsLogging()
        {
            return true;
        }
    }
}
