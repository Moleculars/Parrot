using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;

namespace Bb.Extensions
{
    public static class ConfigurationExtension
    {

        static ConfigurationExtension()
        {
            _stringComparer = StringComparer.CurrentCultureIgnoreCase;
        }


        public static IEnumerable<string> TryGetAddresses(this IHost self)
        {

            List<string> addresses = new List<string>();

            try
            {

                IServer server = self.Services.GetService<IServer>();

                if (server != null)
                {
                    IServerAddressesFeature addressFeature = server.Features.Get<IServerAddressesFeature>();
                    if (addressFeature != null)
                        foreach (var address in addressFeature.Addresses)
                            addresses.Add(address);

                }

            }
            catch (Exception)
            {


            }

            return addresses;

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
