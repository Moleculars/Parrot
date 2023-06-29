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
                _logger.Debug($"no configuration {Path.GetFileName(path)} found.");
                path = Path.Combine(_env.ContentRootPath, $"{file}{extension}");
            }

            if (File.Exists(path))
            {
                _logger.Debug($"configuration {Path.GetFileName(path)} is configured for loading");
                _config.AddJsonFile(path, optional: optional, reloadOnChange: reloadOnChange);
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
