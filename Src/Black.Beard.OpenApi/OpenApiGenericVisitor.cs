using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Bb.OpenApi
{

    public abstract class OpenApiGenericVisitor<T> : OpenApiBase, IOpenApiGenericVisitor<T>
    {
        public abstract T VisitCallback(string key, OpenApiCallback self);
        public abstract T VisitComponents(OpenApiComponents self);
        public abstract T VisitDocument(OpenApiDocument self);
        public abstract T VisitEnumPrimitive(IOpenApiPrimitive self);
        public abstract T VisitInfo(OpenApiInfo self);
        public abstract T VisitJsonSchema(string kind, string key, OpenApiSchema self);
        public abstract T VisitLink(string key, OpenApiLink self);
        public abstract T VisitMediaType(string key, OpenApiMediaType self);
        public abstract T VisitMediaType(OpenApiMediaType self);
        public abstract T VisitOperation(string key, OpenApiOperation self);
        public abstract T VisitParameter(string key, OpenApiParameter self);
        public abstract T VisitParameter(OpenApiParameter self);
        public abstract T VisitPathItem(string key, OpenApiPathItem self);
        public abstract T VisitResponse(OpenApiResponse self);
        public abstract T VisitResponse(string key, OpenApiResponse self);
        public abstract T VisitSchema(OpenApiSchema self);
        public abstract T VisitSecurityRequirement(OpenApiSecurityRequirement self);
        public abstract T VisitSecurityScheme(OpenApiSecurityScheme self);
        public abstract T VisitSecurityScheme(string key, OpenApiSecurityScheme self);
        public abstract T VisitServer(OpenApiServer self);
        public abstract T VisitTag(OpenApiTag self);
    }


}
