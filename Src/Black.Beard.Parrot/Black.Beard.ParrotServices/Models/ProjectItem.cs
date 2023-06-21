namespace Bb.Models
{

    public class ProjectItem
    {

        public ProjectItem()
        {
            Services = new Listener();
            Swagger = new Listener();
            IsUpAndRunningServices = new Listener();

        }



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
        /// Gets a value indicating whether the service is started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if started; otherwise, <c>false</c>.
        /// </value>
        public bool Started { get; internal set; }


        /// <summary>
        /// return utils urls
        /// </summary>
        /// <value>
        /// The swaggers.
        /// </value>
        public Listener Swagger { get; set; }

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
