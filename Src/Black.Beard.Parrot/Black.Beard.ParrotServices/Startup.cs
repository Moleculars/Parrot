using Bb.ParrotServices.Middlewares;
using Bb.ParrotServices.Services;
using Bb.Services;
using System.Diagnostics;
using System.Reflection;
using NLog.Web;
using Bb.Extensions;
using Microsoft.Extensions.Configuration;

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


            services.AddSingleton(typeof(LocalProcessCommandService), typeof(LocalProcessCommandService));
            services.AddSingleton(typeof(ProjectBuilderProvider), typeof(ProjectBuilderProvider));
            services.AddSingleton(typeof(ServiceReferential), typeof(ServiceReferential));
            services.AddSingleton(typeof(GenericTraceListener), typeof(GenericTraceListener));
            //services.AddSingleton<ApiKeyReferentialDatas, ApiKeyReferentialDatas>();
            //services.AddSingleton<ApiKeyReferential, ApiKeyReferential>();

            services.AddConfigurationApiKey(_configuration);

            services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();

            // OpenAPI 
            // https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=visual-studio
            services.AddSwaggerGen(c =>
            {
                c.AddDocumentation();
                c.AddSwaggerWithApiKeySecurity(services, _configuration, $"{Assembly.GetExecutingAssembly().GetName().Name}");
                //c.TagActionsBy(a => new List<string> { a.ActionDescriptor is ControllerActionDescriptor b ? b.ControllerTypeInfo.Assembly.FullName.Split('.')[2].Split(',')[0].Replace("Web", "") : a.ActionDescriptor.DisplayName });
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            var services = app.ApplicationServices;

            var projectBuilder = (ProjectBuilderProvider)services.GetService(typeof(ProjectBuilderProvider));
            projectBuilder.Initialize(Directory.GetCurrentDirectory());

            var tracelistener = (GenericTraceListener)services.GetService(typeof(GenericTraceListener));
            Trace.Listeners.Add(tracelistener);

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

            //loggerFactory.AddLog4Net();
            app
              .UseHttpsRedirection()
              .UseAuthorization()


              .UseMiddleware<ReverseProxyMiddleware>()
              .UseApiKeyHandlerMiddleware()

              //  .ConfigureExceptionHandler()
              .ConfigureHttpInfoLogger()
              .UseRouting()

              .UseEndpoints(endpoints =>
              {
                  endpoints.MapControllers();
              })
            ;
        }
    }

}
