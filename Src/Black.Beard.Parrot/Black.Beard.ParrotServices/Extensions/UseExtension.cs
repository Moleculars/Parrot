using Bb.Models.ExceptionHandling;
using Bb.Models.Security;
using Bb.Services;
using Microsoft.Extensions.Configuration;

namespace Bb.Extensions
{
    public static class UseExtension
    {

        public static void UseExceptionHandling(this IServiceCollection services, IConfiguration configuration)
        {
            // Middleware must be injected as a singleton.
            var problemDetailsConfiguration = new ProblemDetailsConfiguration();
            configuration.Bind(nameof(ProblemDetailsConfiguration), problemDetailsConfiguration);
            services.AddSingleton(problemDetailsConfiguration);

            // This is a dependency within the Middleware class, so it too must be injected as a singleton.
            services.AddSingleton(typeof(IProblemDetailsFactory), typeof(ProblemDetailsFactory));
        }

        public static void UserLoggingBehavior(this IServiceCollection services)
        {
            services.AddScoped(typeof(IPipelineBehavior), typeof(LoggingBehavior));
        }

        public static void UseApiKey(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(typeof(IApiKeyRepository), typeof(ApiKeyRepository));
        }

        

    }


}
