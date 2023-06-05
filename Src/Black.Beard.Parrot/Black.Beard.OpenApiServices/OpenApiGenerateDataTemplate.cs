
using Bb;
using Bb.Codings;
using Bb.Expressions;
using Bb.Json.Jslt.Asts;
using Bb.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SharpYaml.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Xml.Schema;

namespace Bb.OpenApiServices
{

    public class OpenApiGenerateDataTemplate : OpenApiGeneratorJsltBase
    {
        private bool error;

        public OpenApiGenerateDataTemplate()
        {

        }


        public override JsltBase VisitDocument(OpenApiDocument self)
        {

            _self = self;
            foreach (var item in self.Paths)
                item.Value.Accept(item.Key, this);
            return null;

        }

        public override JsltBase VisitPathItem(string key, OpenApiPathItem self)
        {

            foreach (var item in self.Operations)
                item.Value.Accept(item.Key.ToString(), this);

            return null;

        }

        public override JsltBase VisitOperation(string key, OpenApiOperation self)
        {

            foreach (var item in ResolveResponseSchemas(self, "2"))
            {
                GenerateTemplate(self, item, string.Empty);
            }

            foreach (var item in ResolveResponseSchemas(self, "4"))
            {
                GenerateTemplate(self, item, "400");
            }

            foreach (var item in ResolveResponseSchemas(self, "5"))
            {
                GenerateTemplate(self, item, "500");
            }

            return null;

        }

        public override JsltBase VisitSchema(OpenApiSchema self)
        {
            if (self.Type == "object")
            {

                var required2 = this._code.CurrentBlock.Datas.GetData<bool>("required");
                var result = new JsltObject();

                foreach (var item in self.Properties)
                {
                    using (var current = this._code.Stack())
                    {
                        var required = self.Required.Select(c => c == item.Key).Any();
                        current.Datas.SetData("required", required);
                        var value = item.Value.Accept("property", item.Key, this);

                        var property = new JsltProperty()
                        {
                            Name = item.Key,
                            Value = value,
                        };

                        result.Append(property);

                    }
                }

                return result;

            }
            else if (self.Type == "array")
            {
                if (self.Items != null)
                {
                    var item1 = self.Items.Accept("", "", this);
                    var result = new JsltArray(1);
                    result.Items.Add(item1);
                    return result;
                }

            }
            else
            {

            }

            throw new NotImplementedException();

        }

        public override JsltBase VisitJsonSchema(string kind, string key, OpenApiSchema self)
        {

            if (self.Type == "array")
            {
                if (self.Items != null)
                {
                    var item1 = self.Items.Accept(kind, key, this);
                    var result = new JsltArray(1);
                    result.Items.Add(item1);
                    return result;
                }
            }
            else if (self.Type == "object")
            {
                var value = self.Accept(this);
                return value;

            }
            else
            {

                // var required = this._code.CurrentBlock.Datas.GetData<bool>("required");

                var type = self.ResolveType();

                JsltBase result = null;
                
                switch (self?.Format?.ToLower() ?? string.Empty)
                {
                    case "int32":
                        result = new JsltFunctionCall("getrandom_integer"
                            , new JsltConstant(self.Minimum), new JsltConstant(self.ExclusiveMinimum), new JsltConstant(self.Maximum), new JsltConstant(self.ExclusiveMaximum)
                        );
                        break;

                    case "date-time":
                        result = new JsltFunctionCall("getrandom_datatime");
                        break;

                    case "email":
                        result = new JsltFunctionCall("getrandom_email");
                        break;

                    case "hostname":
                        result = new JsltFunctionCall("getrandom_hostname");
                        break;

                    case "ipv4":
                        result = new JsltFunctionCall("getrandom_ipv4");
                        break;

                    case "ipv6":
                        result = new JsltFunctionCall("getrandom_ipv6");
                        break;

                    case "uri":
                        result = new JsltFunctionCall("getrandom_uri");
                        break;

                    case "binary":
                        result = new JsltFunctionCall("getrandom_binary", new JsltConstant(self.MinLength), new JsltConstant(self.MaxLength));
                        break;

                    case "password":
                        result = new JsltFunctionCall("getrandom_password", new JsltConstant(self.MinLength), new JsltConstant(self.MaxLength));
                        break;

                    case "uuid":
                        result = new JsltFunctionCall("uuid");
                        break;


                    case "":
                        switch (type.ToLower())
                        {
                            case "string":
                                result = new JsltFunctionCall("getrandom_string"
                                       , new JsltConstant(self.MinLength), new JsltConstant(self.MaxLength)
                                       , new JsltConstant(self.Pattern)
                                );
                                break;

                            case "int32":
                                result = new JsltFunctionCall("getrandom_integer"
                                       , new JsltConstant(self.Minimum), new JsltConstant(self.Maximum)
                                       , new JsltConstant(self.ExclusiveMinimum), new JsltConstant(self.ExclusiveMaximum)
                                );
                                break;

                            case "boolean":
                                result = new JsltFunctionCall("getrandom_boolean");
                                break;

                            default:
                                result = new JsltFunctionCall("getrandom_" + type
                                       , new JsltConstant(self.MinLength)
                                       , new JsltConstant(self.MaxLength)
                                       , new JsltConstant(self.Pattern)
                                       , new JsltConstant(self.Minimum)
                                       , new JsltConstant(self.Maximum)
                                       , new JsltConstant(self.ExclusiveMinimum)
                                       , new JsltConstant(self.ExclusiveMaximum)
                                );
                                break;
                        }
                        break;

                    default:
                        break;
                }

                if (result != null)
                    return result;

            }

            throw new NotImplementedException();

        }

        public override JsltBase VisitParameter(string key, OpenApiParameter self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitParameter(OpenApiParameter self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitResponse(OpenApiResponse self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitResponse(string key, OpenApiResponse self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitCallback(string key, OpenApiCallback self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitComponents(OpenApiComponents self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitInfo(OpenApiInfo self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitLink(string key, OpenApiLink self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitSecurityRequirement(OpenApiSecurityRequirement self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitSecurityScheme(OpenApiSecurityScheme self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitSecurityScheme(string key, OpenApiSecurityScheme self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitServer(OpenApiServer self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitTag(OpenApiTag self)
        {
            throw new NotImplementedException();
        }

        public override JsltBase VisitEnumPrimitive(IOpenApiPrimitive self)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<OpenApiSchema> ResolveResponseSchemas(OpenApiOperation self, string code)
        {

            string result = null;

            List<KeyValuePair<string, OpenApiSchema>> _resultTypes = new List<KeyValuePair<string, OpenApiSchema>>();
            foreach (var item2 in self.Responses)
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

            var item3 = _resultTypes.OrderBy(c => c.Key).Select(c => c.Value);

            return item3;

        }

        private static string ResolveName(OpenApiSchema item)
        {
            string name = "";
            var reference = item.Reference;
            if (reference != null && reference.Id != null)
                name = reference.Id;
            else
            {
                var o = item.Items;
                if (o != null)
                {
                    reference = o.Reference;
                    if (reference != null && reference.Id != null)
                        name = reference.Id;
                }
                else
                {

                }
            }

            return name;
        }

        private void GenerateTemplate(OpenApiOperation self, OpenApiSchema item, string code)
        {

            this.error = code != "2";

            string name = ResolveName(item);
            var templateName = $"template_{name}_jslt.json";
            var content = item.Accept(this);

            var target = _ctx.AppendDocument("Templates", templateName, content.ToString());
            _ctx.GetDataFor(self).SetData("templateName" + code, target);

        }

    }


}