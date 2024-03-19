using Bb;
using Bb.Codings;
using Bb.ComponentModel.Attributes;
using Bb.Extensions;
using Bb.Projects;
using Microsoft.OpenApi.Models;
using RandomDataGenerator.FieldOptions;

namespace Bb.OpenApiServices
{


    [ExposeClass(Context = Constants.Models.Plugin, ExposedType = typeof(ServiceGenerator))]
    public class MockGenerator : ServiceGenerator<ServiceConfig>
    {

        /// <summary>
        /// initialize the generator
        /// </summary>
        static MockGenerator()
        {

            _directorySource = typeof(MockGenerator).Assembly.Location.AsFile().Directory;

            _assemblies = new string[]
            {
                "Black.Beard.Helpers.ContentLoaders",
                "Black.Beard.Helpers.ContentLoaders.Files",
                "Black.Beard.Analysis",
                "Black.Beard.Roslyn",
                "Black.Beard.Jslt",
                "Black.Beard.Web.Server",
                "Microsoft.AspNetCore",
                "Microsoft.AspNetCore.Mvc.Abstractions",
                "Microsoft.Extensions.DependencyInjection.Abstractions",
                "Microsoft.Extensions.Logging.Abstractions",
                "Microsoft.Extensions.Configuration.Abstractions",
                "Microsoft.Extensions.Logging.Abstractions",
                "Microsoft.Extensions.Configuration.Binder",
                "Microsoft.Extensions.Configuration.Json",
                "Microsoft.Extensions.Logging.Abstractions",
                "System.Net.Http.Json",
                "Newtonsoft.Json",
                "NLog",
                "NLog.Web.AspNetCore",
                "NLog.Extensions.Logging",
                "NLog.DiagnosticSource",
            };

        }

        /// <summary>
        /// Initialize the generator with the openApiDocumentPath
        /// </summary>
        /// <param name="openApiDocumentPath"></param>
        /// <exception cref="Exception"></exception>
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

        /// <summary>
        /// Generate the project
        /// </summary>
        /// <returns></returns>
        public override ContextGenerator Generate()
        {

            _project.Save();

            var ctx = new ContextGenerator(_project.Directory.FullName)
            {
                ContractDocumentFilename = Path.GetFileName(this._file),
            };

            foreach (var item in _assemblies)
                ctx.AddAssemblyName(item);

            new ServiceGeneratorProcess<OpenApiDocument>(ctx)

                .Append(
                    new OpenApiValidator(),
                    new OpenApiGenerateDataTemplate(),
                    new OpenApiGenerateModel("models", this.Configuration.Namespace),
                    // new OpenApiGenerateServices(this.Contract, "services", this.Configuration.Namespace)
                    new GenerateServices(this.Contract, "services", this.Configuration.Namespace)
                )

                .Generate(_document);

            if (ctx.Diagnostics.Success)
            {
                GenerateWatchdog.Generate(ctx, this.Contract, this.Configuration.Namespace);
            }

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
                    c.TargetFramework(new TargetFramework("net8.0"))
                     .Nullable(true)
                     .ImplicitUsings(true)
                     .UserSecretsId(Guid.NewGuid())
                     .DockerDefaultTargetOS(DockerDefaultTargetOS.Linux)
                     .GenerateDocumentationFile(true)
                    ;
                })
                .Packages(p =>
                {
                    p.PackageReference("Newtonsoft.Json", "13.0.3")

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
                    ;

                })
                ;

            _directorySource.AsFile("nlog.config").CopyToDirectory(this.Directory, true);

            // appends documents in the folder.
            project.AppendDocument("Program.cs", Load("Embedded", "Program.cs"))
                   .AppendDocument("Startup.cs", Load("Embedded", "Startup.cs"))
                   .AppendDocument("ServiceProcessor.cs", Load("Embedded", "ServiceProcessor.cs"))
                   .AppendDocument("HttpExceptionModel.cs", Load("Embedded", "HttpExceptionModel.cs"))

                   .ItemGroup(i =>
                   {
                       i.Content(c =>
                       {
                           c.Update("nlog.config")
                            .CopyToOutputDirectory(StrategyCopyEnum.Always)
                            ;
                       });
                   })

                   ;

            return project;

        }

        private MsProject _project;
        private string _file;
        private OpenApiDocument _document;
        private string Description;
        private static readonly DirectoryInfo? _directorySource;
        private static readonly string[] _assemblies;

    }

  
}