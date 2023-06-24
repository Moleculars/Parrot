using System.Text;

namespace Bb.OpenApiServices
{

    public static class HttpHelper
    {


        public static Uri GetLocalUri(bool securised, int port = 0)
        {
            var uri = new UriBuilder();
            uri.Scheme = securised ? "https" : "http";
            uri.Host = "localhost";
            uri.Port = port;
            return uri.Uri;
        }

        public static Uri GetUri(bool securised, string host, int port = 0)
        {
            var uri = new UriBuilder();
            uri.Scheme = securised ? "https" : "http";
            uri.Host = host;
            uri.Port = port;
            return uri.Uri;
        }

        public static int GetAvailablePort(int startingPort)
        {
            var portArray = new List<int>();

            var properties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

            // Ignore active connections
            var connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            // Ignore active tcp listners
            var endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            // Ignore active UDP listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (var i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }

    }


}