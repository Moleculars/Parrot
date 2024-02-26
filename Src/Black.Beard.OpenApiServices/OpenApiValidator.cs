using Bb.Analysis;
using Bb.Codings;
using Bb.Extensions;
using Bb.Json.Jslt.CustomServices.MultiCsv;
using Bb.Json.Jslt.Parser;
using Bb.Json.Jslt.Services;
using Bb.OpenApi;
using DataAnnotationsExtensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SharpCompress.Compressors.Xz;
using SharpYaml.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security;
using System.Text;

namespace Bb.OpenApiServices
{

    public class OpenApiValidator : DiagnosticGeneratorBase, IServiceGenerator<OpenApiDocument>, IOpenApiVisitor
    {

        internal OpenApiValidator()
        {
            this._h = new HashSet<string>();
        }

        public void Parse(OpenApiDocument self, ContextGenerator ctx)
        {
            this._h.Clear();
            Initialize(ctx);
            self.Accept(this);
        }

        public void VisitDocument(OpenApiDocument self)
        {
            self.Components.Accept(this);
            self.Paths.Accept(this);
        }

        public void VisitPath(OpenApiPaths self)
        {
            foreach (var item in self)
            {
                item.Value.Accept(item.Key, this);
            }
        }

        public void VisitComponents(OpenApiComponents self)
        {
            self.Schemas.Accept(this);
        }

        public void VisitSchemas(IDictionary<string, OpenApiSchema> self)
        {

            foreach (KeyValuePair<string, OpenApiSchema> item in self)
            {

                if (item.Value.IsEmptyType())
                    _diag.AddError(GetLocation, item.Key, $"Should specify a type");

                else
                {

                    if (item.Value.Type == "array")
                    {
                        if (item.Value.Items != null)
                            item.Value.Items.Accept(this);
                        
                        else
                        {
                            //_diag.AddError(GetLocation, item.Key, $"Should specify a type");
                            Stop();
                        }

                    }

                    else if (item.Value.Enum.Count > 0)
                        item.Value.Accept("enum", item.Key, this);

                    else
                        item.Value.Accept("class", item.Key, this);
                }

            }

        }


        public void VisitJsonSchema(string kind, string key, OpenApiSchema self)
        {

            switch (kind)
            {

                case "enum":
                    VisitJsonSchemaForEnum(self);
                    break;

                case "class":

                    var typeName = self.Type;

                    if (typeName == null)
                    {

                        if (_h.Add(key))
                        {
                            if (self.Properties.Any())
                                VisitJsonSchemaForClass(key, self, "object");

                            else if (self.Items != null)
                                VisitJsonSchemaForClass(key, self, "array");

                            else
                            {
                                Stop();
                            }
                        }
                    }
                    else if (_h.Add(key))
                        VisitJsonSchemaForClass(key, self, "object");
                    break;

                case "property":
                    VisitJsonSchemaProperty(key, self);
                    break;

                default:
                    break;

            }

        }

        private void VisitJsonSchemaForClass(string key, OpenApiSchema self, string? typeName)
        {
            if (typeName == "array")
            {
                if (self.Items != null)
                {
                    Stop();
                }
                else
                {
                    Stop();
                }

            }

            else if (typeName == "object")
            {

                PushPath("properties");
                foreach (var item in self.Properties)
                    item.Value.Accept("property", item.Key, this);
                PopPath();

                PushPath("required");
                foreach (string item in self.Required)
                {
                    PushPath(item);
                    if (!self.Properties.Any(c => c.Key == item))
                        _diag.AddError(GetLocation, item, $"the required property {item} is not found in the {key} object");
                    PopPath();
                }
                PopPath();

            }

            else
            {
                Stop();
            }
        }

        private void VisitJsonSchemaForEnum(OpenApiSchema self)
        {
            PushPath("enum");
            foreach (IOpenApiAny item in self.Enum)
            {
                if (item is IOpenApiPrimitive p)
                    p.Accept(this);
                else
                {
                    Stop();
                }
            }
            PopPath();
        }


