using Bb.Process;
using Bb.OpenApiServices;
using Bb.Models;
using Bb.Http;
using Bb.Mock;
using Bb.Services.ProcessHosting;
using Bb.ParrotServices.Exceptions;
using System.Text;
using System.Text.Json;
using Bb.Builds;
using Microsoft.CodeAnalysis;
using Bb.Analysis.DiagTraces;
using Bb.Nugets;
using Bb.Analysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Bb.ComponentModel;
using Bb.Projects;

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

            jsonSerializerSettings = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectBuilderTemplate"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="template">The template.</param>
        /// <exception cref="Bb.ParrotServices.Exceptions.MockHttpException">template {template} not found</exception>
        public ProjectBuilderTemplate(ProjectBuilderContract parent, string template, LocalProcessCommandService host)
        {

            _parent = parent;
            _rootParent = parent.Parent;
            _logger = _rootParent._logger;
            _host = host;

            Contract = _parent.Contract;
            Root = parent.Root.Combine(template);

            Template = template;
            _generatorType = _rootParent.ResolveGenerator(Template);
            if (_generatorType == null)
                throw new MockHttpException($"template {template} not found");

            _templateFilename = Root.Combine(template + ".json");
            _templateConfigFilename = Root.Combine(template + ".config.json");
            var instance = GetGenerator();
            if (instance == null)
                throw new MockHttpException($"instance {template} can not be created");

            _configurationType = instance.ConfigurationType;
            _defaultConfig = JsonSerializer.Serialize(instance.GetConfiguration(), _configurationType, jsonSerializerSettings);

        }


        /// <summary>
        /// Gets a path for specified name.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public string GetPath(string filename)
        {
            var filepath = Root.Combine(filename);
            return filepath;
        }


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

            _templateConfigFilename.SerializeAndSaveConfiguration(config, _configurationType);

            var configTxt = JsonSerializer.Serialize(config, _configurationType, jsonSerializerSettings);
            if (configTxt != control)
                _templateConfigFilename.Save(configTxt);
        }

        /// <summary>
        /// return the configurations for manipulate the generator template.
        /// </summary>
        /// <returns></returns>
        public object? Config()
        {
            return GetConfig(out _);
        }

        private object? GetConfig(out string control)
        {

            if (File.Exists(_templateConfigFilename))
            {
                control = _templateConfigFilename.LoadFromFile();
                return control.DeserializeConfiguration(_configurationType, jsonSerializerSettings);
            }
            else
            {
                control = _defaultConfig;
                return control.Deserialize(_configurationType, jsonSerializerSettings);
            }


        }

        #endregion Config


        /// <summary>
        /// Gets the template configuration file path.
        /// </summary>
        /// <value>
        /// The template configuration filename.
        /// </value>
        public string TemplateConfigFilename => _templateConfigFilename;


        /// <summary>
        /// Gets the template filename.
        /// </summary>
        /// <value>
        /// The template filename.
        /// </value>
        public string TemplateFilename => _templateFilename;


        /// <summary>
        /// Removes the template if exists.
        /// </summary>
        public void RemoveTemplateIfExists()
        {
            var f = new FileInfo(_templateFilename);
            if (f.Exists)
                f.Delete();
        }


        /// <summary>
        /// Writes the document on disk.
        /// </summary>
        /// <param name="upfile">The upfile.</param>
        public void WriteOnDisk(IFormFile upfile)
        {
            WriteOnDisk(upfile, _templateFilename);
        }


        /// <summary>
        /// Writes the document on disk.
        /// </summary>
        /// <param name="upFile">The upload document.</param>
        /// <param name="filePath">The file path.</param>
        public void WriteOnDisk(IFormFile upFile, string filePath)
        {

            if (!Directory.Exists(Root))
                Directory.CreateDirectory(Root);

            RemoveTemplateIfExists();

            using (var stream = new FileStream(filePath, FileMode.Create))
                upFile.CopyTo(stream);

            _templateFilename = filePath;

        }


        /// <summary>
        /// Generate project
        /// </summary>
        /// <returns>if the generator can't be resolve, the result is null.</returns>
        public ProjectDocument? GenerateProject()
        {

            ProjectDocument? result = default;
            var config = Config();
            var generator = GetGenerator();

            if (generator != null)
            {

                generator.ApplyConfiguration(config);

                ContextGenerator ctx = generator
                    .Initialize(Contract, Template, Root)
                    .InitializeDataSources(_templateFilename)
                    .Generate();

                result = List(ctx);



                var file = ctx.TargetPath.Combine("assemblies.txt").AsFile();
                if (file.Exists)
                    file.Delete();

                file.FullName.Save(string.Join(Environment.NewLine, ctx.AssemblyNames));


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
        public async Task<Compilers.AssemblyResult> Build()
        {

            //FileInfo? projectFile = GetFileProject();
            int exitResult = 1;
            //if (projectFile != null)
            //{

            _logger.LogInformation("starting build {project}", Root);

            var directoryRoot = Root.AsDirectory();

            var directoryService = Root.Combine("service");
            var path1 = directoryService.Combine("service", "obj");
            var path2 = directoryService.Combine("service", "bin");

            var documents = directoryRoot.GetFiles("*.cs", SearchOption.AllDirectories);
            var files = documents.Where(c =>
            {
                if (c.FullName.StartsWith(path1) || c.FullName.StartsWith(path2))
                    return false;
                return true;
            })
            .Select(c => c.FullName).ToArray();


            HashSet<string> assemblies = ResolveAssemblies(directoryService);

            //var dir = Environment.CurrentDirectory.Combine( Path.GetRandomFileName());
            var nuget = new NugetController()
                //.AddFolderIf(NugetController.IsWindowsPlatform, dir, NugetController.HostNugetOrg)
                .AddDefaultWindowsFolder()
                .WithFilter((n, v) =>
                {
                    return true;
                })
                ;

            var sdk = FrameworkVersion.CurrentVersion;

            BuildCSharp build = new BuildCSharp()
                .SetSdk(FrameworkKey.Net80, FrameworkType.AspNetCore)
                .EnableImplicitUsings()
                .Using("System.Net.Http.Json", c => c.IsGlobal = true)
                .SetOutputKind(OutputKind.WindowsApplication, "Program")
                .SetNugetController(nuget)
                .AddSource(files)
                .AddReferences(ResolveAssemblies(assemblies))
                ;
            Compilers.AssemblyResult buildResult = build.Build();

            if (buildResult.Success)
                _logger.LogInformation("build success {project}", Root);

            else
                _logger.LogError("build failed {project}", Root);

            return buildResult;

        }

        private static HashSet<string> ResolveAssemblies(string directoryService)
        {

            var assemblies = new HashSet<string>();

            var file = directoryService.Combine("assemblies.txt").AsFile();
            if (file.Exists)
            {
                var items = new HashSet<string>(file.LoadFromFile().Split(Environment.NewLine));
                foreach (var item in items)
                    assemblies.Add(item);
            }

            return assemblies;
        
        }

        private static Assembly[] ResolveAssemblies(HashSet<string> namespaces)
        {
            var references = new List<Assembly>();  
            foreach (var item in namespaces)
            {
                var assembly = AssemblyLoader.Instance.LoadAssemblyName(item);
                references.Add(assembly);
            }

            return references.ToArray();

        }


        /// <summary>
        /// Run the contract
        /// </summary>
        public async Task<ProjectItem?> Run(string publicHost, int? httpCurrentPort, int? httpsCurrentPort)
        {

            var projectFile = GetFileProject();

            if (projectFile == null)
            {
                _logger.LogError($"Failed to locate a project to run in {Root}");
                return null;
            }

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

            DotnetCommand cmd = _host.RunAndGet(new DotnetCommand(_id.Value, instance), c =>
            {
                c.SetWorkingDirectory(workingDirectory)
                 .Arguments($"run \"{projectFile.Name}\" --urls {urls.ToString()}") // -c Development 
                 .Intercept(InterceptTraces)
                 ;
            });

            cmd.Wait(1500);

            if (cmd.ExitCode > 0)
                throw new Exception("Service can't start");

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

                        if (args.Status == TaskEventEnum.RanWithException)
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

            var dir = Root.Combine("service");

            foreach (var item in path)
                dir = dir.Combine(item);

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

            var dirRoot = rootPath.Combine("Templates").AsDirectory();
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
                    result.Documents.Add(new Models.Document() { Kind = "jslt", File = relative.ToString() });
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


        private void InterceptTraces(object sender, TaskEventArgs args)
        {

            var id = args.Process.Id;

            switch (args.Status)
            {

                case TaskEventEnum.Completed:
                    var instance = args.Process.Tag as ServiceReferentialContract;
                    if (instance != null)
                    {
                        _rootParent._referential.Remove(instance);
                    }
                    Running = null;
                    _id = null;
                    break;

                case TaskEventEnum.RanWithException:
                    var instance1 = args.Process.Tag as ServiceReferentialContract;
                    if (instance1 != null)
                    {
                        _rootParent._referential.Remove(instance1);
                    }
                    Running = null;
                    _id = null;
                    break;

                default:
                    break;

            }

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
        private readonly LocalProcessCommandService _host;
        private readonly ProjectBuilderProvider _rootParent;
        private readonly ProjectBuilderContract _parent;
        private readonly Type _generatorType;
        private static readonly JsonSerializerOptions jsonSerializerSettings;
        private readonly Type _configurationType;
        private readonly string _defaultConfig;

        private string _templateFilename;
        private string _templateConfigFilename;

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
