using Bb;
using Bb.Codings;
using Bb.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Net;

namespace Black.Beard.OpenApiServices
{


    public class OpenApiGenerateServices : OpenApiGeneratorCSharpBase
    {

        public OpenApiGenerateServices(string artifactName, string @namespace)
            : base(artifactName, @namespace, "Bb.Json.Jslt.Services",
                    "Bb.ParrotServices",
                    "Microsoft.AspNetCore.Http",
                    "Microsoft.AspNetCore.Mvc",
                    "Microsoft.Extensions.Logging",
                    "System",
                    "System.Text",
                    "Newtonsoft.Json",
                    "Newtonsoft.Json.Linq")
        {
         
        }

        public override CSMemberDeclaration VisitDocument(OpenApiDocument self)
        {

            foreach (var item in self.Paths)
                item.Value.Accept(item.Key, this);

            return null;

        }

        public override CSMemberDeclaration VisitPathItem(string key, OpenApiPathItem self)
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

            foreach (var item in self.Operations)
            {
                OpenApiOperation o = item.Value;
                var r = o.Accept(item.Key.ToString(), this);
                if (r != null)
                    crl.Add(r);
            }

            var cs = CreateArtifact(name);
            var ns = CreateNamespace(cs);
            ns.Add(crl);

             _ctx.AppendDocument("Controllers", name + ".cs", cs.Code().ToString());

            return null;

        }

        public override CSMemberDeclaration VisitOperation(string key, OpenApiOperation self)
        {

            string typeReturn = null;

            var method = new CsMethodDeclaration(key.ConvertToCharpName());
            key.ApplyHttpMethod(method);

            if (!string.IsNullOrEmpty(self.Description))
                method.Documentation.Summary(() => self.Description);

            using (var c = _tree.Stack(method))
            {

                if (self.RequestBody != null)
                    foreach(var item2 in self.RequestBody.Content)
                    {
                        var name = item2.Value.Schema.ResolveType();
                        if (name != null)
                        {                            
                            var p = new CsParameterDeclaration("queryBody", name);
                            p.Attribute("FromQuery");
                            var description = item2.Value.Schema.ResolveDescription();
                            if (!string.IsNullOrEmpty(description))
                                method.Documentation.Parameter(name, () => description);
                            method.Add(p);
                        }

                    }
                

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
                    lst.Thrown();
                })
                )
                ;


            });

            return method;

        }

        private string ResolveReturnType(OpenApiOperation self, CsMethodDeclaration method)
        {

            string result = null;

            List<KeyValuePair<string, OpenApiSchema>> _resultTypes = new List<KeyValuePair<string, OpenApiSchema>>();
            foreach (var item2 in self.Responses)
                if (item2.Key.StartsWith("2"))
                {
                    var t2 = item2.Value.Content.FirstOrDefault().Value;
                    if (t2 != null)
                    {
                        OpenApiSchema t = t2.Schema;
                        var v = t.ResolveType();
                        if (v != null)
                            _resultTypes.Add(new KeyValuePair<string, OpenApiSchema>(item2.Key, t));
                    }
                }

            var item3 = _resultTypes.OrderBy(c => c.Key).FirstOrDefault().Value;
            if (item3 != null)
            {
                result = item3.ResolveType();
                method.ReturnType(result);
                if (item3.IsJson())
                    _tree.Current.Attribute("Produces")
                        .Argument("application/json".Literal());
            }

            return result;

        }

        public override CSMemberDeclaration VisitParameter(OpenApiParameter self)
        {

            var n = self.Name;
            var t = self.Schema.ConvertTypeName();

            var p = new CsParameterDeclaration(n, t);
            p.ApplyAttributes(self);

            if (!string.IsNullOrEmpty(self.Description))
            {
                var method = this._tree.Current as CsMethodDeclaration;
                method.Documentation.Parameter(n, () => self.Description);
            }

            return p;

        }

        public override CSMemberDeclaration VisitResponse(string key, OpenApiResponse self)
        {
            return null;
        }

        public override CSMemberDeclaration VisitComponents(OpenApiComponents self)
        {
            return null;
        }

        public override CSMemberDeclaration VisitJsonSchema(string kind, string key, OpenApiSchema self)
        {
            return null;
        }
   
        public override CSMemberDeclaration VisitCallback(string key, OpenApiCallback self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitInfo(OpenApiInfo self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitLink(string key, OpenApiLink self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitParameter(string key, OpenApiParameter self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitResponse(OpenApiResponse self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitSchema(OpenApiSchema self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitSecurityRequirement(OpenApiSecurityRequirement self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitSecurityScheme(OpenApiSecurityScheme self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitSecurityScheme(string key, OpenApiSecurityScheme self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitServer(OpenApiServer self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitTag(OpenApiTag self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitEnumPrimitive(IOpenApiPrimitive self)
        {
            throw new NotImplementedException();
        }


    }


}