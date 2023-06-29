namespace Bb.Extensions
{
    public static class ConfigurationExtension
    {

        static ConfigurationExtension()
        {
            _stringComparer = StringComparer.CurrentCultureIgnoreCase;
        }

        public static bool Evaluate(this string valueName, string expected)
        {

            var valueText = Environment.GetEnvironmentVariable(valueName);
            Console.WriteLine($"Evaluate {valueName} = {valueText} expected to {expected}");

            if (!string.IsNullOrEmpty(valueText))
                return _stringComparer.Compare(valueText.ToLower().Trim(), expected) == 0;

            return false;

        }


        private static readonly StringComparer _stringComparer;

    }


}
