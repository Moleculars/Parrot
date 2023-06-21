using Bb.ComponentModel.Attributes;
using Bb.ParrotServices.Controllers;
using Bb.Process;
using System.Diagnostics;

namespace Bb.Services.ProcessHosting
{

    /// <summary>
    /// Wrap the process command line for log event
    /// </summary>
    [ExposeClass(Context = Constants.Models.Service, LifeCycle = IocScopeEnum.Singleton)]
    public class LocalProcessCommandService : ProcessCommandService
    {

        public LocalProcessCommandService()
        {
            Intercept(Interceptor);
        }

        private void Interceptor(object sender, TaskEventArgs args)
        {

            switch (args.Status)
            {

                case TaskEventEnum.Started:
                    Trace.WriteLine("Started", "trace");
                    break;

                case TaskEventEnum.ErrorReceived:
                    Trace.WriteLine(args.DateReceived.Data, "Error");
                    break;

                case TaskEventEnum.DataReceived:
                    Trace.WriteLine(args.DateReceived.Data, "Info");
                    break;

                case TaskEventEnum.Completed:
                    Trace.WriteLine($"Completed", "Info");
                    break;

                case TaskEventEnum.CompletedWithException:
                    Trace.WriteLine("ended with exception", "Error");
                    break;

                default:
                    break;

            }

        }

    }

}
