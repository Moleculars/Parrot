using Bb.Process;
using Bb.OpenApiServices;
using Bb.ParrotServices;
using Bb;
using Bb.Http;
using System;
using Bb.Curls;
using Bb;

namespace UnitTests
{

    public class ServiceUnitTest
    {

        public ServiceUnitTest()
        {
            _file = Path.Combine(AppContext.BaseDirectory, "swagger.json");
            var file = typeof(UnitTest1).Assembly.Location;
            this._baseDirectory = new FileInfo(file).Directory;
        }

        [Fact]
        public void Test1()
        {


            Configuration.UseSwagger = true;
            Configuration.TraceAll = true;

            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            int port = 5000;
            using (var service = new Bb.ServiceRunner<Startup>()
                .AddLocalhostUrlWithDynamicPort("http", ref port)
                //.AddLocalhostUrlWithDynamicPort("https", ref port) // Créer un certificat pour utilisation
                )
            {

                var t1 = service.RunAsync();
                var adr = new Url(service.Addresses.First(c => c.Scheme == "http" && c.Host == "127.0.0.1"), "Watchdog/isupandrunning");
                CurlInterpreter command = $"curl -X 'GET' '{adr}' -H 'accept: application/json'";
                var a = command.ResultToString();
                
            }

        }

        private readonly string _file;
        private readonly DirectoryInfo? _baseDirectory;

    }

}