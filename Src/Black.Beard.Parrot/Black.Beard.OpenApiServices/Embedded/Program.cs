using Black.Beard.ParrotServices;
using log4net;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using System.IO;

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
                s.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                s.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                s.Configuration.AddEnvironmentVariables();

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