﻿using Bb.Process;
using Bb.OpenApiServices;
using Newtonsoft.Json;
using Bb.Services;
using Flurl;
using Bb.Models;
using Flurl.Http;
using Bb.Mock;
using System.Diagnostics.Contracts;
using System.Diagnostics;

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
            Contract = _parent.Contract;

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
                    .Initialize(this.Contract, this.Template, Root)
                    .InitializeDataSources(file)
                    .Generate()
                ;

            }

            return List();

        }

        public bool Exists()
        {
            var dir = new DirectoryInfo(Root);
            dir.Refresh();
            return dir.Exists;
        }

        /// <summary>
        /// Build the contract
        /// </summary>
        public async Task Build()
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

            switch (args.Status)
            {

                case TaskEventEnum.Started:
                    Trace.WriteLine("Started", "trace");
                    break;

                case TaskEventEnum.ErrorReceived:
                    Trace.WriteLine(args.DateReceived.Data, "Error");
                    break;

                case TaskEventEnum.DataReceived:
                    Trace.WriteLine(args.DateReceived.Data, "Info");
                    break;

                case TaskEventEnum.Completed:
                    var instance = args.Process.Tag as ServiceReferentialContract;
                    if (instance != null)
                    {
                        _rootParent._referential.Remove(instance);
                        Trace.WriteLine($"{instance.Parent.Template}/{instance.Contract} ended", "Info");
                    }
                    else
                        Trace.WriteLine($"Completed", "Info");
                    this.Running = null;
                    _id = null;
                    break;

                case TaskEventEnum.CompletedWithException:
                    var instance1 = args.Process.Tag as ServiceReferentialContract;
                    if (instance1 != null)
                    {
                        _rootParent._referential.Remove(instance1);
                        Trace.WriteLine($"{instance1.Parent.Template}/{instance1.Contract} ended with exception", "Error");
                    }
                    else
                        Trace.WriteLine("ended with exception", "Error");
                    this.Running = null;
                    _id = null;
                    break;

                default:
                    break;

            }

        }

        public async Task<bool> Kill()
        {

            bool result = false;

            var instance = _rootParent._referential.Resolve(this.Template, this._parent.Contract);
            if (instance != null)
            {

                var task = this._rootParent._host.GetTaskByTag(instance).FirstOrDefault();
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

        /// <summary>
        /// Run the contract
        /// </summary>
        public async Task<ProjectItem> Run(string publicHost, int? httpCurrentPort, int? httpsCurrentPort)
        {

            var projectFile = GetFileProject();

            string internalHost = "localhost";


            Uri? uriHttp = null;
            Uri? uriHttps = null;

            if (httpCurrentPort.HasValue)
                uriHttp = HttpHelper.GetUri(false, internalHost, HttpHelper.GetAvailablePort(httpCurrentPort.Value));

            if (httpsCurrentPort.HasValue)
                uriHttps = HttpHelper.GetUri(true, internalHost, HttpHelper.GetAvailablePort(httpsCurrentPort.Value));

            string urls = "\"" + uriHttp.ToString().Trim('/') + ";" + uriHttps.ToString().Trim('/') + "\"";

            var workingDirectory = projectFile.Directory.FullName;

            _id = Guid.NewGuid();

            var instance = _rootParent._referential.Register(this.Template, this._parent.Contract, uriHttp, uriHttps);

            this._rootParent._host
                .Intercept(Intercept)
                .Run(_id.Value, c =>
                {
                    c.Command($"dotnet.exe")
                     .SetWorkingDirectory(workingDirectory)
                     .Arguments($"run \"{projectFile.Name}\" --urls {urls}") // -c Development 
                     ;
                }, instance)
                //.Wait(id, 500)
                ;

            var task = this._rootParent._host.GetTask(_id.Value);
            task.Wait(500);                       

            this.Running = new ProjectItem()
            {
                Contract = this._parent.Contract,
                Template = this.Template,
            };

            if (httpCurrentPort.HasValue)
            {
                this.Running.Services.Http.ExposedReverseProxyUrl = new Url("http", publicHost, httpCurrentPort.Value, "proxy", this.Template, this._parent.Contract);
                this.Running.Swagger.Http.ExposedReverseProxyUrl = new Url("http", publicHost, httpCurrentPort.Value, "proxy", this.Template, this._parent.Contract, "swagger");
                this.Running.IsUpAndRunningServices.Http.ExposedReverseProxyUrl = new Url("http", publicHost, httpCurrentPort.Value, "proxy", this.Template, this._parent.Contract, "Watchdog", "isupandrunning");
            }

            if (httpsCurrentPort.HasValue)
            {
                this.Running.Services.Https.ExposedReverseProxyUrl = new Url("https", publicHost, httpsCurrentPort.Value, "proxy", this.Template, this._parent.Contract, "swagger");
                this.Running.Swagger.Https.ExposedReverseProxyUrl = new Url("https", publicHost, httpsCurrentPort.Value, "proxy", this.Template, this._parent.Contract, "swagger");
                this.Running.IsUpAndRunningServices.Https.ExposedReverseProxyUrl = new Url("https", publicHost, httpsCurrentPort.Value, "proxy", this.Template, this._parent.Contract, "Watchdog", "isupandrunning");
            }

            this.Running.Services.Http.HostedInternalServiceUrl = new Url(uriHttp).AppendPathSegments("proxy", this.Template, this._parent.Contract);
            this.Running.Services.Https.HostedInternalServiceUrl = new Url(uriHttps).AppendPathSegments("proxy", this.Template, this._parent.Contract, "swagger");

            this.Running.Swagger.Http.HostedInternalServiceUrl = new Url(uriHttp).AppendPathSegments("proxy", this.Template, this._parent.Contract, "swagger");
            this.Running.Swagger.Https.HostedInternalServiceUrl = new Url(uriHttps).AppendPathSegments("proxy", this.Template, this._parent.Contract, "swagger");

            this.Running.IsUpAndRunningServices.Http.HostedInternalServiceUrl = new Url(uriHttp).AppendPathSegments("proxy", this.Template, this._parent.Contract, "Watchdog", "isupandrunning");
            this.Running.IsUpAndRunningServices.Https.HostedInternalServiceUrl = new Url(uriHttps).AppendPathSegments("proxy", this.Template, this._parent.Contract, "Watchdog", "isupandrunning");

            try
            {
                var serviceResult = await this.Running.IsUpAndRunningServices.Https.HostedInternalServiceUrl.GetJsonAsync<WatchdogResult>();
                if (serviceResult != null)
                    this.Running.Started = true;
            }
            catch (Exception)
            {
                               
            }

            return this.Running;

        }

        private FileInfo GetFileProject()
        {
            var files = new DirectoryInfo(Root).GetFiles("*.csproj", SearchOption.AllDirectories);
            var _projectFile = files.First();
            return _projectFile;
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
                // curl -X GET "url" -H "accept: application/json"
                var serviceResult = await this.Running.IsUpAndRunningServices.Https.HostedInternalServiceUrl.GetJsonAsync<WatchdogResult>();
                result.UpAndRunningResult = serviceResult;

            }

            return result;

        }

        public readonly string Template;

        public string Contract { get; }

        public readonly string Root;

        private readonly ProjectBuilderProvider _rootParent;
        private readonly ProjectBuilderContract _parent;
        private readonly string _templateConfigFilename;
        private readonly Type _generatorType;
        private static readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly Type _configurationType;
        private readonly string _defaultConfig;
        private Guid? _id;

        public ProjectItem Running { get; private set; }
    }

}
