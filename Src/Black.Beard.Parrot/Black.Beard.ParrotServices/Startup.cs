using Bb.ParrotServices.Services;
using Bb.Services;
using System.Diagnostics;
using System.Reflection;
using NLog.Web;
using Bb.Extensions;
using Microsoft.Extensions.Configuration;
using Bb.Middlewares.Exceptions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using System.Linq;
using Bb.Models.Security;

namespace Bb.ParrotServices
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            services.UseConfigurationByAttribute(_configuration);

            services.AddSingleton(typeof(LocalProcessCommandService), typeof(LocalProcessCommandService));
            services.AddSingleton(typeof(ProjectBuilderProvider), typeof(ProjectBuilderProvider));
            services.AddSingleton(typeof(ServiceReferential), typeof(ServiceReferential));
           
            services.UseExceptionHandling(_configuration);
            services.UserLoggingBehavior();
            services.UseApiKey(_configuration);

            services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle


            // Initialize security policiy
            var policies = PoliciesExtension.GetPolicies();
            if (policies.Any())
                services.AddAuthorization(options =>
                {
                    foreach (var policyM in policies)
                        options.AddPolicy(policyM.Name, policy => policy.RequireAssertion(a => Authorize(a, policyM)));
                });


            var currentAssembly = Assembly.GetAssembly(typeof(Program));
            policies.Save(Path.GetDirectoryName(currentAssembly.Location));


            // OpenAPI 
            // https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=visual-studio
            //services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.AddDocumentation();
                c.AddSwaggerWithApiKeySecurity(services, _configuration, $"{Assembly.GetExecutingAssembly().GetName().Name}");
                //c.TagActionsBy(a => new List<string> { a.ActionDescriptor is ControllerActionDescriptor b ? b.ControllerTypeInfo.Assembly.FullName.Split('.')[2].Split(',')[0].Replace("Web", "") : a.ActionDescriptor.DisplayName });
            });

        }

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


            var services = app.ApplicationServices;

            var projectBuilder = (ProjectBuilderProvider)services.GetService(typeof(ProjectBuilderProvider));
            projectBuilder.Initialize(Directory.GetCurrentDirectory());

            if (env.IsDevelopment())
            {

                app.UseSwagger();

                app.UseSwaggerUI();
                //app.UseSwaggerUI(c =>
                //  {
                //      c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo Items v1");
                //      // https://stackoverflow.com/questions/55914610/disable-try-it-out-in-swagger
                //      //c.SupportedSubmitMethods();
                //  });

            }

            app
              .UseHttpsRedirection()
              .UseRouting()
              .UseApiKey()
              .UseAuthorization()

              .UseCustomExceptionHandler()
              .UseReverseProxy()
              .UsdeHttpInfoLogger()


              .UseEndpoints(endpoints =>
              {
                  endpoints.MapControllers();
              })
            ;
        }
    }

}
