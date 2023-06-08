namespace Bb.Services
{


    public class ServiceReferentialTemplate
    {

        public ServiceReferentialTemplate(ServiceReferentialContract parent, string name)
        {
            this.Parent = parent;
            this.Template = name;
            this._contract = parent.Name;
            this.HttpsUris = new List<KeyValuePair<string, Uri>>();
            this.HttpUris = new List<KeyValuePair<string, Uri>>();
        }

        public List<KeyValuePair<string, Uri>> HttpsUris { get; }
        public List<KeyValuePair<string, Uri>> HttpUris { get; }


        internal void Register(params Uri[] uris)
        {
            foreach (var redirect in uris)
            {
            
                var request = $"/proxy/{_contract}/{this.Template}";

                if (redirect.Scheme == "http")
                    HttpUris.Add(new KeyValuePair<string, Uri>(request, redirect));
                else
                    HttpsUris.Add(new KeyValuePair<string, Uri>(request, redirect));
            
            }
        }

        private readonly string _contract;

        public ServiceReferentialContract Parent { get; }

        public string Template { get; }



    }

}
