using Bb.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SharpYaml.Tokens;
using System.Diagnostics;

namespace Bb.Extensions
{
    public static class OpenApiVisitorExtension
    {

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiMediaType self, IOpenApiVisitor visitor)
        {
            Stop();
            visitor.VisitMediaType(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiMediaType self, string key, IOpenApiVisitor visitor)
        {
            using (visitor.PushPath(key))
                visitor.VisitMediaType(key, self);
        }


        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiDocument self, IOpenApiVisitor visitor)
        {
            visitor.VisitDocument(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiTag self, IOpenApiVisitor visitor)
        {
            Stop();
            visitor.VisitTag(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiServer self, IOpenApiVisitor visitor)
        {
            Stop();
            visitor.VisitServer(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSecurityScheme self, IOpenApiVisitor visitor)
        {
            Stop();
            visitor.VisitSecurityScheme(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSecurityRequirement self, IOpenApiVisitor visitor)
        {
            Stop();
            visitor.VisitSecurityRequirement(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSchema self, IOpenApiVisitor visitor)
        {
            visitor.VisitSchema(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSchema self, string kind, string key, IOpenApiVisitor visitor)
        {
            using (visitor.PushPath(key))
                visitor.VisitJsonSchema(kind, key, self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiResponse self, IOpenApiVisitor visitor)
        {
            Stop();
            visitor.VisitResponse(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiInfo self, IOpenApiVisitor visitor)
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
        public static void Accept(this OpenApiComponents self, IOpenApiVisitor visitor)
        {
            using (visitor.PushPath("components"))
                visitor.VisitComponents(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiParameter self, IOpenApiVisitor visitor)
        {
            visitor.VisitParameter(self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiCallback self, string key, IOpenApiVisitor visitor)
        {
            using (visitor.PushPath(key))
                visitor.VisitCallback(key, self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiParameter self, string key, IOpenApiVisitor visitor)
        {
            using (visitor.PushPath(key))
                visitor.VisitParameter(key, self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiLink self, string key, IOpenApiVisitor visitor)
        {
            using (visitor.PushPath(key))
                visitor.VisitLink(key, self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiResponse self, string key, IOpenApiVisitor visitor)
        {
            using (visitor.PushPath(key))
                visitor.VisitResponse(key, self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiSecurityScheme self, string key, IOpenApiVisitor visitor)
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
        public static void Accept(this OpenApiPathItem self, string key, IOpenApiVisitor visitor)
        {
            using (visitor.PushPath(key))
                visitor.VisitPathItem(key, self);

        }

        public static void Accept(this OpenApiPaths self, IOpenApiVisitor visitor)
        {
            using (visitor.PushPath("paths"))
                visitor.VisitPath(self);
        }

        public static void Accept(this IDictionary<string, OpenApiSchema> self, IOpenApiVisitor visitor)
        {
            using (visitor.PushPath("schemas"))
                visitor.VisitSchemas(self);

        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this OpenApiOperation self, string key, IOpenApiVisitor visitor)
        {
            using (visitor.PushPath(key))
                visitor.VisitOperation(key, self);
        }

        // [System.Diagnostics.DebuggerStepThrough]
        // [System.Diagnostics.DebuggerNonUserCode]
        public static void Accept(this IOpenApiPrimitive self, IOpenApiVisitor visitor)
        {
            visitor.VisitEnumPrimitive(self);
        }



        [DebuggerStepThrough]
        [DebuggerNonUserCode]
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
