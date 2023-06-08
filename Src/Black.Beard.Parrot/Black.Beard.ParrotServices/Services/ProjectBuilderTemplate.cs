using Bb.Process;
using Bb.OpenApiServices;
using Newtonsoft.Json;
using Bb.Services;
using Flurl;
using System.Security.Cryptography.X509Certificates;
using Bb.Models;
using Flurl.Http;
using Bb.Mock;

namespace Bb.ParrotServices.Services
{
    public class ProjectBuilderTemplate
    {

        static ProjectBuilderTemplate()
        {

            jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

        }

        public ProjectBuilderTemplate(ProjectBuilderProvider rootParent, ProjectBuilderContract parent, string template)
        {
            this._rootParent = rootParent;
            this._parent = parent;
            Template = template;

            Root = Path.Combine(parent.Root, template);

            _templateConfigFilename = Path.Combine(Root, template + ".json");

            _generatorType = this._rootParent.ResolveGenerator(Template);
            if (_generatorType == null)
                throw new InvalidOperationException(template);

            var instance = GetGenerator();
            _configurationType = instance.ConfigurationType;
            _defaultConfig = JsonConvert.SerializeObject(instance.GetConfiguration(), jsonSerializerSettings);

        }


        public string GetPath(string filename)
        {
            var filepath = Path.Combine(Root, filename);
            return filepath;
        }

        public string GetPath(string[] path)
        {
            var filepath = Root;
            foreach (var item in path)
                filepath = Path.Combine(filepath, item);
            return filepath;
        }


        #region Config

        public ProjectBuilderTemplate SetConfig(Action<object> action)
        {

            var config = GetConfig(out var control);
            action(config);
            WriteConfig(config, control);
            return this;
        }

        private void WriteConfig(object config, string control)
        {
            var configTxt = JsonConvert.SerializeObject(config, jsonSerializerSettings);
            if (configTxt != control)
                _templateConfigFilename.Save(configTxt);
        }

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

            return JsonConvert.DeserializeObject(control, this._configurationType, jsonSerializerSettings);

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
        /// <param name="file"></param>
        public ProjectDocument GenerateProject(string file)
        {

            var config = Config();

            var generator = GetGenerator();

            if (generator != null)
            {
                generator.ApplyConfiguration(config);
                generator
                    .Initialize(this.Template, Root)
                    .InitializeDataSources(file)
                    .Generate()
                ;

            }

            return List();

        }

        public bool Exists()
        {
            return Directory.Exists(Root);

        }

        /// <summary>
        /// Build the contract
        /// </summary>
        public void Build()
        {

            var projectFile = GetFileProject();

            using (var cmd = new ProcessCommand()
                     .Command($"dotnet.exe", $"build \"{projectFile.FullName}\" -c release /p:Version=1.0.0.0")
                     .Intercept(Intercept)
                     .Run())
            {
                cmd.Wait();
            }

        }

        private void Intercept(object sender, TaskEventArgs args)
        {

            var logger = this._rootParent._host._logger;

            switch (args.Status)
            {

                case TaskEventEnum.Started:
                    logger.LogTrace("Started");
                    break;

                case TaskEventEnum.ErrorReceived:
                    logger.LogError(args.DateReceived.Data);
                    break;

                case TaskEventEnum.DataReceived:
                    logger.LogInformation(args.DateReceived.Data);
                    break;

                case TaskEventEnum.Completed:
                    logger.LogTrace("Completed");
                    var instance = args.Process.Tag as ServiceReferentialTemplate;
                    _rootParent._referential.Remove(instance);
                    break;

                case TaskEventEnum.CompletedWithException:
                    logger.LogError("Completed with exception");
                    break;

                default:
                    break;

            }

        }

