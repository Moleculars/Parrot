using Bb.ComponentModel.Factories;
using Microsoft.Extensions.Configuration;

namespace Bb.Models
{

    public class ConfigurationBase : IInitialize
    {

        public ConfigurationBase()
        {
                
        }

        public void Initialize(IServiceProvider services)
        {
            var configuration = services.GetService<IConfiguration>();
            configuration.Bind(GetType().Name, this);
        }

    }

}
