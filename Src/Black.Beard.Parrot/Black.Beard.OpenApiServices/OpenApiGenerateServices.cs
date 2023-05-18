using Bb.Codings;
using Bb.OpenApi;
using Namotion.Reflection;
using NJsonSchema;
using NSwag;

namespace Black.Beard.OpenApiServices
{
    public class OpenApiGenerateServices : IOpenApiDocumentVisitor<CSMemberDeclaration>
    {

        public OpenApiGenerateServices(string artifactName, string @namespace)
        {

            _cs = new CSharpArtifact(artifactName)
                .Usings("System",
                "Microsoft.Extensions.Logging",
                "Microsoft.AspNetCore.Mvc",
                "Microsoft.AspNetCore.Http",
                "Bb.Json.Jslt.Services",
                "System.Text",
                "Newtonsoft.Json.Linq",
                "Bb.Mock",
                "Black.Beard.OpenApiServices.Embedded"

                );
            ;

            _ns = _cs.Namespace(@namespace);

        }

        public CSMemberDeclaration VisitDocument(OpenApiDocument self)
        {
            _self = self;
            foreach (var item in self.Paths)
            {

                var service = item.Value.Accept(item.Key, this);
                if (service != null)
                    _ns.Add(service);

            }
            return _cs;
        }

        public CSMemberDeclaration VisitPathItem(string key, OpenApiPathItem self)
        {

            var _n = key.Trim('/');
            string name = _n + "Controller";
            string typeLogger = $"ILogger<{name}>";

            CsClassDeclaration crl = new CsClassDeclaration(name)
                .Base("Controller")
                .Field("_logger", typeLogger, f =>
                {
                    f.IsPrivate()
                    ;
                })
                .Ctor(c =>
                {
                    c.Parameter("logger", typeLogger);
                    c.Body(b =>
                    {
                        b.Set("_logger".Identifier(), "logger".Identifier());
                    });
                    ;
                })
                ;

            crl.Attribute("ApiController");
            crl.Attribute("Route")
                .Argument(key.Literal())
                ;

            foreach (var item in self)
            {
                var r = item.Value.Accept(item.Key, this);
                if (r != null)
                    crl.Add(r);
            }

            return crl;

        }

        public CSMemberDeclaration VisitOperation(string key, OpenApiOperation self)
        {

            string typeReturn = null;

            var method = new CsMethodDeclaration(key.ConvertToCharpName());
            key.ApplyHttpMethod(method);

            if (!string.IsNullOrEmpty(self.Description))
                method.Documentation.Summary(() => self.Description);

            using (var c = _tree.Stack(method))
            {

                foreach (var item1 in self.Parameters)
                {
                    var p = item1.Accept(this);
                    if (p != null)
                        method.Add(p);
                }


                typeReturn = ResolveReturnType(self, method);


                foreach (var item2 in self.Responses)
                    item2.Value.Accept(item2.Key, this);


            }

            method.Body(c =>
            {

                c.TryCatchs
                (t =>
                {
                    // var service = new ServiceProcessor<ParcelTrackingList>();
                    var type = CodeHelper.AsType("ServiceProcessor", typeReturn);
                    t.DeclareLocalVar("var".AsType(), "service", type.NewObject());
                    foreach (var item3 in method.Items<CsParameterDeclaration>())
                        t.Add("service".Identifier().Call("Add", item3.Name.Literal(), item3.Name.Identifier()));
                    // return service.GetDatas("template.json", "datas.json");
                    t.DeclareLocalVar("var".AsType(), "result", "service".Identifier().Call("GetDatas", "template.json".Literal(), "datas.json".Literal()));
                    t.Return("result".Identifier());

                }, "Exception".AsType().Catch("ex", lst =>
                {
                    var p = "_logger".Identifier().Call("LogError", "ex".Identifier(), "ex".Identifier().MemberAccess("Message"));
                    lst.Add(p.ToStatement());
                    lst.Thrown("ex".Identifier());
                })
                )
                ;


            });

            return method;

        }

        private string ResolveReturnType(OpenApiOperation self, CsMethodDeclaration method)
        {

            string result = null;

            List<KeyValuePair<string, JsonSchema>> _resultTypes = new List<KeyValuePair<string, JsonSchema>>();
            foreach (var item2 in self.Responses)
                if (item2.Key.StartsWith("2"))
                {
                    var t2 = item2.Value.Content.FirstOrDefault().Value;
                    if (t2 != null)
                    {
                        var t = t2.Schema;
                        var v = t.ResolveType(this._self.Components);
                        if (v != null)
                            _resultTypes.Add(new KeyValuePair<string, JsonSchema>(item2.Key, t));
                    }
                }

            var item3 = _resultTypes.OrderBy(c => c.Key).FirstOrDefault().Value;
            if (item3 != null)
            {
                result = item3.ResolveType(this._self.Components);
                method.ReturnType(result);
                if (item3.IsJson())
                    _tree.Current.Attribute("Produces")
                        .Argument("application/json".Literal());
            }

            return result;

        }

        public CSMemberDeclaration VisitParameter(OpenApiParameter self)
        {

            var n = self.Name;
            var t = self.Schema.ResolveType(this._self.Components);

            var p = new CsParameterDeclaration(n, t);
            p.ApplyAttributes(self);

            if (self.Kind == OpenApiParameterKind.Body)
            {
                _tree.Current.Attribute("Consumes")
                    .Argument("application/json".Literal());
            }

            if (!string.IsNullOrEmpty(self.Description))
            {
                var method = this._tree.Current as CsMethodDeclaration;
                method.Documentation.Parameter(n, () => self.Description);
            }

            return p;

        }

        public CSMemberDeclaration VisitResponse(string key, OpenApiResponse self)
        {

            var description = self.Description;
            var c = self.Content;
            foreach (var item in c)
            {
                string _key = item.Key;
                var schema = item.Value.Schema;

                var attr = new CsAttributeDeclaration("ProducesResponseType");
                attr.Argument(key.CodeHttp());
                var type = schema.ResolveType(this._self.Components);
                if (type != null)
                    attr.Argument("Type", CodeHelper.TypeOf(type.AsType()));

                _tree.Current.Add(attr);

            }

            return null;

        }

        public CSMemberDeclaration VisitJsonSchema(JsonSchema self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitComponents(OpenApiComponents self)
        {

            //foreach (var item in self.Schemas)
            //{
            //    var cls = item.Value.Accept(item.Key, this);
            //    if (cls != null)
            //        _ns.Add(cls);
            //}

            return _ns;

        }

        public CSMemberDeclaration VisitJsonSchema(string key, JsonSchema self)
        {



            return null;
        }

        public CSMemberDeclaration VisitJsonSchemaProperty(JsonSchemaProperty self)
        {


            return null;

        }


        public CSMemberDeclaration VisitCallback(string key, OpenApiCallback self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitExternalDocumentation(OpenApiExternalDocumentation self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitInfo(OpenApiInfo self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitLink(string key, OpenApiLink self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitOperationDescription(OpenApiOperationDescription self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitParameter(string key, OpenApiParameter self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitResponse(OpenApiResponse self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitSchema(OpenApiSchema self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitSecurityRequirement(OpenApiSecurityRequirement self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitSecurityScheme(OpenApiSecurityScheme self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitSecurityScheme(string key, OpenApiSecurityScheme self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitServer(OpenApiServer self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitTag(OpenApiTag self)
        {
            throw new NotImplementedException();
        }


        private DeclarationBloc _tree = new DeclarationBloc();
        private OpenApiDocument _self;
        private readonly CSharpArtifact _cs;
        private readonly CSNamespace _ns;

    }


}