using Bb.Builds;
using System;
using System.Reflection;
using static Refs.System.Private;

namespace Bb.Services.Runnings
{

    /// <summary>
    /// wrapper for run <see cref="ServiceRunner{TStartup}"/>"/>
    /// </summary>
    public class EmbeddedWebService : EmbeddedService
    {


        static EmbeddedWebService()
        {

            _names = new HashSet<string>()
            {
                ServiceRunnerStatus.Stopped.ToString(),
                ServiceRunnerStatus.Launching.ToString(),
                ServiceRunnerStatus.Preparing.ToString(),
                ServiceRunnerStatus.Starting.ToString(),
                //ServiceRunnerStatus.Running.ToString(),
                //ServiceRunnerStatus.Stopping.ToString(),
            };
        }

        /// <summary>
        /// initializes a new instance of EmbeddedWebService
        /// </summary>
        /// <param name="assemblyPath"></param>
        /// <param name="references"></param>
        /// <param name="scheme"></param>
        /// <param name="startpPort"></param>
        public EmbeddedWebService(string assemblyPath, AssemblyReferences references, params (string, string, int)[] listeners)
            : base(assemblyPath, references, "Program", "GetService")
        {
            this._listeners = listeners;
            _dynamiclisteners = new List<(string, string, int, int)>(listeners.Length);
        }


        /// <summary>
        /// Start the service
        /// </summary>
        /// <returns></returns>
        public override dynamic? Start()
        {

            dynamic? instance = base.StartService(c =>
            {

                bool running = false;
                var _task = Task.Run(() =>
                {
                    running = true;


                    // apply the listeners
                    foreach (var l in _listeners)
                    {
                        var port = l.Item3;
                        c.AddLocalhostUrlWithDynamicPort(l.Item1, l.Item2, ref port);
                        _dynamiclisteners.Add((l.Item1, l.Item2, l.Item3, port));
                    }

                    var s = c.RunAsync();

                    s.Wait();

                    instance = null;

                });

                // wait task running.
                while (!running) Task.Yield();

            });

            if (instance != null)
            {

                // wait the service running or fail.
                var timeOut = DateTime.Now.AddMinutes(1);
                while (_names.Contains(instance.Status.ToString()) && timeOut > DateTime.Now) Task.Yield();

            }

            return instance;

        }

        /// <summary>
        /// Return the list from Uri accepted by the server.
        /// </summary>
        public List<(string, string, int, int)> Uris => _dynamiclisteners;

        private static readonly HashSet<string> _names;
        private readonly (string, string, int)[] _listeners;
        private readonly List<(string, string, int, int)> _dynamiclisteners;


    }



}
