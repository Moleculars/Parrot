using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bb.Swashbuckle
{

    /// <summary>
    /// Auto append derived types of abstract type in parameters
    /// </summary>
    /// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter" />
    public class AppendInheritanceDocumentFilter : IDocumentFilter
    {

        /// <summary>
        /// Applies the specified swagger document.
        /// </summary>
        /// <param name="swaggerDoc">The swagger document.</param>
        /// <param name="context">The context.</param>
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {

            Dictionary<string, OpenApiSchema> derivedTypesSchemas = new Dictionary<string, OpenApiSchema>();

            foreach (var apiDescription in context.ApiDescriptions)
            {

                var parameters = apiDescription.ParameterDescriptions.Where(pd => pd.ModelMetadata?.ModelType != null);

                foreach (var parameter in parameters)
                    InspectAndAddDerivedTypes(parameter.ModelMetadata.ModelType, derivedTypesSchemas, context);

            }

            foreach (var derivedSchema in derivedTypesSchemas)
                if (!swaggerDoc.Components.Schemas.ContainsKey(derivedSchema.Key))
                    swaggerDoc.Components.Schemas.Add(derivedSchema.Key, derivedSchema.Value);

        }

        private void InspectAndAddDerivedTypes(Type baseType, Dictionary<string, OpenApiSchema> derivedTypesSchemas, DocumentFilterContext context)
        {

            //var p = swaggerDoc.Components.Schemas.Where(d => d.Key == baseType.Name ).FirstOrDefault();

            // Vérifier si le type est une classe et a des types dérivés
            if (baseType.IsClass && baseType.IsAbstract)
            {
                var derivedTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => baseType.IsAssignableFrom(p) && p != baseType);

                foreach (Type derivedType in derivedTypes)
                {
                    // Ajouter le schéma du type dérivé
                    var derivedTypeName = derivedType.Name;
                    if (!derivedTypesSchemas.ContainsKey(derivedTypeName))
                    {
                        var s = context.SchemaGenerator.GenerateSchema(derivedType, context.SchemaRepository);
                        derivedTypesSchemas.Add(derivedTypeName, s);
                    }
                }

            }
        }

    }


}


