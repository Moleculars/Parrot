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

            if (_document == null)
                throw new Exception($"document {openApiDocument} can't be loaded");

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
                    Description = this.Template + " " + _document.Info.Description;
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
            new OpenApiGenerateServices(this.Contract, "services", this.Configuration.Namespace).Parse(_document, ctx);
            GenerateWatchdog.Generate(ctx, this.Contract, this.Configuration.Namespace);

        }


        private string Description;


        private MsProject GenerateProject()
        {

            bool withApiKey = false;
            var infos = _document?.Info;
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));

            // Initialize data for map in templates
            this.SetObjectForMap(new
                                {
                                    title = infos.Title ?? string.Empty,
                                    version = infos.Version ?? "v1.0",
                                    template = Template,
                                    contract = Contract,
                                    description = infos.Description ?? "A set of REST APIs mock generated",
                                    testApiKey = withApiKey
                                      ? @"c.AddSecurityDefinition(""key"", new OpenApiSecurityScheme { Scheme = ""ApiKey"", In = ParameterLocation.{{apiSecureIn}} });"
                                      : string.Empty,
                                    apiSecureIn = "Header",
                                    origin = ("Parrot mock service " + (infos.Title ?? string.Empty)).Trim(),
                                });

            // Generate file csproj
            var project = new MsProject(Template, _dir)
                .Sdk(ProjectSdk.MicrosoftNETSdkWeb)
                .SetPropertyGroup(c =>
                {
                    c.TargetFramework(TargetFramework.Net6)
                     .Nullable(true)
                     .ImplicitUsings(true)
                     //.UserSecretsId("d6db51a2-9287-431c-93bd-2c255dedbc2a")
                     .DockerDefaultTargetOS(DockerDefaultTargetOS.Linux)
                     .GenerateDocumentationFile(true)
                    ;
                })
                .Packages(p =>
                {
                    p.PackageReference("Black.Beard.Jslt", "1.0.214")
                     .PackageReference("Black.Beard.Helpers.ContentLoaders", "2.0.1")
                     .PackageReference("Black.Beard.Helpers.ContentLoaders.Files", "2.0.1")
                     .PackageReference("Black.Beard.Helpers.ContentLoaders.Newtonsoft", "2.0.1")
                     .PackageReference("log4net", "2.0.15")
                     .PackageReference("Microsoft.Extensions.Configuration.Binder", "7.0.4")
                     .PackageReference("Microsoft.Extensions.Configuration.Json", "7.0.0")
                     .PackageReference("Microsoft.Extensions.Logging.Log4Net.AspNetCore", "6.1.0")
                     .PackageReference("Microsoft.OpenApi", "1.6.4")
                     .PackageReference("Microsoft.VisualStudio.Azure.Containers.Tools.Targets", "1.18.1")
                     .PackageReference("Swashbuckle.AspNetCore", "6.5.0")
                    ;
                });

            // appends documents in the folder.
            project.AppendDocument("Program.cs", Load("Embedded", "Program.cs"))
                   .AppendDocument("SwaggerExtension.cs", Load("Embedded", "SwaggerExtension.cs"))
                   .AppendDocument("Setup.cs", Load("Embedded", "Setup.cs"))
                   .AppendDocument("ServiceProcessor.cs", Load("Embedded", "ServiceProcessor.cs"))
                   .AppendDocument("ServiceTrace.cs", Load("Embedded", "ServiceTrace.cs"))
                   .AppendDocument("HttpExceptionModel.cs", Load("Embedded", "HttpExceptionModel.cs"))
                   .AppendDocument("log4net.config", Load("Embedded", "log4net.config"))

                   .ItemGroup(i =>
                   {
                        i.Content(c =>
                        {
                            c.Update("log4net.config")
                             .CopyToOutputDirectory(StrategyCopyEnum.Always)
                             ;
                        });
                   })
                   
                   ;

            return project;

        }

        private MsProject _project;
        private OpenApiDocument _document;
    }


}