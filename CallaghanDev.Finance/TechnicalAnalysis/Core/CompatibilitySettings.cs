﻿namespace CallaghanDev.Finance.TechnicalAnalysis
{
    public static partial class Core
    {
        /// <summary>
        /// Provides settings for handling the compatibility with other software.
        /// </summary>
        public static class CompatibilitySettings
        {
            
            /// Initializes the default compatibility mode.
            
            private static CompatibilityMode _compatibilityMode = CompatibilityMode.Default;

            /// <summary>
            /// Retrieves the current compatibility mode.
            /// </summary>
            /// <returns>A <see cref="CompatibilityMode"/> enum value representing the current compatibility mode.</returns>
            public static CompatibilityMode Get() => _compatibilityMode;

            /// <summary>
            /// Sets a new compatibility mode.
            /// </summary>
            /// <param name="mode">The <see cref="CompatibilityMode"/> to be set.</param>
            public static void Set(CompatibilityMode mode) => _compatibilityMode = mode;
        }
    }
}