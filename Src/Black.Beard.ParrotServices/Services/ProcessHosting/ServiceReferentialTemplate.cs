using Bb;
using Bb.Models;

namespace Bb.Services.ProcessHosting
{


    public class ServiceReferentialContract
    {

        public const string LabelProxy = "proxy";

        public ServiceReferentialContract(ServiceReferentialTemplate parent, string name)
        {
            Parent = parent;
            Contract = name;
            _template = parent.Template;

        }

        public AddressTranslator Https { get; private set; }

        public AddressTranslator Http { get; private set; }

        internal ServiceReferentialContract Register(ServiceHost host)
        {
            this.Service = host;
            return Register(host.Services);
        }

        internal ServiceReferentialContract UnRegister()
        {
            this.Service = null;
            Http = null;
            Https = null;
            return this;
        }

        internal ServiceReferentialContract Register(Listener listener)
        {

            var request = $"/{ServiceReferentialContract.LabelProxy}/{_template}/{Contract}";

            if (listener.Http != null)
            {
                Http = new AddressTranslator()
                {
                    QuerySource = request,
                    TargetPort = listener.Http.InternalUrl.Port.Value,
                    TargetUri = listener.Http.InternalUrl.ToUri(),
                    TargetUrl = listener.Http.InternalUrl,
                };
            }

            if (listener.Https != null)
            {
                Https = new AddressTranslator()
                {
                    QuerySource = request,
                    TargetPort = listener.Https.InternalUrl.Port.Value,
                    TargetUri = listener.Https.InternalUrl.ToUri(),
                    TargetUrl = listener.Https.InternalUrl,
                };
            }

            return this;

        }

        /// <summary>
        /// Contract name
        /// </summary>
        public string Contract { get; }

        /// <summary>
        /// get the parent template
        /// </summary>
        public ServiceReferentialTemplate Parent { get; }

        /// <summary>
        /// hosted Service
        /// </summary>
        public ServiceHost Service { get; private set; }


        private readonly string _template;


    }

    /// <summary>
    /// 
    /// </summary>
    public class AddressTranslator
    {
        public string QuerySource { get; internal set; }
        public int TargetPort { get; internal set; }

        public Uri TargetUri { get; internal set; }

        public Url TargetUrl { get; internal set; }

    }

}
