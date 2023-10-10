using Bb.OpenApiServices;
using Bb.Models;
using Bb.ComponentModel.Attributes;
using Bb.Models.Security;
using Microsoft.AspNetCore.Components;
using Bb.ComponentModel.Factories;
using Bb.Services.ProcessHosting;
using Bb.ParrotServices.Controllers;

namespace Bb.Services.Managers
{


    [ExposeClass(Context = Constants.Models.Service, LifeCycle = IocScopeEnum.Singleton)]
    public class ProjectBuilderProvider : IInitialize
    {

        /// <summary>
        /// Initializes the <see cref="ProjectBuilderProvider"/> class.
        /// </summary>
        static ProjectBuilderProvider()
        {
            BuildGeneratorList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectBuilderProvider"/> class.
        /// </summary>
        /// <param name="host">The host centralize all ran service.</param>
        /// <param name="referential">The referential.</param>
        /// <param name="logger">The logger.</param>
        public ProjectBuilderProvider(LocalProcessCommandService host, ServiceReferential referential, ILogger<ProjectBuilderProvider> logger)
        {
            _logger = logger;
            _referential = referential;
            _host = host;
            _items = new Dictionary<string, ProjectBuilderContract>();
        }

        /// <summary>
        /// Initializes the <see cref="ProjectBuilderProvider" />.
        /// </summary>
        /// <param name="services">The service provider.</param>
        public virtual void Initialize(IServiceProvider services)
        {
            Initialize(Configuration.CurrentDirectoryToWriteProjects);
        }

        /// <summary>
        /// Initializes the <see cref="ProjectBuilderProvider" />.
        /// </summary>
        /// <param name="pathRoot">The path root where the contracts will be generate.</param>
        public virtual void Initialize(string pathRoot)
        {

            if (!Directory.Exists(pathRoot))
                Directory.CreateDirectory(pathRoot);

            _root = pathRoot;

        }

        /// <summary>
        /// return the contract for specified contract name.
        /// </summary>
        /// <param name="contractName">The contract name.</param>
        /// <returns></returns>
        public ProjectBuilderContract Contract(string contractName)
        {

            contractName = contractName.ToLower();

            if (!_items.TryGetValue(contractName, out var builder))
                lock (_lock)
                    if (!_items.TryGetValue(contractName, out builder))
                        _items.Add(contractName, builder = new ProjectBuilderContract(this, contractName));

            return builder;

        }

        /// <summary>
        /// Lists the by template.
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <returns></returns>
        public ProjectDocuments ListByTemplate(string templateName)
        {

            _logger.LogDebug("looking in folder Root : {root}", _root);

            var result = new ProjectDocuments()
            {
                Root = _root
            };

            // c:\tmp\parrot\projects
            // C:\tmp\parrot\projects\parcel\mock

            var dirRoot = new DirectoryInfo(_root);
            var dirs = dirRoot.GetDirectories();
            foreach (var dir in dirs)
            {
                var contract = Contract(dir.Name);
                if (contract.TemplateExistsOnDisk(templateName))
                {

                    var template = contract.Template(templateName);

                    ContextGenerator ctx = new ContextGenerator(template.Root);

                    var item = template.List(ctx);
                    result.Add(item);
                }
            }

            return result;

        }

        /// <summary>
        /// return the list of contracts already exists in the referential
        /// </summary>
        /// <returns></returns>
        public async Task<List<ProjectBuilderTemplate>> List()
        {

            List<ProjectBuilderTemplate> items = new List<ProjectBuilderTemplate>();

            var dirRoot = new DirectoryInfo(_root);
            var dirs = dirRoot.GetDirectories();
            foreach (var dir in dirs)
            {
                ProjectBuilderContract contract = Contract(dir.Name);
                items.AddRange(contract.List());
            }

            Thread.Yield();

            return items;

        }

        /// <summary>
        /// return the list of template services runnings. every service runs is tested
        /// </summary>
        /// <param name="templateName">Name of the template.</param>
        /// <returns></returns>
        public async Task<List<ProjectRunning>> ListRunningsByTemplate(string templateName)
        {

            var result = new List<ProjectRunning>();

            var dirRoot = new DirectoryInfo(_root);
            var dirs = dirRoot.GetDirectories();
            foreach (var dir in dirs)
            {
                var contract = Contract(dir.Name);
                if (contract.TemplateExistsOnDisk(templateName))
                {
                    var template = contract.Template(templateName);
                    var item = await template.IsRunnings();
                    result.Add(item);
                }
            }

            return result;

        }

        /// <summary>
        /// Gets a value indicating whether contract exists in the referential.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [contract exists]; otherwise, <c>false</c>.
        /// </value>
        public bool ContractExists
        {
            get
            {
                return Directory.Exists(_root)
                    && File.Exists(Path.Combine(_root, "contract.json"))
                    ;
            }
        }


        #region generators

        internal Type ResolveGenerator(string template)
        {

            if (_generators.TryGetValue(template, out var generator))
                return generator;

            return null;

        }

        private static void BuildGeneratorList()
        {
            _generators = new Dictionary<string, Type>();
            var list = GetGeneratorTypes().ToList();
            foreach (var type in list)
            {

                var name = type.Name;
                if (name.EndsWith("Generator"))
                    name = name.Substring(0, name.Length - "Generator".Length);
                if (name.EndsWith("Service"))
                    name = name.Substring(0, name.Length - "Service".Length);
                _generators.Add(name.ToLower(), type);
            }
        }

        private static IEnumerable<Type> GetGeneratorTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
                foreach (var type in assembly.GetTypes())
                    if (!type.IsAbstract && typeof(ServiceGenerator).IsAssignableFrom(type))
                        yield return type;
        }

        #endregion generators

        /// <summary>
        /// Gets the root path for access to the ressource.
        /// </summary>
        /// <value>
        /// The root.
        /// </value>
        public string Root => _root;

        internal readonly ILogger<ProjectBuilderProvider> _logger;
        internal readonly ServiceReferential _referential;
        internal readonly LocalProcessCommandService _host;
        private readonly Dictionary<string, ProjectBuilderContract> _items;
        private string _root;
        private static Dictionary<string, Type> _generators;
        private volatile object _lock = new object();

    }

}
