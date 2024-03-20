using Bb.Mock;

namespace Bb.Models
{
    /// <summary>
    /// Describes a project
    /// </summary>
    public class ProjectInfo
    {

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
        public bool Running { get; set; }

        public List<WatchdogResultItem> Infos { get; set; }
        public string Http { get; internal set; }
        public string Https { get; internal set; }
    }

}