        /// <summary>
        /// Run the contract
        /// </summary>
        public ProjectItem Run(int currentPort)
        {

            var projectFile = GetFileProject();


            var port1 = HttpHelper.GetAvailablePort(currentPort);
            var port2 = HttpHelper.GetAvailablePort(HttpHelper.GetAvailablePort(port1 + 1));
            var uri1 = HttpHelper.GetLocalUri(false, port1);
            var uri2 = HttpHelper.GetLocalUri(true, port2);
            string urls = "\"" + uri1.ToString().Trim('/') + ";" + uri2.ToString().Trim('/') + "\"";


            var workingDirectory = projectFile.Directory.FullName;

            Guid id = Guid.NewGuid();

            var instance = _rootParent._referential.Get(this._parent.Contract, this.Template, uri1, uri2);

            this._rootParent._host
                .Intercept(Intercept)
                .Run(id, c =>
                {
                    c.Command($"dotnet.exe")
                     .SetWorkingDirectory(workingDirectory)
                     .Arguments($"run \"{projectFile.Name}\" --urls {urls}") // -c Development 
                     ;
                }, instance)
                .Wait(s =>
                {

                });


            this.Running = new ProjectItem()
            {

                Contract = this._parent.Contract,
                Template = this.Template,

                Services = new Listener()
                {
                    Http = new Swagger()
                    {
                        ReverseProxyUrl = new Url("http", "localhost", currentPort, "proxy", this._parent.Contract, this.Template),
                        HostedInternalServiceUrl = new Url(uri1),
                    },

                    Https =
                    new Swagger()
                    {
                        ReverseProxyUrl = new Url("https", "localhost", currentPort, "proxy", this._parent.Contract, this.Template, "swagger"),
                        HostedInternalServiceUrl = new Url(uri2).AppendPathSegments("swagger"),
                    }
                },

                Swaggers = new Listener()
                {
                    Http = new Swagger()
                    {
                        ReverseProxyUrl = new Url("http", "localhost", currentPort, "proxy", this._parent.Contract, this.Template, "swagger"),
                        HostedInternalServiceUrl = new Url(uri1).AppendPathSegments("swagger"),
                    },

                    Https =
                    new Swagger()
                    {
                        ReverseProxyUrl = new Url("https", "localhost", currentPort, "proxy", this._parent.Contract, this.Template, "swagger"),
                        HostedInternalServiceUrl = new Url(uri2).AppendPathSegments("swagger"),
                    }
                },

                IsUpAndRunningServices =
                new Listener()
                {
                    Http = new Swagger()
                    {
                        ReverseProxyUrl = new Url("http", "localhost", currentPort, "proxy", this._parent.Contract, this.Template, "Watchdog", "isupandrunning"),
                        HostedInternalServiceUrl = new Url(uri1).AppendPathSegments("Watchdog", "isupandrunning"),
                    },

                    Https =
                    new Swagger()
                    {
                        ReverseProxyUrl = new Url("https", "localhost", currentPort, "proxy", this._parent.Contract, this.Template, "Watchdog", "isupandrunning"),
                        HostedInternalServiceUrl = new Url(uri2).AppendPathSegments("Watchdog", "isupandrunning"),
                    }
                },




            };


            return this.Running;

        }

        private FileInfo GetFileProject()
        {
            var files = new DirectoryInfo(Root).GetFiles("*.csproj", SearchOption.AllDirectories);
            var _projectFile = files.First();
            return _projectFile;
        }


        private ServiceGenerator? GetGenerator() => (ServiceGenerator)Activator.CreateInstance(_generatorType);

        public ProjectDocument List()
        {

            ProjectDocument result = new ProjectDocument()
            {
                Contract = this._parent.Contract,
                Template = this.Template,
            };

            var dirRoot = new DirectoryInfo(Path.Combine(Root, "service", "Templates"));
            var files = dirRoot.GetFiles("*.json");
            foreach (var item in files)
            {
                var relative = new Uri(Root).MakeRelativeUri(new Uri(item.FullName));
                result.Documents.Add(new Document() { Kind = "jslt", File = relative.ToString() });
            }

            return result;

        }

        public async Task<ProjectRunning> ListRunnings()
        {

            ProjectRunning result = new ProjectRunning(this.Running)
            {
                Contract = this._parent.Contract,
                Template = this.Template,
            };

            if (this.Running != null)
            {

                /*
                 curl -X 'GET' \
                    'https://localhost:55865/Watchdog/isupandrunning' \
                    -H 'accept: application/json'
                */

                var serviceResult = await this.Running.IsUpAndRunningServices.Https.HostedInternalServiceUrl.GetJsonAsync<WatchdogResult>();
                result.UpAndRunningResult = serviceResult;

            }

            return result;

        }

        public readonly string Template;
        public readonly string Root;

        private readonly ProjectBuilderProvider _rootParent;
        private readonly ProjectBuilderContract _parent;
        private readonly string _templateConfigFilename;
        private readonly Type _generatorType;
        private static readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly Type _configurationType;
        private readonly string _defaultConfig;

        public ProjectItem Running { get; private set; }
    }

}
