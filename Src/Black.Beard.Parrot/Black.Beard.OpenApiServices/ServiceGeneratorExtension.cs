namespace Bb.OpenApiServices
{
    public static class ServiceGeneratorExtension
    {



        public static T InitializeDataSources<T>(this T self, string file)
            where T : ServiceGenerator
        {
            self.InitializeDatas(file);
            return self;

        }

        public static T Initialize<T>(this T self, string name, string baseDirectory)
            where T : ServiceGenerator
        {
            self.Name = name;
            var directoryName = baseDirectory ?? AppContext.BaseDirectory;
            self._dir = new DirectoryInfo(Path.Combine(directoryName, "service"));

            return self;

        }


    }


}