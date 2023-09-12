using Bb.Analysis;
using Bb.Codings;
using Bb.OpenApi;
using DataAnnotationsExtensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SharpCompress.Compressors.Xz;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security;

namespace Bb.OpenApiServices
{

    public class OpenApiValidator : DiagnosticGeneratorBase, IServiceGenerator<OpenApiDocument>, IOpenApiDocumentVisitor
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

            foreach (var item in self.Paths)
                item.Value.Accept(item.Key, this);

        }

        public void VisitComponents(OpenApiComponents self)
        {

            foreach (KeyValuePair<string, OpenApiSchema> item in self.Schemas)
            {

                if (item.Value.IsEmptyType())
                {
                    Stop();
                }
                else
                {
                    if (item.Value.Enum.Count > 0)
                    {
                        Stop();
                        item.Value.Accept("enum", item.Key, this);
                    }
                    else
                    {
                        item.Value.Accept("class", item.Key, this);
                    }
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

                foreach (var item in self.Properties)
                    item.Value.Accept("property", item.Key, this);

                foreach (string item in self.Required)
                    if (!self.Properties.Any(c => c.Key == item))
                        _diag.AddError(TryToResolvePath(self), item, $"the required property {item} is not found in the {key} object");

            }

            else
            {
                Stop();
            }
        }

        private void VisitJsonSchemaForEnum(OpenApiSchema self)
        {
            Stop();

            foreach (IOpenApiAny item in self.Enum)
            {
                if (item is IOpenApiPrimitive p)
                    p.Accept(this);
                else
                {
                    Stop();
                }
            }
        }

        private DiagnosticLocation TryToResolvePath(OpenApiSchema self)
        {

            if (self.Reference != null)
            {

                Stop();

                if (self.Reference != null)
                    self.Accept("class", self.Reference.Id, this);

            }

            return DiagnosticLocation.Empty;

        }

        public void VisitEnumPrimitive(IOpenApiPrimitive self)
        {
            Stop();

            string fieldName = "";
            string type = string.Empty;
            object initialValue = null;

            switch (self.PrimitiveType)
            {
                case PrimitiveType.Integer:
                    var e1 = self as OpenApiInteger;
                    fieldName = "Value_" + e1.Value.ToString();
                    initialValue = e1.Value;
                    break;

                case PrimitiveType.String:
                    var e2 = self as OpenApiString;
                    fieldName = e2.Value.ToString();
                    //initialValue = e2.Value;
                    break;

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

        }

        public void VisitJsonSchemaProperty(string propertyName, OpenApiSchema self)
        {

            if (self.Type == null)
            {
                if( self.Reference == null)
                    _diag.AddError(TryToResolvePath(self), "type", $"type or reference of the property {propertyName} should be specified");

                else
                {
                    Stop();
                }
            }
            else
            {

                bool acceptMinMax = CheckType(self);

                if (self.MaxLength.HasValue)
                {
                    if (!acceptMinMax)
                        _diag.AddError(TryToResolvePath(self), self.Format, $"maxLength is unexpected for the type {self.Type.ToLower()}");

                    else
                    {
                        Stop();
                    }
                }

                if (self.MinLength.HasValue)
                {
                    if (!acceptMinMax)
                        _diag.AddError(TryToResolvePath(self), self.Format, $"minLength is unexpected for the type {self.Type.ToLower()}");
                    else
                    {
                        Stop();
                    }
                }

            }

        }

        private bool CheckType(OpenApiSchema self)
        {

            bool acceptMinMax = false;

            switch (self.Type.ToLower())
            {

                case "boolean":
                    break;

                case "integer":
                    acceptMinMax = true;
                    if (!string.IsNullOrEmpty(self.Format))
                        switch (self.Format)
                        {
                            case "int32":
                            case "int64":
                                break;
                            default:
                                _diag.AddError(TryToResolvePath(self), self.Format, $"the format {self.Format} is not managed for {self.Type.ToLower()}");
                                break;
                        }
                    break;

                case "number":
                    acceptMinMax = true;
                    if (!string.IsNullOrEmpty(self.Format))
                        switch (self.Format)
                        {
                            case "double":
                            case "float":
                                break;
                            default:
                                _diag.AddError(TryToResolvePath(self), self.Format, $"the format {self.Format} is not managed for {self.Type.ToLower()}");
                                break;
                        }
                    break;

                case "string":
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
                            case "date-time":
                                break;
                            default:
                                _diag.AddError(TryToResolvePath(self), self.Format, $"the format {self.Format} is not managed for string");
                                break;
                        }

                    if (self.Enum.Count > 0)
                        foreach (var item in self.Enum)
                            if (item is IOpenApiPrimitive primitive)
                                if (primitive.PrimitiveType != PrimitiveType.String)
                                {
                                    dynamic d = item as dynamic;
                                    _diag.AddError(TryToResolvePath(self), self.Format, $"the enum value {d.Value} should be {primitive.PrimitiveType} type");
                                }

                    break;

                case "array":
                    if (self.Items != null)
                    {

                        if (self.Items.Reference != null)
                            self.Items.Accept("class", self.Items.Reference.Id, this);

                        else
                            _diag.AddError(TryToResolvePath(self), self.Format, $"Array has not specified type");

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
                    _diag.AddError(TryToResolvePath(self), self.Format, $"the format {self.Format} is not managed for string");
                    break;
            }

            return acceptMinMax;
        }

        private LiteralExpressionSyntax GetLiteral(decimal value)
        {
            return Convert.ChangeType(value, typeof(double)).Literal();
        }

        public void VisitPathItem(string key, OpenApiPathItem self)
        {
            foreach (var item in self.Operations)
            {
                OpenApiOperation o = item.Value;
                o.Accept(item.Key.ToString(), this);
            }
        }

        public void VisitOperation(string key, OpenApiOperation self)
        {

            foreach (var item in self.Parameters)
                item.Accept(this);

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

            foreach (var item in self.Responses)
                item.Value.Accept(item.Key, this);

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

            if (self.Content != null ||self.Content.Count == 0)
            {
                foreach (var item in self.Content)
                    item.Value.Accept(item.Key, this);

            }
            else
            {
                _diag.AddError(DiagnosticLocation.Empty, "content", $"the http result code '{key}' should contains mime result type.");
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

            if (self.In == ParameterLocation.Path)
            {
                if (self.Style != ParameterStyle.Simple)
                    _diag.AddWarning(DiagnosticLocation.Empty, self.Style.ToString(), $"for parameter '{key}', the style should be simple for parameter specified by path");

                if (!self.Required)
                    _diag.AddWarning(DiagnosticLocation.Empty, "required", $"for parameter '{key}', the style required");

            }

        }

        private HashSet<string> _h;

    }


}