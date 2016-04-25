// <copyright file="PlatformSingleton.cs" company="Microsoft">
// Copyright © Microsoft. All Rights Reserved.
// </copyright>

namespace Microsoft.HockeyApp.Extensibility.Implementation.Platform
{
    using System;
    using System.Linq;
    using System.Reflection;

    ////using Platform;

    /// <summary>
    /// Provides access to the <see cref="Current"/> platform.
    /// </summary>
    internal static class PlatformSingleton
    {
        private static IPlatform current;

        /// <summary>
        /// Gets or sets the current <see cref="IPlatform"/> implementation.
        /// </summary>
        public static IPlatform Current 
        {
            get
            {
                return current ?? (current = Microsoft.HockeyApp.ServiceLocator.GetService<IPlatform>());
            }

            set
            {
                current = value;
            } 
        }
    }
}
