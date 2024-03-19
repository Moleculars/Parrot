using Bb.ComponentModel;
using Bb.Extensions;
using Bb.Models.Security;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Instrumentation.AspNetCore;
using Bb.Swashbuckle;
using System.Reflection;
using Bb.Models;
using Microsoft.AspNetCore.Diagnostics;
using Bb.Middlewares.EntryFullLogger;
using Microsoft.AspNetCore.HttpOverrides;

namespace Bb
{

    public class StartupBase
    {


        public StartupBase(IConfiguration configuration)
        {
            this.CurrentConfiguration = configuration;
        }


        #region Configure


        public virtual void ConfigureServices(IServiceCollection services)
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


            if (Configuration.UseSwagger) // Swagger OpenAPI 
                RegisterServicesSwagger(services);

            if (Configuration.UseTelemetry)
                RegisterTelemetry(services);

            AppendServices(services);

            services.AddControllers();

        }

        /// <summary>
        /// Configures the custom services.
        /// </summary>
        /// <param name="services"></param>
        public virtual void AppendServices(IServiceCollection services)
        {



        }

        protected virtual void InterceptExceptions(IApplicationBuilder c)
        {
            c.Run(async context =>          // Intercepts exceptions, format 
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
            });
        }

        protected virtual void ConfigureEnvironmentProduction(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            //app.UseDeveloperExceptionPage();
            app.UseForwardedHeaders();
        }

        protected virtual void ConfigureEnvironmentDevelopment(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseForwardedHeaders();
            app.UseHsts();
        }

        protected virtual void ConfigureSwagger(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDeveloperExceptionPage()
            .UseSwagger(c =>
            {
               

            })
            .UseSwaggerUI(c =>
            {


            });
        }

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            if (Configuration.UseSwagger) ConfigureSwagger(app, env, loggerFactory);
            if (Configuration.UseTelemetry) ConfigureTelemetry(app, env, loggerFactory);
            if (Configuration.TraceAll) app.UseMiddleware<RequestResponseLoggerMiddleware>();

            if (!env.IsDevelopment()) ConfigureEnvironmentDevelopment(app, env, loggerFactory);
            else ConfigureEnvironmentProduction(app, env, loggerFactory);

            ConfigureApplication(app, env, loggerFactory);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }


        public virtual void ConfigureApplication(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            app.UseExceptionHandler(InterceptExceptions);
            app.UseHttpInfoLogger();             // log entries requests

        }

        #endregion Configure


        protected virtual void RegisterServicesPolicies(IServiceCollection services)
        {
            // Auto discovers all services with Authorize attribute and 
            // Initialize security policies for apply permissions based on identityPrincipal authorizations
            var policies = PoliciesExtension.GetPolicies();
            if (policies.Any())
                services.AddAuthorization(options =>
                {
                    foreach (var policyModel in policies)
                        options.AddPolicy(policyModel.Name, policy => policy.RequireAssertion(a => Authorize(a, policyModel)));
                });
            var currentAssembly = Assembly.GetAssembly(GetType());
            policies.SaveInFolder(Path.GetDirectoryName(currentAssembly.Location));
        }

        protected virtual void RegisterServicesSwagger(IServiceCollection services)
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
                c.AddSwaggerWithApiKeySecurity(services, CurrentConfiguration);
                c.DocumentFilter<AppendInheritanceDocumentFilter>();
                c.UseOneOfForPolymorphism();

            });
        }

        public void RegisterTelemetry(IServiceCollection services)
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
                    //.AddAspNetCoreInstrumentation()
                    .AddMeter(keys);

            });

            //services.Configure<AspNetCoreTraceInstrumentationOptions>(options =>
            //{
            //    //options.Filter = (httpContext) =>
            //    //{
            //    //    // only collect telemetry about HTTP GET requests
            //    //    return httpContext.Request.Method.Equals("GET");
            //    //};
            //});

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
        public virtual void RegisterTypes(IServiceCollection services)
        {

            // Auto discover all types with attribute [ExposeClass] for register  in ioc.
            services.UseTypeExposedByAttribute(CurrentConfiguration, ConstantsCore.Configuration, c =>
            {
                services.BindConfiguration(c, CurrentConfiguration);
                //var cc1 = JsonSchema.FromType(c).ToJson();
                //var cc2 = c.GenerateContracts();
            })
            .UseTypeExposedByAttribute(CurrentConfiguration, Constants.Models.Model)
            .UseTypeExposedByAttribute(CurrentConfiguration, Constants.Models.Service);

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


        public IConfiguration CurrentConfiguration { get; }


    }

}
