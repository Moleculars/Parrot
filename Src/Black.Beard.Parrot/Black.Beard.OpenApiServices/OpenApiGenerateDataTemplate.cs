
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

namespace Black.Beard.OpenApiServices
{

    public class OpenApiGenerateDataTemplate : OpenApiGeneratorJsltBase
    {

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

            foreach (var item in ResolveResponseSchemas(self))
            {

                string name = ResolveName(item);
                var templateName = $"template_{name}_jslt.json";
                var content = item.Accept(this);

                _ctx.AppendDocument("Templates", templateName, content.ToString());

            }

            return null;

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

                var required = this._code.CurrentBlock.Datas.GetData<bool>("required");

                var type = self.ResolveType();

                switch (self?.Format?.ToLower() ?? string.Empty)
                {
                    case "int32":
                        return new JsltFunctionCall("getrandom_integer"
                            , new JsltConstant(self.Minimum), new JsltConstant(self.ExclusiveMinimum), new JsltConstant(self.Maximum), new JsltConstant(self.ExclusiveMaximum)
                        );
                    case "date-time":
                        return new JsltFunctionCall("getrandom_datatime");
                    case "email":
                        return new JsltFunctionCall("getrandom_email");
                    case "hostname":
                        return new JsltFunctionCall("getrandom_hostname");
                    case "ipv4":
                        return new JsltFunctionCall("getrandom_ipv4");
                    case "ipv6":
                        return new JsltFunctionCall("getrandom_ipv6");
                    case "uri":
                        return new JsltFunctionCall("getrandom_uri");
                    case "binary":
                        return new JsltFunctionCall("getrandom_binary", new JsltConstant(self.MinLength), new JsltConstant(self.MaxLength));
                    case "password":
                        return new JsltFunctionCall("getrandom_password", new JsltConstant(self.MinLength), new JsltConstant(self.MaxLength));
                    case "uuid":
                        return new JsltFunctionCall("uuid");

                    case "":
                        switch (type.ToLower())
                        {
                            case "string":
                                return new JsltFunctionCall("getrandom_string"
                                    , new JsltConstant(self.MinLength), new JsltConstant(self.MaxLength)
                                    , new JsltConstant(self.Pattern)
                                );
                            case "int32":
                                return new JsltFunctionCall("getrandom_integer"
                                    , new JsltConstant(self.Minimum), new JsltConstant(self.Maximum)
                                    , new JsltConstant(self.ExclusiveMinimum), new JsltConstant(self.ExclusiveMaximum)
                                );

                            case "boolean":
                                return new JsltFunctionCall("getrandom_boolean");

                            default:
                                return new JsltFunctionCall("getrandom_" + type
                                    , new JsltConstant(self.MinLength)
                                    , new JsltConstant(self.MaxLength)
                                    , new JsltConstant(self.Pattern)
                                    , new JsltConstant(self.Minimum)
                                    , new JsltConstant(self.Maximum)
                                    , new JsltConstant(self.ExclusiveMinimum)
                                    , new JsltConstant(self.ExclusiveMaximum)
                                );
                        }


                    default:
                        break;
                }

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

        private IEnumerable<OpenApiSchema> ResolveResponseSchemas(OpenApiOperation self)
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

            var item3 = _resultTypes.OrderBy(c => c.Key).Select(c => c.Value);

            return item3;

        }

    }


}