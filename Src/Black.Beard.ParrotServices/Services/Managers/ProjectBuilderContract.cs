namespace Bb.Services.Managers
{


    public class ProjectBuilderContract
    {


        public ProjectBuilderContract(ProjectBuilderProvider parent, string contract)
        {
            _logger = _parent._logger;
            _parent = parent;
            Contract = contract;

            Root = Path.Combine(parent.Root, contract);

            //if (!Directory.Exists(Root))
            //    Directory.CreateDirectory(Root);

            _templates = new Dictionary<string, ProjectBuilderTemplate>();

        }

        public bool TemplateExist(string templateName)
        {
            return Directory.Exists(Path.Combine(Root, templateName));
        }

        public ProjectBuilderTemplate Template(string templateName)
        {

            if (!_templates.TryGetValue(templateName, out var template1))
                _templates.Add(templateName, template1 = new ProjectBuilderTemplate(_parent, this, templateName));

            return template1;

        }

        public List<ProjectBuilderTemplate> List()
        {

            List<ProjectBuilderTemplate> items = new List<ProjectBuilderTemplate>();
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
        internal readonly ILogger<ProjectBuilderProvider> _logger;
        private readonly ProjectBuilderProvider _parent;
        private Dictionary<string, ProjectBuilderTemplate> _templates;
    }

}
