using Bb.Projects;
using System;
using System.Collections.Generic;

namespace Bb.Services
{


    public class ServiceReferential
    {

        public ServiceReferential()
        {
            this._contracts = new Dictionary<string, ServiceReferentialContract>();
        }

        public ServiceReferentialInstance Get(string serviceName, string templateName, Guid id, params Uri[] uris)
        {

            lock(_lock)
            {
                var instance = Get(serviceName).Get(templateName).Get(id);
                instance.Register(uris);
                return instance;
            }

        }


        public ServiceReferentialInstance TryGet(string[] route, int index)
        {

            var serviceName = route[index];

            if (_contracts.TryGetValue(serviceName, out var contract))
                return contract.TryGet(route, index + 1);

            return null;

        }

        public ServiceReferentialContract Get(string serviceName)
        {

            if (!_contracts.TryGetValue(serviceName, out var project))
                _contracts.Add(serviceName, project = new ServiceReferentialContract(this, serviceName));

            return project;

        }

        public void Remove(ServiceReferentialInstance? instance)
        {
            lock (_lock)
            {
                instance.Parent.Remove(instance);
            }
        }

        internal ServiceReferentialInstance TryToMatch(PathString path)
        {
            var route = path.Value.Trim('/').Split('/');
            var result = TryGet(route, 1);
            return result;
        }

        private readonly Dictionary<string, ServiceReferentialContract> _contracts;
        private volatile object _lock = new object();
    }

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

    public class ServiceReferentialTemplate
    {

        public ServiceReferentialTemplate(ServiceReferentialContract parent, string name)
        {
            this.Parent = parent;
            this.Template = name;
            this._instances = new Dictionary<Guid, ServiceReferentialInstance>();
        }

        public ServiceReferentialInstance TryGet()
        {
            return _instances.Values.FirstOrDefault();
        }


        public ServiceReferentialInstance Get(Guid instanceId)
        {

            if (!_instances.TryGetValue(instanceId, out var instance))
                _instances.Add(instanceId, instance = new ServiceReferentialInstance(this, instanceId));

            return instance;

        }

        public void Remove(ServiceReferentialInstance instance)
        {

            if (_instances.ContainsKey(instance.Id))
                _instances.Remove(instance.Id);

        }

        public ServiceReferentialContract Parent { get; }

        public string Template { get; }

        private readonly Dictionary<Guid, ServiceReferentialInstance> _instances;

    }

    public class ServiceReferentialInstance
    {

        public ServiceReferentialInstance(ServiceReferentialTemplate parent, Guid id)
        {
            this.Parent = parent;
            this.Id = id;
            this.Uris = new List<Uri>();
        }

        public ServiceReferentialTemplate Parent { get; }

        public Guid Id { get; }

        public List<Uri> Uris { get; }

        public void Register(params Uri[] uris)
        {

            foreach (var uri in uris)
                Uris.Add(uri);

        }

    }

}
