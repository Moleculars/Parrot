using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Bb.OpenApi
{

    public interface IOpenApiDocumentVisitor<T>
    {
        T VisitDocument(OpenApiDocument self);

        T VisitTag(OpenApiTag self);

        T VisitServer(OpenApiServer self);

        T VisitSecurityScheme(OpenApiSecurityScheme self);

        T VisitSecurityRequirement(OpenApiSecurityRequirement self);

        T VisitResponse(OpenApiResponse self);

        T VisitInfo(OpenApiInfo self);

        T VisitComponents(OpenApiComponents self);

        T VisitCallback(string key, OpenApiCallback self);

        T VisitParameter(string key, OpenApiParameter self);

        T VisitLink(string key, OpenApiLink self);

        T VisitResponse(string key, OpenApiResponse self);

        T VisitSchema(OpenApiSchema self);

        T VisitJsonSchema(string kind, string key, OpenApiSchema self);

        T VisitSecurityScheme(string key, OpenApiSecurityScheme self);

        T VisitPathItem(string key, OpenApiPathItem self);

        T VisitOperation(string key, OpenApiOperation self);

        T VisitParameter(OpenApiParameter self);

        T VisitEnumPrimitive(IOpenApiPrimitive self);
    }


}
