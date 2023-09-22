using Bb.Analysis;
using Bb.Codings;
using Bb.Json.Jslt.CustomServices;
using Bb.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Net;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace Bb.OpenApiServices
{

    public class ServiceGeneratorProcess<T>
    {


        public ServiceGeneratorProcess(ContextGenerator ctx)
        {
            this._ctx = ctx;
            _services = new List<IServiceGenerator<T>>();
        }


        public ServiceGeneratorProcess<T> Append(params IServiceGenerator<T>[] services)
        {

            this._services.AddRange(services);
            return this;
        }

        internal ServiceGeneratorProcess<T> Generate(T document)
        {

            foreach (var service in _services)
            {
                try
                {

                    service.Parse(document, _ctx);

                }
                catch (Exception ex)
                {

                    throw;
                }

                if (!this._ctx.Diagnostics.Success)
                    return this;
            
            }

            return this;

        }

        private readonly ContextGenerator _ctx;
        private List<IServiceGenerator<T>> _services;

    }

    public interface IServiceGenerator<T>
    {

        public void Parse(T self, ContextGenerator ctx);


    }

    public abstract class OpenApiGeneratorCSharpBase : DiagnosticGeneratorBase, IOpenApiDocumentGenericVisitor<CSMemberDeclaration>, IServiceGenerator<OpenApiDocument>
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
            Initialize(ctx);
            Trace.WriteLine("Generating " + _artifactName);
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

        public abstract CSMemberDeclaration VisitMediaType(string key, OpenApiMediaType self);

        public abstract CSMemberDeclaration VisitMediaType(OpenApiMediaType self);



        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        protected void Stop()
        {

            var st = new StackTrace();
            var f = st.GetFrame(1);
            Debug.WriteLine($"{f.ToString().Trim()} try to stop");

            if (Debugger.IsAttached)
                Debugger.Break();

        }

        protected readonly string _namespace;
        private readonly string _artifactName;
        private readonly string[] _usings;
        protected readonly DeclarationBloc _tree;
        protected OpenApiDocument _self;
        protected Data _datas = new Data();

    }




}