        private DiagnosticLocation GetLocation => new DiagnosticLocation(this._ctx.ContractDocumentFilename, GetPath());


        public void VisitEnumPrimitive(IOpenApiPrimitive self)
        {

            switch (self.PrimitiveType)
            {
                case PrimitiveType.Integer:
                case PrimitiveType.String:
                case PrimitiveType.Long:
                case PrimitiveType.Float:
                case PrimitiveType.Double:
                case PrimitiveType.Byte:
                case PrimitiveType.Binary:
                case PrimitiveType.Boolean:
                case PrimitiveType.Date:
                case PrimitiveType.DateTime:
                case PrimitiveType.Password:
                    break;
                default:
                    Stop();
                    break;
            }

        }

        public void VisitJsonSchemaProperty(string propertyName, OpenApiSchema self)
        {

            if (self.Type == null)
            {
                if (self.Reference == null)
                    _diag.AddError(GetLocation, "type", $"type or reference of the property {propertyName} should be specified");

                else if (self.Properties.Count > 0)                
                    _diag.AddWarning(GetLocation, "type", $"type of the property {propertyName} should be 'object'");

                else if (self.Enum.Count > 0)
                    _diag.AddError(GetLocation, "type", $"type of the property {propertyName} should be specified");

            }
            else
            {

                var accept = CheckType(self);

                if (!accept.AcceptMinMaxValue)
                {

                    if (self.Maximum.HasValue)
                        _diag.AddError(GetLocation, self.Format, $"Maximum is unexpected for the type {self.Type.ToLower()}");

                    if (self.Minimum.HasValue)
                        _diag.AddError(GetLocation, self.Format, $"minimum is unexpected for the type {self.Type.ToLower()}");

                }
                else if (!accept.AcceptComma)
                {

                    if (self.Maximum.HasValue && HasComma(self.Maximum.Value))
                        _diag.AddError(GetLocation, self.Format, $"Maximum float with decimal is unexpected for the type {self.Type.ToLower()}");

                    if (self.Minimum.HasValue && HasComma(self.Minimum.Value))
                        _diag.AddError(GetLocation, self.Format, $"minimum float with decimal is unexpected for the type {self.Type.ToLower()}");
                }

                if (!accept.AcceptMinMaxItems)
                {
                    if (self.UniqueItems.HasValue)
                        _diag.AddError(GetLocation, self.Format, $"uniqueItems is unexpected for the type {self.Type.ToLower()}");

                    if (self.MaxItems.HasValue)
                        _diag.AddError(GetLocation, self.Format, $"maxItems is unexpected for the type {self.Type.ToLower()}");

                    if (self.MinItems.HasValue)
                        _diag.AddError(GetLocation, self.Format, $"minItems is unexpected for the type {self.Type.ToLower()}");

                }

                if (!accept.AcceptMinMaxLength)
                {

                    if (self.MaxLength.HasValue)
                        _diag.AddError(GetLocation, self.Format, $"maxLength is unexpected for the type {self.Type.ToLower()}");

                    if (self.MinLength.HasValue)
                        _diag.AddError(GetLocation, self.Format, $"minLength is unexpected for the type {self.Type.ToLower()}");

                }


            }

        }

        private bool HasComma(decimal value)
        {

            return decimal.Round(value) != value;

        }

