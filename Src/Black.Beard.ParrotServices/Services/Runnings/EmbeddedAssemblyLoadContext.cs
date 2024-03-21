using Bb.Builds;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;

namespace Bb.Services.Runnings
{
 
    /// <summary>
    /// EmbeddedAssemblyLoadContext
    /// </summary>
    class EmbeddedAssemblyLoadContext : AssemblyLoadContext
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedAssemblyLoadContext"/> class.
        /// </summary>
        /// <param name="references"></param>
        public EmbeddedAssemblyLoadContext(AssemblyReferences references)
            : base(isCollectible: true)
        {
            _references = references;
            //_resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);
        }

        /// <summary>
        /// Resolve assembly
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override Assembly? Load(AssemblyName name)
        {

            Assembly result = null;

            var reference = _references.ResolveAssemblyName(name.Name, (c, d) =>
            {
                return d.FirstOrDefault();
            });

            if (reference != null)
                result = LoadFromAssemblyPath(reference.Location);

            if (result == null)
                Trace.TraceWarning($"Assembly {name.Name} not resolved");

            return result;
        }

        private readonly AssemblyReferences _references;

    }



}
