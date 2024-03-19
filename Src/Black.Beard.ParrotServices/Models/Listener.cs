namespace Bb.Models
{

    public class Listener
    {

        public Listener()
        {

        }


        public void Add(Url externalUrl, Url internalUrl)
        {
            Add(new Endpoint()
            {
                ProxyUrl = externalUrl,
                InternalUrl = internalUrl
            });
        }

        private void Add(Endpoint endpoint)
        {
            if (endpoint.InternalUrl.IsSecureScheme)
                Https = endpoint;
            else
                Http = endpoint;
        }


        public Endpoint Http { get; private set; }

        public Endpoint Https { get; private set; }

    }

}
