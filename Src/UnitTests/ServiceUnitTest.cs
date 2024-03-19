using Bb;
using Bb.Curls;
using Bb.ParrotServices;

namespace UnitTests
{

    public class ServiceUnitTest
    {

        public ServiceUnitTest()
        {
            _file = AppContext.BaseDirectory.Combine("swagger.json");
            var file = typeof(ServiceUnitTest).Assembly.Location;
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
                .AddLocalhostUrlWithDynamicPort("http", null, ref port)
                //.AddLocalhostUrlWithDynamicPort("https", ref port) // Créer un certificat pour utilisation
                )
            {
                var t1 = service.RunAsync();
                var uri = service.Addresses.First(c => c.Scheme == "http" && c.Host == "127.0.0.1");


                var adr1 = new Url(uri, "Watchdog", "isupandrunning");
                CurlInterpreter command1 = $"curl -X 'GET' '{adr1}' -H 'accept: application/json'";
                var a = command1.ResultToJson();

                var adr2 = new Url(uri, "Generator", "mock", "parcel", "run"); // 'https://localhost:7033/Generator/mock/parcel/run'
                CurlInterpreter command2 = $"curl -X 'PUT' '{adr2}' -H 'accept: */*'";
                var tt= command2.ResultToJson();


            }

        }

        private readonly string _file;
        private readonly DirectoryInfo? _baseDirectory;

    }

}