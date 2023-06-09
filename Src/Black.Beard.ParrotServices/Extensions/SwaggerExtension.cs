﻿using Bb.Models.Security;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Bb.Extensions
{


    internal static class SwaggerExtension
    {

        public static void AddDocumentation(this SwaggerGenOptions self)
        {

            self.DescribeAllParametersInCamelCase();
            self.IgnoreObsoleteActions();
            //c.DocInclusionPredicate((f, a) => { return a.ActionDescriptor is ControllerActionDescriptor b && b.MethodInfo.GetCustomAttributes<ExternalApiRouteAttribute>().Any(); });
            self.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Parrot APIs",
                Version = "v1",
                Description = "A set of REST APIs used by Parrot for manage service generator",
                License = new OpenApiLicense() { Name = "Only usable with a valid PU partner contract." },
            });

            self.IncludeXmlComments(() => LoadXmlFiles());


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
