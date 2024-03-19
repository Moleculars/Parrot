using Bb.Extensions;
using System.Reflection;
using System.Runtime.InteropServices;
using NLog;
using NLog.Web;
using System.Collections;
using System.Diagnostics;
using Bb.OpenApiServices;
using System.ComponentModel.Design;
using Bb.ComponentModel;
using Microsoft.AspNetCore.Hosting;

namespace Bb
{
    public class ServiceRunnerBase
    {




        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceRunner{TStartup}"/> class.
        /// </summary>
        /// <param name="start">if set to <c>true</c> [start].</param>
        /// <param name="args">The arguments.</param>
        public ServiceRunnerBase(params string[] args)
        {
            _args = args;
            _tokenSource = new CancellationTokenSource();
            _token = _tokenSource.Token;
            Console.CancelKeyPress += Console_CancelKeyPress;

            InitializeByOs();

            Logger = InitializeLogger();

        }

        /// <summary>
        /// Gets the status of the service.
        /// </summary>
        public ServiceRunnerStatus Status { get; protected set; }


        /// <summary>
        /// Gets the exit code.
        /// </summary>
        /// <value>
        /// The exit code.
        /// </value>
        public int ExitCode { get; protected set; }

        /// <summary>
        /// Gets the build host.
        /// </summary>
        /// <value>
        /// The build.
        /// </value>
        public IHost Build { get; protected set; }

        /// <summary>
        /// Gets the addresses service listener.
        /// </summary>
        /// <value>
        /// The addresses.
        /// </value>
        public List<Uri> Addresses { get; protected set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public Logger Logger { get; }

        /// <summary>
        /// Gets the runner task.
        /// </summary>
        /// <value>
        /// The runner.
        /// </value>
        public Task Task => _task;



        protected virtual NLogAspNetCoreOptions ConfigureNlog()
        {
            return new NLogAspNetCoreOptions()
            {
                IncludeScopes = true,
                IncludeActivityIdsWithBeginScope = true,
            };
        }

        protected virtual void ConfigureLogging(ILoggingBuilder builder)
        {
            builder.ClearProviders()
           ;
        }

        protected virtual void SetConfiguration(Logger logger, WebHostBuilderContext hostingContext, IConfigurationBuilder config)
        {
            Configuration.UseSwagger = "use_swagger".EnvironmentVariableExists()
                                           ? "use_swagger".EnvironmentVariableIsTrue()
                                           : hostingContext.HostingEnvironment.IsDevelopment();

            Configuration.TraceAll = "trace_all".EnvironmentVariableExists()
                ? "trace_all".EnvironmentVariableIsTrue()
                : Configuration.TraceAll = hostingContext.HostingEnvironment.IsDevelopment();

            Configuration.UseTelemetry = "use_telemetry".EnvironmentVariableExists()
                 ? "use_telemetry".EnvironmentVariableIsTrue()
                 : Configuration.UseTelemetry = hostingContext.HostingEnvironment.IsDevelopment();

            // Load configurations files
            new ConfigurationLoader(logger, hostingContext, config)
             .TryToLoadConfigurationFile("appsettings.json", false, false)
             .TryToLoadConfigurationFile("apikeysettings.json", false, false)
             .TryToLoadConfigurationFile("policiessettings.json", false, false)
             ;

        }


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
        /// Runs asynchronous service
        /// </summary>
        /// <returns></returns>
        public Task RunAsync()
        {

            Status = ServiceRunnerStatus.Launching;

            var r = Task.Run(() =>
            {
                Run();
            }, _token);

            while (Status != ServiceRunnerStatus.Running)
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

            Status = ServiceRunnerStatus.Preparing;            

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
                
                Status = ServiceRunnerStatus.Running;

                _task?.Wait();

                ExitCode = 0;

                Status = ServiceRunnerStatus.Stopped;

            }
            catch (Exception exception)
            {
                Status = ServiceRunnerStatus.Stopped;
                ExitCode = exception.HResult;
                Logger.Error(exception, "Stopped program because of exception");

                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();

            }
            finally
            {
                Status = ServiceRunnerStatus.Stopped;
                NLog.LogManager.Shutdown();
                Environment.ExitCode = ExitCode;
            }

        }


        private IHostBuilder CreateHostBuilder(NLog.Logger logger, string[] args)
        {
            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {

                if (_urls != null)
                    webBuilder.UseUrls(_urls.ConcatUrl().ToString());

                webBuilder.ConfigureAppConfiguration((hostingContext, config) => SetConfiguration(logger, hostingContext, config));

                webBuilder.ConfigureLogging(l => ConfigureLogging(l))
                          .UseNLog(ConfigureNlog());

                TuneHostBuilder(webBuilder);

            });

            return hostBuilder;

        }

        protected virtual void TuneHostBuilder(IWebHostBuilder webBuilder)
        {
            
        }

        protected virtual Logger InitializeLogger()
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
            var configLogPath = Directory.GetCurrentDirectory().Combine("nlog.config");
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


        private async Task RunAsync(IHost host, CancellationToken token = default)
        {
            try
            {
                Status = ServiceRunnerStatus.Starting;
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
                Status = ServiceRunnerStatus.Stopping;
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

        private void EnumerateListeners()
        {

            var addresses = Build.GetServerAcceptedAddresses();
            foreach (var address in addresses)
            {
                Trace.TraceInformation($"address : {address}");
                Logger.Info($"address : {address}");
            }

            this.Addresses = addresses;

        }

        private static void InitializeByOs()
        {

            Console.WriteLine("Current directory : " + Directory.GetCurrentDirectory());

            var currentAssembly = Assembly.GetEntryAssembly();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(currentAssembly.Location));

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                //Configuration.CurrentDirectoryToWriteProjects = "c:\\".Combine("tmp", "parrot", "projects");
                Configuration.CurrentDirectoryToWriteGenerators = "c:\\".Combine("tmp", "parrot", "contracts");
                Configuration.TraceLogToWrite = "c:\\".Combine("tmp", "parrot", "logs");
            }

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                //Configuration.CurrentDirectoryToWriteProjects = "tmp".Combine("parrot", "projects");
                Configuration.CurrentDirectoryToWriteGenerators = "tmp".Combine("parrot", "contracts");
                Configuration.TraceLogToWrite = "tmp".Combine("parrot", "logs");
            }

            else
                throw new Exception($"Os {RuntimeInformation.OSDescription} not managed");

            //if (!Directory.Exists(Configuration.CurrentDirectoryToWriteProjects))
            //    Directory.CreateDirectory(Configuration.CurrentDirectoryToWriteProjects);

            if (!Directory.Exists(Configuration.CurrentDirectoryToWriteGenerators))
                Directory.CreateDirectory(Configuration.CurrentDirectoryToWriteGenerators);

            if (!Directory.Exists(Configuration.TraceLogToWrite))
                Directory.CreateDirectory(Configuration.TraceLogToWrite);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Configuration.TraceLogToWrite += "\\";

            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Configuration.TraceLogToWrite += "/";

            //Trace.TraceInformation("setting directory to generate projects in : " + Configuration.CurrentDirectoryToWriteProjects);
            Trace.TraceInformation("setting directory to output logs : " + Configuration.TraceLogToWrite);

        }

        private void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            Cancel();
        }


        private Task _task;
        private Exception _exception;
        protected List<Url> _urls;
        private readonly string[] _args;
        private readonly CancellationTokenSource _tokenSource;
        private readonly CancellationToken _token;

    }

}
