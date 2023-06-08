using Bb.Mock;

namespace Bb.Models
{
    public class ProjectRunning : ProjectItem
    {

        public ProjectRunning(ProjectItem root)
        {
            _root = root;
            Started = root != null;
            Swaggers = root?.Swaggers;
            Services = root?.Services;
        }

        public bool Started { get; }
        public WatchdogResult UpAndRunningResult { get; internal set; }

        private ProjectItem _root;

    }

}
