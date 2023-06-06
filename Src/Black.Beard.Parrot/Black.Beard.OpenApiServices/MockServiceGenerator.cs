using Bb;
using Bb.Codings;
using Bb.Projects;
using System.Text;
using Bb.Json.Jslt.Asts;
using System.Net;
using Microsoft.OpenApi.Models;
using System.Xml.Linq;

namespace Bb.OpenApiServices
{


    public class MockServiceGenerator : ServiceGenerator<MockServiceGeneratorConfig>
    {

        internal override void InitializeDatas(string openApiDocument)
        {

            _document = OpenApiHelper.LoadOpenApiContract(openApiDocument);

            if (string.IsNullOrEmpty(this.Configuration.Namespace))
            {
                if (!string.IsNullOrEmpty(_document.Info.Title))
                    this.Configuration.Namespace = _document.Info.Title.ConvertToNamespace();
                else
                    this.Configuration.Namespace = "Bb";
            }

            if (string.IsNullOrEmpty(Description))
            {
                if (!string.IsNullOrEmpty(_document.Info.Description))
                    Description = this.Name + " " + _document.Info.Description;
                else
                    Description = Path.GetFileNameWithoutExtension(openApiDocument);
            }

            _project = GenerateProject();

        }


        public MsProject Project => _project;

        public override void Generate()
        {

            _project.Save();

            var ctx = new ContextGenerator(_project.Directory.FullName);

            new OpenApiGenerateDataTemplate().Parse(_document, ctx);
            new OpenApiGenerateModel("models", this.Configuration.Namespace).Parse(_document, ctx);
            new OpenApiGenerateServices("services", this.Configuration.Namespace).Parse(_document, ctx);
            GenerateWatchdog.Generate(ctx, this.Configuration.Namespace);

        }


        private string Description;


        private MsProject GenerateProject()
        {
            
            var project = new MsProject(Name, _dir)
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
                    p.PackageReference("Black.Beard.Jslt", "1.0.212")
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

        private MsProject _project;
        private OpenApiDocument _document;
    }


}