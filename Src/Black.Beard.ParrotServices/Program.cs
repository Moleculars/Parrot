using Bb;
using Bb.Extensions;
using Bb.ParrotServices;
using System.Reflection;
using System.Runtime.InteropServices;
using NLog;
using NLog.Web;
using Bb.Services;
using System.Collections;
using System.Diagnostics;

internal class Program
{
    private static void Main(string[] args)
    {

        var exitCode = 0;

        InitializeByOs();

        Logger logger = InitializeLogger();

        var build = CreateHostBuilder(logger, args)
                   .Build();

        try
        {

            var runner = build.RunAsync();

            // TestService(logger, build);
            EnumerateListeners(logger, build); // logger all ip can accept connection 

            var awaiter = runner.GetAwaiter();
            awaiter.GetResult();

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

    private static Logger InitializeLogger()
    {

        // target folder where write
        NLog.GlobalDiagnosticsContext.Set("parrot_log_directory", Configuration.TraceLogToWrite);

        // push environment variables in the log
        foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            if (item.Key != null
                && !string.IsNullOrEmpty(item.Key.ToString())
                && item.Key.ToString().StartsWith("parrot_log_"))
                NLog.GlobalDiagnosticsContext.Set(item.Key.ToString(), item.Value?.ToString());

        // load the configuration file
        var configLogPath = Path.Combine(Directory.GetCurrentDirectory(), "nlog.config");
        NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configLogPath);

        // Initialize log
        var logger = NLog.LogManager
            .Setup()
            .SetupExtensions(s => { })
            .GetCurrentClassLogger()
            ;

        logger.Debug("log initialized");

        return logger;
    }

    private static void EnumerateListeners(Logger logger, IHost build)
    {
        var addresses = build.GetServerAcceptedAddresses();
        foreach (var address in addresses)
            logger.Info($"address : {address}");

    }

    //private static void TestService(Logger logger, IHost build)
    //{
    //    var addresses =  build.GetServerAcceptedAddresses();
    //    foreach (var address in addresses)
    //    {
    //        if (string.IsNullOrEmpty(address.Host))
    //            logger.Error($"{address} can't be tested, because host is not specified.");
    //        else
    //        {
    //            var url = new Url(address).AppendPathSegments("watchdog", "isupandrunning");
    //            var urlTxt = url.ToString();
    //            try
    //            {
    //                var oo = url.SendAsync(HttpMethod.Get).GetAwaiter();
    //                var pp = oo.GetResult();
    //                if (pp.StatusCode == 200)
    //                    logger.Info($"{urlTxt} is listening");
    //                else
    //                    logger.Error($"{urlTxt} is not listening");
    //            }
    //            catch (Exception)
    //            {
    //            }
    //        }
    //    }
    //}

    private static void InitializeByOs()
    {

        Console.WriteLine("Current directory : " + Directory.GetCurrentDirectory());

        var currentAssembly = Assembly.GetAssembly(typeof(Program));
        Directory.SetCurrentDirectory(Path.GetDirectoryName(currentAssembly.Location));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Configuration.CurrentDirectoryToWriteProjects = Path.Combine("c:\\", "tmp", "parrot", "project");
            Configuration.TraceLogToWrite = Path.Combine("c:\\", "tmp", "parrot", "logs");
        }

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Configuration.CurrentDirectoryToWriteProjects = Path.Combine("tmp", "parrot", "project");
            Configuration.TraceLogToWrite = Path.Combine("tmp", "parrot", "logs");
        }

        else
            throw new Exception($"Os {RuntimeInformation.OSDescription} not managed");

        if (!Directory.Exists(Configuration.CurrentDirectoryToWriteProjects))
            Directory.CreateDirectory(Configuration.CurrentDirectoryToWriteProjects);

        if (!Directory.Exists(Configuration.TraceLogToWrite))
            Directory.CreateDirectory(Configuration.TraceLogToWrite);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Configuration.TraceLogToWrite += "\\";
        }

        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Configuration.TraceLogToWrite += "/";
        }

        Console.WriteLine("setting directory to generate projects in : " + Configuration.CurrentDirectoryToWriteProjects);
        Console.WriteLine("setting directory to output logs : " + Configuration.TraceLogToWrite);

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