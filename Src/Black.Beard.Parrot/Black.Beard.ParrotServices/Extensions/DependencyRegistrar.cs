using Bb.Models.Security;
using Bb.ParrotServices.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace Bb.Extensions
{

    public static class DependencyRegistrar
    {

        public static IApplicationBuilder ConfigureHttpInfoLogger(this IApplicationBuilder builder) => builder.UseMiddleware<HttpInfoLoggerMiddleware>();

        public static IApplicationBuilder UseApiKeyHandlerMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<ApiKeyHandlerMiddleware>();


        public static void AddConfigurationApiKey(this IServiceCollection services, IConfiguration configuration)
        {
            var apiKeyConfiguration = new ApiKeyConfiguration();
            configuration.Bind(nameof(ApiKeyConfiguration), apiKeyConfiguration);

            services.AddSingleton(typeof(ApiKeyConfiguration), apiKeyConfiguration);
            services.AddSingleton(typeof(IApiKeyRepository), typeof(ApiKeyRepository));

        }
    }


}
