using Bb.Process;
using Bb.OpenApiServices;

namespace UnitTests
{

    public class UnitTest1
    {

        public UnitTest1()
        {
            _file = Path.Combine(AppContext.BaseDirectory, "swagger.json");
            var file = typeof(UnitTest1).Assembly.Location;
            this._baseDirectory = new FileInfo(file).Directory;
        }

        [Fact]
        public void Test1()
        {

            var name = "Black.Beard.Mock";
            var @namespace = "Bb.Mock";
            var generator = new MockServiceGenerator();

            generator.Configuration = new MockServiceGeneratorConfig() { Namespace = @namespace};

            generator
            .Initialize(name, this._baseDirectory.FullName)
            .InitializeDataSources(_file)
            .Generate()
            ;

            var dir = Path.Combine(generator.Directory, name + ".csproj");


            using (var cmd = new ProcessCommand()
                     .Command($"dotnet.exe" , $"build \"{dir}\" -c release /p:Version=1.0.0.0")
                     //.OutputOnTraces()
                     .Run())
            {
                cmd.Wait();
            }


        }


        private readonly string _file;
        private readonly DirectoryInfo? _baseDirectory;

    }

}