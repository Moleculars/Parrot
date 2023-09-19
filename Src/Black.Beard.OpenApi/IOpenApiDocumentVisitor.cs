using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Bb.OpenApi
{
    public interface IOpenApiDocumentVisitor
    {

        /// <summary>
        /// Gets the path of the current location.
        /// </summary>
        /// <returns></returns>
        string GetPath();

        /// <summary>
        /// Adds a new segment on current path.
        /// </summary>
        /// <param name="segment">The segment.</param>
        void PushPath(params string[] segments);

        /// <summary>
        /// remove last segment from current path.
        /// </summary>
        /// <param name="segment">The segment.</param>
        void PopPath();

        /// <summary>
        /// remove last segment from current path.
        /// </summary>
        /// <param name="segment">The segment.</param>
        void ClearPath();

        void VisitDocument(OpenApiDocument self);

        void VisitTag(OpenApiTag self);

        void VisitServer(OpenApiServer self);

        void VisitSecurityScheme(OpenApiSecurityScheme self);

        void VisitSecurityRequirement(OpenApiSecurityRequirement self);

        void VisitResponse(OpenApiResponse self);

        void VisitInfo(OpenApiInfo self);

        void VisitComponents(OpenApiComponents self);

        void VisitCallback(string key, OpenApiCallback self);

        void VisitParameter(string key, OpenApiParameter self);

        void VisitLink(string key, OpenApiLink self);

        void VisitResponse(string key, OpenApiResponse self);

        void VisitSchema(OpenApiSchema self);

        void VisitJsonSchema(string kind, string key, OpenApiSchema self);

        void VisitSecurityScheme(string key, OpenApiSecurityScheme self);

        void VisitPath(OpenApiPaths self);

        void VisitPathItem(string key, OpenApiPathItem self);

        void VisitOperation(string key, OpenApiOperation self);

        void VisitParameter(OpenApiParameter self);

        void VisitEnumPrimitive(IOpenApiPrimitive self);

        void VisitMediaType(OpenApiMediaType self);

        void VisitMediaType(string key, OpenApiMediaType self);

        void VisitSchemas(IDictionary<string, OpenApiSchema> self);


    }


}
