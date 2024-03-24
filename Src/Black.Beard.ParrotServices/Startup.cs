﻿using Bb.Extensions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Bb.Servers.Web.Models;
namespace Bb.ParrotServices
{


    /// <summary>
    /// Startup class par parameter
    /// </summary>
    public class Startup : Bb.Servers.Web.StartupBase
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration) 
            : base(configuration)
        {
      
        }

        public override void ConfigureServices(IServiceCollection services)
        {

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GlobalConfiguration.CurrentDirectoryToWriteGenerators = "c:\\".Combine("tmp", "parrot", "contracts");
                GlobalConfiguration.DirectoryToTrace = "c:\\".Combine("tmp", "parrot", "logs");
            }
            else 
            {
                GlobalConfiguration.CurrentDirectoryToWriteGenerators = "tmp".Combine("parrot", "contracts");
                GlobalConfiguration.DirectoryToTrace = "tmp".Combine("parrot", "logs");
            }


            if (!Directory.Exists(GlobalConfiguration.CurrentDirectoryToWriteGenerators))
                Directory.CreateDirectory(GlobalConfiguration.CurrentDirectoryToWriteGenerators);

            if (!Directory.Exists(GlobalConfiguration.DirectoryToTrace))
                Directory.CreateDirectory(GlobalConfiguration.DirectoryToTrace);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                GlobalConfiguration.DirectoryToTrace += "\\";

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                GlobalConfiguration.DirectoryToTrace += "/";

            //Trace.TraceInformation("setting directory to generate projects in : " + Configuration.CurrentDirectoryToWriteProjects);
            Trace.TraceInformation("setting directory to output logs : " + GlobalConfiguration.DirectoryToTrace);

            base.ConfigureServices(services);

        }


        /// <summary>
        /// Configures the custom services.
        /// </summary>
        /// <param name="services"></param>
        public override void AppendServices(IServiceCollection services)
        {

            RegisterServicesPolicies(services);

        }

    
        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public override void ConfigureApplication(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            base.ConfigureApplication(app, env, loggerFactory);
                        
            app
                .UseHttpsRedirection()
                .UseRouting()

                .UseReverseProxy()                // Redirect all call start with /proxy/mock/{contractname} on the hosted service
                .UseApiKey()                      // Intercept apiKey and create identityPrincipal associated
                .UseAuthorization()               // Apply authorization for identityPrincipal
                ;

        }

    }



}
