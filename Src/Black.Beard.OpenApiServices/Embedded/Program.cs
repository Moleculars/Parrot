using log4net;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Bb.ParrotServices;
using Microsoft.AspNetCore.Mvc.Controllers;
using Bb.Mock;

#pragma warning disable CS0162
#pragma warning disable CS1591

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

                s.Services.Add(ServiceDescriptor.Singleton(typeof(ServiceTrace), typeof(ServiceTrace)));

                s.Logging.AddLog4Net();
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
                        Description = "{{title}} - {{version}}",
                        License = new OpenApiLicense() { Name = "Only usable with a valid PU partner contract." },
                    });
                    c.IncludeXmlComments(() => SwaggerExtension.LoadXmlFiles());

                    {{testApiKey}}

                    //c.TagActionsBy(a =>
                    //{
                    //    var result = new List<string> { "{{title}}" };
                    //    /*
                    //    string? c;
                    //    var b = a.ActionDescriptor as ControllerActionDescriptor;
                    //    if (b != null)
                    //    {
                    //        c = b.ControllerTypeInfo?.Assembly?.FullName?.Split('.')[2]?.Split(',')[0]?.Replace("Web", "");
                    //    }
                    //    else
                    //        c = a.ActionDescriptor?.DisplayName;
                    //    if (c != null)
                    //        result.Add(c);
                    //    */
                    //    return result;
                    //});

                });

            }
            )
            .Build()
            .Configure()
            .Configure(app =>
            {

                app.UseSwagger(c =>
                {
                    c.RouteTemplate = "proxy/mock/{{contract}}/swagger/{documentname}/swagger.json";
                });

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/proxy/mock/{{contract}}/swagger/v1/swagger.json", "{{title}}");
                    c.RoutePrefix = "proxy/mock/{{contract}}/swagger";
                });

            })
            .Run()
            ;

        Environment.Exit(exitCode);

    }
}