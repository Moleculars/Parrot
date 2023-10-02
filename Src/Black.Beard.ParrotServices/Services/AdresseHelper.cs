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


    public static class AdresseHelper
    {


        public static List<Uri> GetServerAcceptedAddresses(this IHost self)
        {

            List<Uri> addresses = new List<Uri>();

            IServer server = self.Services.GetService<IServer>();

            if (server != null)
                return server.GetServerAcceptedAddresses();

            return addresses;

        }

        public static List<Uri> GetServerAcceptedAddresses(this IServer self)
        {

            var localIps = GetLocalIPv4();
            HashSet<string> hash = new HashSet<string>();
            List<Uri> uris = new List<Uri>();
            var schemes = self.GetServerAddresses();

            foreach (Uri u in schemes)
                foreach (var item in localIps)
                    hash.Add(new Url(u.Scheme, item.ToString(), u.Port, u.Segments).ToString());

            foreach (string address in hash)
                uris.Add(new Uri(address));

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

        public static List<IPAddress> GetLocalIPv4Ethernet() => GetLocalIPv4(NetworkInterfaceType.Ethernet);

        public static List<IPAddress> GetLocalIPv4Wireless() => GetLocalIPv4(NetworkInterfaceType.Wireless80211);

        public static List<IPAddress> GetLocalIPv4(NetworkInterfaceType _type) => GetLocalIPv4(c => c.NetworkInterfaceType == _type);

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
