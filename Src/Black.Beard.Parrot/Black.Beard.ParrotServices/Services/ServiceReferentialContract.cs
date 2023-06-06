namespace Bb.Services
{
    public class ServiceReferentialContract
    {

        public ServiceReferentialContract(ServiceReferential parent, string name)
        {
            this.Parent = parent;
            this.Name = name;
            this._templates = new Dictionary<string, ServiceReferentialTemplate>();
        }


        public ServiceReferentialInstance TryGet(string[] route, int index)
        {

            var templateName = route[index];

            if (_templates.TryGetValue(templateName, out var template))
                return template.TryGet();

            return null;

        }

        public ServiceReferentialTemplate Get(string template)
        {

            if (!_templates.TryGetValue(template, out var project))
                _templates.Add(template, project = new ServiceReferentialTemplate(this, template));

            return project;

        }

        public ServiceReferential Parent { get; }

        public string Name { get; }

        private readonly Dictionary<string, ServiceReferentialTemplate> _templates;
             
    }

}
