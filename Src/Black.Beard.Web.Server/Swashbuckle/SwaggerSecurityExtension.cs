// Ignore Spelling: Jwt Api

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bb.Swashbuckle
{

    /// <summary>
    /// Swagger extension for security
    /// </summary>
    public static class SwaggerSecurityExtension
    {

        /// <summary>
        /// Appends the JWT security.
        /// </summary>
        /// <param name="self">The self.</param>
        public static void AppendJwt(this SwaggerGenOptions self)
        {

            self.AddSecurityDefinition(bearer, new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                
                In = ParameterLocation.Header, Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = bearer
            });

            self.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {                       
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = bearer },
                        Scheme = "oauth2",
                        Name = bearer,
                        In = ParameterLocation.Header,
                    },
                    Array.Empty<string>()
                }
            });

        }

        /// <summary>
        /// Appends API key security.
        /// </summary>
        /// <param name="self">The self.</param>
        /// <param name="name">The name.</param>
        /// <param name="headerName">Name of the header.</param>
        public static void AppendApiKey(this SwaggerGenOptions self, string name = "ApiKey", string headerName = "X-API-KEY")
        {

            // https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-5.0&tabs=visual-studio

            // https://stackoverflow.com/questions/36975389/api-key-in-header-with-swashbuckle
            self.AddSecurityDefinition(name, new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header, Name = headerName,
                Type = SecuritySchemeType.ApiKey,
                Scheme = name,
            });

            // https://stackoverflow.com/questions/57227912/swaggerui-not-adding-apikey-to-header-with-swashbuckle-5-x
            self.AddSecurityRequirement(new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header, Name = headerName,
                        Type = SecuritySchemeType.ApiKey,
                        Scheme = name,
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = name },
                    },
                    Array.Empty<string>()
                }
            });

        }

        /// <summary>
        /// Appends the basic authentication.
        /// </summary>
        /// <param name="self">The self.</param>
        public static void AppendBasic(this SwaggerGenOptions self)
        {

            self.AddSecurityDefinition(basic, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = basic,
                Description = "Basic Authorization header using the Bearer scheme."
            });

            // Use the "basic" security definition in Swagger UI
            self.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = basic }
                    },
                    new string[] { }
                }
            });

        }

        private const string bearer = "Bearer";
        private const string basic = "basic";

    }


}


