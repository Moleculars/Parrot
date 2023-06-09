
using Bb.ParrotServices;
using System.Reflection;
using NLog;
using NLog.Web;
using Bb.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Bb;
using System.Runtime.InteropServices;

internal class Program
{
    private static void Main(string[] args)
    {

        var exitCode = 0;

        InitializeOs();

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

    private static void InitializeOs()
    {

        var currentAssembly = Assembly.GetAssembly(typeof(Program));
        Directory.SetCurrentDirectory(Path.GetDirectoryName(currentAssembly.Location));

        Console.WriteLine("Current directory : " + Directory.GetCurrentDirectory());

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        if (isWindows)
        {

            Configuration.CurrentDirectoryToWrite = Path.GetDirectoryName(currentAssembly.Location);

        }
        else
        {
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            if (isLinux)
            {
                Configuration.CurrentDirectoryToWrite = Path.Combine("home", "parrot");
            }
            else
                throw new Exception($"Os {RuntimeInformation.OSDescription} not managed");

        }

        Console.WriteLine("writing directory : " + Configuration.CurrentDirectoryToWrite);

    }

    public static IHostBuilder CreateHostBuilder(NLog.Logger logger, string[] args) =>
           Host.CreateDefaultBuilder(args)

               .ConfigureWebHostDefaults(webBuilder =>
               {

                   webBuilder.UseStartup<Startup>()
                   ;


                   webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                   {

                       Configuration.UseSwagger = "use_swagger".Evaluate("true") || hostingContext.HostingEnvironment.IsDevelopment();
                       Configuration.TraceAll = "trace_all".Evaluate("true") || hostingContext.HostingEnvironment.IsDevelopment();

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