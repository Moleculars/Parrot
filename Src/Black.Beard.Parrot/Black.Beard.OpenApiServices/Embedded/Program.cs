using log4net;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Bb.ParrotServices;
using Microsoft.AspNetCore.Mvc.Controllers;

#pragma warning disable CS0162

internal class Program
{
    private static void Main(string[] args)
    {

        // Proofing against weird starting directories
        var currentAssembly = Assembly.GetAssembly(typeof(Program));
        if (currentAssembly != null && !string.IsNullOrEmpty(currentAssembly.Location))
        {
            var name = Path.GetDirectoryName(currentAssembly.Location);
            if (!string.IsNullOrEmpty(name))
                Directory.SetCurrentDirectory(name);
        }

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
                    
                    c.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = ("Parrot mock service " + "{{title}}").Trim(),
                        Version = "{{version}}",
                        Description = "{{version}}",
                        License = new OpenApiLicense() { Name = "Only usable with a valid PU partner contract." },
                    });
                    c.IncludeXmlComments(() => SwaggerExtension.LoadXmlFiles());

                    if ({{testApiKey}})
                        c.AddSecurityDefinition("key", new OpenApiSecurityScheme { Scheme = "ApiKey", In = ParameterLocation.{{apiSecureIn}} });

                    c.TagActionsBy(a =>
                    {

                        var result = new List<string> { "mock", "{{title}}" };

                        string? c;
                        var b = a.ActionDescriptor as ControllerActionDescriptor;
                        if (b != null)
                        {
                            c = b.ControllerTypeInfo?.Assembly?.FullName?.Split('.')[2]?.Split(',')[0]?.Replace("Web", "");
                        }
                        else
                            c = a.ActionDescriptor?.DisplayName;

                        if (c != null)
                            result.Add(c);

                        return result;

                    });

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

        Environment.Exit(exitCode);

    }
}