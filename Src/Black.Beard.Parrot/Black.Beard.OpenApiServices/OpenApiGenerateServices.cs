using Bb;
using Bb.Codings;
using Bb.OpenApi;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SharpYaml.Tokens;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Bb.OpenApiServices
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

            var _n = ConvertClassName(key);
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
            ns.DisableWarning("CS8618", "CS1591");

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
                    foreach (var item2 in self.RequestBody.Content)
                    {
                        var name = item2.Value.Schema.ResolveType();
                        if (name != null)
                        {
                            var p = new CsParameterDeclaration("queryBody", name);
                            p.Attribute("FromBody");
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

                foreach (var item2 in self.Responses.OrderBy(c => c.Key))
                    item2.Value.Accept(item2.Key, this);

                typeReturn = ResolveReturnType(self, method, "2");

                // Attribute ProduceResponse
                foreach (var item2 in self.Responses)
                {

                    var t2 = item2.Value.Content.FirstOrDefault().Value;
                    if (t2 != null)
                    {
                        OpenApiSchema t = t2.Schema;
                        var v = t.ResolveType();
                        if (v != null)
                        {
                            method.Attribute("ProducesResponseType", a =>
                            {
                                a.Argument(GeneratorHelper.CodeHttp(item2.Key));
                                a.Argument("Type", CodeHelper.TypeOf(v));
                            });
                        }
                    }
                }

            }

            method.Body(c =>
            {

                var templateName = _ctx.GetDataFor(self).GetData<string>("templateName");
                string diff = _ctx.GetRelativePath(templateName);

                c.TryCatchs
                (t =>
                {

                    // var service = new ServiceProcessor<ParcelTrackingList>();
                    var type = CodeHelper.AsType("ServiceProcessor", typeReturn);
                    t.DeclareLocalVar("var".AsType(), "service", type.NewObject());
                    foreach (var item3 in method.Items<CsParameterDeclaration>())
                        t.Add("service".Identifier().Call("Add", item3.Name.Literal(), item3.Name.Identifier()));
                    // return service.GetDatas("template.json", "datas.json");
                    t.DeclareLocalVar("var".AsType(), "result", "service".Identifier().Call("GetDatas", diff.Literal()));
                    t.Return(SyntaxFactory.ThisExpression().Call("Ok", "result".Identifier()));

                }, "Exception".AsType().Catch("ex", lst =>
                {

                    lst.Add(CodeHelper.DeclareLocalVar("errorId", "Guid".AsType(), "Guid".Identifier().Call("NewGuid")));
                    lst.Add("_logger".Identifier().Call("LogError", "ex".Identifier(), "ex".Identifier().MemberAccess("Message"), "errorId".Identifier()));

                    if (this.error500 != null)
                    {

                        var typeReturn500 = ResolveReturnType(self, method, "5");

                        var templateName500 = _ctx.GetDataFor(self).GetData<string>("templateName00");
                        string diff500 = _ctx.GetRelativePath(templateName);

                        var type = CodeHelper.AsType("ServiceProcessor", typeReturn500);
                        lst.DeclareLocalVar("var".AsType(), "service", type.NewObject());
                        foreach (var item3 in method.Items<CsParameterDeclaration>())
                            lst.Add("service".Identifier().Call("Add", "errorId".Literal(), "errorId".Identifier()));
                        lst.Add("service".Identifier().Call("Add", "message".Literal(), "Sorry, an error has occurred. Please contact our customer service with id for assistance.".Literal()));

                        lst.DeclareLocalVar("var".AsType(), "result", "service".Identifier().Call("GetDatas", diff500.Literal()));
                        lst.Return(SyntaxFactory.ThisExpression().Call("BadRequest", "result".Identifier()));

                    }
                    else
                    {
                        var arg = "errorId".Identifier();
                        lst.Add(CodeHelper.Return(SyntaxFactory.ThisExpression().Call("BadRequest", arg)));
                    }


                    //lst.Thrown();

                })
                )
                ;


            });

            return method;

        }

        private string ResolveReturnType(OpenApiOperation self, CsMethodDeclaration method, string code)
        {

            string result = null;

            List<KeyValuePair<string, OpenApiSchema>> _resultTypes = new List<KeyValuePair<string, OpenApiSchema>>();
            foreach (var item2 in self.Responses)
            {

                if (item2.Key.StartsWith(code))
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

            }

            var item3 = _resultTypes.OrderBy(c => c.Key).FirstOrDefault().Value;
            if (item3 != null)
            {

                result = item3.ResolveType();

                if (code == "2")
                {

                    var typeReturn = CodeHelper.BuildTypename("ActionResult", result).ToString();
                    method.ReturnType(typeReturn);


                    if (item3.IsJson())
                        _tree.Current.Attribute("Produces")
                            .Argument("application/json".Literal());
                }
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

            switch (key[0])
            {

                case '2':
                    break;

                case '4':
                    if (!error400.HasValue)
                        error400 = new KeyValuePair<string, OpenApiResponse>(key, self);
                    else
                    {
                        if (int.Parse(error400.Value.Key) > int.Parse(key))
                            error400 = new KeyValuePair<string, OpenApiResponse>(key, self);
                    }
                    break;

                case '5':
                    if (!error500.HasValue)
                        error500 = new KeyValuePair<string, OpenApiResponse>(key, self);
                    else
                    {
                        if (int.Parse(error500.Value.Key) > int.Parse(key))
                            error500 = new KeyValuePair<string, OpenApiResponse>(key, self);
                    }
                    break;

                default:
                    Stop();
                    break;

            }

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

        public static string ConvertClassName(string text)
        {

            var _n = text.Trim('/');
            var regex = new Regex(@"{|}");
            var items = regex.Matches(_n);
            foreach (Match item in items)
                _n = _n.Replace(item.Value, string.Empty);

            _n = _n.Trim('/');

            StringBuilder stringBuilder = new StringBuilder(_n?.Length ?? 0);
            if (text != null)
            {
                char c = '\0';
                for (int i = 0; i < text.Length; i++)
                {
                    char c2 = text[i];
                    if (!char.IsLetterOrDigit(c2))
                    {
                        c2 = '/';
                    }

                    if (stringBuilder.Length == 0)
                    {
                        c2 = char.ToUpper(c2);
                    }
                    else
                    {
                        if (c2 == '/')
                        {
                            c = c2;
                            continue;
                        }

                        c2 = ((c != '/') ? char.ToLower(c2) : char.ToUpper(c2));
                    }

                    stringBuilder.Append(c2);
                    c = c2;
                }
            }

            return stringBuilder.ToString().Trim().Trim('/');
        }


        private KeyValuePair<string, OpenApiResponse>? error500 = null;
        private KeyValuePair<string, OpenApiResponse>? error400 = null;

    }


}