using Microsoft.Extensions.Logging;
using NLog;
using System;
using Bb;

namespace Bb.Extensions
{


    /// <summary>
    /// Configuration Loader
    /// </summary>
    /// <example>
    /// new ConfigurationLoader(logger, hostingContext, config)
    ///     .TryToLoadConfigurationFile("appsettings.json", false, false)
    ///     .TryToLoadConfigurationFile("another_file.json", false, false)
    ///     ;
    /// </example>
    /// </summary>
    public class ConfigurationLoader
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationLoader"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="hostingContext">The hosting context.</param>
        /// <param name="config">The configuration.</param>
        public ConfigurationLoader(Logger logger, WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            this._logger = logger;
            this._hostingContext = hostingContext;
            this._env = hostingContext.HostingEnvironment;
            this._config = config;
        }

        /// <summary>
        /// Try to load configuration file.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// <param name="reloadOnChange">if set to <c>true</c> [reload on change].</param>
        /// <returns></returns>
        public ConfigurationLoader TryToLoadConfigurationFile(string filename, bool optional, bool reloadOnChange)
        {

            var file = Path.GetFileNameWithoutExtension(filename);
            var extension = Path.GetExtension(filename);

            var path = _env.ContentRootPath.Combine( $"{file}.{_env.EnvironmentName}{extension}");

            if (!File.Exists(path))
            {
                _logger.Debug($"Configuration file '{Path.GetFileName(path)}' not found.");
                path = _env.ContentRootPath.Combine( $"{file}{extension}");
            }

            if (File.Exists(path))
            {
                _logger.Debug($"configuration file {Path.GetFileName(path)} is loading.");
                _config.AddJsonFile(path, optional: optional, reloadOnChange: reloadOnChange);
                _logger.Debug($"configuration file {Path.GetFileName(path)} is loaded successfully.");
            }
            else
                _logger.Error($"missing configuration {path}");

            return this;

        }

        private readonly Logger _logger;
        private readonly WebHostBuilderContext _hostingContext;
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationBuilder _config;

    }


}
