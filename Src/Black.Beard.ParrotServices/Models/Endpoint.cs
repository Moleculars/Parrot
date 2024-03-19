using System.Text.Json.Serialization;

namespace Bb.Models
{

    public class Endpoint
    {

        public string InternalAddress { get => InternalUrl?.ToString(); }

        public string ProxyAddress { get => ProxyUrl?.ToString(); }

        [JsonIgnore]
        public Url InternalUrl { get; set; }

        [JsonIgnore]
        public Url ProxyUrl { get; set; }

    }

}
