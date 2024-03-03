using Bb.ComponentModel;
using Bb.Extensions;
using Bb.Models.Security;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using OpenTelemetry.Instrumentation.AspNetCore;
using System.Reflection.PortableExecutable;

namespace Bb
{

    public class StartupBase
    {



        public void RegisterTelemetry(IServiceCollection services, IConfiguration configuration)
        {

            var resource = ResourceBuilder.CreateDefault().AddService("Black.Beard.Web.Server");

            var builder = services.AddOpenTelemetry();

            builder.WithTracing((builder) =>
            {
                
                var activities = Bb.Diagnostics.DiagnosticProviderExtensions.GetActivityNames();
                var keys = activities.Select(c => c.Item2).ToArray();

                builder.SetResourceBuilder(resource)
                    .AddConsoleExporter()
                    .AddAspNetCoreInstrumentation(o =>
                    {

                        o.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag("requestProtocol", httpRequest.Protocol);
                        };
                        o.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag("responseLength", httpResponse.ContentLength);
                        };
                        o.EnrichWithException = (activity, exception) =>
                        {
                            activity.SetTag("exceptionType", exception.GetType().ToString());
                        };

                    })
                    .AddSource(keys)
                    ;
            });

            builder.WithMetrics((builder) =>
            {

                var meters = Bb.Diagnostics.DiagnosticProviderExtensions.GetMeterNames();
                var keys = meters.Select(c => c.Item2).ToArray();

                builder.SetResourceBuilder(resource)
                    .AddConsoleExporter()
                    .AddAspNetCoreInstrumentation()
                    .AddMeter(keys);

            });

            services.Configure<AspNetCoreTraceInstrumentationOptions>(options =>
            {
                //options.Filter = (httpContext) =>
                //{
                //    // only collect telemetry about HTTP GET requests
                //    return httpContext.Request.Method.Equals("GET");
                //};
            });

            //builder.Configuration.AddInMemoryCollection(
            //    new Dictionary<string, string?>
            //    {
            //        ["OTEL_DOTNET_EXPERIMENTAL_ASPNETCORE_ENABLE_GRPC_INSTRUMENTATION"] = "true",
            //    });

        }

        public virtual void ConfigureTelemetry(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
                   

        }

        /// <summary>
        /// Auto discover all types with attribute [ExposeClass] for register  in ioc.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public virtual void DiscoversTypes(IServiceCollection services, IConfiguration configuration)
        {

            // Auto discover all types with attribute [ExposeClass] for register  in ioc.
            services.UseTypeExposedByAttribute(configuration, ConstantsCore.Configuration, c =>
            {
                services.BindConfiguration(c, configuration);
                //var cc1 = JsonSchema.FromType(c).ToJson();
                //var cc2 = c.GenerateContracts();
            })
            .UseTypeExposedByAttribute(configuration, Constants.Models.Model)
            .UseTypeExposedByAttribute(configuration, Constants.Models.Service);

        }


        /// <summary>
        /// evaluate permissions.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="policy">The policy.</param>
        /// <returns></returns>
        protected async Task<bool> Authorize(AuthorizationHandlerContext arg, PolicyModel policy)
        {

            if (arg.User != null)
            {

                var res = (DefaultHttpContext)arg.Resource;
                var path = res.Request.Path;

                PolicyModelRoute route = policy.Evaluate(path);

                var i = arg.User.Identity as ClaimsIdentity;
                var roles = i.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

                //if (roles.Where(c => c.Value == ""))

            }

            await Task.Yield();

            return true;

        }



    }

}
