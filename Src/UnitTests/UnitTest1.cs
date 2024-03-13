using Bb.Process;
using Bb.OpenApiServices;
using Bb;
namespace UnitTests
{

    public class UnitTest1
    {

        public UnitTest1()
        {
            _file = AppContext.BaseDirectory.Combine( "swagger.json");
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
            .Initialize("myContract", name, this._baseDirectory.FullName)
            .InitializeDataSources(_file)
            .Generate()
            ;

            var dir = generator.Directory.Combine( name + ".csproj");
                        
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