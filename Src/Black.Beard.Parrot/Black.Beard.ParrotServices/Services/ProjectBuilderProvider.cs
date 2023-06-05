using Bb.Process;
using Bb.OpenApiServices;
using Newtonsoft.Json.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Bb.Services;

namespace Bb.ParrotServices.Services
{


    public class ProjectBuilderProvider
    {


        static ProjectBuilderProvider()
        {
            BuildGeneratorList();
        }

        public ProjectBuilderProvider(LocalProcessCommandService host, ServiceReferential referential)
        {
            _referential = referential;
            _host = host;
            this._items = new Dictionary<string, ProjectBuilderContract>();
        }


        public void Initialize(string pathRoot)
        {

            _baseDirectory = pathRoot;

            if (!Directory.Exists(pathRoot))
                Directory.CreateDirectory(pathRoot);

            _root = Path.Combine(pathRoot, "Services");

            if (!Directory.Exists(_root))
                Directory.CreateDirectory(_root);

        }

        public ProjectBuilderContract Contract(string contract)
        {

            contract = contract.ToLower();

            if (!_items.TryGetValue(contract, out var builder))
                _items.Add(contract, builder = new ProjectBuilderContract(this, contract));

            return builder;
        }

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

        public bool ContractExists
        {
            get
            {
                return Directory.Exists(_root)
                    && File.Exists(Path.Combine(_root, "contract.json"))
                    ;
            }
        }

        public string Root => _root;

        internal readonly ServiceReferential _referential;
        internal readonly LocalProcessCommandService _host;
        private readonly Dictionary<string, ProjectBuilderContract> _items;
        private string _baseDirectory;
        private string _root;
        private static Dictionary<string, Type> _generators;
    }

}
