using Bb.ComponentModel.Attributes;

namespace Bb.Middlewares.EntryFullLogger
{
    [ExposeClass(Context = Constants.Models.Service, ExposedType = typeof(IRequestResponseLogger), LifeCycle = IocScopeEnum.Singleton)]
    public class RequestResponseLogger : IRequestResponseLogger
    {
        private readonly ILogger<RequestResponseLogger> _logger;

        public RequestResponseLogger(ILogger<RequestResponseLogger> logger)
        {
            _logger = logger;
        }
        public void Log(IRequestResponseLogModelCreator logCreator)
        {
            //_logger.LogTrace(jsonString);
            //_logger.LogInformation(jsonString);
            //_logger.LogWarning(jsonString);
            _logger.LogCritical(logCreator.LogString());
        }

    }


}
