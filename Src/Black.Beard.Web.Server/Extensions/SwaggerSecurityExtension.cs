using Bb.Models.Security;
using Bb.Swashbuckle;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bb.Extensions
{

    /// <summary>
    /// Append swagger security
    /// </summary>
    public static class SwaggerSecurityExtension
    {

        /// <summary>
        /// Adds the swagger with API key security read in the configuration .
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="services">The services.</param>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="System.InvalidOperationException">ApiKeyConfiguration.ApiHeader is null or empty.</exception>
        public static void AddSwaggerWithApiKeySecurity(this SwaggerGenOptions self, IServiceCollection services, IConfiguration configuration)
        {

            var apiKeyConfiguration = ApiKeyConfiguration.New(configuration);

            if (string.IsNullOrEmpty(apiKeyConfiguration?.ApiHeader))
                throw new InvalidOperationException("ApiKeyConfiguration.ApiHeader is null or empty.");

            self.AppendApiKey(securityDefinition, apiKeyConfiguration.ApiHeader);

        }

        const string securityDefinition = "ApiKey";

    }


}


