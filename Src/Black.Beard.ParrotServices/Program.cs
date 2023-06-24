
using Bb.ParrotServices;
using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Controllers;
using Bb.Services;
using System.Diagnostics;
using NLog;
using Microsoft.AspNetCore.Authorization;
using Bb.Models;
using Bb.Json.Jslt.CustomServices;
using Microsoft.Extensions.Configuration;
using Bb.Json.Jslt.CustomServices.Csv;
using NLog.Web;
using NLog.Layouts;
using NLog.Targets;
using System;
using Bb.ParrotServices.Middlewares;
using NLog.Config;
using NLog.LayoutRenderers;
using Bb.Extensions;
using Microsoft.Extensions.Logging;

internal class Program
{
    private static void Main(string[] args)
    {

        var exitCode = 0;
        
        // Proofing against weird starting directories
        var currentAssembly = Assembly.GetAssembly(typeof(Program));
        Directory.SetCurrentDirectory(Path.GetDirectoryName(currentAssembly.Location));

        var logger = NLog.LogManager
            .Setup()
            .SetupExtensions(s =>
            {
                //s.RegisterLayoutRenderer("trace_id1", (logevent) =>
                //{
                //    return "1111";
                //});
                //s.RegisterLayoutRenderer<AspNetRequestAllHeadersLayoutRenderer>()
                //;
            }
            )
            .GetCurrentClassLogger()
            ;
        //NLog.Web.LayoutRenderers.AspNetLayoutRendererBase.Register("aspnet-request-all-headers", typeof(AspNetRequestAllHeadersLayoutRenderer));

        logger.Debug("init main");

        try
        {

            CreateHostBuilder(logger, args)
                .Build()
                .Run();

        }
        catch (Exception exception)
        {
            exitCode = exception.HResult;
            logger.Error(exception, "Stopped program because of exception");
        }
        finally
        {
            NLog.LogManager.Shutdown();
            Environment.ExitCode = exitCode;
        }

    }


    public static IHostBuilder CreateHostBuilder(NLog.Logger logger, string[] args) =>
           Host.CreateDefaultBuilder(args)
                
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   
                   webBuilder.UseStartup<Startup>()
                   ;

                   webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                   {

                       new ConfigurationLoader(logger, hostingContext, config)
                        .TryToLoadConfigurationFile("appsettings.json", false, false)
                        .TryToLoadConfigurationFile("apikeysettings.json", false, false)
                        .TryToLoadConfigurationFile("policiessettings.json", false, false)
                       ;

                   });

                   webBuilder.ConfigureLogging(l =>
                   {

                       l.ClearProviders()

                       ;                   })
                   .UseNLog( new NLogAspNetCoreOptions() 
                   {  
                       IncludeScopes = true,
                       IncludeActivityIdsWithBeginScope = true,
                   });




               });


    //private static void EnsureDb()
    //{
    //    if (File.Exists("Log.db3"))
    //        return;
    //    using (SqliteConnection connection = new SqliteConnection(@"c:\"))
    //    using (SqliteCommand command = new SqliteCommand(
    //        "CREATE TABLE Log (Timestamp TEXT, Loglevel TEXT, Callsite TEXT, Message TEXT)",
    //        connection))
    //    {
    //        connection.Open();
    //        command.ExecuteNonQuery();
    //    }
    //}

}