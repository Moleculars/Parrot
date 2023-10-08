using Bb.Mock;

namespace Bb.Models
{
    /// <summary>
    /// object for modeling running status
    /// </summary>
    /// <seealso cref="Bb.Models.ProjectItem" />
    public class ProjectRunning : ProjectItem
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectRunning"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        public ProjectRunning(ProjectItem root)
        {
            _root = root;
            Started = root != null;
            if (root != null)
            {
                Swagger = root.Swagger;
                Services = root.Services;
                IsUpAndRunningServices = root.Services;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the service is started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if started; otherwise, <c>false</c>.
        /// </value>
        public bool Started { get; }

        /// <summary>
        /// Gets up and running result.
        /// </summary>
        /// <value>
        /// Up and running result.
        /// </value>
        public WatchdogResult UpAndRunningResult { get; internal set; }

        private ProjectItem _root;

    }

}
