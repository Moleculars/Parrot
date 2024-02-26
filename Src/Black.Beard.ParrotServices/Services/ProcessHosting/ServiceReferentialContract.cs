
namespace Bb.Services.ProcessHosting
{
    public class ServiceReferentialTemplate
    {

        public ServiceReferentialTemplate(ServiceReferential parent, string name)
        {
            Parent = parent;
            Template = name;
            _contracts = new Dictionary<string, ServiceReferentialContract>();
        }


        internal ServiceReferentialContract TryGet(string[] route, int index)
        {

            var contractName = route[index];

            if (_contracts.TryGetValue(contractName, out var contract))
                return contract;

            return null;

        }

        internal ServiceReferentialContract Get(string contract)
        {

            if (!_contracts.TryGetValue(contract, out var project))
                _contracts.Add(contract, project = new ServiceReferentialContract(this, contract));

            return project;

        }

        internal void Remove(ServiceReferentialContract template)
        {
            _contracts.Remove(template.Contract);
        }

        public ServiceReferentialContract Resolve(string contractName)
        {

            if (_contracts.TryGetValue(contractName, out var contract))
                return contract;

            return null;

        }

        public ServiceReferential Parent { get; }

        public string Template { get; }

        private readonly Dictionary<string, ServiceReferentialContract> _contracts;

    }

}
