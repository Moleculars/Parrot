using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using System.Net;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Core.Infrastructure;
using Flurl;

namespace Bb.Services
{

    /// <summary>
    /// Addresses & ip helper
    /// </summary>
    public static class AddressesHelper
    {


        /// <summary>
        /// Gets addresses accepted by the server.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <returns></returns>
        public static List<Uri>? GetServerAcceptedAddresses(this IHost self)
        {

            List<Uri> addresses = new List<Uri>();

            try
            {

                IServer server = self.Services.GetService<IServer>();

                if (server != null)
                    return server.GetServerAcceptedAddresses();

            }
            catch (Exception) 
            {
                return null;
            }

            return addresses;

        }

        /// <summary>
        /// Gets addresses accepted by the server.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <returns></returns>
        public static List<Uri> GetServerAcceptedAddresses(this IServer self)
        {

            List<Uri> uris = new List<Uri>();

            var localIps = GetLocalIPv4();
            HashSet<string> hash = new HashSet<string>();
            var schemes = self.GetServerAddresses();
            
            if (schemes != null)
            {
            
                foreach (Uri u in schemes)
                    foreach (var item in localIps)
                        hash.Add(new Url(u.Scheme, item.ToString(), u.Port, u.Segments).ToString());

                foreach (string address in hash)
                    uris.Add(new Uri(address));
            
            }

            return uris;

        }


        public static List<Uri> GetServerAddresses(this IServer self)
        {

            List<Uri> schemes = new List<Uri>();

            var addresses = self.Features.Get<IServerAddressesFeature>()?.Addresses?.ToArray();
            if (addresses != null)
                foreach (string? address in addresses)
                {
                    try
                    {
                        var u = new Uri(address);
                        schemes.Add(u);
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine($"address {address} can't be convert in uri");
                    }
                }

            return schemes;

        }

        /// <summary>
        /// Gets the ethernet local IP v4 address.
        /// </summary>
        /// <returns></returns>
        public static List<IPAddress> GetLocalIPv4Ethernet() => GetLocalIPv4(NetworkInterfaceType.Ethernet);

        /// <summary>
        /// Gets the wireless local IP v4 address.
        /// </summary>
        /// <returns></returns>
        public static List<IPAddress> GetLocalIPv4Wireless() => GetLocalIPv4(NetworkInterfaceType.Wireless80211);

        /// <summary>
        /// Gets the local IP v4 address for the specified type.
        /// </summary>
        /// <param name="_type">The type.</param>
        /// <returns></returns>
        public static List<IPAddress> GetLocalIPv4(NetworkInterfaceType _type) => GetLocalIPv4(c => c.NetworkInterfaceType == _type);

        /// <summary>
        /// Gets the local IP v4 address for the specified filter
        /// </summary>
        /// <param name="func">The function.</param>
        /// <returns></returns>
        public static List<IPAddress> GetLocalIPv4(Func<NetworkInterface, bool> func = null)
        {

            if (func == null) func = a => true;

            List<IPAddress> output = new List<IPAddress>();

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
                if (item.OperationalStatus == OperationalStatus.Up && func(item))
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                            output.Add(ip.Address);

            return output;

        }

    }

}
