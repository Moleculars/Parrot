using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using Microsoft.OpenApi.Validations;
using System.Diagnostics;
using System.Reflection;

namespace Bb.Services.Managers
{


    public class PluginManager<TPlugin>
        where TPlugin : class //, IPlugin
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
        /// Type to discovering
        /// </summary>
        public Type Type { get => typeof(TPlugin); }

        /// <summary>
        /// Discovers the plugIn's list.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Type> DiscoverPlugInList()
        {

            var typeReference = Type;
            HashSet<Assembly> assemblies = new HashSet<Assembly>();
            var instance = AssemblyDirectoryResolver.Instance;

            var dir = TypeDiscoveryExtension.AspNetDirectory?.FullName;
            var dir2 = AssemblyDirectoryResolver.SystemDirectory.FullName;
            var dirs = AssemblyDirectoryResolver.Instance.GetDirectories().Where(c => c.FullName != dir && c.FullName != dir2).ToList();

            Trace.TraceInformation($"DiscoverPlugInList is looking in {dirs.Count} directories");
            foreach (var folder in dirs)
                if (folder.Exists)
                    foreach (var item in folder.GetAssembliesFromFolder(Evaluate).Where(c => c != null).ToList())
                    {
                        assemblies.Add(item);
                        Trace.TraceInformation($"Append assembly '{item.FullName}'");
                    }
                else
                    Trace.TraceInformation($"Directory '{folder.FullName}' don't exists.");


            foreach (var assembly in assemblies)
                foreach (var typeitem in assembly.GetTypes().GetTypesWithAttributes<ExposeClassAttribute>(null, c => c.Context == Constants.Models.Plugin))
                    if (typeReference.IsAssignableFrom(typeitem))
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
