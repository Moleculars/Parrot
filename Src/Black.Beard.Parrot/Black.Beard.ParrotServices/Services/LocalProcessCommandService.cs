using Bb.ParrotServices.Controllers;
using Bb.Process;
using System.Diagnostics;

namespace Bb.ParrotServices.Services
{

    public class LocalProcessCommandService : ProcessCommandService
    {

        public LocalProcessCommandService()
        {
            this.Intercept(this.Interceptor);
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
