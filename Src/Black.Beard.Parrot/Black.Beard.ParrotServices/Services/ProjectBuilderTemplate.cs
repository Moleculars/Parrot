using Bb.Process;
using Bb.OpenApiServices;
using System.Diagnostics;
using Newtonsoft.Json;
using Bb.Projects.Properties;

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
        public void GenerateProject(string file)
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
                     //.Output(c =>
                     //{

                     //    Trace.WriteLine(c.Datas);

                     //})
                     .Run())
            {
                cmd.Wait();
            }

        }

        /// <summary>
        /// Run the contract
        /// </summary>
        public Uri[] Run(int currentPort)
        {

            var projectFile = GetFileProject();

            #region lauchsettings

            var port1 = HttpHelper.GetAvailablePort(currentPort);
            var port2 = HttpHelper.GetAvailablePort(port1 + 1);
            var uri1 = HttpHelper.GetLocalUri(false, port1);
            var uri2 = HttpHelper.GetLocalUri(true, port2);

            var content = LaunchSettingsBuilder.New(c =>
            {
                c.Profiles("mock", d =>
                {
                    d.CommandName("Project")
                    .LaunchBrowser(true)
                    .EnvironmentVariables(e =>
                    {
                        e.Add("ASPNETCORE_ENVIRONMENT", "Development");
                    })
                    .ApplicationUrl(uri1, uri2)
                    ;
                });
            }).ToString();

            var appsettingFilename = Path.Combine(projectFile.Directory.FullName, "Properties", "appsettings.json");
            appsettingFilename.Save(content);

            #endregion lauchsettings


            var pa = new Uri(Environment.CurrentDirectory).MakeRelativeUri(new Uri(projectFile.FullName));

            Guid id = Guid.NewGuid();

            this._rootParent._host
                .Intercept((sender, args) =>
                {
                    if (args.Status == TaskEventEnum.ErrorReceived)
                    {
                        Trace.WriteLine(args.DateReceived.Data);
                    }
                })               
                .Run(id, c =>
                {
                    
                    c.Command($"dotnet.exe")
                     .Arguments($"run --project \"{pa.ToString()}\" -c release -p:Version=1.0.0.0")
                     ;

                })
                .Wait(s =>
                {
                    

                });
          
            return new Uri[] { uri1, uri2 };
        }

        private FileInfo GetFileProject()
        {
            var files = new DirectoryInfo(Root).GetFiles("*.csproj", SearchOption.AllDirectories);
            var _projectFile = files.First();
            return _projectFile;
        }


        private ServiceGenerator? GetGenerator() =>  (ServiceGenerator) Activator.CreateInstance(_generatorType);

        public readonly string Template;
        public readonly string Root;

        private readonly ProjectBuilderProvider _rootParent;
        private readonly ProjectBuilderContract _parent;
        private readonly string _templateConfigFilename;
        private readonly Type _generatorType;
        private static readonly JsonSerializerSettings jsonSerializerSettings;
        private readonly Type _configurationType;
        private readonly string _defaultConfig;
    }

}
