using Bb.ComponentModel.Attributes;

using System.Text.Json;
using System;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using System.Text.Json.Serialization;
using Bb.ComponentModel;

namespace Bb.Models.Security
{


    [ExposeClass(Context = ConstantsCore.Configuration, ExposedType = typeof(ApiKeyConfiguration), LifeCycle = IocScopeEnum.Singleton, Name = "")]
    public class ApiKeyConfiguration
    {
               
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyConfiguration"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ApiKeyConfiguration()
        {
            Items = new List<ApiKey>();
        }

        public static ApiKeyConfiguration New(IConfiguration configuration)
        {
            var config = new ApiKeyConfiguration();
            configuration.Bind(nameof(ApiKeyConfiguration), config);
            return config;
        }


        /// <summary>
        /// Return the name of the header will give the api key
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

            var t2 = new JsonObject
            {
                [nameof(ApiKeyConfiguration)] = JsonSerializer.SerializeToNode(this)
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string config = t2.ToJsonString(options);

            Filename.Save(config.ToString());

        }

        internal volatile object _lock = new object();

    }

}
