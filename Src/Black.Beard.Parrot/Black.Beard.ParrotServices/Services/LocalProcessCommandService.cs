using Antlr4.Runtime.Misc;
using Bb.ParrotServices.Controllers;
using Bb.Process;
using System.Diagnostics;

namespace Bb.ParrotServices.Services
{

    public class LocalProcessCommandService : ProcessCommandService
    {

        public LocalProcessCommandService(ILogger<ProcessCommandService> logger)
        {
            this._logger = logger;
            this.Intercept(this.Interceptor);

        }

        private void Interceptor(object sender, TaskEventArgs args)
        {

            switch (args.Status)
            {

                case TaskEventEnum.Started:
                    this._logger.LogTrace("Started");
                    break;

                case TaskEventEnum.ErrorReceived:
                    this._logger.LogError(args.DateReceived.Data);
                    break;

                case TaskEventEnum.DataReceived:
                    this._logger.LogInformation(args.DateReceived.Data);
                    break;

                case TaskEventEnum.Completed:
                    this._logger.LogTrace("Completed");
                    break;

                case TaskEventEnum.CompletedWithException:
                    this._logger.LogError("Completed with exception");
                    break;

                default:
                    break;

            }

        }

        internal ILogger<ProcessCommandService> _logger;

    }

}
