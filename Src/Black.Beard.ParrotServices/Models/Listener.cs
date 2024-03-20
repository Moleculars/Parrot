namespace Bb.Models
{

    public class Listener
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="Listener"/> class.
        /// </summary>
        public Listener()
        {

        }

        /// <summary>
        /// Add a new endpoint
        /// </summary>
        /// <param name="externalUrl"></param>
        /// <param name="internalUrl"></param>
        public void Add(Url externalUrl, Url internalUrl)
        {
            Add(new Endpoint()
            {
                ProxyUrl = externalUrl,
                InternalUrl = internalUrl
            });
        }

        /// <summary>
        /// Add a new endpoint
        /// </summary>
        /// <param name="endpoint"></param>
        private void Add(Endpoint endpoint)
        {
            if (endpoint.InternalUrl.IsSecureScheme)
                Https = endpoint;
            else
                Http = endpoint;
        }

        /// <summary>
        /// Http address
        /// </summary>
        public Endpoint Http { get; private set; }

        /// <summary>
        /// Https address
        /// </summary>
        public Endpoint Https { get; private set; }

    }

}
