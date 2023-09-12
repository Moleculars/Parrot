using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Bb.OpenApi
{


    public static class OpenApiDocumentVisitorGenericExtension
    {

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiMediaType self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitMediaType(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiMediaType self, string key, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitMediaType(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiDocument self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitDocument(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiTag self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitTag(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiServer self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitServer(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiSecurityScheme self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitSecurityScheme(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiSecurityRequirement self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitSecurityRequirement(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiSchema self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitSchema(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiSchema self, string kind, string key, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitJsonSchema(kind, key, self);
        }


        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiResponse self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitResponse(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiInfo self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitInfo(self);
        }

        //[System.Diagnostics.DebuggerStepThrough]
        //[System.Diagnostics.DebuggerNonUserCode]
        //public static T Accept<T>(this OpenApiExternalDocumentation self, IOpenApiDocumentVisitor<T> visitor)
        //{
        //    return visitor.VisitExternalDocumentation(self);
        //}       

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiComponents self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitComponents(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiParameter self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitParameter(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiCallback self, string key, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitCallback(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiParameter self, string key, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitParameter(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiLink self, string key, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitLink(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiResponse self, string key, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitResponse(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiSecurityScheme self, string key, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitSecurityScheme(key, self);
        }

        //[System.Diagnostics.DebuggerStepThrough]
        //[System.Diagnostics.DebuggerNonUserCode]
        //public static T Accept<T>(this JsonSchemaProperty self, IOpenApiDocumentVisitor<T> visitor)
        //{
        //    return visitor.VisitJsonSchemaProperty(self);
        //}

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiPathItem self, string key, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitPathItem(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiOperation self, string key, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitOperation(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this IOpenApiPrimitive self, IOpenApiDocumentGenericVisitor<T> visitor)
        {
            return visitor.VisitEnumPrimitive(self);
        }


    }


}
