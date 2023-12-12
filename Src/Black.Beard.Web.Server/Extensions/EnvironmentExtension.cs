using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;

namespace Bb.Extensions
{

    /// <summary>
    /// Environment extensions
    /// </summary>
    public static class EnvironmentExtension
    {

        static EnvironmentExtension()
        {
            _stringComparer = StringComparer.CurrentCultureIgnoreCase;
        }

        public static bool EnvironmentVariableExists(this string valueName)
        {
            var valueText = Environment.GetEnvironmentVariable(valueName);
            return !string.IsNullOrEmpty(valueText);
        }

        public static bool EnvironmentVariableIsTrue(this string valueName)
        {
            return valueName.EnvironmentVariableEvaluate("true");
        }

        public static bool EnvironmentVariableEvaluate(this string valueName, string expected)
        {

            var valueText = Environment.GetEnvironmentVariable(valueName);
            Console.WriteLine($"Evaluate {valueName} = {valueText} expected to {expected}");

            if (!string.IsNullOrEmpty(valueText))
                return _stringComparer.Compare(valueText.ToLower().Trim(), expected.ToLower()) == 0;

            return false;

        }


        private static readonly StringComparer _stringComparer;

    }


}
