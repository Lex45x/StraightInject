using System;

namespace StraightInject.Core.Debugging
{
    public static class DebugMode
    {
        private static readonly string DebugModeVariableName = "STRAIGHT_INJECT_ENABLE_DIAGNOSTIC";

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

        public static void Execute(Action action)
        {
            if (Enabled())
            {
                action();
            }
        }
    }
}