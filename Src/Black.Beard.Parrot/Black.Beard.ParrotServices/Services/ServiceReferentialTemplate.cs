namespace Bb.Services
{


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

}
