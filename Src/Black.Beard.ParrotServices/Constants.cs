namespace Bb
{


    public class Configuration
    {

        public static bool TraceAll { get; set; }

        public static bool UseSwagger { get; internal set; }

        public static string? CurrentDirectoryToWriteProjects { get; internal set; }
        public static string? TraceLogToWrite { get; internal set; }

    }


    public class Constants
    {

        public static class Models
        {

            public const string Configuration = "Configuration";
            public const string Service = "Service";
            public const string Model = "Model";
        
        }

    }


}
