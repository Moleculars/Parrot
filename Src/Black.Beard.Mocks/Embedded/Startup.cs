
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable CS0162
#pragma warning disable CS1591

namespace Bb.ParrotServices
{

    /// <summary>
    /// Startup class for parameter web service
    /// </summary>
    public class Startup : StartupBase
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
            : base(configuration)
        {

        }


        protected override void ConfigureSwagger(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSwagger(c =>
            {
                c.RouteTemplate = _rootSwagger + c.RouteTemplate;
            })
            .UseSwaggerUI(c =>
            {
                c.RoutePrefix = _rootSwagger + c.RoutePrefix;
            });
        }


        /// <summary>
        /// Configures the custom services.
        /// </summary>
        /// <param name="services"></param>
        public override void AppendServices(IServiceCollection services)
        {

        }

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public override void ConfigureApplication(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            base.ConfigureApplication(app, env, loggerFactory);

            app
                .UseRouting()
                ;

        }

        private const string _rootSwagger = "proxy/{{template}}/{{contract}}/";

    }




}
