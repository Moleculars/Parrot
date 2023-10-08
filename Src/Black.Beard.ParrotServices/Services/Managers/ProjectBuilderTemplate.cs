using Bb.Process;
using Bb.OpenApiServices;
using Newtonsoft.Json;
using Flurl;
using Bb.Models;
using Flurl.Http;
using Bb.Mock;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using Bb.Services.ProcessHosting;
using Bb.ParrotServices.Exceptions;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Bb.Services.Managers
{


    /// <summary>
    /// manipulate templates
    /// </summary>
    public class ProjectBuilderTemplate
    {

        /// <summary>
        /// Initializes the <see cref="ProjectBuilderTemplate"/> class.
        /// </summary>
        static ProjectBuilderTemplate()
        {

            jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectBuilderTemplate"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="template">The template.</param>
        /// <exception cref="Bb.ParrotServices.Exceptions.MockHttpException">template {template} not found</exception>
        public ProjectBuilderTemplate(ProjectBuilderContract parent, string template)
        {
            _parent = parent;
            _rootParent = parent.Parent;
            _logger = _rootParent._logger;

            Contract = _parent.Contract;
            Root = Path.Combine(parent.Root, template);

            
            Template = template;
            _generatorType = _rootParent.ResolveGenerator(Template);
            if (_generatorType == null)
                throw new MockHttpException($"template {template} not found");
            _templateConfigFilename = Path.Combine(Root, template + ".json");
            var instance = GetGenerator();


            _configurationType = instance.ConfigurationType;
            _defaultConfig = JsonConvert.SerializeObject(instance.GetConfiguration(), jsonSerializerSettings);

        }

        /// <summary>
        /// Gets a path for specified name.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public string GetPath(string filename)
        {
            var filepath = Path.Combine(Root, filename);
            return filepath;
        }

        ///// <summary>
        ///// return  the path.
        ///// </summary>
        ///// <param name="path">The path.</param>
        ///// <returns></returns>
        //public string GetPath(string[] path)
        //{
        //    var filepath = Root;
        //    foreach (var item in path)
        //        filepath = Path.Combine(filepath, item);
        //    return filepath;
        //}

        #region Config

        /// <summary>
        /// Sets the configuration for the template.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public ProjectBuilderTemplate SetConfig(Action<object> action)
        {

            var config = GetConfig(out var control);
            action(config);
            WriteConfig(config, control);
            return this;
        }

        /// <summary>
        /// Writes the configuration in the referential.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="control">The control.</param>
        private void WriteConfig(object config, string control)
        {
            var configTxt = JsonConvert.SerializeObject(config, jsonSerializerSettings);
            if (configTxt != control)
                _templateConfigFilename.Save(configTxt);
        }

        /// <summary>
        /// return the configurations for manipulate the generator template.
        /// </summary>
        /// <returns></returns>
        public object Config()
        {
            return GetConfig(out _);
        }

        private object GetConfig(out string control)
        {

            if (File.Exists(_templateConfigFilename))
                control = _templateConfigFilename.LoadFromFile();

            else
                control = _defaultConfig;

            return JsonConvert.DeserializeObject(control, _configurationType, jsonSerializerSettings);

        }

        #endregion Config

        /// <summary>
        /// Writes the document on disk.
        /// </summary>
        /// <param name="upfile">The upfile.</param>
        /// <param name="filepath">The filepath.</param>
        public void WriteOnDisk(IFormFile upfile, string filepath)
        {

            if (!Directory.Exists(Root))
                Directory.CreateDirectory(Root);

            using (var stream = new FileStream(filepath, FileMode.Create))
                upfile.CopyTo(stream);

        }

        /// <summary>
        /// Generate project
        /// </summary>
        /// <param name="fileContract">the file where the contract is located</param>
        /// <returns>if the generator can't be resolve, the result is null.</returns>
        public ProjectDocument? GenerateProject(string fileContract)
        {

            ProjectDocument result = null;
            var config = Config();
            var generator = GetGenerator();

            if (generator != null)
            {

                generator.ApplyConfiguration(config);

                ContextGenerator ctx = generator
                    .Initialize(Contract, Template, Root)
                    .InitializeDataSources(fileContract)
                    .Generate();

                result = List(ctx);

            }

            return result;

        }

        /// <summary>
        /// test if the template was generated for the current template.
        /// </summary>
        /// <returns></returns>
        public bool Exists()
        {
            var dir = new DirectoryInfo(Root);
            dir.Refresh();
            return dir.Exists;
        }

        /// <summary>
        /// Build the project for the specified contract
        /// </summary>
        public async Task<int?> Build()
        {

            var projectFile = GetFileProject();
            int? exitResult = 0;
            if (projectFile != null)
            {
                using (var cmd = new ProcessCommand()
                         .Command($"dotnet.exe", $"build \"{projectFile.FullName}\" -c release /p:Version=1.0.0.0")
                         .Intercept(InterceptTraces)
                         .Run())
                {
                    cmd.Wait();
                    exitResult = cmd.ExitCode;
                }

                return exitResult;

            }

            return 1;

        }

        private void InterceptTraces(object sender, TaskEventArgs args)
        {

            var id = args.Process.Id;

            switch (args.Status)
            {

                case TaskEventEnum.Started:

                    _logger.LogInformation("Started process {id}", id);
                    break;

                case TaskEventEnum.ErrorReceived:
                    _logger.LogError("{id} : " + Format(args?.DateReceived?.Data), id);
                    break;

                case TaskEventEnum.DataReceived:
                    _logger.LogInformation("{id} : " + Format(args?.DateReceived?.Data), id);
                    break;

                case TaskEventEnum.Completed:
                    var instance = args.Process.Tag as ServiceReferentialContract;
                    if (instance != null)
                    {
                        _rootParent._referential.Remove(instance);
                        _logger.LogInformation($"{instance.Parent.Template}/{instance.Contract} {id} is ended", id);
                    }
                    else
                        _logger.LogInformation("{id} is Completed", id);
                    Running = null;
                    _id = null;
                    break;

                case TaskEventEnum.CompletedWithException:
                    var instance1 = args.Process.Tag as ServiceReferentialContract;
                    if (instance1 != null)
                    {
                        _rootParent._referential.Remove(instance1);
                        _logger.LogInformation($"{instance1.Parent.Template}/{instance1.Contract} ended with exception", "Error");
                    }
                    else
                        _logger.LogInformation("ended with exception", "Error");
                    Running = null;
                    _id = null;
                    break;

                default:
                    break;

            }

        }

        private static string Format(string? data)
        {
            if (!string.IsNullOrEmpty(data))
                return data.Replace("{", "'").Replace("}", "'");

            return string.Empty;

        }

        /// <summary>
        /// Run the contract
        /// </summary>
        public async Task<ProjectItem> Run(string publicHost, int? httpCurrentPort, int? httpsCurrentPort)
        {

            var projectFile = GetFileProject();

            if (projectFile == null)
                return null;

            string internalHost = "localhost";


            Uri? uriHttp = null;
            Uri? uriHttps = null;

            if (httpCurrentPort.HasValue)
                uriHttp = HttpHelper.GetUri(false, internalHost, HttpHelper.GetAvailablePort(httpCurrentPort.Value));

            if (httpsCurrentPort.HasValue)
                uriHttps = HttpHelper.GetUri(true, internalHost, HttpHelper.GetAvailablePort(httpsCurrentPort.Value));

            StringBuilder urls = new StringBuilder();

            urls.Append("\"");
            if (uriHttp != null)
                urls.Append(uriHttp.ToString().Trim('/'));

            if (uriHttps != null)
            {
                if (urls.Length > 1)
                    urls.Append(";");
                urls.Append(uriHttps.ToString().Trim('/'));
            }
            urls.Append("\"");

            var workingDirectory = projectFile.Directory?.FullName;

            if (workingDirectory == null)
            {

                return null;
            }

            _id = Guid.NewGuid();

            var instance = _rootParent._referential.Register(Template, _parent.Contract, uriHttp, uriHttps);

            _rootParent._host
                .Intercept(InterceptTraces)
                .Run(_id.Value, c =>
                {
                    c.Command($"dotnet.exe")
                     .SetWorkingDirectory(workingDirectory)
                     .Arguments($"run \"{projectFile.Name}\" --urls {urls.ToString()}") // -c Development 
                     ;
                }, instance)
                //.Wait(id, 500)
                ;

            var task = _rootParent._host.GetTask(_id.Value);
            task.Wait(1500);

            if (task.ExitCode > 0)
            {
                throw new Exception("Service can't start");
            }

            Running = new ProjectItem()
            {
                Contract = _parent.Contract,
                Template = Template,
            };

            if (httpCurrentPort.HasValue)
            {
                Running.Services.Http.ExposedReverseProxyUrl = new Url("http", publicHost, httpCurrentPort.Value, "proxy", Template, _parent.Contract);
                Running.Swagger.Http.ExposedReverseProxyUrl = new Url("http", publicHost, httpCurrentPort.Value, "proxy", Template, _parent.Contract, "swagger");
                Running.IsUpAndRunningServices.Http.ExposedReverseProxyUrl = new Url("http", publicHost, httpCurrentPort.Value, "proxy", Template, _parent.Contract, "Watchdog", "isupandrunning");
            }

            if (httpsCurrentPort.HasValue)
            {
                Running.Services.Https.ExposedReverseProxyUrl = new Url("https", publicHost, httpsCurrentPort.Value, "proxy", Template, _parent.Contract, "swagger");
                Running.Swagger.Https.ExposedReverseProxyUrl = new Url("https", publicHost, httpsCurrentPort.Value, "proxy", Template, _parent.Contract, "swagger");
                Running.IsUpAndRunningServices.Https.ExposedReverseProxyUrl = new Url("https", publicHost, httpsCurrentPort.Value, "proxy", Template, _parent.Contract, "Watchdog", "isupandrunning");
            }

            if (uriHttp != null)
                Running.Services.Http.HostedInternalServiceUrl = new Url(uriHttp).AppendPathSegments("proxy", Template, _parent.Contract);
            if (uriHttps != null)
                Running.Services.Https.HostedInternalServiceUrl = new Url(uriHttps).AppendPathSegments("proxy", Template, _parent.Contract, "swagger");


            if (uriHttp != null)
                Running.Swagger.Http.HostedInternalServiceUrl = new Url(uriHttp).AppendPathSegments("proxy", Template, _parent.Contract, "swagger");
            if (uriHttps != null)
                Running.Swagger.Https.HostedInternalServiceUrl = new Url(uriHttps).AppendPathSegments("proxy", Template, _parent.Contract, "swagger");


            if (uriHttp != null)
                Running.IsUpAndRunningServices.Http.HostedInternalServiceUrl = new Url(uriHttp).AppendPathSegments("proxy", Template, _parent.Contract, "Watchdog", "isupandrunning");
            if (uriHttps != null)
                Running.IsUpAndRunningServices.Https.HostedInternalServiceUrl = new Url(uriHttps).AppendPathSegments("proxy", Template, _parent.Contract, "Watchdog", "isupandrunning");

            try
            {
                var serviceResult = await Running.IsUpAndRunningServices.Https.HostedInternalServiceUrl.GetJsonAsync<WatchdogResult>();
                if (serviceResult != null)
                    Running.Started = true;
            }
            catch (Exception)
            {

            }

            return Running;

        }


        /// <summary>
        /// Kills the process.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Kill()
        {

            bool result = false;

            var instance = _rootParent._referential.Resolve(Template, _parent.Contract);
            if (instance != null)
            {

                var task = _rootParent._host.GetTaskByTag(instance).FirstOrDefault();
                if (task != null)
                {

                    task.Intercept((c, args) =>
                    {
                        if (args.Status == TaskEventEnum.Completed)
                            result = true;
                        if (args.Status == TaskEventEnum.CompletedWithException)
                            result = true;
                    });

                    task.Cancel();

                    task.Wait(200);

                }

                result = true;

            }
            else
                result = true;

            return result;

        }

        private FileInfo? GetFileProject()
        {

            var directory = new DirectoryInfo(Root);
            if (directory.Exists)
            {
                var files = directory.GetFiles("*.csproj", SearchOption.AllDirectories);
                var _projectFile = files.FirstOrDefault();
                return _projectFile;
            }

            return null;

        }


        internal string GetDirectoryProject(params string[] path)
        {

            var dir = Path.Combine(Root, "service");

            foreach (var item in path)
                dir = Path.Combine(dir, item);

            return dir;

        }

        internal FileInfo[] GetFiles(string path, string pattern)
        {

            var dir = new DirectoryInfo(path);
            dir.Refresh();

            FileInfo[] files = dir.GetFiles(pattern);

            return files;

        }

        /// <summary>
        /// return the list of document in the project.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <returns></returns>
        public ProjectDocument List(ContextGenerator ctx)
        {

            string rootPath = ctx.TargetPath;
            ProjectDocument result = new ProjectDocument()
            {
                Contract = _parent.Contract,
                Template = Template,
                Context = ctx

            };

            var dirRoot = new DirectoryInfo(Path.Combine(rootPath, "Templates"));
            _logger.LogDebug($"ProjectBuilderTemplate.List : dirRoot = {dirRoot}");
            _logger.LogDebug($"ProjectBuilderTemplate.List : Root = {Root}");
            if (dirRoot.Exists)
            {
                var files = dirRoot.GetFiles("*.json");
                foreach (var item in files)
                {
                    _logger.LogDebug($"ProjectBuilderTemplate.List : file = {item.FullName}");
                    var target = new Uri(item.FullName);
                    var relative = new Uri(rootPath);
                    relative = relative.MakeRelativeUri(target);
                    result.Documents.Add(new Document() { Kind = "jslt", File = relative.ToString() });
                }
            }

            return result;

        }

        /// <summary>
        /// return runnings status
        /// </summary>
        /// <returns></returns>
        public async Task<ProjectRunning> IsRunnings()
        {

            ProjectRunning result = new ProjectRunning(Running)
            {
                Contract = _parent.Contract,
                Template = Template,
            };

            if (Running != null)
            {
                // curl -X GET "url" -H "accept: application/json"
                var serviceResult = await Running.IsUpAndRunningServices.Https.HostedInternalServiceUrl.GetJsonAsync<WatchdogResult>();
                result.UpAndRunningResult = serviceResult;

            }

            return result;

        }

        /// <summary>
        /// The template name
        /// </summary>
        public readonly string Template;

        /// <summary>
        /// Gets the contract name.
        /// </summary>
        /// <value>
        /// The contract.
        /// </value>
        public string Contract { get; }

        /// <summary>
        /// The root path of the template
        /// </summary>
        public readonly string Root;
        
        private ServiceGenerator? GetGenerator() => (ServiceGenerator)Activator.CreateInstance(_generatorType);
        private readonly ILogger<ProjectBuilderProvider> _logger;
        private readonly ProjectBuilderProvider _rootParent;
        private readonly ProjectBuilderContract _parent;
        private readonly string _templateConfigFilename;
        private readonly Type _generatorType;
        private static readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly Type _configurationType;
        private readonly string _defaultConfig;
        private Guid? _id;

        /// <summary>
        /// Gets the running description.
        /// if the service don't run. the instance is null.
        /// </summary>
        /// <value>
        /// The running.
        /// </value>
        public ProjectItem? Running { get; private set; }
    }

}