        private ExceptedProperties CheckType(OpenApiSchema self)
        {

            ExceptedProperties result = new ExceptedProperties();

            switch (self.Type.ToLower())
            {

                case "boolean":
                    break;

                case "integer":
                    result.AcceptMinMaxValue = true;
                    if (!string.IsNullOrEmpty(self.Format))
                        switch (self.Format)
                        {
                            case "int32":
                            case "int64":
                                break;
                            default:
                                _diag.AddError(GetLocation, self.Format, $"the format {self.Format} is not managed for {self.Type.ToLower()}");
                                break;
                        }
                    break;

                case "number":
                    result.AcceptMinMaxValue = true;
                    result.AcceptComma = true;
                    if (!string.IsNullOrEmpty(self.Format))
                        switch (self.Format)
                        {
                            case "double":
                            case "float":
                                break;
                            default:
                                _diag.AddError(GetLocation, self.Format, $"the format {self.Format} is not managed for {self.Type.ToLower()}");
                                break;
                        }
                    break;

                case "string":
                    result.AcceptMinMaxLength = true;
                    if (!string.IsNullOrEmpty(self.Format))
                        switch (self.Format)
                        {
                            case "email":
                            case "hostname":
                            case "ipv4":
                            case "ipv6":
                            case "uri":
                            case "binary":
                            case "byte":
                            case "password":
                            case "uuid":
                            case "date":
                            case "date-time":
                                break;
                            default:
                                _diag.AddError(GetLocation, self.Format, $"the format {self.Format} is not managed for string");
                                break;
                        }

                    if (self.Enum.Count > 0)
                        foreach (var item in self.Enum)
                            if (item is IOpenApiPrimitive primitive)
                                if (primitive.PrimitiveType != PrimitiveType.String)
                                {
                                    dynamic d = item as dynamic;
                                    _diag.AddError(GetLocation, self.Format, $"the enum value {d.Value} should be {primitive.PrimitiveType} type");
                                }

                    break;

                case "array":
                    result.AcceptMinMaxItems = true;
                    if (self.Items != null)
                    {
                                                
                        if (self.Items.Reference != null)
                            self.Items.Accept("class", self.Items.Reference.Id, this);

                        else if (self.IsEmptyType())
                            _diag.AddError(GetLocation, "", $"Array has not specified type");

                    }
                    else if (self.OneOf != null && self.OneOf.Count > 0)
                    {
                        Stop();
                    }
                    break;

                case "object":
                    break;

                //case JsonObjectType.Null:
                //case JsonObjectType.File:
                //case JsonObjectType.None:
                default:
                    _diag.AddError(GetLocation, self.Format, $"the format {self.Format} is not managed for string");
                    break;
            }

            return result;

        }

        private LiteralExpressionSyntax GetLiteral(decimal value)
        {
            return Convert.ChangeType(value, typeof(double)).Literal();
        }

        public void VisitPathItem(string key, OpenApiPathItem self)
        {
            PushPath("operations");
            foreach (var item in self.Operations)
            {
                OpenApiOperation o = item.Value;
                o.Accept(item.Key.ToString(), this);
            }
            PopPath();
        }

        public void VisitOperation(string key, OpenApiOperation self)
        {

            PushPath("parameters");
            foreach (var item in self.Parameters)
                item.Accept(this);
            PopPath();

            if (self.RequestBody != null)
            {

                foreach (var item in self.RequestBody.Content)
                {

                    var t = item.Value.Schema;
                    var t1 = t.ConvertTypeName();

                    if (t1 == typeof(object))
                        t.Accept("class", key, this);

                    else if (t1 == typeof(Array))
                    {
                        if (t.Items != null)
                        {

                            if (t.Items.ResolveName() == null)
                                t.Items.Accept("class", key, this);

                        }
                        else if (t.Reference != null)
                        {

                        }
                        else
                        {
                            Stop();
                        }
                    }
                    else
                    {
                        Stop();
                    }

                }

            }

            PushPath("responses");
            foreach (var item in self.Responses)
                item.Value.Accept(item.Key, this);
            PopPath();

        }

        public void VisitCallback(string key, OpenApiCallback self)
        {
            Stop();
        }

        public void VisitInfo(OpenApiInfo self)
        {
            Stop();
        }

        public void VisitLink(string key, OpenApiLink self)
        {
            Stop();
        }


