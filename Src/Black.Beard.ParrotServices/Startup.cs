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
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Bb.ParrotServices
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
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


            // Auto discover all types with attribute [ExposeClass] and register in ioc.
            services.UseTypeExposedByAttribute(_configuration, Constants.Models.Configuration, c =>
            {
                services.BindConfiguration(c, _configuration);
                //var cc1 = JsonSchema.FromType(c).ToJson();
                //var cc2 = c.GenerateContracts();
            })
            .UseTypeExposedByAttribute(_configuration, Constants.Models.Model)
            .UseTypeExposedByAttribute(_configuration, Constants.Models.Service)

            .AddControllers()            
            ;



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
            policies.Save(Path.GetDirectoryName(currentAssembly.Location));



            if (Configuration.UseSwagger) // Swagger OpenAPI 
            {
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                // https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=visual-studio
                //services.AddEndpointsApiExplorer();
                services.AddSwaggerGen(c =>
                {
                    c.AddDocumentation();
                    c.AddSwaggerWithApiKeySecurity(services, _configuration, $"{Assembly.GetExecutingAssembly().GetName().Name}");
                    //c.TagActionsBy(a => new List<string> { a.ActionDescriptor is ControllerActionDescriptor b ? b.ControllerTypeInfo.Assembly.FullName.Split('.')[2].Split(',')[0].Replace("Web", "") : a.ActionDescriptor.DisplayName });
                });
            }

        }

        /// <summary>
        /// evaluate permissions.
        /// </summary>
        /// <param name="arg">The argument.</param>
        /// <param name="policy">The policy.</param>
        /// <returns></returns>
        private async Task<bool> Authorize(AuthorizationHandlerContext arg, PolicyModel policy)
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            if (Configuration.UseSwagger)
            {
                app.UseDeveloperExceptionPage()
                   .UseSwagger()
                   .UseSwaggerUI();
            }

            if (Configuration.TraceAll)
            {
                app.UseMiddleware<RequestResponseLoggerMiddleware>();
            }

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
              .UsdeHttpInfoLogger()             // log entries requests

              .UseEndpoints(endpoints =>
              {
                  endpoints.MapControllers();
              })              
                         
            ;

        }

        private readonly IConfiguration _configuration;

    }



}
