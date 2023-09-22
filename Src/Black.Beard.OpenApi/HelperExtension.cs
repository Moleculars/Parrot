
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using System.Security;
using System.Text;
using System.Xml.Schema;

namespace Bb.OpenApi
{

    public static class HelperExtension
    {


        public static string ResolveName(this OpenApiSchema schema)
        {

            if (schema.Items != null)
            {
                OpenApiReference r = schema.Items.Reference;
                if (r != null)
                    return r.Id;
            }

            if (schema.Reference != null)
                return schema.Reference.Id;

            Stop();

            return null;

        }


        public static bool IsJson(this OpenApiSchema self)
        {
            var type = self.ConvertTypeName();
            return type == typeof(Array) || type == typeof(object);
        }

        public static string? ResolveDescription(this OpenApiSchema self)
        {

            if (self == null)
                return null;

            var type = self.ConvertTypeName();

            if (type != null)
            {
                if (type == typeof(Object))
                    return self.Description;

                else if (type == typeof(Array))
                {
                    if (self.Items != null)
                        return self.Items.Description;

                }

                return type.Name;

            }
            else if (self.Items != null)
                return self.Items.ResolveType(out var value);

            else if (self.OneOf != null && self.OneOf.Count > 0)
            {
                Stop();
            }

            Stop();
            return null;

        }


        public static string ResolveType(this OpenApiSchema self, out OpenApiSchema toReportInProperty)
        {

            toReportInProperty = null;

            var r = self.Reference;
            if (r != null && !string.IsNullOrEmpty(r.Id))
            {

                var i = r.HostDocument.ResolveReference(r);
                if (i is OpenApiSchema s)
                {
                    if (s.IsEmptyType())
                    {

                        toReportInProperty = s;
                        var rr = s.ConvertTypeName();
                        if (rr != null)
                            return rr.Name;
                        Stop();
                    }
                }

                return r.Id;

            }

            var typeName = self.Type;
            if (typeName == null && self.Items != null)
                typeName = "array";

            if (typeName == "array")
            {
                if (self.Items != null)
                {
                    var p = self.Items.ResolveType(out toReportInProperty);
                    return BuildTypename("List", p).ToString();
                }
                if (self.OneOf != null && self.OneOf.Count > 0)
                {
                    Stop();
                }

            }

            var type = self.ConvertTypeName();

            if (type != null)
                return type.Name;

            if (self.AllOf != null && self.AllOf.Any())
            {
                foreach (OpenApiSchema? item in self.AllOf)
                    if (item != null)
                    {
                        var r2 = item.ResolveType(out toReportInProperty);
                        if (r2 != null)
                        {
                            //if (toReportInProperty != null)
                            //    Stop();
                            // Manage required
                            return r2;
                        }
                    }
            }

            Stop();
            return null;

        }

        public static bool IsEmptyType(this OpenApiSchema value)
        {

            var p = value.ConvertTypeName();

            if (p == null)
                return false;

            if (_acceptedType.Contains(value.Type))
                return false;

            if (value.Enum.Count > 0)
                return false;

            if (value.Properties.Any())
                return false;

            if (value.Reference != null)
            {
                Stop();
            }

            return true;

        }

        public static Type ConvertTypeName(this OpenApiSchema schema)
        {
            return schema.Type.ConvertTypeName(schema.Format, schema.MaxLength);
        }

        public static Type ConvertTypeName(this string typeName, string format, int? maxLength)
        {

            switch (typeName?.ToLower())
            {

                case "boolean":
                    return typeof(bool);

                case "number":
                    if (!string.IsNullOrEmpty(format))
                    {
                        switch (format)
                        {

                            case "float":
                                return typeof(double);

                            default:
                                Stop();
                                break;
                        }
                    }

                    if (maxLength.HasValue && maxLength.Value > 9)
                        return typeof(long);

                    return typeof(int);


                case "integer":
                    return typeof(int);

                //case JsonObjectType.Number:
                //    return typeof(decimal);

                case "string":
                    if (!string.IsNullOrEmpty(format))
                    {
                        switch (format)
                        {

                            case "email":
                            case "hostname":
                            case "ipv4":
                            case "ipv6":
                                break;

                            case "uri":
                                return typeof(Uri);

                            case "binary":
                                return typeof(byte[]);

                            case "byte":
                                return typeof(byte[]);

                            case "password":
                                return typeof(SecureString);

                            case "uuid":
                                return typeof(Guid);

                            case "date":
                            case "date-time":
                                return typeof(DateTime);

                            default:
                                break;
                        }
                    }

                    return typeof(string);

                case "array":
                    return typeof(Array);

                case "object":
                    return typeof(object);

                //case JsonObjectType.Null:

                //case JsonObjectType.File:
                //case JsonObjectType.None:
                default:
                    break;
            }

            return null;

        }

        private static StringBuilder BuildTypename(this string self, params string[] genericArguments)
        {
            StringBuilder sb = new StringBuilder(self);
            if (genericArguments != null && genericArguments.Length > 0)
            {

                sb.Append("<");

                var g = genericArguments[0];
                sb.Append(g);

                for (int i = 1; i < genericArguments.Length; i++)
                {
                    sb.Append(", ");
                    g = genericArguments[i];
                    sb.Append(g);
                }
                sb.Append(">");
            }

            return sb;
        }

        [System.Diagnostics.DebuggerStepThrough]
        [System.Diagnostics.DebuggerNonUserCode]
        private static void Stop()
        {
            System.Diagnostics.Debugger.Break();
        }

        private static HashSet<string> _acceptedType = new HashSet<string>() { "array", "string", "number", "integer", "boolean", "object" };

    }


}
