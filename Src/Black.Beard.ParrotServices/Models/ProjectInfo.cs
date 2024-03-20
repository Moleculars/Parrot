using Bb.Mock;

namespace Bb.Models
{
    /// <summary>
    /// Describes a project
    /// </summary>
    public class ProjectInfo
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectInfo"/> class.
        /// </summary>
        public ProjectInfo()
        {
            Listeners = new List<Endpoint>();
            Infos = new List<WatchdogResultItem>();
        }

        /// <summary>
        /// Name of the contract
        /// </summary>
        public string Contract { get; set; }

        /// <summary>
        /// Name of the template
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// return true if the project is running
        /// </summary>
        public bool Hosted { get; set; }

        /// <summary>
        /// return true if the project is running
        /// </summary>
        public bool Running { get; set; }

        /// <summary>
        /// Infos
        /// </summary>
        public List<WatchdogResultItem> Infos { get; set; }

        /// <summary>
        /// List of listeners
        /// </summary>
        public List<Endpoint> Listeners { get; internal set; }

    }

}
