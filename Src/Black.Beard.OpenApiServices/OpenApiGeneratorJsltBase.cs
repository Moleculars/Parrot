using Bb.Codings;
using Bb.Json.Jslt.Asts;
using Bb.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Reflection.Emit;

namespace Bb.OpenApiServices
{

    public abstract class OpenApiGeneratorJsltBase : IOpenApiDocumentVisitor<JsltBase>
    {

        public OpenApiGeneratorJsltBase()
        {
            _tree = new DeclarationBloc();
            _code = new CodeBlock();
        }

        public void Parse(OpenApiDocument self, ContextGenerator ctx)
        {
            _self = self;
            _ctx = ctx;
            self.Accept(this);
        }


        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        protected void Stop()
        {
            System.Diagnostics.Debugger.Break();
        }

        public abstract JsltBase VisitDocument(OpenApiDocument self);
        public abstract JsltBase VisitTag(OpenApiTag self);
        public abstract JsltBase VisitServer(OpenApiServer self);
        public abstract JsltBase VisitSecurityScheme(OpenApiSecurityScheme self);
        public abstract JsltBase VisitSecurityRequirement(OpenApiSecurityRequirement self);
        public abstract JsltBase VisitResponse(OpenApiResponse self);
        public abstract JsltBase VisitInfo(OpenApiInfo self);
        public abstract JsltBase VisitComponents(OpenApiComponents self);
        public abstract JsltBase VisitCallback(string key, OpenApiCallback self);
        public abstract JsltBase VisitParameter(string key, OpenApiParameter self);
        public abstract JsltBase VisitLink(string key, OpenApiLink self);
        public abstract JsltBase VisitResponse(string key, OpenApiResponse self);
        public abstract JsltBase VisitSchema(OpenApiSchema self);
        public abstract JsltBase VisitJsonSchema(string kind, string key, OpenApiSchema self);
        public abstract JsltBase VisitSecurityScheme(string key, OpenApiSecurityScheme self);
        public abstract JsltBase VisitPathItem(string key, OpenApiPathItem self);
        public abstract JsltBase VisitOperation(string key, OpenApiOperation self);
        public abstract JsltBase VisitParameter(OpenApiParameter self);
        public abstract JsltBase VisitEnumPrimitive(IOpenApiPrimitive self);

        protected readonly DeclarationBloc _tree;
        protected OpenApiDocument _self;
        protected ContextGenerator _ctx;
        protected Data _datas = new Data();
        protected readonly CodeBlock _code;

    }

}