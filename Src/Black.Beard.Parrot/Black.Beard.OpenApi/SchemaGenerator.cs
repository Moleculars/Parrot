using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Bb
{

    public class SchemaGenerator
    {

        internal SchemaGenerator(SchemaGeneratorContext ctx)
        {
            this._ctx = ctx;
        }

        public static OpenApiComponents GenerateSchema(Type type)
        {
            var ctx = new SchemaGeneratorContext();
            var s = new SchemaGenerator(ctx);
            s.Generate(type);
            return ctx.OpenApiComponents;
        }


        private void Generate(Type type)
        {

            if (!_ctx.Exists(type)) return;
            var schema = new OpenApiSchema();
            _ctx.Add(type, schema);

            if (type.IsEnum)
            {

            }
            else
            {

                var properties = GenerateProperties(type);

                if (type.BaseType != null && type.BaseType != typeof(object) && !type.IsGenericType)
                {
                    Generate(type.BaseType);
                    schema.AllOf.Add(_ctx.Resolve(type.BaseType));

                    var schema2 = new OpenApiSchema();

                    foreach (var item in properties)
                        schema2.Properties.Add(item.Item1, item.Item2);

                    foreach (var item in _required)
                        schema2.Required.Add(item);

                    _required.Clear();

                    schema.AllOf.Add(schema2);

                }
                else
                {

                    foreach (var item in properties)
                        schema.Properties.Add(item.Item1, item.Item2);

                    foreach (var item in _required)
                        schema.Required.Add(item);

                    _required.Clear();

                }

            }

        }

        private IEnumerable<(string, OpenApiSchema)> GenerateProperties(Type type)
        {

            List<(string, OpenApiSchema)> result = new List<(string, OpenApiSchema)>();

            var p = type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var p2 in p)
                result.Add(GenerateProperty(p2));

            var f = type.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            foreach (var f2 in f)
                result.Add(GenerateField(f2));

            return result;
        }

        private (string, OpenApiSchema) GenerateProperty(PropertyInfo m)
        {

            var s = new OpenApiSchema();

            if (IsArray(m.PropertyType))
            {

                s.Type = "array";
                s.Type = Type(m.PropertyType, out var format);
                if (s.Type == "object")
                {

                    Type t = ResolveEnumarableItem(m.PropertyType);
                    s.Type = Type(t, out format);
                    s.Format = format;
                    if (s.Type == "object")
                    {
                        Generate(t);
                        s.Items = _ctx.Resolve(t);
                    }
                }
                else
                {
                    s.Format = format;
                }

            }
            else
            {

                s.Type = Type(m.PropertyType, out var format);
                s.Format = format;
                if (s.Type == "object")
                {
                    Generate(m.PropertyType);
                    // _ctx.Resolve(m.PropertyType);
                }

                foreach (var item in m.GetCustomAttributes())
                {
                    var attribute = item.GetType().Name;

                    switch (attribute)
                    {

                        case "RequiredAttribute":
                            _required.Add(m.Name);
                            break;
                        case "EmailAddressAttribute":
                            break;
                        case "FileExtensionsAttribute":
                            break;
                        case "MaxLengthAttribute":
                            break;
                        case "MinLengthAttribute":
                            break;
                        case "PhoneAttribute":
                            break;
                        case "RangeAttribute":
                            break;
                        case "RegularExpressionAttribute":
                            break;
                        case "StringLengthAttribute":
                            break;
                        case "UrlAttribute":
                            break;
                            default: 
                            break;
                    }

                }

            }

            return (m.Name, s);

        }

        private Type ResolveEnumarableItem(Type propertyType)
        {

            foreach (var item in propertyType.GetInterfaces())
            {
                if (item.IsGenericType)
                {
                    var typeBase = item.GetGenericTypeDefinition();

                    if (typeBase == typeof(IList<>) || typeBase == typeof(IEnumerable<>))
                    {
                        var arg = item.GetGenericArguments();
                        return arg[0];
                    }

                }

            }

            throw new NotImplementedException();
        }

        private OpenApiReference GetReferenceFromType(Type propertyType)
        {
            var s = _ctx.Resolve(propertyType);
            return new OpenApiReference() { Type = new ReferenceType() { } };
        }

        private string Type(Type type, out string format)
        {

            format = null;

            if (type == typeof(string))
                return "string";

            if (type == typeof(short) | type == typeof(int) | type == typeof(long)
              | type == typeof(ushort) | type == typeof(uint) | type == typeof(ulong))
                return "string";

            if (type == typeof(bool))
                return "boolean";

            if (type == typeof(DateTime))
            {
                format = "date-time";
                return "string";
            }

            if (type == typeof(Guid))
            {
                format = "guid";
                return "string";
            }

            return "object";

        }

        private bool IsArray(Type type)
        {

            if (type.IsArray)
                return true;

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                if (type != typeof(string))
                    return true;

            return false;

        }

        private (string, OpenApiSchema) GenerateField(FieldInfo m)
        {
            throw new NotImplementedException();
        }

        private HashSet<string> _required = new HashSet<string>();
        private readonly SchemaGeneratorContext _ctx;

    }


    internal class SchemaGeneratorContext
    {

        public SchemaGeneratorContext()
        {
            this._models = new Dictionary<Type, OpenApiSchema>();
        }


        internal bool Exists(Type type)
        {
            return !_models.ContainsKey(type);
        }

        internal void Add(Type type, OpenApiSchema schema)
        {
            this._models.Add(type, schema);
        }

        internal OpenApiSchema Resolve(Type type)
        {
            return this._models[type];
        }

        public OpenApiComponents OpenApiComponents
        {
            get
            {
                
                var component = new OpenApiComponents();

                foreach (var model in this._models)
                {
                    var type = model.Key;
                    string name;
                    if (type.IsGenericType)
                    {

                        name = type.Name.Substring(0, type.Name.IndexOf('`'));

                        string comma = "Of";
                        foreach (var item in model.Key.GetGenericArguments())
                        {
                            name += comma + item.Name;
                            comma = "And";
                        }

                    }
                    else
                        name = type.Name;

                    component.Schemas.Add(name, model.Value);

                }

                return component;

            }
        }

        private Dictionary<Type, OpenApiSchema> _models;

    }

}
