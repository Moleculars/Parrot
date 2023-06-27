
using Bb.ParrotServices;
using System.Reflection;
using NLog;
using NLog.Web;
using Bb.Extensions;

internal class Program
{
    private static void Main(string[] args)
    {

        var exitCode = 0;

        // Proofing against weird starting directories
        var currentAssembly = Assembly.GetAssembly(typeof(Program));
        Directory.SetCurrentDirectory(Path.GetDirectoryName(currentAssembly.Location));


        // Initialize log
        var logger = NLog.LogManager
            .Setup()
            .SetupExtensions(s => { })
            .GetCurrentClassLogger()
            ;
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

            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();

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

                       // Load configurations files
                       new ConfigurationLoader(logger, hostingContext, config)
                        .TryToLoadConfigurationFile("appsettings.json", false, false)
                        .TryToLoadConfigurationFile("apikeysettings.json", false, false)
                        .TryToLoadConfigurationFile("policiessettings.json", false, false)
                        ;

                   });


                   
                   webBuilder.ConfigureLogging(l =>
                   {
                       l.ClearProviders()
                       ;
                   })
                   .UseNLog(new NLogAspNetCoreOptions()
                   {
                       IncludeScopes = true,
                       IncludeActivityIdsWithBeginScope = true,
                   });




               });


}