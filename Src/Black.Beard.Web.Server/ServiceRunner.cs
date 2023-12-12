using Bb.Extensions;
using System.Reflection;
using System.Runtime.InteropServices;
using NLog;
using NLog.Web;
using Bb.Services;
using System.Collections;
using System.Diagnostics;
using Bb.OpenApiServices;
using System.Text;
using Flurl;
using Microsoft.AspNetCore.Hosting;
using Bb.Flurl;

namespace Bb
{

    /// <summary>
    /// ServiceRunner
    /// </summary>
    public class ServiceRunner<TStartup>
        : IDisposable
        where TStartup : class
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRunner{TStartup}"/> class.
        /// </summary>
        /// <param name="start">if set to <c>true</c> [start].</param>
        /// <param name="args">The arguments.</param>
        public ServiceRunner(params string[] args)
        {

            _args = args;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            Console.CancelKeyPress += Console_CancelKeyPress;

            InitializeByOs();

            Logger = InitializeLogger();             

        }

        /// <summary>
        /// Adds an <see cref="Url"/> on the list of listeners.
        /// </summary>
        /// <param name="scheme">The scheme protocol.</param>
        /// <param name="startPorts">The starting range port</param>
        /// <param name="count">The count range port</param>
        /// <returns></returns>
        public ServiceRunner<TStartup> AddLocalhostUrl(string scheme, ref int startPorts, int count = 1)
        {

            int _first = startPorts;

            List<string> ports = new List<string>();
            for (int i = 0; i < count; i++)
                AddLocalhostUrl(scheme, _first = (HttpHelper.GetAvailablePort(_first)) + 1);

            startPorts = _first;

            return this;

        }

        /// <summary>
        /// Adds an <see cref="Url"/> on the list of listeners.
        /// </summary>
        /// <param name="scheme">The scheme protocol.</param>
        /// <param name="hostname">The hostname to listen</param>
        /// <param name="port">The port to listen.</param>
        /// <returns></returns>
        public ServiceRunner<TStartup> AddUrl(string scheme, string hostname, int port)
        {

            if (_urls == null)
                _urls = new List<Url>();

            this._urls.Add(new Url(scheme, hostname, port));

            return this;
        }

        /// <summary>
        /// Adds an <see cref="Url"/> on the list of listeners.
        /// </summary>
        /// <param name="scheme">The scheme protocol.</param>
        /// <param name="port">The port to listen</param>
        /// <returns></returns>
        public ServiceRunner<TStartup> AddLocalhostUrl(string scheme, int port)
        {
            AddUrl(scheme, "localhost", port);
            return this;
        }

        /// <summary>
        /// Adds an <see cref="Url"/> on the list of listeners.
        /// </summary>
        /// <param name="scheme">The scheme protocol.</param>
        /// <param name="startingport">starting port to search.</param>
        /// <returns></returns>
        public ServiceRunner<TStartup> AddLocalhostUrlWithDynamicPort(string scheme, ref int startingport)
        {

            if (_urls == null)
                _urls = new List<Url>();

            var ports = _urls.Select(c => c.Port).OrderBy(c => c).ToList();

            startingport = HttpHelper.GetAvailablePort(startingport);

            while (ports.Contains(startingport))
            {
                startingport++;
                startingport = HttpHelper.GetAvailablePort(startingport);
            }

            AddUrl(scheme, "localhost", startingport);
            return this;
        }

        /// <summary>
        /// Runs asynchronous service
        /// </summary>
        /// <returns></returns>
        public Task RunAsync()
        {

            this._running = false;

            var r = Task.Run(() =>
            {
                Run();
            }, _token);

            while (!this._running)
            {
                Task.Yield();
            }

            return r;

        }

        /// <summary>
        /// Runs this instance and wait closing.
        /// </summary>
        public void Run()
        {

            this._running = false;

            this.Build
                = CreateHostBuilder(Logger, _args)
                 .Build()
                ;

            try
            {

                this._task = RunAsync(Build);

                if (this._exception != null)
                    throw this._exception;

                EnumerateListeners();

                _running = true;
                _task?.Wait();

                ExitCode = 0;
            }
            catch (Exception exception)
            {

                ExitCode = exception.HResult;
                Logger.Error(exception, "Stopped program because of exception");

                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();

            }
            finally
            {
                NLog.LogManager.Shutdown();
                Environment.ExitCode = ExitCode;
            }

        }

