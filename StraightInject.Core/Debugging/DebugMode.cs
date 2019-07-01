using System;

namespace StraightInject.Core.Debugging
{
    /// <summary>
    /// Static class that encapsulate all debug/trace-related operations 
    /// </summary>
    public static class DebugMode
    {
        private static readonly string DebugModeVariableName = "STRAIGHT_INJECT_ENABLE_DIAGNOSTIC";

        /// <summary>
        /// To enable debug mode you must provide an STRAIGHT_INJECT_ENABLE_DIAGNOSTIC environment variable with 'True' value
        /// </summary>
        /// <returns></returns>
        public static bool Enabled()
        {
            var environmentVariable = Environment.GetEnvironmentVariable(DebugModeVariableName);
            if (environmentVariable == null)
            {
                return false;
            }

            var enabled = bool.Parse(environmentVariable);
            return enabled;
        }

        /// <summary>
        /// Execute an action if DebugMode is enabled
        /// </summary>
        /// <param name="action"></param>
        public static void Execute(Action action)
        {
            if (Enabled())
            {
                action();
            }
        }
    }
}