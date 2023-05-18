using NJsonSchema;
using NSwag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bb.OpenApi
{

    public interface IOpenApiDocumentVisitor<T>
    {
        T VisitDocument(OpenApiDocument self);

        T VisitTag(OpenApiTag self);

        T VisitServer(OpenApiServer self);

        T VisitSecurityScheme(OpenApiSecurityScheme self);

        T VisitSecurityRequirement(OpenApiSecurityRequirement self);

        T VisitSchema(OpenApiSchema self);

        T VisitResponse(OpenApiResponse self);

        T VisitOperationDescription(OpenApiOperationDescription self);

        T VisitInfo(OpenApiInfo self);

        T VisitExternalDocumentation(OpenApiExternalDocumentation self);

        T VisitJsonSchema(JsonSchema self);

        T VisitComponents(OpenApiComponents self);

        T VisitCallback(string key, OpenApiCallback self);

        T VisitParameter(string key, OpenApiParameter self);

        T VisitLink(string key, OpenApiLink self);

        T VisitResponse(string key, OpenApiResponse self);

        T VisitJsonSchema(string key, JsonSchema self);

        T VisitSecurityScheme(string key, OpenApiSecurityScheme self);

        T VisitJsonSchemaProperty(JsonSchemaProperty self);

        T VisitPathItem(string key, OpenApiPathItem self);
        T VisitOperation(string key, OpenApiOperation self);
        T VisitParameter(OpenApiParameter self);
    }


}
