using Bb.Json.Jslt.CustomServices.MultiCsv;
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

        public ServiceReferentialTemplate Get(string serviceName, string templateName, params Uri[] uris)
        {
            lock (_lock)
            {
                var instance = Get(serviceName).Get(templateName);
                instance.Register(uris);
                return instance;
            }
        }

        internal void Remove(ServiceReferentialTemplate? template)
        {
            lock (_lock)
            {

                template.Parent.Remove(template);
            
            }

        }


        internal ServiceReferentialContract Get(string serviceName)
        {

            if (!_contracts.TryGetValue(serviceName, out var project))
                _contracts.Add(serviceName, project = new ServiceReferentialContract(this, serviceName));

            return project;

        }


        internal ServiceReferentialTemplate TryToMatch(PathString path)
        {
            var route = path.Value.Trim('/').Split('/');
            var result = TryGet(route, 1);
            return result;
        }

        private ServiceReferentialTemplate TryGet(string[] route, int index)
        {

            var serviceName = route[index];

            if (_contracts.TryGetValue(serviceName, out var contract))
                return contract.TryGet(route, index + 1);

            return null;

        }

        private readonly Dictionary<string, ServiceReferentialContract> _contracts;
        private int _version = 0;
        private volatile object _lock = new object();

    }

}
