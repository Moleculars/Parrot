
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

        public bool TemplateExist(string templateName)
        {
            return Directory.Exists(Path.Combine(Root, templateName));
        }

        public ProjectBuilderTemplate Template(string templateName)
        {

            if (!this._templates.TryGetValue(templateName, out var template1))
                this._templates.Add(templateName, template1 = new ProjectBuilderTemplate(_parent, this, templateName));

            return template1;

        }

        public List<ProjectBuilderTemplate> List()
        {

            List< ProjectBuilderTemplate> items = new List<ProjectBuilderTemplate>();
            var dirRoot = new DirectoryInfo(Root);
            var dirs = dirRoot.GetDirectories();
            foreach (var dir in dirs)
            {
                var template = Template(dir.Name);
                items.Add(template);
            }

            return items;

        }

        public readonly string Contract;
        public readonly string Root;

        private readonly ProjectBuilderProvider _parent;
        private Dictionary<string, ProjectBuilderTemplate> _templates;
    }

}
