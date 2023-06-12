
using Bb.ParrotServices;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Controllers;
using Bb.ParrotServices.Services;
using Bb.Services;
using System.Diagnostics;
using NLog;
using Microsoft.AspNetCore.Authorization;
using Bb.Models;
using Bb.Json.Jslt.CustomServices;
using Microsoft.Extensions.Configuration;
using Bb.Json.Jslt.CustomServices.Csv;
using NLog.Web;

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


        try
        {

            CreateHostBuilder(args).Build().Run();

        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            NLog.LogManager.Shutdown();
        }



    }


    public static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();

                   webBuilder.ConfigureAppConfiguration(a =>
                   {
                       a.AddEnvironmentVariables();
                       a.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                       //a.AddJsonProjectBuilderFile("appsettings.json", optional: true, reloadOnChange: false);

                       var file = Path.Combine(Environment.CurrentDirectory, "apikeysettings.json");
                       if (File.Exists(file))
                       {
                           a.AddJsonFile("apikeysettings.json");
                       }


                   });
                   webBuilder.ConfigureLogging(l =>
                   {
                       l.ClearProviders();
                   })
                   .UseNLog();




               });
}