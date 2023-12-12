using Bb.ComponentModel.Attributes;

namespace Bb.Middlewares.EntryFullLogger
{

    [ExposeClass(Context = Constants.Models.Service, ExposedType = typeof(IRequestResponseLogger), LifeCycle = IocScopeEnum.Singleton)]
    public class RequestResponseLogger : IRequestResponseLogger
    {

        public RequestResponseLogger(ILogger<RequestResponseLogger> logger)
        {
            _logger = logger;
        }
        public void Log(IRequestResponseLogModelCreator logCreator)
        {
            _logger.LogCritical(logCreator.LogString());
        }

        private readonly ILogger<RequestResponseLogger> _logger;

    }


}
