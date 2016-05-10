// <copyright file="PlatformSingleton.cs" company="Microsoft">
// Copyright © Microsoft. All Rights Reserved.
// </copyright>

namespace Microsoft.HockeyApp.Extensibility.Implementation.Platform
{
    using Services;

    /// <summary>
    /// Provides access to the <see cref="Current"/> platform.
    /// </summary>
    internal static class PlatformSingleton
    {
        private static IPlatformService current;

        /// <summary>
        /// Gets or sets the current <see cref="IPlatformService"/> implementation.
        /// </summary>
        public static IPlatformService Current 
        {
            get
            {
                return current ?? (current = Microsoft.HockeyApp.ServiceLocator.GetService<IPlatformService>());
            }

            set
            {
                current = value;
            } 
        }
    }
}
