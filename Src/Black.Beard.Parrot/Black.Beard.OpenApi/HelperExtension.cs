using NJsonSchema;
using NSwag;

namespace Bb.OpenApi
{

    public static class HelperExtension
    {


        public static bool IsJson(this JsonSchema self)
        {

            switch (self.Type)
            {

                case JsonObjectType.Array:
                case JsonObjectType.None:
                case JsonObjectType.Object:
                    return true;

                case JsonObjectType.Boolean:
                case JsonObjectType.Integer:
                case JsonObjectType.Number:
                case JsonObjectType.String:
                case JsonObjectType.Null:
                    return false;

                case JsonObjectType.File:
                default:
                    break;
            }

            return false;

        }

        public static string ResolveType(this JsonSchema self, OpenApiComponents root)
        {

            switch (self.Type)
            {
                case JsonObjectType.None:
                    return self.Reference.ResolveName(root);

                case JsonObjectType.Boolean:
                    return nameof(Boolean);

                case JsonObjectType.Integer:
                    return nameof(Int32);

                case JsonObjectType.Number:
                    return nameof(Decimal);

                case JsonObjectType.String:
                    return nameof(String);

                case JsonObjectType.Array:
                    return self.Item?.ResolveName(root);

                case JsonObjectType.Object:
                    return self.ResolveName(root);

                case JsonObjectType.File:
                    break;

                case JsonObjectType.Null:
                    break;
                default:
                    break;

            }


            return null;

        }

        public static string ResolveName(this JsonSchema self, OpenApiComponents root)
        {

            foreach (var item in root.Schemas)
                if (item.Value == self)
                    return item.Key;

            return string.Empty;

        }

        //public static JsonSchema GetRoot(this JsonSchema self)
        //{

        //    JsonSchema p = self.Parent as JsonSchema;

        //    while (p.Parent != null)
        //    {
        //        p = p.Parent as JsonSchema;
        //        if (p.Parent == null)
        //            break;
        //    }

        //    return p;

        //}

        public static Type ConvertTypeName(this JsonObjectType typeName)
        {

            switch (typeName)
            {

                case JsonObjectType.Boolean:
                    return typeof(bool);

                case JsonObjectType.Integer:
                    return typeof(int);

                case JsonObjectType.Number:
                    return typeof(decimal);

                case JsonObjectType.String:
                    return typeof(string);

                case JsonObjectType.Array:
                    return typeof(Array);

                case JsonObjectType.Object:
                    return typeof(object);

                case JsonObjectType.Null:

                case JsonObjectType.File:
                case JsonObjectType.None:
                default:
                    break;
            }


            return typeof(void);

        }

    }


}
