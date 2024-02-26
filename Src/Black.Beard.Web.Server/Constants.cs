namespace Bb
{


    public class Configuration
    {

        /// <summary>
        /// Gets or sets a value indicating whether [trace all] (verbose mode).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [trace all]; otherwise, <c>false</c>.
        /// </value>
        public static bool TraceAll { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [use swagger].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use swagger]; otherwise, <c>false</c>.
        /// </value>
        public static bool UseSwagger { get; set; }

        /// <summary>
        /// Gets or sets the current directory to write projects.
        /// </summary>
        /// <value>
        /// The current directory to write projects.
        /// </value>
        public static string? CurrentDirectoryToWriteProjects { get; set; }


        /// <summary>
        /// Gets or sets the current directory to write generators.
        /// </summary>
        /// <value>
        /// The current directory to write generators.
        /// </value>
        public static string? CurrentDirectoryToWriteGenerators { get; set; }

        /// <summary>
        /// Gets or sets the trace log to write.
        /// </summary>
        /// <value>
        /// The trace log to write.
        /// </value>
        public static string? TraceLogToWrite { get; set; }

    }


    public class Constants
    {

        public static class Models
        {

            public const string Service = "Service";
            public const string Model = "Model";
            public const string Plugin = "Plugin";

        }

    }


}
