using Bb.Builds;
using Bb.Expressions;
using Bb.Services.ProcessHosting;
using Bb.Services.Runnings;

namespace Bb.Models
{

    /// <summary>
    /// service host
    /// </summary>
    public class ServiceHost
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceHost"/> class.
        /// </summary>
        /// <param name="assemblyFile"></param>
        /// <param name="references"></param>
        public ServiceHost(string assemblyFile, AssemblyReferences references, string publicHost, params (string, string, int)[] listeners)
        {
            this._publicHost = publicHost;
            _listeners = new Dictionary<string, Listener>();
            _service = new EmbeddedWebService(assemblyFile, references, listeners);
        }


        private const string LabelServices = "Services";
        private const string LabelSwagger = "Swagger";
        private const string LabelIsUpAndRunningServices = "IsUpAndRunningServices";


        /// <summary>
        /// Build directory
        /// </summary>
        /// <param name="scheme"></param>
        /// <param name="host"></param>
        /// <param name="originalPort"></param>
        /// <param name="dynamicPort"></param>
        /// <returns></returns>
        public ServiceHost AddListener(string scheme, string host, int originalPort, int dynamicPort)
        {

            Listener? listener;

            if (!_listeners.TryGetValue(LabelServices, out listener)) _listeners.Add(LabelServices, listener = new Listener());
            listener.Add
            (
                new Url(scheme, _publicHost, originalPort, ServiceReferentialContract.LabelProxy, Template, Contract),
                new Url(scheme, host, dynamicPort, ServiceReferentialContract.LabelProxy, Template, Contract)
            );


            if (!_listeners.TryGetValue(LabelSwagger, out listener)) _listeners.Add(LabelSwagger, listener = new Listener());
            listener.Add
            (
                new Url(scheme, _publicHost, originalPort, ServiceReferentialContract.LabelProxy, Template, Contract, "swagger"),
                new Url(scheme, host, dynamicPort, ServiceReferentialContract.LabelProxy, Template, Contract, "swagger")
            );


            if (!_listeners.TryGetValue(LabelIsUpAndRunningServices, out listener)) _listeners.Add(LabelIsUpAndRunningServices, listener = new Listener());
            listener.Add
            (
                new Url(scheme, _publicHost, originalPort, ServiceReferentialContract.LabelProxy, Template, Contract, "Watchdog", "isupandrunning"),
                new Url(scheme, host, dynamicPort, ServiceReferentialContract.LabelProxy, Template, Contract, "Watchdog", "isupandrunning")
            );

            return this;

        }


        /// <summary>
        /// Swagger listener
        /// </summary>
        public Listener? Swagger
        {
            get
            {
                _listeners.TryGetValue(LabelSwagger, out var listener);
                return listener;
            }
        }

        /// <summary>
        /// Watchdog listener
        /// </summary>
        public Listener? IsUpAndRunningServices
        {
            get
            {
                _listeners.TryGetValue(LabelIsUpAndRunningServices, out var listener);
                return listener;
            }
        }

        /// <summary>
        /// Hosted services
        /// </summary>
        public Listener? Services
        {
            get
            {
                _listeners.TryGetValue(LabelServices, out var listener);
                return listener;
            }
        }

        /// <summary>
        /// Get a listener by name
        /// </summary>
        /// <param name="listernName"></param>
        /// <returns></returns>
        public Listener? this[string listernName]
        {
            get
            {
                _listeners.TryGetValue(listernName, out var listener);
                return listener;
            }
        }



        /// <summary>
        /// Start the service
        /// </summary>
        /// <returns></returns>
        public dynamic? Start()
        {
            var result = _service.Start();
            if (result != null)
            {

                var uris = _service.Uris;
                foreach (var item in uris)
                    AddListener(item.Item1, item.Item2, item.Item3, item.Item4);

                Started = true;

            }

            return result;

        }


        /// <summary>
        /// Stop the service
        /// </summary>
        public void Stop()
        {
            _service.Stop();
        }


        /// <summary>
        /// Gets or sets the contract name.
        /// </summary>
        /// <value>
        /// The contract.
        /// </value>
        public string Contract { get; set; }

        /// <summary>
        /// Gets or sets the template name for generating the project.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        public string Template { get; set; }

        /// <summary>
        /// Gets a value indicating whether the service is started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if started; otherwise, <c>false</c>.
        /// </value>
        public bool Started { get; internal set; }


        /// <summary>
        /// Gets or sets a value indicating whether the service is listening.
        /// </summary>
        public bool Listen { get; internal set; }


        private readonly EmbeddedWebService _service;
        private readonly string _publicHost;
        private Dictionary<string, Listener> _listeners = new Dictionary<string, Listener>();


    }

}
