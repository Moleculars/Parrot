namespace Bb.Models
{
    public class ProjectItem
    {

        /// <summary>
        /// Gets or sets the contract name.
        /// </summary>
        /// <value>
        /// The contract.
        /// </value>
        public string Contract { get; set; }

        /// <summary>
        /// Gets or sets the template name for generating the project.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        public string Template { get; set; }

        /// <summary>
        /// return utils urls
        /// </summary>
        /// <value>
        /// The swaggers.
        /// </value>
        public Listener Swaggers { get; set; }

        /// <summary>
        /// Gets or sets the services addresses.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public Listener Services { get; set; }

        /// <summary>
        /// Gets or sets the IsUpAndRunning services addresses.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public Listener IsUpAndRunningServices { get; set; }


    }

}
