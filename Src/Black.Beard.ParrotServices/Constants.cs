namespace Bb
{


    public class Configuration
    {

        internal static bool TraceAll;

        public static bool UseSwagger { get; internal set; }

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
