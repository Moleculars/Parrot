using Bb.Expressions;
using Bb.Process;
using System.Text;

namespace Bb.Services.ProcessHosting
{

    /// <summary>
    /// dotnet commands
    /// </summary>
    /// <seealso cref="Bb.Process.ProcessCommand" />
    public class DotnetCommand : ProcessCommand
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DotnetCommand"/> class.
        /// </summary>
        public DotnetCommand() : this(Guid.NewGuid(), null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotnetCommand"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public DotnetCommand(Guid id) : this(id, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotnetCommand"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="tag">tag index</param>
        public DotnetCommand(Guid id, object tag)
            : base(id, tag)
        {
            Command($"dotnet.exe");
            Intercept(log);
        }

        private void log(object sender, TaskEventArgs args)
        {

            switch (args.Status)
            {
                case TaskEventEnum.Started:
                    _sb.AppendLine($"Started process. {args.Process.FileNameText} {args.Process.ArgumentText}");
                    break;

                case TaskEventEnum.ErrorReceived:
                case TaskEventEnum.DataReceived:
                    _sb.AppendLine(args?.DateReceived?.Data);
                    break;

                case TaskEventEnum.Completed:
                    var instance = args.Process.Tag as ServiceReferentialContract;
                    if (instance != null)
                        _sb.AppendLine($"{instance.Parent.Template}/{instance.Contract} process is ended");
                    else
                        _sb.AppendLine("process is Completed");
                    break;

                case TaskEventEnum.RanWithException:
                    var instance1 = args.Process.Tag as ServiceReferentialContract;
                    if (instance1 != null)
                        _sb.AppendLine($"{instance1.Parent.Template}/{instance1.Contract} ended with exception");
                    else
                        _sb.AppendLine("ended with exception");
                    break;

                case TaskEventEnum.RanCanceled:
                    _sb.AppendLine("process is canceled");
                    break;
                case TaskEventEnum.FailedToCancel:
                    _sb.AppendLine("process is failed to cancel");
                    break;

                case TaskEventEnum.FailedToStart:
                case TaskEventEnum.Releasing:
                case TaskEventEnum.Disposing:
                default:
                    break;
            }

        }

        /// <summary>
        /// Gets the trace.
        /// </summary>
        /// <value>
        /// The trace.
        /// </value>
        public string Trace => _sb.ToString();

        private StringBuilder _sb = new StringBuilder();

    }

}
