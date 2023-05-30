using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace Bb.OpenApi
{

    public static class OpenApiDocumentVisitorExtension
    {

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiDocument self, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitDocument(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiTag self, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitTag(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiServer self, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitServer(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiSecurityScheme self, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitSecurityScheme(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiSecurityRequirement self, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitSecurityRequirement(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiSchema self, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitSchema(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiSchema self, string kind, string key, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitJsonSchema(kind, key, self);
        }


        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiResponse self, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitResponse(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiInfo self, IOpenApiDocumentVisitor<T> visitor)
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
        public static T Accept<T>(this OpenApiComponents self, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitComponents(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiParameter self, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitParameter(self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiCallback self, string key, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitCallback(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiParameter self, string key, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitParameter(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiLink self, string key, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitLink(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiResponse self, string key, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitResponse(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiSecurityScheme self, string key, IOpenApiDocumentVisitor<T> visitor)
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
        public static T Accept<T>(this OpenApiPathItem self, string key, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitPathItem(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this OpenApiOperation self, string key, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitOperation(key, self);
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        public static T Accept<T>(this IOpenApiPrimitive self, IOpenApiDocumentVisitor<T> visitor)
        {
            return visitor.VisitEnumPrimitive(self);
        }


    }


}
