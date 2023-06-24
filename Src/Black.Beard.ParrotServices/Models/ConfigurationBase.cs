using Bb.ComponentModel.Factories;
using Microsoft.Extensions.Configuration;

namespace Bb.Models
{

    public class ConfigurationBase : IInitialize
    {

        protected ConfigurationBase()
        {
                
        }

        protected ConfigurationBase(ILoggerFactory logger)
        {
            if (logger != null)
            {
                this._logger = logger.CreateLogger("Initializing");
                this._logger?.LogDebug($"{GetType().Name} is created");
            }
        }

        public virtual void Initialize(IServiceProvider services)
        {
            var configuration = services.GetService<IConfiguration>();
            configuration.Bind(GetType().Name, this);
            
            this._logger?.LogDebug($"{GetType().Name} is initialized");

        }

        private readonly ILogger _logger;

    }

}
