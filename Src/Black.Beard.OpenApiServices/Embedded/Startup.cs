using log4net;
using Microsoft.AspNetCore.Diagnostics;


#pragma warning disable CS0162
#pragma warning disable CS1591

namespace Bb.ParrotServices
{

    public class Startup : StartupBase
    {
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration) : base(configuration)
        {
      
        }

        /// <summary>
        /// Configures the custom services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public virtual void ConfigureCustomServices(IServiceCollection services)
        {

        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {

            // Auto discover all types with attribute [ExposeClass] for register  in ioc.
            RegisterTypes(services);

            // see : https://learn.microsoft.com/fr-fr/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-7.0#fhmo
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor
                  | ForwardedHeaders.XForwardedProto;

                //options.ForwardLimit = 2;
                //options.KnownProxies.Add(IPAddress.Parse("127.0.10.1"));
                //options.ForwardedForHeaderName = "X-Forwarded-For-My-Custom-Header-Name";

            });


            //if (Configuration.UseSwagger) // Swagger OpenAPI 
            //    RegisterServicesSwagger(services);


            //if (Configuration.UseTelemetry)
            //    RegisterTelemetry(services);

            ConfigureCustomServices(services);

            services.AddControllers();

        }



        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public override void ConfigureExtensions(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            app
                .UseHttpsRedirection()
                .UseRouting()

                .UseReverseProxy()                // Redirect all call start with /proxy/mock/{contractname} on the hosted service
                .UseApiKey()                      // Intercept apikey and create identityPrincipal associated
                .UseAuthorization()               // Apply authorisation for identityPrincipal
                ;

        }

    }

}
