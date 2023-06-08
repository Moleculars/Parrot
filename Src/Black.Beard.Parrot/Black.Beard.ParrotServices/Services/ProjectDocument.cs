using Bb.Models;

namespace Bb.ParrotServices.Services
{

    public class ProjectDocument
    {

        public ProjectDocument()
        {
            this.Documents = new List<Document>();    
        }

        public string Contract { get; internal set; }

        public string Template { get; internal set; }

        public List<Document> Documents { get; set; }

    }

}
