using Bb.Middlewares.ApiKeys;
using Bb.ParrotServices.Middlewares;

namespace Bb.Extensions
{

    public static class DependencyRegistrarExtension
    {

        /// <summary>
        /// Uses the HTTP information logger.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseHttpInfoLogger(this IApplicationBuilder builder) => builder.UseMiddleware<HttpInfoLoggerMiddleware>();

        /// <summary>
        /// Uses the API key for authorisation.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseApiKey(this IApplicationBuilder builder) => builder.UseMiddleware<ApiKeyHandlerMiddleware>();

    }


}
