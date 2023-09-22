using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Diagnostics;

namespace Bb.OpenApi
{
    public static class OpenApiDocumentVisitorExtension
    {

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiMediaType self, IOpenApiDocumentVisitor visitor)
        {
            Stop();
            visitor.VisitMediaType(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiMediaType self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.PushPath(key);
            visitor.VisitMediaType(key, self);
            visitor.PopPath();
        }


        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiDocument self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitDocument(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiTag self, IOpenApiDocumentVisitor visitor)
        {
            Stop();
            visitor.VisitTag(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiServer self, IOpenApiDocumentVisitor visitor)
        {
            Stop();
            visitor.VisitServer(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSecurityScheme self, IOpenApiDocumentVisitor visitor)
        {
            Stop();
            visitor.VisitSecurityScheme(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSecurityRequirement self, IOpenApiDocumentVisitor visitor)
        {
            Stop();
            visitor.VisitSecurityRequirement(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSchema self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitSchema(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSchema self, string kind, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.PushPath(key);
            visitor.VisitJsonSchema(kind, key, self);
            visitor.PopPath();
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiResponse self, IOpenApiDocumentVisitor visitor)
        {
            Stop();
            visitor.VisitResponse(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiInfo self, IOpenApiDocumentVisitor visitor)
        {
            Stop();
            visitor.VisitInfo(self);
        }

        //// [System.Diagnostics.DebuggerStepThrough]
        //// [System.Diagnostics.DebuggerNonUserCode]
        //public static void Accept(this OpenApiExternalDocumentation self, IOpenApiDocumentVisitor visitor)
        //{
        //    Stop(); visitor.VisitExternalDocumentation(self);
        //}       

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiComponents self, IOpenApiDocumentVisitor visitor)
        {
            visitor.PushPath("components");
            visitor.VisitComponents(self);
            visitor.PopPath();
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiParameter self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitParameter(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiCallback self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.PushPath(key);
            visitor.VisitCallback(key, self);
            visitor.PopPath();
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiParameter self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.PushPath(key);
            visitor.VisitParameter(key, self);
            visitor.PopPath();
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiLink self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.PushPath(key);
            visitor.VisitLink(key, self);
            visitor.PopPath();
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiResponse self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.PushPath(key);
            visitor.VisitResponse(key, self);
            visitor.PopPath();
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSecurityScheme self, string key, IOpenApiDocumentVisitor visitor)
        {
            Stop();
            visitor.VisitSecurityScheme(key, self);
        }

        //// [System.Diagnostics.DebuggerStepThrough]
        //// [System.Diagnostics.DebuggerNonUserCode]
        //public static void Accept(this JsonSchemaProperty self, IOpenApiDocumentVisitor visitor)
        //{
        //    Stop(); visitor.VisitJsonSchemaProperty(self);
        //}

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiPathItem self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.PushPath(key);
            visitor.VisitPathItem(key, self);
            visitor.PopPath();
        }

        public static void Accept(this OpenApiPaths self, IOpenApiDocumentVisitor visitor)
        {
            visitor.PushPath("paths");
            visitor.VisitPath(self);
            visitor.PopPath();
        }

        public static void Accept(this IDictionary<string, OpenApiSchema> self, IOpenApiDocumentVisitor visitor)
        {
            visitor.PushPath("schemas");
            visitor.VisitSchemas(self);
            visitor.PopPath();
        }              

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiOperation self, string key, IOpenApiDocumentVisitor visitor)
        {
            visitor.PushPath(key);
            visitor.VisitOperation(key, self);
            visitor.PopPath();
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this IOpenApiPrimitive self, IOpenApiDocumentVisitor visitor)
        {
            visitor.VisitEnumPrimitive(self);
        }



        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        private static void Stop()
        {

            var st = new StackTrace();
            var f = st.GetFrame(1);
            Debug.WriteLine($"{f.ToString().Trim()} try to stop");

            if (Debugger.IsAttached)
                Debugger.Break();

        }

    }


}
