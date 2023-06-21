using Flurl;

namespace Bb.Services.ProcessHosting
{


    public class ServiceReferentialContract
    {

        public ServiceReferentialContract(ServiceReferentialTemplate parent, string name)
        {
            Parent = parent;
            Contract = name;
            _template = parent.Template;

        }

        public AddressTranslator Https { get; private set; }

        public AddressTranslator Http { get; private set; }


        internal void Register(params Uri[] uris)
        {
            foreach (var redirect in uris)
            {
                if (redirect != null)
                {
                    var request = $"/proxy/{_template}/{Contract}";

                    //     .Add(new KeyValuePair<string, Uri>(request, redirect));
                    var u = new AddressTranslator()
                    {
                        QuerySource = request,
                        TargetPort = redirect.Port,
                        TargetUri = redirect,
                        TargetUrl = new Url(redirect).AppendPathSegment(request),
                    };

                    if (redirect.Scheme == "http")
                        Http = u;

                    else
                        Https = u;
                }
            }
        }

        public string Contract { get; }

        public ServiceReferentialTemplate Parent { get; }

        private readonly string _template;


    }

    public class AddressTranslator
    {
        public string QuerySource { get; internal set; }
        public int TargetPort { get; internal set; }
        public Uri TargetUri { get; internal set; }
        public Url TargetUrl { get; internal set; }
    }

}
