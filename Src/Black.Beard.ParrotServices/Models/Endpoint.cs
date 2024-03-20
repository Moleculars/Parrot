using System.Text.Json.Serialization;

namespace Bb.Models
{


    public class Endpoint
    {

        public Endpoint()
        {

        }

        public string Name { get; set; }

        /// <summary>
        /// Real internal listener address
        /// </summary>
        public string InternalAddress { get => InternalUrl?.ToString(); }

        /// <summary>
        /// Accessible address from outside
        /// </summary>
        public string ProxyAddress { get => ProxyUrl?.ToString(); }

        [JsonIgnore]
        public Url InternalUrl { get; set; }

        [JsonIgnore]
        public Url ProxyUrl { get; set; }

    }

}
