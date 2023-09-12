using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Bb.OpenApi
{
    public static class OpenApiDocumentVisitorExtension
    {

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiMediaType self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitMediaType(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiMediaType self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitMediaType(key, self);
        }


        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiDocument self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitDocument(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiTag self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitTag(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiServer self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitServer(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSecurityScheme self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitSecurityScheme(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSecurityRequirement self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitSecurityRequirement(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSchema self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitSchema(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSchema self, string kind, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitJsonSchema(kind, key, self);
        }


        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiResponse self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitResponse(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiInfo self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitInfo(self);
        }

        //[System.Diagnostics.DebuggerStepThrough]
        //[System.Diagnostics.DebuggerNonUserCode]
        //public static void Accept(this OpenApiExternalDocumentation self, IOpenApiDocumentVisitor visitor)
        //{
        //    visitor.VisitExternalDocumentation(self);
        //}       

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiComponents self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitComponents(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiParameter self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitParameter(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiCallback self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitCallback(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiParameter self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitParameter(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiLink self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitLink(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiResponse self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitResponse(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSecurityScheme self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitSecurityScheme(key, self);
        }

        //[System.Diagnostics.DebuggerStepThrough]
        //[System.Diagnostics.DebuggerNonUserCode]
        //public static void Accept(this JsonSchemaProperty self, IOpenApiDocumentVisitor visitor)
        //{
        //    visitor.VisitJsonSchemaProperty(self);
        //}

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiPathItem self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitPathItem(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiOperation self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitOperation(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this IOpenApiPrimitive self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitEnumPrimitive(self);
        }


    }


}
