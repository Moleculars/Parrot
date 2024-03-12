using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using Microsoft.OpenApi.Validations;
using System.Reflection;

namespace Bb.Services.Managers
{


    public static class TypeDiscoveryExtension
    {

        static TypeDiscoveryExtension()
        {

            TypeDiscoveryExtension._AspNetDirectory = new FileInfo(typeof(Microsoft.AspNetCore.Antiforgery.AntiforgeryTokenSet).Assembly.Location).Directory;

        }

        /// <summary>
        /// return the list of type contains in the folders registered.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="folders">the folders</param>
        /// <returns></returns>
        public static IEnumerable<Type> InFolder(this IEnumerable<Type> types, Bb.ComponentModel.AssemblyDirectoryResolver folders)
        {
            return types.InFolder(new HashSet<string>(folders.GetPaths()));
        }

        /// <summary>
        /// return the list of type contains in the specified folders.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <param name="folders">The folders.</param>
        /// <returns></returns>
        public static IEnumerable<Type> InFolder(this IEnumerable<Type> types, HashSet<string> folders)
        {
            foreach (var item in types)
                if (item.InFolder(folders))
                    yield return item;
        }

        /// <summary>
        /// return <c>true</c> if the type is contained in the specified folders.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="folders">The folders.</param>
        /// <returns></returns>
        public static bool InFolder(this Type type, HashSet<string> folders)
        {
            var dir = new FileInfo(type.Assembly.Location).Directory;
            return folders.Contains(dir.FullName);
        }

        /// <summary>
        /// if the list of type contains synonym, only the last version is keep.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetLastVersion(this IEnumerable<Type> self)
        {

            Dictionary<string, Type> items = new Dictionary<string, Type>();
            foreach (var item in self)
            {
                var assemblyName = item.Assembly.GetName();
                if (!string.IsNullOrEmpty(assemblyName.Name))
                {
                    if (items.TryGetValue(assemblyName.Name, out Type type))
                    {
                        if (assemblyName.Version > item.Assembly.GetName().Version)
                            items[assemblyName.Name] = item;
                    }
                    else
                        items.Add(assemblyName.Name, item);
                }
            }

            return items.Values;

        }


        /// <summary>
        /// if the list of type contains synonym, use specified func to choose the type to keep.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <returns></returns>
        public static IEnumerable<Type> FilterDuplicated(this IEnumerable<Type> self, Func<Type, Type, Type> func)
        {

            Dictionary<string, Type> items = new Dictionary<string, Type>();
            foreach (var item in self)
            {
                var assemblyName = item.Assembly.GetName();
                var name = assemblyName.Name;
                if (!string.IsNullOrEmpty(name))
                {
                    if (items.TryGetValue(name, out Type currentType))
                    {

                        var typeToKeep = func(item, currentType);

                        if (typeToKeep != null && typeToKeep != currentType)
                            items[name] = typeToKeep;

                    }
                    else
                        items.Add(name, item);
                }
            }

            return items.Values;

        }

        internal static readonly DirectoryInfo? _AspNetDirectory;

    }



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

            string directoryPath = Path.Combine(_root.FullName, pluginName);

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
