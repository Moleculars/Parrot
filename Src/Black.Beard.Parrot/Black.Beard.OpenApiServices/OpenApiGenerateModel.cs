using Bb.Codings;
using Bb.OpenApi;
using NJsonSchema;
using NSwag;
using System.ComponentModel.DataAnnotations;

namespace Black.Beard.OpenApiServices
{
    public class OpenApiGenerateModel : IOpenApiDocumentVisitor<CSMemberDeclaration>
    {


        public OpenApiGenerateModel(string artifactName, string @namespace)
        {

            _cs = new CSharpArtifact(artifactName)
                .Usings("System",
                        "System.Collections.Generic",
                        "System.ComponentModel.DataAnnotations"
                        )
                ;
            _ns = _cs.Namespace(@namespace);

        }

        public CSMemberDeclaration VisitDocument(OpenApiDocument self)
        {
            _self = self;
            self.Components.Accept(this);
            return _cs;
        }

        public CSMemberDeclaration VisitComponents(OpenApiComponents self)
        {

            foreach (var item in self.Schemas)
            {
                var cls = item.Value.Accept(item.Key, this);
                if (cls != null)
                    _ns.Add(cls);
            }

            return _ns;

        }

        public CSMemberDeclaration VisitJsonSchema(string key, JsonSchema self)
        {

            var cls = new CsClassDeclaration(key);


            foreach (var item in self.ActualProperties)
            {
                var property = item.Value.Accept(this);
                if (property != null)
                {
                    cls.Add(property);
                }
            }

            return cls;
        }

        public CSMemberDeclaration VisitJsonSchemaProperty(JsonSchemaProperty self)
        {
            CsPropertyDeclaration property = null;

            if (self.Type == JsonObjectType.None)
            {
                var reference = self.Reference.ResolveName(this._self.Components);
                property = new CsPropertyDeclaration(self.Name, reference);
            }
            else
            {
                var type = self.Type.ConvertTypeName();
                if (type == typeof(Array))
                {
                    if (self.Item != null)
                    {
                        if (self.Item.Type == JsonObjectType.None)
                        {
                            var reference = self.Item.Reference.ResolveName(this._self.Components);
                            property = new CsPropertyDeclaration(self.Name, reference);
                        }
                        else
                        {
                            type = self.Item.Type.ConvertTypeName();
                            type = typeof(List<>).MakeGenericType(type);
                            property = new CsPropertyDeclaration(self.Name, type.BuildTypename().ToString());
                        }
                    }
                    else
                    {

                    }
                }
                else
                    property = new CsPropertyDeclaration(self.Name, type.BuildTypename().ToString());

            }

            property
                .AutoGet()
                .AutoSet();

            if (self.IsRequired)
                property.Attribute(typeof(RequiredAttribute));

            if (!string.IsNullOrEmpty(self.Description))
                property.Documentation.Summary(() => self.Description);

            //self.IsNullableRaw;
            //self.IsObject;
            //self.IsReadOnly;
            //self.IsTuple;
            //self.IsWriteOnly;
            //self.MaxItems;
            //self.MaxLength;
            //self.MaxProperties;
            //self.Minimum;
            //self.MinItems;
            //self.MinLength;
            //self.MinProperties;
            //self.Not;
            //self.OneOf;
            //self.ActualDiscriminator;
            //self.ActualDiscriminatorObject;
            //self.DeprecatedMessage;
            //self.Enumeration;
            //self.EnumerationNames;
            //self.ExclusiveMaximum;
            //self.ExclusiveMinimum;
            //self.Format;
            //self.IsAbstract
            //self.IsAnyType
            //self.IsAbstract
            //self.IsBinary
            //self.IsFlagEnumerable
            //self.Maximum
            //self.MaxProperties

            return property;

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

        public CSMemberDeclaration VisitJsonSchema(JsonSchema self)
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

        public CSMemberDeclaration VisitResponse(string key, OpenApiResponse self)
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

        public CSMemberDeclaration VisitOperation(string key, OpenApiOperation self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitParameter(OpenApiParameter self)
        {
            throw new NotImplementedException();
        }

        public CSMemberDeclaration VisitPathItem(string key, OpenApiPathItem self)
        {
            throw new NotImplementedException();
        }

        private readonly CSharpArtifact _cs;
        private readonly CSNamespace _ns;
        private OpenApiDocument _self;
    }


}