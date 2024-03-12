using Bb;
using Bb.Codings;
using Bb.ComponentModel.Attributes;
using Bb.Extensions;
using Bb.Projects;
using Microsoft.OpenApi.Models;

namespace Bb.OpenApiServices
{


    [ExposeClass(Context = Constants.Models.Plugin, ExposedType = typeof(ServiceGenerator))]
    public class MockServiceGenerator : ServiceGenerator<MockServiceGeneratorConfig>
    {


        public override void InitializeDatas(string openApiDocumentPath)
        {

            this._file = openApiDocumentPath;
            _document = openApiDocumentPath.LoadOpenApiContract();

            if (_document == null)
                throw new Exception($"document {openApiDocumentPath} can't be loaded");

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
                    Description = Path.GetFileNameWithoutExtension(openApiDocumentPath);
            }

            _project = GenerateProject();

        }

        public MsProject Project => _project;

        public override ContextGenerator Generate()
        {

            _project.Save();

            var ctx = new ContextGenerator(_project.Directory.FullName)
            {
                ContractDocumentFilename = Path.GetFileName(this._file),
            };

            new ServiceGeneratorProcess<OpenApiDocument>(ctx)

                .Append(
                    new OpenApiValidator(),
                    new OpenApiGenerateDataTemplate(),
                    new OpenApiGenerateModel("models", this.Configuration.Namespace),
                    new OpenApiGenerateServices(this.Contract, "services", this.Configuration.Namespace)
                )

                .Generate(_document);

            if (ctx.Diagnostics.Success)
                GenerateWatchdog.Generate(ctx, this.Contract, this.Configuration.Namespace);

            return ctx;

        }

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
                //.Packages(p =>
                //{
                //    p.PackageReference("Black.Beard.Jslt", "1.0.300")
                //     .PackageReference("Black.Beard.Roslyn", "1.0.99")
                //     .PackageReference("Black.Beard.Helpers.ContentLoaders", "2.0.26")
                //     .PackageReference("Black.Beard.Helpers.ContentLoaders.Compress", "2.0.26")
                //     .PackageReference("Black.Beard.Helpers.ContentLoaders.Files", "2.0.26")
                //     .PackageReference("Black.Beard.Helpers.ContentLoaders.Newtonsoft", "2.0.26")
                //     .PackageReference("DataAnnotationsExtensions", "5.0.1.27")
                //     .PackageReference("Microsoft.Extensions.Configuration.Binder", "8.0.1")
                //     .PackageReference("Microsoft.Extensions.Configuration.Json", "8.0.0")
                //     .PackageReference("Microsoft.Extensions.Hosting", "8.0.0")
                //     .PackageReference("Microsoft.Extensions.Configuration.NewtonsoftJson", "5.0.1")
                //     .PackageReference("Microsoft.Extensions.PlatformAbstractions", "1.1.0")
                //     .PackageReference("Swashbuckle.AspNetCore", "6.5.0")
                //     .PackageReference("NLog", "5.2.8")
                //     .PackageReference("NLog.DiagnosticSource", "5.2.1")
                //     .PackageReference("NLog.Extensions.Logging", "5.3.8")
                //     .PackageReference("NLog.Web.AspNetCore", "5.3.8")
                //    ;
                //})
                ;

            // appends documents in the folder.
            project.AppendDocument("Program.cs", Load("Embedded", "Program.cs"))
                   .AppendDocument("Startup.cs", Load("Embedded", "Startup.cs"))
                   .AppendDocument("ServiceProcessor.cs", Load("Embedded", "ServiceProcessor.cs"))
                   .AppendDocument("HttpExceptionModel.cs", Load("Embedded", "HttpExceptionModel.cs"))

                   //.ItemGroup(i =>
                   //{
                   //    i.Content(c =>
                   //    {
                   //        //c.Update("log4net.config")
                   //        // .CopyToOutputDirectory(StrategyCopyEnum.Always)
                   //        // ;
                   //    });
                   //})

                   ;

            return project;

        }

        private MsProject _project;
        private string _file;
        private OpenApiDocument _document;
        private string Description;

    }


}