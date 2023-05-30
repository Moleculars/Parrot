using Bb.ParrotServices;
using log4net;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Controllers;

internal class Program
{
    private static void Main(string[] args)
    {

        // Proofing against weird starting directories
        var currentAssembly = Assembly.GetAssembly(typeof(Program));
        Directory.SetCurrentDirectory(Path.GetDirectoryName(currentAssembly.Location));

        // Read and load Log4Net Config File
        var repo = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(log4net.Repository.Hierarchy.Hierarchy));
        log4net.Config.XmlConfigurator.Configure(repo, new FileInfo("log4net.config"));
        var exitCode = 0;
        //TechnicalLog.AsyncContext.ServiceType = TechnicalLog.EngineServiceType;

        var setup = new Setup()
            .Initialize(args)
            .Services()
            .Services(
            s =>
            {

                s.Logging.AddLog4Net();
                //s.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                //s.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                s.Configuration.AddEnvironmentVariables();

                // OpenAPI
                s.Services.AddSwaggerGen(c =>
                {
                    c.DescribeAllParametersInCamelCase();
                    c.IgnoreObsoleteActions();
                    //c.DocInclusionPredicate((f, a) =>
                    //{
                    //    return a.ActionDescriptor is ControllerActionDescriptor b && b.MethodInfo.GetCustomAttributes<ExternalApiRouteAttribute>().Any();
                    //});

                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Parrot APIs",
                        Version = "v1",
                        Description = "A set of REST APIs used by Parrot for manage mock service generator",
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
            .Configure(c =>
            {

            })
            .Run()
            
            ;


    }
}