using Bb.Codings;
using Bb.OpenApi;
using DataAnnotationsExtensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Bb.OpenApiServices
{


    public class OpenApiGenerateModel : OpenApiGeneratorCSharpBase
    {
        private CSNamespace _ns;

        internal OpenApiGenerateModel(string artifactName, string @namespace)
            : base(artifactName, @namespace,
                  "Newtonsoft.Json",
                  "System",
                  "System.Collections.Generic",
                  "System.ComponentModel.DataAnnotations",
                  "DataAnnotationsExtensions")
        {

        }

        public override CSMemberDeclaration VisitDocument(OpenApiDocument self)
        {

            self.Components.Accept(this);

            foreach (var item in self.Paths)
                item.Value.Accept(item.Key, this);

            return null;

        }

        public override CSMemberDeclaration VisitComponents(OpenApiComponents self)
        {

            foreach (var item in self.Schemas)
            {

                bool hasMember = false;
                var cs = CreateArtifact(item.Key);
                _ns = CreateNamespace(cs);
                _ns.DisableWarning("CS8618", "CS1591");

                CSMemberDeclaration member = null;

                if (!item.Value.IsEmptyType())
                {

                    if (item.Value.Enum.Count > 0)
                        member = item.Value.Accept("enum", item.Key, this);
                    else
                        member = item.Value.Accept("class", item.Key, this);

                    if (member != null)
                    {
                        _ns.Add(member);
                        hasMember = true;
                    }
                }
                else
                {

                }

                if (hasMember)
                {
                    var c = cs.Code().ToString();
                    _ctx.AppendDocument("Models", item.Key + ".cs", c);
                }
            }

            return null;

        }

        public override CSMemberDeclaration VisitJsonSchema(string kind, string key, OpenApiSchema self)
        {

            var typeName = self.Type;

            if (typeName == null)
            {

                if (self.Properties.Any())
                    typeName = "object";

                else if (self.Items != null)
                    typeName = "array";

            }

            switch (kind)
            {

                case "enum":
                    var cls1 = new CsEnumDeclaration(key);

                    foreach (IOpenApiAny item in self.Enum)
                    {
                        if (item is IOpenApiPrimitive p)
                        {
                            var enumMember = p.Accept(this);
                            if (enumMember != null)
                                cls1.Add(enumMember);
                        }
                        else
                        {
                            Stop();
                        }
                    }
                    return cls1;

                case "class":

                    if (typeName == "array")
                    {

                        string type2;
                        string type;

                        if (self.Items != null)
                        {

                            type2 = self.Items.ResolveType(out var t);
                            type = CodeHelper.BuildTypename("List", type2).ToString();
                            //var p = VisitJsonSchema(kind, type2, self.Items);
                            //_ns.Add(p);
                        }
                        else
                        {
                            type2 = self.ResolveType(out var t);
                            type = CodeHelper.BuildTypename("List", type2).ToString();
                        }

                        var cls2 = new CsClassDeclaration(EnsureClassNotExists(key))
                            .Base(type);

                        return cls2;

                    }
                    else if (typeName == "object")
                    {
                        var cls2 = new CsClassDeclaration(EnsureClassNotExists(key));

                        this._datas.SetData("class_key", key);
                        foreach (var item in self.Properties)
                        {

                            var isRequired = self.Required?.Contains(item.Key) != null;
                            var property = item.Value.Accept("property", item.Key, this);

                            if (property != null)
                            {

                                if (isRequired)
                                    property.Attribute(typeof(RequiredAttribute));

                                cls2.Add(property);
                            }


                        }
                        return cls2;
                    }

                    else
                    {
                        Stop();
                    }

                    break;

                case "property":
                    return VisitJsonSchemaProperty(key, self);

                default:
                    break;
            }


            Stop();

            return null;

        }

        public override CSMemberDeclaration VisitEnumPrimitive(IOpenApiPrimitive self)
        {

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

            var result = new CsFieldDeclaration(fieldName, type) { IsEnumMember = true };

            if (initialValue != null)
                result.SetInitialValue(initialValue)
            ;

            return result;

        }

        public CSMemberDeclaration VisitJsonSchemaProperty(string propertyName, OpenApiSchema self)
        {

            CsPropertyDeclaration property = null;
            string type = null;

            type = self.ResolveType(out var value);

            if (type == null)
            {
                var p = VisitJsonSchema("class", propertyName, self);
                _ns.Add(p);
                type = propertyName;
            }

            if (type != null)
            {

                var prp = propertyName;
                //if (_scharpReservedKeyword.Contains(prp))
                //    prp = "@" + prp;

                property = new CsPropertyDeclaration(prp, type)
                    .AutoGet()
                    .AutoSet()
                    ;

                if (!string.IsNullOrEmpty(self.Description))
                    property.Documentation.Summary(() => self.Description);

                else if (!string.IsNullOrEmpty(value?.Description))
                    property.Documentation.Summary(() => value.Description);

                property.Attribute(typeof(JsonPropertyAttribute), a =>
                {
                    a.Argument(propertyName.Literal())
                    ;
                });

                if (self.Deprecated)
                {
                    property.Attribute(typeof(ObsoleteAttribute), a =>
                    {
                        a.Argument("this property is deprecated".Literal())
                         .Argument(false.Literal())
                        ;
                    });
                }

                ApplyAttributes(self, property);

                //if (value != null)
                //    ApplyAttributes(value, property);

            }

            return property;

        }

        private void ApplyAttributes(OpenApiSchema self, CsPropertyDeclaration property)
        {

            //if (self.Nullable)
            //if (self.IsObject)
            //if (self.IsReadOnly)
            //if (self.IsTuple)
            //if (self.IsWriteOnly)

            if (self.MinLength.HasValue && self.MinLength > 0)
                property.Attribute(typeof(MinLengthAttribute), a =>
                {
                    a.Argument(self.MinLength.Value.Literal())
                    ;
                });

            if (self.MaxLength.HasValue && self.MaxLength > 0)
                property.Attribute(typeof(MaxLengthAttribute), a =>
                {
                    a.Argument(self.MaxLength.Value.Literal())
                    ;
                });

            if (!string.IsNullOrEmpty(self.Pattern))
                property.Attribute(typeof(RegularExpressionAttribute), a =>
                {
                    a.Argument(self.Pattern.Literal())
                    ;
                });


            if (self.Minimum.HasValue && self.Minimum > 0)
            {

                //property.Attribute(typeof(RangeAttribute), a =>
                //{
                //    a.Argument(self.Minimum.Value.Literal());
                //    a.Argument(self.Maximum.Value.Literal());
                //    ;
                //});

                property.Attribute(typeof(MaxAttribute), a =>
                {
                    a.Argument(GetLiteral(self.Minimum.Value))
                    ;
                });
                Stop();
            }

            if (self.Maximum.HasValue && self.Maximum > 0)
            {
                property.Attribute(typeof(MaxAttribute), a =>
                {
                    a.Argument(GetLiteral(self.Maximum.Value))
                    ;
                });
            }

            if (self.MinItems > 0)
            {
                property.Attribute(typeof(MinLengthAttribute), a =>
                {
                    a.Argument(self.MinItems.Value.Literal())
                    ;
                });
            }

            if (self.MaxItems > 0)
            {
                property.Attribute(typeof(MaxLengthAttribute), a =>
                {
                    a.Argument(self.MaxItems.Value.Literal())
                    ;
                });
            }

            if (self.MinProperties > 0)
            {
                Stop();
            }

            if (self.MaxProperties > 0)
            {
                Stop();
            }

            if (self.Not != null)
            {
                Stop();
            }

            if (self.OneOf != null && self.OneOf.Count > 0)
            {
                Stop();
            }

            if (self.Discriminator != null)
            {
                Stop();
            }

            if (self.ExclusiveMinimum != null)
            {
                Stop();
            }

            if (self.ExclusiveMaximum != null)
            {
                Stop();
            }

            //if (self.IsBinary)
            //{
            //    Stop();
            //}
        }

        private LiteralExpressionSyntax GetLiteral(decimal value)
        {
            return Convert.ChangeType(value, typeof(double)).Literal();
        }

        public override CSMemberDeclaration VisitPathItem(string key, OpenApiPathItem self)
        {

            foreach (var item in self.Operations)
            {
                OpenApiOperation o = item.Value;
                var cls = o.Accept(item.Key.ToString(), this);
                if (cls != null)
                {
                    //_ns.Add(cls);
                }
            }

            return null;

        }

        public override CSMemberDeclaration VisitOperation(string key, OpenApiOperation self)
        {

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
                                return t.Items.Accept("class", key, this);

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

            return null;

        }

        public override CSMemberDeclaration VisitCallback(string key, OpenApiCallback self)
        {
            throw new NotImplementedException();
        }

        //public override CSMemberDeclaration VisitExternalDocumentation(OpenApiExternalDocumentation self)
        //{
        //    throw new NotImplementedException();
        //}

        public override CSMemberDeclaration VisitInfo(OpenApiInfo self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitLink(string key, OpenApiLink self)
        {
            throw new NotImplementedException();
        }

        //public override CSMemberDeclaration VisitOperationDescription(OpenApiOperationDescription self)
        //{
        //    throw new NotImplementedException();
        //}

        public override CSMemberDeclaration VisitParameter(string key, OpenApiParameter self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitResponse(OpenApiResponse self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitResponse(string key, OpenApiResponse self)
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

        public override CSMemberDeclaration VisitParameter(OpenApiParameter self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitMediaType(string key, OpenApiMediaType self)
        {
            throw new NotImplementedException();
        }

        public override CSMemberDeclaration VisitMediaType(OpenApiMediaType self)
        {
            throw new NotImplementedException();
        }

        private string EnsureClassNotExists(string key)
        {

            var name = key;

            while (!_classes.Add(name))
                name = key + "_" + GeneratorHelper.GenerateRandomCode(3);

            return name;

        }


        private HashSet<string> _classes = new HashSet<string>();

    }


}