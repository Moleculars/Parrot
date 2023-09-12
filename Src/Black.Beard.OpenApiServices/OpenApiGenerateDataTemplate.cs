
using Bb;
using Bb.Codings;
using Bb.Expressions;
using Bb.Json.Jslt.Asts;
using Bb.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SharpYaml.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
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

            var typeName = self.Type;

            if (typeName == null)
            {

                if (self.Properties.Any())
                    typeName = "object";

                else if (self.Items != null)
                    typeName = "array";

            }
           
            if (typeName == "object")
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

            else if (typeName == "array")
            {
                if (self.Items != null)
                {
                    var item1 = self.Items.Accept("", "", this);
                    var result = new JsltArray(1);
                    result.Items.Add(item1);
                    return result;
                }
                Stop();
            }

            else if (typeName == "string")
            {

                var result = new JsltObject();
                var d = new JsltDirectives()
                    .SetCulture(CultureInfo.CurrentCulture)
                    .Output(c => c.SetFilter("$.value"))
                    ;
                result.Append(d);

                JsltBase value = null;
                switch (self?.Format?.ToLower() ?? string.Empty)
                {

                    case "float":
                        value = new JsltFunctionCall("getrandom_float"
                            , new JsltConstant(self?.Minimum, JsltKind.Float), new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.Maximum, JsltKind.Integer), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                        );
                        break;

                    case "int32":
                        value = new JsltFunctionCall("getrandom_integer"
                            , new JsltConstant(self?.Minimum, JsltKind.Integer), new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.Maximum, JsltKind.Integer), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                        );
                        break;

                    case "date":
                    case "date-time":
                        value = new JsltFunctionCall("getrandom_datatime");
                        break;

                    case "email":
                        value = new JsltFunctionCall("getrandom_email");
                        break;

                    case "hostname":
                        value = new JsltFunctionCall("getrandom_hostname");
                        break;

                    case "ipv4":
                        value = new JsltFunctionCall("getrandom_ipv4");
                        break;

                    case "ipv6":
                        value = new JsltFunctionCall("getrandom_ipv6");
                        break;

                    case "uri":
                        value = new JsltFunctionCall("getrandom_uri");
                        break;

                    case "binary":
                        value = new JsltFunctionCall("getrandom_binary", new JsltConstant(self.MinLength, JsltKind.Integer), new JsltConstant(self.MaxLength, JsltKind.Integer));
                        break;

                    case "password":
                        value = new JsltFunctionCall("getrandom_password", new JsltConstant(self.MinLength, JsltKind.Integer), new JsltConstant(self.MaxLength, JsltKind.Integer));
                        break;

                    case "uuid":
                        value = new JsltFunctionCall("uuid");
                        break;

                    default:
                    case "":
                        value = new JsltFunctionCall("getrandom_string"
                           , new JsltConstant(self.MinLength, JsltKind.Integer), new JsltConstant(self.MaxLength, JsltKind.Integer)
                           , new JsltConstant(self.Pattern, JsltKind.String)
                        );
                        break;
                }


                result.Append(new JsltProperty() { Name = "value", Value = value });
                return result;
            }

            else if (typeName == "boolean")
            {

                var result = new JsltObject();
                var d = new JsltDirectives()
                    .SetCulture(CultureInfo.CurrentCulture)
                    .Output(c => c.SetFilter("$.value"))
                    ;
                result.Append(d);
                var value = new JsltFunctionCall("getrandom_boolean");
                result.Append(new JsltProperty() { Name = "value", Value = value });
                return result;
            }

            else if (typeName == "integer")
            {

                var result = new JsltObject();
                var d = new JsltDirectives()
                    .SetCulture(CultureInfo.CurrentCulture)
                    .Output(c => c.SetFilter("$.value"))
                    ;
                result.Append(d);
                var value = new JsltFunctionCall("getrandom_integer"
                            , new JsltConstant(self?.Minimum, JsltKind.Integer), new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.Maximum, JsltKind.Integer), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                        );
                result.Append(new JsltProperty() { Name = "value", Value = value });
                return result;
            }

            Stop();
            throw new NotImplementedException();

        }

        public override JsltBase VisitJsonSchema(string kind, string key, OpenApiSchema self)
        {

            var typeName = self.Type;

            if (typeName == null)
            {

                if (self.Properties.Any())
                    typeName = "object";

                else if (self.Items != null)
                    typeName = "array";

            }

            if (typeName == "array")
            {
                if (self.Items != null)
                {
                    var item1 = self.Items.Accept(kind, key, this);
                    var result = new JsltArray(1);
                    result.Items.Add(item1);
                    return result;
                }
            }

            else if (typeName == "object")
            {
                var value = self.Accept(this);
                return value;

            }

            else
            {

                JsltBase result = null;

                if (self.Enum.Count > 0)
                {
                    List<JsltBase> list = new List<JsltBase>(self.Enum.Count);
                    foreach (IOpenApiAny item in self.Enum)
                    {
                        if (item is IOpenApiPrimitive p)
                            list.Add(p.Accept(this));
                       
                        else
                        {
                            Stop();
                        }
                    }
                    result = new JsltFunctionCall("getrandom_in_list", new JsltArray(list));
                }
                else
                {

                    var type = self.ResolveType(out var schema2);

                    switch (self?.Format?.ToLower() ?? string.Empty)
                    {

                        case "float":
                            result = new JsltFunctionCall("getrandom_float"
                                , new JsltConstant(self?.Minimum, JsltKind.Float), new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.Maximum, JsltKind.Integer), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                            );
                            break;

                        case "int32":
                            result = new JsltFunctionCall("getrandom_integer"
                                , new JsltConstant(self?.Minimum, JsltKind.Integer), new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.Maximum, JsltKind.Integer), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                            );
                            break;

                        case "date":
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
                            result = new JsltFunctionCall("getrandom_binary", new JsltConstant(self.MinLength, JsltKind.Integer), new JsltConstant(self.MaxLength, JsltKind.Integer));
                            break;

                        case "password":
                            result = new JsltFunctionCall("getrandom_password", new JsltConstant(self.MinLength, JsltKind.Integer), new JsltConstant(self.MaxLength, JsltKind.Integer));
                            break;

                        case "uuid":
                            result = new JsltFunctionCall("uuid");
                            break;


                        case "":
                            switch (type.ToLower())
                            {
                                case "string":
                                    result = new JsltFunctionCall("getrandom_string"
                                           , new JsltConstant(self.MinLength, JsltKind.Integer), new JsltConstant(self.MaxLength, JsltKind.Integer)
                                           , new JsltConstant(self.Pattern, JsltKind.String)
                                    );
                                    break;

                                case "int32":
                                    result = new JsltFunctionCall("getrandom_integer"
                                           , new JsltConstant(self.Minimum, JsltKind.Integer), new JsltConstant(self.Maximum, JsltKind.Integer)
                                           , new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean), new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                                    );
                                    break;

                                case "boolean":
                                    result = new JsltFunctionCall("getrandom_boolean");
                                    break;

                                default:
                                    result = new JsltFunctionCall("getrandom_" + type
                                           , new JsltConstant(self.MinLength, JsltKind.Integer)
                                           , new JsltConstant(self.MaxLength, JsltKind.Integer)
                                           , new JsltConstant(self.Pattern, JsltKind.String)
                                           , new JsltConstant(self.Minimum, JsltKind.Integer)
                                           , new JsltConstant(self.Maximum, JsltKind.Integer)
                                           , new JsltConstant(self.ExclusiveMinimum, JsltKind.Boolean)
                                           , new JsltConstant(self.ExclusiveMaximum, JsltKind.Boolean)
                                    );
                                    break;
                            }
                            break;

                        default:
                            break;
                    }

                }
                if (result != null)
                    return result;

            }

            Stop();
            throw new NotImplementedException();

        }

        public override JsltConstant VisitEnumPrimitive(IOpenApiPrimitive self)
        {

            switch (self.PrimitiveType)
            {
                case PrimitiveType.Integer:
                    var e1 = self as OpenApiInteger;
                    return new JsltConstant(e1.Value, JsltKind.Integer);

                case PrimitiveType.String:
                    var e2 = self as OpenApiString;
                    return new JsltConstant(e2.Value, JsltKind.String);

                case PrimitiveType.Long:
                    Stop();
                    break;
                case PrimitiveType.Float:
                    Stop();
                    break;
                case PrimitiveType.Double:
                    Stop();
                    break;

                case PrimitiveType.Byte:
                    Stop();
                    break;
                case PrimitiveType.Binary:
                    Stop();
                    break;
                case PrimitiveType.Boolean:
                    Stop();
                    break;
                case PrimitiveType.Date:
                    Stop();
                    break;
                case PrimitiveType.DateTime:
                    Stop();
                    break;
                case PrimitiveType.Password:
                    Stop();
                    break;
                default:
                    Stop();
                    break;
            }

            Stop();
            throw new NotImplementedException();

        }

        public override JsltBase VisitParameter(string key, OpenApiParameter self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitParameter(OpenApiParameter self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitResponse(OpenApiResponse self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitResponse(string key, OpenApiResponse self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitCallback(string key, OpenApiCallback self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitComponents(OpenApiComponents self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitInfo(OpenApiInfo self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitLink(string key, OpenApiLink self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitSecurityRequirement(OpenApiSecurityRequirement self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitSecurityScheme(OpenApiSecurityScheme self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitSecurityScheme(string key, OpenApiSecurityScheme self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitServer(OpenApiServer self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitTag(OpenApiTag self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitMediaType(string key, OpenApiMediaType self)
        {
            Stop();
            throw new NotImplementedException();
        }

        public override JsltBase VisitMediaType(OpenApiMediaType self)
        {
            Stop();
            throw new NotImplementedException();
        }
        private class Response
        {
            public string Code { get; internal set; }
            public OpenApiSchema Schema { get; internal set; }
            public string Kind { get; internal set; }
        }

        private IEnumerable<Response> ResolveResponseSchemas(OpenApiOperation self, string code)
        {

            string result = null;

            List<Response> _resultTypes = new List<Response>();
            foreach (var item2 in self.Responses)
                if (item2.Key.StartsWith(code))
                {
                    var content = item2.Value.Content.FirstOrDefault();
                    var t2 = content.Value;
                    if (t2 != null)
                    {
                        OpenApiSchema t = t2.Schema;
                        var v = t.ResolveType(out var schema2);
                        if (v != null)
                        {
                            if (schema2 != null)
                            {
                                Stop();
                            }
                            _resultTypes.Add(new Response() { Code = item2.Key, Schema = t, Kind = content.Key });
                        }
                    }
                }

            var item3 = _resultTypes.OrderBy(c => c.Code);

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

        private void GenerateTemplate(OpenApiOperation self, Response item, string code)
        {

            this.error = code != "2";

            string name = ResolveName(item.Schema);
            var templateName = $"template_{name}_jslt.json";

            if (item.Kind == @"application/json")
            {
                var content = item.Schema.Accept(this);

                var target = _ctx.AppendDocument("Templates", templateName, content.ToString());
                _ctx.GetDataFor(self).SetData("templateName" + code, target);

            }
            else
            {
                Stop();
            }
        }

    }


}