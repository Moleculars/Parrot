using Bb;
using Bb.Extensions;
using Bb.Middlewares.EntryFullLogger;
using Bb.Models;
using Bb.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using System.Reflection;
using Bb.Swashbuckle;
using Bb.ComponentModel;
using Microsoft.Extensions.Logging;

namespace Bb.ParrotServices
{
    public class Startup : StartupBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {


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


            // Auto discover all types with attribute [ExposeClass] for register  in ioc.
            DiscoversTypes(services, _configuration);

            services.AddControllers();


            // Auto discovers all services with Authorize attribute and 
            // Initialize security policies for apply permissions based on identityPrincipal authorizations
            var policies = PoliciesExtension.GetPolicies();
            if (policies.Any())
                services.AddAuthorization(options =>
                {
                    foreach (var policyModel in policies)
                        options.AddPolicy(policyModel.Name, policy => policy.RequireAssertion(a => Authorize(a, policyModel)));
                });
            var currentAssembly = Assembly.GetAssembly(typeof(Program));
            policies.SaveInFolder(Path.GetDirectoryName(currentAssembly.Location));


            if (Configuration.UseSwagger) // Swagger OpenAPI 
            {
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                // https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=visual-studio
                services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(c =>
                {
                    c.DescribeAllParametersInCamelCase();
                    c.IgnoreObsoleteActions();
                    c.AddDocumentation(i =>
                    {
                        i.Licence(l => l.Name("Only usable with a valid partner contract."));
                    }, "Black.*.xml");
                    c.AddSwaggerWithApiKeySecurity(services, _configuration);
                    c.DocumentFilter<AppendInheritanceDocumentFilter>();
                    c.UseOneOfForPolymorphism();

                });
            }

            if (Configuration.UseTelemetry)
                RegisterTelemetry(services, _configuration);

        }
               

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            if (Configuration.UseSwagger)
            {
                app.UseDeveloperExceptionPage()
                   .UseSwagger()
                   .UseSwaggerUI();
            }

            if (Configuration.UseTelemetry)
                ConfigureTelemetry(app, env, loggerFactory);
            
            if (Configuration.TraceAll)
                app.UseMiddleware<RequestResponseLoggerMiddleware>();


            if (!env.IsDevelopment())
            {
                app.UseForwardedHeaders(); 
                app.UseHsts();
            }
            else
            {
                //app.UseDeveloperExceptionPage();
                app.UseForwardedHeaders();
            }

            app
              .UseHttpsRedirection()
              .UseRouting()
              .UseApiKey()                      // Intercept apikey and create identityPrincipal associated
              .UseAuthorization()               // Apply authorisation for identityPrincipal

              .UseExceptionHandler(c => c.Run(async context =>          // Intercepts exceptions, format 
              {                                                         // the message result and log with trace identifier.
                  var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
                  var error = exceptionHandler.Error;
                  var response = new HttpExceptionModel
                  {
                      Origin = "Parrot services",
                      TraceIdentifier = context.TraceIdentifier,
                      Session = context.Session.Id
                  };
                  context.Response.StatusCode = 500;
                  await context.Response.WriteAsJsonAsync(response);
              }))

              .UseReverseProxy()                // Redirect all call start with /proxy/mock/{contractname} on the hosted service
              .UseHttpInfoLogger()             // log entries requests

              .UseEndpoints(endpoints =>
              {
                  endpoints.MapControllers();
              })

            ;

        }

        private readonly IConfiguration _configuration;

    }



}
