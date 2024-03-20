using Antlr4.Runtime.Misc;
using Bb.Builds;
using NLog.Filters;
using OpenTelemetry.Trace;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml.Linq;

namespace Bb.Services.Runnings
{


    public class EmbeddedService : IDisposable
    {


        /// <summary>
        /// Create a new instance of EmbeddedService
        /// </summary>
        /// <param name="assemblyPath">full path of the assembly to start</param>
        /// <param name="references">referential of the assemblies must be resolved for starting</param>
        /// <param name="typeName">type name to resolve</param>
        /// <param name="startingMethod">method to execute for starting the service</param>
        public EmbeddedService(string assemblyPath, AssemblyReferences references, string typeName, string startingMethod)
        {
            _filter = c => GetType(c, typeName);
            _startingMethod = startingMethod;
            _assemblyPath = assemblyPath;
            _references = references;
        }

        /// <summary>
        /// Start the service
        /// </summary>
        public virtual dynamic? Start()
        {
            _instance = StartService(null);
            return _instance;
        }

        /// <summary>
        /// Start the service
        /// </summary>
        protected dynamic? StartService(Action<dynamic>? action)
        {

            if (ExecuteAndUnload(_assemblyPath, out var assemblyWeakRef, out var type))
            {

                _typeWeakRef = type;
                _alcWeakRef = assemblyWeakRef;

                _instance = Execute(_startingMethod, [new string[] { }]);

                if (action != null && _instance != null)
                    action(_instance);

                return _instance;

            }

            return default;

        }


        /// <summary>
        /// Stop the service
        /// </summary>
        public virtual bool StopService()
        {

            bool result = false;

            if (_instance != null)
            {

                result = _instance.CancelAsync();

                if (result)
                {

                    _instance = null;
                    _typeWeakRef = null;

                    if (_alc != null)
                    {
                        _alc.Unload();
                        _alc = null;
                    }

                }

                return result;

            }

            return true;

        }


        void IDisposable.Dispose()
        {
            StopService();
        }



        /// <summary>
        /// return the current running instance
        /// </summary>
        protected dynamic? Instance { get => _instance;  }



        [MethodImpl(MethodImplOptions.NoInlining)]
        bool ExecuteAndUnload(string assemblyPath, out WeakReference assemblyWeakRef, out WeakReference typeWeakRef)
        {

            _alc = new EmbeddedAssemblyLoadContext(_references);
            assemblyWeakRef = new WeakReference(_alc);
            typeWeakRef = null;

            Assembly a = _alc.LoadFromAssemblyPath(assemblyPath);
            if (a == null)
            {
                Console.WriteLine("Loading the test assembly failed");
                return false;
            }

            Type type = _filter(a);
            typeWeakRef = new WeakReference(type);

            return true;

        }


        #region Starting method

        private static Type GetType(Assembly a, string name)
        {

            var type = a.ExportedTypes.FirstOrDefault(d => d.Name == name);

            if (type != null)
                return type;

            throw new Exception("Main method not found");

        }


        private MethodInfo GetMethod(string methodName)
        {

            var type = _typeWeakRef.Target as Type;

            if (type != null)
            {

                var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                if (method != null)
                    return method;

            }

            throw new Exception("Main method not found");

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="objects"></param>
        /// <returns></returns>
        private object Execute(string methodName, object[] objects)
        {
            var m = GetMethod(methodName);
            var result = m.Invoke(null, objects);
            return result;
        }

        #endregion Starting method


        private EmbeddedAssemblyLoadContext? _alc;
        private readonly Func<Assembly, Type> _filter;
        private readonly string _startingMethod;
        private readonly string _assemblyPath;
        private readonly AssemblyReferences _references;
        private WeakReference _alcWeakRef;
        private WeakReference? _typeWeakRef;
        private Task _task;
        private dynamic? _instance;

    }



}
