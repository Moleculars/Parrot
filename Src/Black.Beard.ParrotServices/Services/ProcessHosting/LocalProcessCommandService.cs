using Bb.ComponentModel.Attributes;
using Bb.Process;

namespace Bb.Services.ProcessHosting
{

    /// <summary>
    /// Wrap the process command line for log event
    /// </summary>
    [ExposeClass(Context = Constants.Models.Service, LifeCycle = IocScopeEnum.Singleton)]
    public class LocalProcessCommandService : ProcessCommandService
    {

        public LocalProcessCommandService(ILogger<LocalProcessCommandService> logger)
        {
            _logger = logger;
            Intercept(log);
            var o = this;
        }

        private void log(object sender, TaskEventArgs args)
        {

            var id = args.Process.Id;

            switch (args.Status)
            {
                case TaskEventEnum.Started:
                    _logger.LogInformation("Started process {id}. {cmd} {args}", id, args.Process.FileNameText, args.Process.ArgumentText);
                    break;

                case TaskEventEnum.FailedToStart:
                    break;

                case TaskEventEnum.ErrorReceived:
                    _logger.LogError("process {id} : " + Format(args?.DateReceived?.Data), id);
                    break;

                case TaskEventEnum.DataReceived:
                    _logger.LogInformation("process {id} : " + Format(args?.DateReceived?.Data), id);
                    break;

                case TaskEventEnum.Completed:
                    var instance = args.Process.Tag as ServiceReferentialContract;
                    if (instance != null)
                    {
                        _logger.LogInformation($"{instance.Parent.Template}/{instance.Contract} process {id} is ended", id);
                    }
                    else
                        _logger.LogInformation("process {id} is Completed", id);
                    break;

                case TaskEventEnum.RanWithException:
                    var instance1 = args.Process.Tag as ServiceReferentialContract;
                    if (instance1 != null)
                    {
                        _logger.LogError($"{instance1.Parent.Template}/{instance1.Contract} ended with exception");
                    }
                    else
                        _logger.LogError("ended with exception");
                    break;

                case TaskEventEnum.RanCanceled:
                    _logger.LogInformation("process {id} is canceled", id);
                    break;
                case TaskEventEnum.FailedToCancel:
                    _logger.LogInformation("process {id} is failed to cancel", id);
                    break;

                case TaskEventEnum.Releasing:
                    _logger.LogInformation("process {id} is Completed", id);
                    break;

                case TaskEventEnum.Disposing:
                    _logger.LogInformation("process {id} is Completed", id);
                    break;

                default:
                    break;
            }


        }


        private static string Format(string? data)
        {
            if (!string.IsNullOrEmpty(data))
                return data.Replace("{", "'").Replace("}", "'");

            return string.Empty;

        }

        private readonly ILogger<LocalProcessCommandService> _logger;

    }

}
