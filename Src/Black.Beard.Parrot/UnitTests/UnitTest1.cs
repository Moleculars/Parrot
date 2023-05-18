using Black.Beard.OpenApiServices;

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
            var generator = new MockServiceGenerator(name, this._baseDirectory.FullName)
            {
                Namespace = @namespace
            }
            .InitializeContract(_file)
            .ConfigureProject(prj =>
            {

            })
            .Generate()
            ;

            ;

        }


        private readonly string _file;
        private readonly DirectoryInfo? _baseDirectory;

    }

}