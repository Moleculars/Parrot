using Bb.Models;

namespace Bb.Services.Managers
{

    public class ProjectDocument
    {

        public ProjectDocument()
        {
            Documents = new List<Document>();
        }

        public string Contract { get; internal set; }

        public string Template { get; internal set; }

        public List<Document> Documents { get; set; }

    }

}
