using Bb.Codings;
using Bb.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Xml.Linq;

namespace Bb.OpenApiServices
{

    public abstract class OpenApiGeneratorCSharpBase : IOpenApiDocumentVisitor<CSMemberDeclaration>
    {

        public OpenApiGeneratorCSharpBase(string artifactName, string @namespace, params string[] usings)
        {
            _tree = new DeclarationBloc();
            _namespace = @namespace;
            _artifactName = artifactName;
            _usings = usings;

        }

        public void Parse(OpenApiDocument self, ContextGenerator ctx)
        {
            _self = self;
            _ctx = ctx;
            self.Accept(this);
        }


        protected CSharpArtifact CreateArtifact(string suffix) 
            => new CSharpArtifact(suffix + _artifactName)
                .Usings(_usings);

        protected CSNamespace CreateNamespace(CSharpArtifact cs) => cs.Namespace(_namespace); 


        public abstract CSMemberDeclaration VisitDocument(OpenApiDocument self);
        public abstract CSMemberDeclaration VisitTag(OpenApiTag self);
        public abstract CSMemberDeclaration VisitServer(OpenApiServer self);
        public abstract CSMemberDeclaration VisitSecurityScheme(OpenApiSecurityScheme self);
        public abstract CSMemberDeclaration VisitSecurityRequirement(OpenApiSecurityRequirement self);
        public abstract CSMemberDeclaration VisitResponse(OpenApiResponse self);
        public abstract CSMemberDeclaration VisitInfo(OpenApiInfo self);
        public abstract CSMemberDeclaration VisitComponents(OpenApiComponents self);
        public abstract CSMemberDeclaration VisitCallback(string key, OpenApiCallback self);
        public abstract CSMemberDeclaration VisitParameter(string key, OpenApiParameter self);
        public abstract CSMemberDeclaration VisitLink(string key, OpenApiLink self);
        public abstract CSMemberDeclaration VisitResponse(string key, OpenApiResponse self);
        public abstract CSMemberDeclaration VisitSchema(OpenApiSchema self);
        public abstract CSMemberDeclaration VisitJsonSchema(string kind, string key, OpenApiSchema self);
        public abstract CSMemberDeclaration VisitSecurityScheme(string key, OpenApiSecurityScheme self);
        public abstract CSMemberDeclaration VisitPathItem(string key, OpenApiPathItem self);
        public abstract CSMemberDeclaration VisitOperation(string key, OpenApiOperation self);
        public abstract CSMemberDeclaration VisitParameter(OpenApiParameter self);
        public abstract CSMemberDeclaration VisitEnumPrimitive(IOpenApiPrimitive self);



        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        protected void Stop()
        {
            System.Diagnostics.Debugger.Break();
        }

        protected readonly string _namespace;
        private readonly string _artifactName;
        private readonly string[] _usings;
        protected readonly DeclarationBloc _tree;
        protected OpenApiDocument _self;
        protected ContextGenerator _ctx;
        protected Data _datas = new Data();

    }

}