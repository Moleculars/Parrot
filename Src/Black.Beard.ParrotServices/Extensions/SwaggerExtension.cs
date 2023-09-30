using Bb.Models.Security;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Bb.Extensions
{


    public static class SwaggerExtension
    {

        public static void AddDocumentation(this SwaggerGenOptions self, Action<OpenApiInfoGenerator<OpenApiInfo>> builder)
        {

            Action<OpenApiInfoGenerator<OpenApiInfo>> defaultBuilder = GetDefaultBuilder();

            var info = defaultBuilder.Generate(builder);
            self.SwaggerDoc("v1", info);

            self.IncludeXmlComments(() => LoadXmlFiles());
            //c.DocInclusionPredicate((f, a) => { return a.ActionDescriptor is ControllerActionDescriptor b && b.MethodInfo.GetCustomAttributes<ExternalApiRouteAttribute>().Any(); });
        }

        private static Action<OpenApiInfoGenerator<OpenApiInfo>> GetDefaultBuilder()
        {
            var ass = Assembly.GetEntryAssembly();
            var c = ass.CustomAttributes.ToList();

            string? title = GetServiceName(c);
            string? version = GetVersion(c);
            string? description = GetDescription(c);

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(version))
                version = $"{title} {version}";

            Action<OpenApiInfoGenerator<OpenApiInfo>> builder1 = b =>
            {

                if (!string.IsNullOrEmpty(title))
                    b.Add(c => c.Title = title);

                if (!string.IsNullOrEmpty(version))
                    b.Add(c => c.Version = version);

                if (!string.IsNullOrEmpty(description))
                    b.Add(c => c.Description = description);

            };
            return builder1;
        }

        private static string? GetDescription(List<CustomAttributeData> c)
        {
            return GetValue(c, typeof(AssemblyDescriptionAttribute));
        }

        private static string? GetServiceName(List<CustomAttributeData> c)
        {
            return GetValue(c, typeof(AssemblyTitleAttribute), typeof(AssemblyProductAttribute));
        }

        private static string? GetVersion(List<CustomAttributeData> c)
        {
            return GetValue(c, typeof(AssemblyInformationalVersionAttribute), typeof(AssemblyFileVersionAttribute));
        }

        private static string? GetValue(List<CustomAttributeData> c, params Type[] types)
        {

            foreach (var type in types)
            {
                var o = c.Where(d => type == d.AttributeType).Select(e => e.ConstructorArguments.First().Value?.ToString()).FirstOrDefault();
                if (o != null)
                    return o;
            }

            return null;

        }

        public static void AddSwaggerWithApiKeySecurity(this SwaggerGenOptions self, IServiceCollection services, IConfiguration configuration, string assemblyName)
        {

            var apiKeyConfiguration = ApiKeyConfiguration.New(configuration);

            if (string.IsNullOrEmpty(apiKeyConfiguration?.ApiHeader))
                throw new InvalidOperationException("ApiKeyConfiguration.ApiHeader is null or empty.");

            // https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=visual-studio


            const string securityDefinition = "ApiKey";


            // https://stackoverflow.com/questions/36975389/api-key-in-header-with-swashbuckle
            self.AddSecurityDefinition(securityDefinition, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = apiKeyConfiguration.ApiHeader,
                Type = SecuritySchemeType.ApiKey
            });




            // https://stackoverflow.com/questions/57227912/swaggerui-not-adding-apikey-to-header-with-swashbuckle-5-x
            self.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Name = apiKeyConfiguration.ApiHeader,
                        Type = SecuritySchemeType.ApiKey,
                        In = ParameterLocation.Header,
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = securityDefinition }
                    },
                    new List<string>()
                }
            });



        }

        /// <summary>
        /// Loads and concat all XML documentation files.
        /// </summary>
        /// <param name="patternGlobing">The pattern globing.</param>
        /// <returns></returns>
        internal static XPathDocument LoadXmlFiles(string patternGlobing = "*.xml")
        {

            XElement? xml = null;

            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            directory.Refresh();

            // Build one large xml with all comments files
            foreach (var file in directory.GetFiles(patternGlobing))
            {

                try
                {

                    Console.WriteLine($"try to load {file.FullName}");
                    XElement dependentXml = XElement.Load(file.FullName);

                    if (xml == null)
                        xml = dependentXml;

                    else
                        foreach (var ele in dependentXml.Descendants())
                            xml.Add(ele);

                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to parse '{file}'. maybe the file don't contains documentation. ({e.ToString()})");
                }
            }

            return new XPathDocument(xml?.CreateReader());
        }

    }


}
