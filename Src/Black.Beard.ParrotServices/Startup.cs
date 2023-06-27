using Bb;
using Bb.Extensions;
using Bb.Middlewares.EntryFullLogger;
using Bb.Models;
using Bb.Models.Security;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;

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



            // Swagger OpenAPI 
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

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage()
                   .UseSwagger()
                   .UseSwaggerUI()
                   .UseMiddleware<RequestResponseLoggerMiddleware>();
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
                      Session = context.Session 
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
