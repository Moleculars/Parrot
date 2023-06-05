using Bb.Process;
using Bb.OpenApiServices;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Bb.ParrotServices.Services
{


    public class ProjectBuilderContract
    {


        public ProjectBuilderContract(ProjectBuilderProvider parent, string contract)
        {
            this._parent = parent;
            this.Contract = contract;

            Root = Path.Combine(parent.Root, contract);

            //if (!Directory.Exists(Root))
            //    Directory.CreateDirectory(Root);

            this._templates = new Dictionary<string, ProjectBuilderTemplate>();
        }

        public ProjectBuilderTemplate Template(string templateName)
        {

            if (!this._templates.TryGetValue(templateName, out var template1))
                this._templates.Add(templateName, template1 = new ProjectBuilderTemplate(_parent, this, templateName));

            return template1;

        }

        public readonly string Contract;
        public readonly string Root;

        private readonly ProjectBuilderProvider _parent;
        private Dictionary<string, ProjectBuilderTemplate> _templates;
    }

}