        public void VisitResponse(OpenApiResponse self)
        {
            Stop();
        }

        public void VisitResponse(string key, OpenApiResponse self)
        {

            if (self.Content != null || self.Content.Count == 0)
            {
                foreach (var item in self.Content)
                    item.Value.Accept(item.Key, this);

            }
            else
            {
                _diag.AddError(GetLocation, "content", $"the http result code '{key}' should contains mime result type.");
            }

            if (self.Headers != null)
            {
                foreach (var item in self.Headers)
                {

                    Stop();

                }
            }

        }

        public void VisitMediaType(OpenApiMediaType self)
        {
            Stop();
        }

        public void VisitMediaType(string key, OpenApiMediaType self)
        {

            if (key == "application/json")
                self.Schema.Accept(this);

            else
            {
                Stop();
            }

        }

        public void VisitSchema(OpenApiSchema self)
        {

            if (self.Properties.Count == 0)
                CheckType(self);

            else
            {

                if (self.Reference != null)
                    self.Accept("class", self.Reference.Id, this);

                else
                {
                    foreach (var item in self.Properties)
                        item.Value.Accept("property", item.Key, this);
                }
            }
        }

        public void VisitSecurityRequirement(OpenApiSecurityRequirement self)
        {
            Stop();
        }

        public void VisitSecurityScheme(OpenApiSecurityScheme self)
        {
            Stop();
        }

        public void VisitSecurityScheme(string key, OpenApiSecurityScheme self)
        {
            Stop();
        }

        public void VisitServer(OpenApiServer self)
        {
            Stop();
        }

        public void VisitTag(OpenApiTag self)
        {
            Stop();
        }

        public void VisitParameter(OpenApiParameter self)
        {
            VisitParameter(self.Name, self);
        }

        public void VisitParameter(string key, OpenApiParameter self)
        {
            self.Schema.Accept(this);

            if (!self.In.HasValue)
                _diag.AddError(GetLocation, "in", $"the parameter '{key}', should specified 'In value' or the value is invalid.");

            else
                switch (self.In.Value)
                {
                    case ParameterLocation.Query:
                        if (self.Style == ParameterStyle.PipeDelimited || self.Style == ParameterStyle.DeepObject)
                            _diag.AddWarning(GetLocation, "style", $"for parameter '{key}', the style should not be '{self.Style}'");
                        if (self.Style == ParameterStyle.SpaceDelimited && !self.Explode)
                            _diag.AddWarning(GetLocation, "style", $"for parameter '{key}', the style should not be '{self.Style.ToString()}'");
                        break;

                    case ParameterLocation.Header:
                        break;

                    case ParameterLocation.Path:
                        if (self.Style != ParameterStyle.Simple)
                            _diag.AddWarning(GetLocation, "style", $"for parameter '{key}', the style should be simple for parameter specified by path");

                        //if (!self.Required)
                        //    _diag.AddWarning(GetLocation, "required", $"for parameter '{key}', the style required");
                        break;

                    case ParameterLocation.Cookie:
                        break;

                    default:
                        break;
                }

        }


        public string GetPath()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("#");

            var l = _path.Reverse().ToList();

            foreach (var item in l)
            {
                sb.Append("/");
                sb.Append(item);
            }

            return sb.ToString();
        }

        public void PushPath(params string[] segments)
        {
            foreach (var item in segments)
                _path.Push(item);
        }

        public void PopPath()
        {
            _path.Pop();
        }

        public void ClearPath()
        {
            _path.Clear();
        }

        private Stack<string> _path = new Stack<string>();

        private HashSet<string> _h;

    }


    public struct ExceptedProperties
    {
        public bool AcceptMinMaxValue { get; set; }
        public bool AcceptMinMaxItems { get; set; }
        public bool AcceptMinMaxLength { get; set; }
        public bool AcceptComma { get; internal set; }
    }

}