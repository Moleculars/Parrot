using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bb.Models.Security
{


    public class ApiKeyConfiguration
    {

        public ApiKeyConfiguration()
        {
            Items = new List<ApiKey>();
        }

        /// <summary>
        /// Return the name of the header will give de api key
        /// </summary>
        public string ApiHeader { get; set; } = "X-API-KEY";

        /// <summary>
        /// return the list a api key
        /// </summary>
        public List<ApiKey> Items { get; set; }

        [JsonIgnore]
        internal string? Filename { get; set; }
             
        internal void Save()
        {

            var config = 
                new JObject(
                    new JProperty(nameof(ApiKeyConfiguration), JObject.FromObject(this))
                );

            Filename.Save(config.ToString());

        }

        internal volatile object _lock = new object();

    }

}
