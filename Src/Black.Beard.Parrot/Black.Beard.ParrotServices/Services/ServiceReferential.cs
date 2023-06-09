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
            this._contracts = new Dictionary<string, ServiceReferentialTemplate>();
        }

        public ServiceReferentialContract Register(string templateName, string contractName, params Uri[] uris)
        {
            lock (_lock)
            {
                var instance = Get(templateName).Get(contractName);
                instance.Register(uris);
                return instance;
            }
        }

        internal void Remove(ServiceReferentialContract? contract)
        {
            lock (_lock)
            {

                contract.Parent.Remove(contract);
            
            }

        }


        internal ServiceReferentialTemplate Get(string serviceName)
        {

            if (!_contracts.TryGetValue(serviceName, out var project))
                _contracts.Add(serviceName, project = new ServiceReferentialTemplate(this, serviceName));

            return project;

        }


        internal ServiceReferentialContract TryToMatch(PathString path)
        {
            var route = path.Value.Trim('/').Split('/');
            var result = TryGet(route, 1);
            return result;
        }

        private ServiceReferentialContract TryGet(string[] route, int index)
        {

            var serviceName = route[index];

            if (_contracts.TryGetValue(serviceName, out var contract))
                return contract.TryGet(route, index + 1);

            return null;

        }

        private readonly Dictionary<string, ServiceReferentialTemplate> _contracts;
        private int _version = 0;
        private volatile object _lock = new object();

    }

}
