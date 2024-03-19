using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using Microsoft.OpenApi.Validations;
using System.Reflection;

namespace Bb.Services.Managers
{


    public class PluginManager<T>
        where T : class //, IPlugin
    {


        public PluginManager(params string[] itemToExcludes)
        {
            _typeDiscovery = TypeDiscovery.Instance;
            _names = new HashSet<string>(itemToExcludes);
        }

        /// <summary>
        /// Initializes the <see cref="ProjectBuilderProvider" />.
        /// </summary>
        /// <param name="pathRoot">The path root where the contracts will be generate.</param>
        public virtual void Initialize(string pathRoot)
        {

            if (!Directory.Exists(pathRoot))
                Directory.CreateDirectory(pathRoot);

            _root = new DirectoryInfo(pathRoot);

        }

        private void RefreshPluginDirectories()
        {
            _root.Refresh();
            var dirs = _root.GetDirectories()?.ToArray();
            _typeDiscovery.AddDirectories(dirs);
        }

        public DirectoryInfo GetPlugInDirectory(string pluginName)
        {

            string directoryPath = _root.Combine(pluginName);

            var directoryTarget = new DirectoryInfo(directoryPath);
            directoryTarget.Refresh();

            if (!directoryTarget.Exists)
                directoryTarget.Create();

            return directoryTarget;

        }

        /// <summary>
        /// Discovers the plugIn's list.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Type> DiscoverPlugInList()
        {

            var instance = AssemblyDirectoryResolver.Instance;

            var typeReference = typeof(T);
            var dir = TypeDiscoveryExtension._AspNetDirectory.FullName;
            var dir2 = AssemblyDirectoryResolver.SystemDirectory.FullName;
            var dirs = AssemblyDirectoryResolver.Instance.GetDirectories().Where(c => c.FullName != dir && c.FullName != dir2).ToList();

            HashSet<Assembly> assemblies = new HashSet<Assembly>();
            foreach (var folder in dirs)
                foreach (var item in folder.GetAssembliesFromFolder(c => Evaluate(c)).Where(c => c != null).ToList())
                    assemblies.Add(item);

            foreach (var assembly in assemblies)
                foreach (var typeitem in assembly.GetTypes().GetTypesWithAttributes<ExposeClassAttribute>(null, c => c.Context == Constants.Models.Plugin))
                    yield return typeitem;

        }

        bool Evaluate(FileInfo file)
        {

            foreach (var item in _names)
                if (file.Name.StartsWith(item))
                    return false;

            return true;

        }


        private readonly TypeDiscovery _typeDiscovery;
        private readonly HashSet<string> _names;
        private DirectoryInfo _root;

    }




}
