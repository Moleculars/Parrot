using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using System.Net;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Bb.Services
{


    public static class AdresseHelper
    {


        public static List<Uri> GetServerAcceptedAddresses(this IServer self)
        {

            var d = GetLocalIPv4();

            List<Uri> uris = new List<Uri>();

            var add = self.Features.Get<IHttpConnectionFeature>();
            var addresses = self.Features.Get<IServerAddressesFeature>()?.Addresses?.ToArray();
            if (addresses != null)
            {
                foreach (string? address in addresses)
                {

                    var a = new Uri(address);
                    uris.Add(a);

                }
            }

            return uris;

        }

        public static List<Uri> GetServerListenAddresses(this IServer self)
        {

            var listenAdresses = GetLocalIPv4();

            List<Uri> uris = new List<Uri>();

            var add = self.Features.Get<IHttpConnectionFeature>();
            var addresses = self.Features.Get<IServerAddressesFeature>()?.Addresses?.ToArray();
            if (addresses != null)
            {
                foreach (string? address in addresses)
                {
                    if (address.ToLower().StartsWith("http:"))
                    {



                    }

                }
            }

            return uris;

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
