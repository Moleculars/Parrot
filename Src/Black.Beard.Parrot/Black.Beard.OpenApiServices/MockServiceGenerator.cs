using Bb;
using Bb.Codings;
using Bb.Projects;
using NSwag;
using System.Linq;
using System.Text;

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

            var code = new OpenApiGenerateModel("models", this.Namespace).VisitDocument(_document) as CSharpArtifact;
            _project.AppendDocument("Models.cs", code.Code());

            code = new OpenApiGenerateServices("services", this.Namespace).VisitDocument(_document) as CSharpArtifact;
            _project.AppendDocument("Controllers", "Service.cs", code.Code());

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
                    //.RazorLangVersion(Version.Parse("3.0"))
                    //.AddRazorSupportForMvc(true)
                    //.GenerateDocumentationFile(true)
                    //.RootNamespace(Namespace ?? "Bb")
                    //.Description(Description)
                    //.LangVersion(Version.Parse("8.0"))
                    //.RepositoryUrl(new System.Uri("http://github.com"))
                    ;
                })
                .Packages(p =>
                {
                    p.PackageReference("Black.Beard.Jslt")
                     .PackageReference("log4net", "2.0.15")
                     .PackageReference("Microsoft.Extensions.Configuration.Binder", "7.0.4")
                     .PackageReference("Microsoft.Extensions.Configuration.Json")
                     .PackageReference("Microsoft.Extensions.Configuration.NewtonsoftJson")
                     .PackageReference("Microsoft.Extensions.Logging.Log4Net.AspNetCore", "6.1.0")
                     .PackageReference("Microsoft.Extensions.PlatformAbstractions")
                     .PackageReference("Microsoft.OpenApi")
                     .PackageReference("Microsoft.VisualStudio.Azure.Containers.Tools.Targets")
                     .PackageReference("Swashbuckle.AspNetCore", "6.2.3")
                    //p.PackageReference("Microsoft.AspNetCore.Mvc.Core", new Version("2.2.5"))
                    //p.PackageReference("Black.Beard.ComponentModel", new Version("1.0.36"))
                     .PackageReference("Black.Beard.Helpers.ContentLoaders")
                     .PackageReference("Black.Beard.Helpers.ContentLoaders.Files")
                     .PackageReference("Black.Beard.Helpers.ContentLoaders.Newtonsoft")
                    ;
                });

            project.AppendDocument("Program.cs", new StringBuilder(@"Embedded\Program.cs".LoadFromFile()));
            project.AppendDocument("Setup.cs", new StringBuilder(@"Embedded\Setup.cs".LoadFromFile()));
            project.AppendDocument("ServiceProcessor.cs", new StringBuilder(@"Embedded\ServiceProcessor.cs".LoadFromFile()));

            return project;

        }

        private readonly DirectoryInfo _dir;
        private readonly string _name;
        private MsProject _project;
        private OpenApiDocument _document;
    }


}