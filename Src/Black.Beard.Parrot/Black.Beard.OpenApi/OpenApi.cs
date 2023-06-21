using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Bb
{
    public static class OpenApiHelper
    {


        public static OpenApiComponents GenerateComponents(this Type self)
        {
            return SchemaGenerator.GenerateSchema(self);
        }

        public static string GenerateContracts(this Type self)
        {
            return SchemaGenerator
                .GenerateSchema(self)
                .GenerateContracts();
        }

        public static OpenApiDocument LoadOpenApiContract(this string pathFile)
        {

            using (FileStream fs = File.Open(pathFile, FileMode.Open))
            {
                var openApiDocument = new OpenApiStreamReader().Read(fs, out var diagnostic);
                return openApiDocument;
            }

        }

        public static void SaveOpenApi(this string pathFile, OpenApiDocument document)
        {

            using (FileStream fs = File.Open(pathFile, FileMode.Open))
            using (var writer = new StreamWriter(fs, Encoding.UTF8, bufferSize: 2048, leaveOpen: true))
            {
                var jsonWriter = new OpenApiJsonWriter(writer);
                document.SerializeAsV3(jsonWriter);
            }

        }

        public static string GenerateContracts(this OpenApiComponents componentDatas)
        {
            var doc = new Microsoft.OpenApi.Models.OpenApiDocument()
            {
                Components = componentDatas
            };
            return doc.GenerateContracts();
        }

        public static string GenerateContracts(this OpenApiDocument document)
        {

            JObject components = null;

            using (MemoryStream fs = new MemoryStream())
            {

                using (var writer = new StreamWriter(fs, Encoding.UTF8, bufferSize: 2048, leaveOpen: true))
                {
                    var jsonWriter = new OpenApiJsonWriter(writer);
                    document.SerializeAsV3(jsonWriter);
                }

                fs.Flush();
                var datas = fs.ToArray();
                var payload = Encoding.UTF8.GetString(datas);
                payload = payload.Substring(1).Trim(); // Remove bom

                var model = JObject.Parse(payload);

                components = (JObject)model["components"]["schemas"];

            }

            JProperty root = null;
            var definitions = new JObject();
            foreach (JProperty item in components.Properties())
            {
                if (root == null)
                    root = item;
                else
                    definitions.Add(new JProperty(item.Name, item.Value));
            }

            var result = new JObject(
                    new JProperty("schema", "http://json-schema.org/draft-04/schema#"),
                    new JProperty("title", root.Name),
                    new JProperty("type", root.Type),
                    new JProperty("additionalProperties", false),
                    new JProperty("properties", root.Value["properties"]),
                    new JProperty("definitions", definitions)

                );

            return result.ToString();

        }


    }

}
