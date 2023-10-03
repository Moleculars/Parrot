using NLog;
using System;

namespace Bb.Extensions
{

    public class ConfigurationLoader
    {

        public ConfigurationLoader(Logger logger, WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            this._logger = logger;
            this._hostingContext = hostingContext;
            this._env = hostingContext.HostingEnvironment;
            this._config = config;
        }

        public ConfigurationLoader TryToLoadConfigurationFile(string filename, bool optional, bool reloadOnChange)
        {

            var file = Path.GetFileNameWithoutExtension(filename);
            var extension = Path.GetExtension(filename);

            var path = Path.Combine(_env.ContentRootPath, $"{file}.{_env.EnvironmentName}{extension}");

            if (!File.Exists(path))
            {
                _logger.Debug($"Configuration file '{Path.GetFileName(path)}' not found.");
                path = Path.Combine(_env.ContentRootPath, $"{file}{extension}");
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