        private async Task RunAsync(IHost host, CancellationToken token = default)
        {
            try
            {
                await host.StartAsync(token).ConfigureAwait(false);
                await host.WaitForShutdownAsync(token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.Logger.Error(e);
                this._exception = e;
            }
            finally
            {
                if (host is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else
                {
                    host.Dispose();
                }
            }
        }

        private static Logger InitializeLogger()
        {

            // target folder where write
            NLog.GlobalDiagnosticsContext.Set("parrot_log_directory", Configuration.TraceLogToWrite);

            // push environment variables in the log
            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
                if (item.Key != null
                    && !string.IsNullOrEmpty(item.Key.ToString())
                    && item.Key.ToString().StartsWith("parrot_log_"))
                    NLog.GlobalDiagnosticsContext.Set(item.Key.ToString(), item.Value?.ToString());

            // load the configuration file
            var configLogPath = Path.Combine(Directory.GetCurrentDirectory(), "nlog.config");
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configLogPath);

            // Initialize log
            var logger = NLog.LogManager
                .Setup()
                .SetupExtensions(s => { })
                .GetCurrentClassLogger()
                ;

            logger.Debug("log initialized");

            return logger;
        }

        private void EnumerateListeners()
        {

            var addresses = Build.GetServerAcceptedAddresses();
            foreach (var address in addresses)
            {
                Trace.WriteLine($"address : {address}");
                Logger.Info($"address : {address}");
            }

            this.Addresses = addresses;

        }

        //private static void TestService(Logger logger, IHost build)
        //{
        //    var addresses = build.GetServerAcceptedAddresses();
        //    foreach (var address in addresses)
        //    {
        //        if (string.IsNullOrEmpty(address.Host))
        //            logger.Error($"{address} can't be tested, because host is not specified.");
        //        else
        //        {
        //            var url = new Url(address).AppendPathSegments("watchdog", "isupandrunning");
        //            var urlTxt = url.ToString();
        //            try
        //            {
        //                var oo = url.SendAsync(HttpMethod.Get).GetAwaiter();
        //                var pp = oo.GetResult();
        //                if (pp.StatusCode == 200)
        //                    logger.Info($"{urlTxt} is listening");
        //                else
        //                    logger.Error($"{urlTxt} is not listening");
        //            }
        //            catch (Exception)
        //            {
        //            }
        //        }
        //    }
        //}

        private static void InitializeByOs()
        {

            Console.WriteLine("Current directory : " + Directory.GetCurrentDirectory());

            var currentAssembly = Assembly.GetEntryAssembly();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(currentAssembly.Location));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Configuration.CurrentDirectoryToWriteProjects = Path.Combine("c:\\", "tmp", "parrot", "projects");
                Configuration.TraceLogToWrite = Path.Combine("c:\\", "tmp", "parrot", "logs");
            }

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Configuration.CurrentDirectoryToWriteProjects = Path.Combine("tmp", "parrot", "projects");
                Configuration.TraceLogToWrite = Path.Combine("tmp", "parrot", "logs");
            }

            else
                throw new Exception($"Os {RuntimeInformation.OSDescription} not managed");

            if (!Directory.Exists(Configuration.CurrentDirectoryToWriteProjects))
                Directory.CreateDirectory(Configuration.CurrentDirectoryToWriteProjects);

            if (!Directory.Exists(Configuration.TraceLogToWrite))
                Directory.CreateDirectory(Configuration.TraceLogToWrite);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Configuration.TraceLogToWrite += "\\";
            }

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Configuration.TraceLogToWrite += "/";
            }

            Console.WriteLine("setting directory to generate projects in : " + Configuration.CurrentDirectoryToWriteProjects);
            Console.WriteLine("setting directory to output logs : " + Configuration.TraceLogToWrite);

        }

        /// <summary>
        /// Gets the exit code.
        /// </summary>
        /// <value>
        /// The exit code.
        /// </value>
        public int ExitCode { get; private set; }

        /// <summary>
        /// Gets the build host.
        /// </summary>
        /// <value>
        /// The build.
        /// </value>
        public IHost Build { get; private set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public Logger Logger { get; }

        /// <summary>
        /// Gets the addresses service listener.
        /// </summary>
        /// <value>
        /// The addresses.
        /// </value>
        public List<Uri> Addresses { get; private set; }

        internal IHostBuilder CreateHostBuilder(NLog.Logger logger, string[] args) =>
               Host.CreateDefaultBuilder(args)
                   .ConfigureWebHostDefaults(webBuilder =>
                   {

                       webBuilder.UseStartup<TStartup>();

                       if (_urls != null)
                           webBuilder.UseUrls(_urls.ConcatUrl().ToString());

                       // 
                       webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                       {

                           Configuration.UseSwagger = "use_swagger".EnvironmentVariableExists()
                               ? "use_swagger".EnvironmentVariableIsTrue()
                               : hostingContext.HostingEnvironment.IsDevelopment();

                           Configuration.TraceAll = "trace_all".EnvironmentVariableExists()
                               ? "trace_all".EnvironmentVariableIsTrue()
                               : Configuration.TraceAll = hostingContext.HostingEnvironment.IsDevelopment();

                           // Load configurations files
                           new ConfigurationLoader(logger, hostingContext, config)
                            .TryToLoadConfigurationFile("appsettings.json", false, false)
                            .TryToLoadConfigurationFile("apikeysettings.json", false, false)
                            .TryToLoadConfigurationFile("policiessettings.json", false, false)
                            ;

                       });

                       webBuilder.ConfigureLogging(l =>
                       {
                           l.ClearProviders()
                           ;
                       })
                       .UseNLog(new NLogAspNetCoreOptions()
                       {
                           IncludeScopes = true,
                           IncludeActivityIdsWithBeginScope = true,
                       });
                   });

        /// <summary>
        /// Cancels the running instance.
        /// </summary>
        public void Cancel()
        {

            Build.StopAsync().Wait(_token);

            try
            {
                if (_task != null
                && _tokenSource != null
                && _tokenSource.Token.CanBeCanceled)
                    _tokenSource.Cancel();
            }
            catch (Exception)
            {

                throw;
            }


        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Cancel();
            Console.CancelKeyPress -= Console_CancelKeyPress;
            _tokenSource?.Dispose();
        }

        private void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            Cancel();
        }

        /// <summary>
        /// Waits for the <see cref="Task"/> to complete execution.
        /// </summary>
        /// <exception cref="System.AggregateException">
        /// The <see cref="Task"/> was canceled -or- an exception was thrown during
        /// the execution of the <see cref="Task"/>.
        /// </exception>
        public void Wait()
        {
            _task?.Wait(_token);
        }

        /// <summary>
        /// Waits for the <see cref="Task"/> to complete execution.
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// The number of milliseconds to wait, or <see cref="System.Threading.Timeout.Infinite"/> (-1) to
        /// wait indefinitely.</param>
        /// <returns>true if the <see cref="Task"/> completed execution within the allotted time; otherwise,
        /// false.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="millisecondsTimeout"/> is a negative number other than -1, which represents an
        /// infinite time-out.
        /// </exception>
        /// <exception cref="System.AggregateException">
        /// The <see cref="Task"/> was canceled -or- an exception was thrown during the execution of the <see
        /// cref="Task"/>.
        /// </exception>
        public void Wait(int millisecondsTimeout)
        {
            _task?.Wait(millisecondsTimeout);
        }

        /// <summary>
        /// Gets the runner task.
        /// </summary>
        /// <value>
        /// The runner.
        /// </value>
        public Task Task => _task;

        private Task _task;
        private Exception _exception;
        private List<Url> _urls;
        private volatile bool _running;
        private readonly string[] _args;
        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _token;
    }

}
