using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Bb.OpenApiServices
{

    public abstract class ServiceGenerator<T> : ServiceGenerator
        where T : class, new()
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceGenerator{T}"/> class.
        /// </summary>
        public ServiceGenerator()
        {
            this.Configuration = new T();
        }

        /// <summary>
        /// Gets the type of the configuration.
        /// </summary>
        /// <value>
        /// The type of the configuration.
        /// </value>
        public override Type ConfigurationType => typeof(T);

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public T Configuration { get; set; }

        public override object GetConfiguration() => Configuration;

        public override void ApplyConfiguration(object configuration)
        {
            this.Configuration = (T)configuration;
        }

    }


    public abstract class ServiceGenerator
    {

        static ServiceGenerator()
        {
            jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public abstract object GetConfiguration();

        public abstract void ApplyConfiguration(object token);

        public abstract Type ConfigurationType { get; }

        internal abstract void InitializeDatas(string file);

        /// <summary>
        /// Gets the directory.
        /// </summary>
        /// <value>
        /// The directory.
        /// </value>
        public string Directory { get => _dir.FullName; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Template { get; internal set; }

        public string Contract { get; internal set; }

        public abstract ContextGenerator Generate();

        protected string Load(params string[] paths)
        {
            var path = Path.Combine(paths);
            return path.LoadFromFile().Map(_objectForMap);
        }

        public void SetObjectForMap (object mapObject)
        {
            _objectForMap = mapObject;
        }

        private object _objectForMap;


        internal DirectoryInfo _dir;
        protected static readonly JsonSerializerSettings jsonSerializerSettings;

    }


}