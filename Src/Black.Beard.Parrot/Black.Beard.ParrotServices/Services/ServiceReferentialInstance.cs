namespace Bb.Services
{
    public class ServiceReferentialInstance
    {

        public ServiceReferentialInstance(ServiceReferentialTemplate parent, Guid id)
        {
            this.Parent = parent;
            this.Id = id;
            this.Uris = new List<KeyValuePair<string, Uri>>();
                this._template = this.Parent.Template;
            this._contract = this.Parent.Parent.Name;

        }

        public ServiceReferentialTemplate Parent { get; }

        public Guid Id { get; }

        public List<KeyValuePair<string, Uri>> Uris { get; }

        public void Register(params Uri[] uris)
        {
            foreach (var redirect in uris)
            {
                var request = $"/proxy/{_contract}/{_template}";
                Uris.Add(new KeyValuePair<string, Uri>(request, redirect));
            }
        }

        private readonly string _template;
        private readonly string _contract;

    }

}
