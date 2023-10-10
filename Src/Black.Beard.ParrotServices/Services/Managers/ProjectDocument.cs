using Bb.Models;
using Bb.OpenApiServices;
using System.Text.Json.Serialization;

namespace Bb.Services.Managers
{


    /// <summary>
    /// List of project
    /// </summary>
    public class ProjectDocuments : List<ProjectDocument>
    {
        /// <summary>
        /// Root of the projects
        /// </summary>
        public string Root { get; set; }

    }

    /// <summary>
    /// describes a file of the project
    /// </summary>
    public class ProjectDocument
    {

        public ProjectDocument()
        {
            Documents = new List<Document>();
        }

        /// <summary>
        /// the contract name
        /// </summary>
        public string Contract { get; internal set; }

        /// <summary>
        /// the generator template
        /// </summary>
        public string Template { get; internal set; }

        /// <summary>
        /// List of Documents for the project
        /// </summary>
        public List<Document> Documents { get; set; }

        [JsonIgnore]
        internal ContextGenerator? Context { get; set; }

    }

}
