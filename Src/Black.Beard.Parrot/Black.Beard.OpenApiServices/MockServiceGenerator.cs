using Bb;
using Bb.Codings;
using Bb.Projects;
using System.Text;
using Bb.Json.Jslt.Asts;
using System.Net;
using Microsoft.OpenApi.Models;

namespace Black.Beard.OpenApiServices
{


    public class MockServiceGenerator
    {

        public MockServiceGenerator(string name, string baseDirectory)
        {
            this._name = name;
            var directoryName = baseDirectory ?? AppContext.BaseDirectory;
            _dir = new DirectoryInfo(Path.Combine(directoryName, name));
        }

        public string Directory => _dir.FullName;

        public MockServiceGenerator InitializeContract(string openApiDocument)
        {

            _document = OpenApiHelper.LoadOpenApiContract(openApiDocument);

            if (string.IsNullOrEmpty(Namespace))
            {
                if (!string.IsNullOrEmpty(_document.Info.Title))
                    Namespace = _document.Info.Title.ConvertToNamespace();
                else
                    Namespace = "Bb";
            }

            if (string.IsNullOrEmpty(Description))
            {
                if (!string.IsNullOrEmpty(_document.Info.Description))
                    Description = this._name + " " + _document.Info.Description;
                else
                    Description = Path.GetFileNameWithoutExtension(openApiDocument);
            }

            _project = GenerateProject();

            return this;

        }

        public MockServiceGenerator ConfigureProject(Action<MsProject> action)
        {
            if (action != null)
                action(_project);
            return this;
        }

        public MockServiceGenerator Generate()
        {

            _project.Save();

            var ctx = new ContextGenerator(_project.Directory.FullName);

            new OpenApiGenerateDataTemplate().Parse(_document, ctx);
            new OpenApiGenerateModel("models", this.Namespace).Parse(_document, ctx);
            new OpenApiGenerateServices("services", this.Namespace).Parse(_document, ctx);

            return this;

        }

        public string Namespace { get; set; }

        public string Description { get; set; }


        private MsProject GenerateProject()
        {

            var project = new MsProject(_name, _dir)
                .Sdk(ProjectSdk.MicrosoftNETSdkWeb)
                .SetPropertyGroup(c =>
                {
                    c.TargetFramework(TargetFramework.Net6)
                     .Nullable(true)
                     .ImplicitUsings(true)
                     .UserSecretsId("d6db51a2-9287-431c-93bd-2c255dedbc2a")
                     .DockerDefaultTargetOS(DockerDefaultTargetOS.Linux)
                     .GenerateDocumentationFile(true)
                    ;
                })
                .Packages(p =>
                {
                    p.PackageReference("Black.Beard.Jslt", "1.0.190")
                     .PackageReference("Black.Beard.Helpers.ContentLoaders", "2.0.1")
                     .PackageReference("Black.Beard.Helpers.ContentLoaders.Files", "2.0.1")
                     .PackageReference("Black.Beard.Helpers.ContentLoaders.Newtonsoft", "2.0.1")
                     //.PackageReference("Black.Beard.Helpers.Roslyn", "1.0.29")
                     .PackageReference("log4net", "2.0.15")
                     .PackageReference("Microsoft.Extensions.Configuration.Binder", "7.0.4")
                     .PackageReference("Microsoft.Extensions.Configuration.Json", "7.0.0")
                     //.PackageReference("Microsoft.Extensions.Configuration.NewtonsoftJson", "5.0.1")
                     .PackageReference("Microsoft.Extensions.Logging.Log4Net.AspNetCore", "6.1.0")
                     //.PackageReference("Microsoft.Extensions.PlatformAbstractions", "1.1.0")
                     .PackageReference("Microsoft.OpenApi", "1.6.4")
                     .PackageReference("Microsoft.VisualStudio.Azure.Containers.Tools.Targets", "1.18.1")
                     //.PackageReference("Newtonsoft.Json", "13.0.1")
                     .PackageReference("Swashbuckle.AspNetCore", "6.5.0")                     
                    ;
                });

            bool withApiKey = false;
            string inArgument = "Header";

            project.AppendDocument("Program.cs",
                @"Embedded\Program.cs"
                    .LoadFromFile()
                    .Replace("{{title}}", _document.Info.Title ?? string.Empty)
                    .Replace("{{version}}", _document.Info.Version ?? "v1.0")
                    .Replace("{{description}}", _document.Info.Description ?? "A set of REST APIs mock generated")
                    .Replace("{{testApiKey}}", withApiKey ? "true" : "false")
                    .Replace("{{apiSecureIn}}", inArgument)
                );

            project.AppendDocument("SwaggerExtension.cs", @"Embedded\SwaggerExtension.cs".LoadFromFile());
            project.AppendDocument("Setup.cs", @"Embedded\Setup.cs".LoadFromFile());
            project.AppendDocument("ServiceProcessor.cs", @"Embedded\ServiceProcessor.cs".LoadFromFile());
            project.AppendDocument("log4net.config", @"Embedded\log4net.config".LoadFromFile());

            project.ItemGroup(i =>
            {

                i.Content(c =>
                {
                    c.Update("log4net.config")
                     .CopyToOutputDirectory(StrategyCopyEnum.Always)
                     ;
                });

            });

            return project;

        }

        private readonly DirectoryInfo _dir;
        private readonly string _name;
        private MsProject _project;
        private OpenApiDocument _document;
    }


}