using Bb.ParrotServices;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Controllers;
using Bb.ParrotServices.Services;
using Bb.Services;
using System.Diagnostics;
using NLog.Web;
using NLog;

internal class Program
{
    private static void Main(string[] args)
    {

        // Proofing against weird starting directories
        var currentAssembly = Assembly.GetAssembly(typeof(Program));
        Directory.SetCurrentDirectory(Path.GetDirectoryName(currentAssembly.Location));

        // Read and load Log4Net Config File
        // Early init of NLog to allow startup and exception logging, before host is built
        var logger = NLog.LogManager
            .Setup()
            .LoadConfigurationFromFile("nlog.config")
            .GetCurrentClassLogger(); 

        // .LoadConfigurationFromAppSettings().GetCurrentClassLogger();
        logger.Debug("init main");
        var exitCode = 0;
        //TechnicalLog.AsyncContext.ServiceType = TechnicalLog.EngineServiceType;

        var setup = new Setup()
            .Initialize(args)
            .Services()
            .Services(
            s =>
            {

                //s.WebHost.UseKestrel(options =>
                //{
                //    options.Listen(IPAddress.Loopback, 0); // dynamic port
                //});
                
                s.Services.Add(ServiceDescriptor.Singleton(typeof(LocalProcessCommandService), typeof(LocalProcessCommandService)));
                s.Services.Add(ServiceDescriptor.Singleton(typeof(ProjectBuilderProvider), typeof(ProjectBuilderProvider)));
                s.Services.Add(ServiceDescriptor.Singleton(typeof(ServiceReferential), typeof(ServiceReferential)));
                s.Services.Add(ServiceDescriptor.Singleton(typeof(Log4netTraceListener), typeof(Log4netTraceListener)));


                //s.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                //s.Configuration.AddJsoProjectBuildernFile("appsettings.json", optional: true, reloadOnChange: false);
                s.Configuration.AddEnvironmentVariables();

                // OpenAPI
                s.Services.AddSwaggerGen(c =>
                {
                    c.DescribeAllParametersInCamelCase();
                    c.IgnoreObsoleteActions();
                    //c.DocInclusionPredicate((f, a) => { return a.ActionDescriptor is ControllerActionDescriptor b && b.MethodInfo.GetCustomAttributes<ExternalApiRouteAttribute>().Any(); });

                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Parrot APIs",
                        Version = "v1",
                        Description = "A set of REST APIs used by Parrot for manage service generator",
                        License = new OpenApiLicense() { Name = "Only usable with a valid PU partner contract." },
                    });

                    c.IncludeXmlComments(() => SwaggerExtension.LoadXmlFiles());
                    c.AddSecurityDefinition("key", new OpenApiSecurityScheme { Scheme = "ApiKey", In = ParameterLocation.Header });
                    c.TagActionsBy(a => new List<string> { a.ActionDescriptor is ControllerActionDescriptor b ? b.ControllerTypeInfo.Assembly.FullName.Split('.')[2].Split(',')[0].Replace("Web", "") : a.ActionDescriptor.DisplayName });

                });

            }
            )
            .Build()
            .Configure()
            .Configure(app =>
            {


                //IServerAddressesFeature serverAddressesFeature = app.ServerFeatures.Get<IServerAddressesFeature>();
                //var addresses = serverAddressesFeature?.Addresses?.ToArray();


                var projectBuilder = (ProjectBuilderProvider)app.Services.GetService(typeof(ProjectBuilderProvider));
                projectBuilder.Initialize(Directory.GetCurrentDirectory());

                var tracelistener = (Log4netTraceListener)app.Services.GetService(typeof(Log4netTraceListener));
                Trace.Listeners.Add(tracelistener);

            })
            .Run()
            
            ;


    }
}