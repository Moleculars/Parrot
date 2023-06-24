using Bb.Middlewares.ApiKeys;
using Bb.Middlewares.ReversProxy;
using Bb.ParrotServices.Middlewares;

namespace Bb.Extensions
{
    public static class DependencyRegistrarExtension
    {


        public static IApplicationBuilder UsdeHttpInfoLogger(this IApplicationBuilder builder) => builder.UseMiddleware<HttpInfoLoggerMiddleware>();

        public static IApplicationBuilder UseApiKey(this IApplicationBuilder builder) => builder.UseMiddleware<ApiKeyHandlerMiddleware>();

        public static IApplicationBuilder UseReverseProxy(this IApplicationBuilder builder) => builder.UseMiddleware<ReverseProxyMiddleware>();

    }


}
