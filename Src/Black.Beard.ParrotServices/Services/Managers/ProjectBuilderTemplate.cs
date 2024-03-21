using Bb.OpenApiServices;
using Bb.Models;
using Bb.Http;
using Bb.Mock;
using Bb.ParrotServices.Exceptions;
using System.Text.Json;
using Bb.Builds;
using Microsoft.CodeAnalysis;
using Bb.Nugets;
using Bb.Analysis;
using System.Reflection;
using Bb.ComponentModel;

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
        public ProjectBuilderTemplate(ProjectBuilderContract parent, string template)
        {

            _parent = parent;
            _rootParent = parent.Parent;
            _logger = _rootParent._logger;

            Contract = _parent.ContractName;
            Root = parent.Root.Combine(template);
            DirectoryService = Root.Combine("service");


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

            var directoryService = DirectoryService.AsDirectory();
            var path1 = DirectoryService.Combine("service", "obj");
            var path2 = DirectoryService.Combine("service", "bin");

            var documents = directoryService.GetFiles("*.cs", SearchOption.AllDirectories);
            var files = documents.Where(c =>
            {
                if (c.FullName.StartsWith(path1) || c.FullName.StartsWith(path2))
                    return false;
                return true;
            })
            .Select(c => c.FullName).ToArray();


            HashSet<string> assemblies = ResolveAssemblies(DirectoryService);

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
            {
                _logger.LogError("build failed {project}", Root);
                LocalDebug.Stop();
            }

            return buildResult;

        }


        /// <summary>
        /// Run the contract
        /// </summary>
        public async Task<ServiceHost?> Run(Compilers.AssemblyResult result, string publicHost, int? httpCurrentPort, int? httpsCurrentPort)
        {

            var running = Prepare(result, publicHost, httpCurrentPort, httpsCurrentPort);

            var instance = running.Start();

            string status = string.Empty;
            if (instance != null)
                status = instance.Status.ToString();

            if (status == ServiceRunnerStatus.Running.ToString())
            {
                try
                {
                    if (running.IsUpAndRunningServices != null)
                    {
                        var url = running.IsUpAndRunningServices.Http.InternalUrl;
                        var serviceResult = await url.GetObjectAsync<WatchdogResult>();
                        if (serviceResult != null)
                        {
                            running.Listen = true;
                            var instance1 = _rootParent._referential.Register(running);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

            }
            else
                _logger.LogError($"service failed to start service {Template} {Contract}");


            return running;

        }


        /// <summary>
        /// Kills the process.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Kill()
        {

            bool result = false;

            var running = _rootParent._referential.Resolve(Template, _parent.ContractName);
            if (running != null)
            {
                result = running.Service.Stop();
                if (result)
                {
                    _rootParent._referential.Remove(running);
                }
            }
            else
                result = true;

            return result;

        }


        /// <summary>
        /// return the list of document in the project.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        /// <returns></returns>
        public ProjectDocument List(ContextGenerator? ctx = null)
        {
            
            ProjectDocument result = new ProjectDocument()
            {
                Contract = _parent.ContractName,
                Template = Template,
                Context = ctx
            };

            var dirRoot = DirectoryService.Combine("Templates").AsDirectory();
            _logger.LogDebug($"ProjectBuilderTemplate.List : dirRoot = {dirRoot}");
            _logger.LogDebug($"ProjectBuilderTemplate.List : Root = {Root}");
            if (dirRoot.Exists)
            {
                var files = dirRoot.GetFiles("*.json");
                foreach (var item in files)
                {
                    _logger.LogDebug($"ProjectBuilderTemplate.List : file = {item.FullName}");
                    var target = new Uri(item.FullName);
                    var relative = new Uri(DirectoryService);
                    relative = relative.MakeRelativeUri(target);
                    result.Documents.Add(new Models.Document() { Kind = "jslt", File = relative.ToString() });
                }
            }

            return result;

        }

        /// <summary>
        /// test running service
        /// </summary>
        /// <param name="result"> out result watch dog</param>
        /// <returns>return true if hosted</returns>
        public bool IsRunnings()
        {
            var running = _rootParent._referential.Resolve(Template, _parent.ContractName);
            if (running != null && running.Service != null && running.Service.IsUpAndRunningServices != null)
                return true;

            return false;

        }


        /// <summary>
        /// test running service
        /// </summary>
        /// <param name="result"> out result watch dog</param>
        /// <returns>return true if hosted</returns>
        public bool IsRunnings(out WatchdogResult? result)
        {

            result = default;
            var running = _rootParent._referential.Resolve(Template, _parent.ContractName);
            if (running != null && running.Service != null && running.Service.IsUpAndRunningServices != null)
            {
                var url = running.Service.IsUpAndRunningServices.Http.InternalUrl;
                try
                {
                    var serviceResult = url.GetObjectAsync<WatchdogResult>();
                    serviceResult.Wait();
                    result = serviceResult.Result;
                }
                catch (Exception)
                {
                    
                }
                return true;
            }

            return false;

        }


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

        public string DirectoryService { get; }


        /// <summary>
        /// The template name
        /// </summary>
        public readonly string Template;




        private ServiceHost Prepare(Compilers.AssemblyResult result, string publicHost, int? httpCurrentPort, int? httpsCurrentPort)
        {

            List<(string, string, int)> listeners = new List<(string, string, int)>(); // ("http", "localhost", 5000), { "https", "localhost", 5001 } };
            string internalHost = "localhost";

            if (httpCurrentPort.HasValue)
                listeners.Add(("http", internalHost, httpCurrentPort.Value));

            if (httpsCurrentPort.HasValue)
                listeners.Add(("https", internalHost, httpsCurrentPort.Value));

            var running = new ServiceHost(result.FullAssemblyFile, result.References, publicHost, listeners.ToArray())
            {
                Contract = _parent.ContractName,
                Template = Template,
            }
            //.AddListener(publicHost, httpCurrentPort, httpsCurrentPort, uriHttp, uriHttps)
            ;

            return running;

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

        private ServiceGenerator? GetGenerator() => (ServiceGenerator)Activator.CreateInstance(_generatorType);
        private readonly ILogger<ProjectBuilderProvider> _logger;
        private readonly ProjectBuilderProvider _rootParent;
        private readonly ProjectBuilderContract _parent;
        private readonly Type? _generatorType;
        private static readonly JsonSerializerOptions jsonSerializerSettings;
        private readonly Type _configurationType;
        private readonly string _defaultConfig;

        private string _templateFilename;
        private string _templateConfigFilename;

    }




}
