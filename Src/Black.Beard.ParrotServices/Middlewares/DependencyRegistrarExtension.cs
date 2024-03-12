using Bb.Middlewares.ReversProxy;


namespace Bb.Extensions
{
    public static class DependencyRegistrarExtension
    {


        public static IApplicationBuilder UseReverseProxy(this IApplicationBuilder builder) => builder.UseMiddleware<ReverseProxyMiddleware>();

    }


}
