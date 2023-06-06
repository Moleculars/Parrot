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

}
