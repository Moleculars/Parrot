using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;

namespace Bb
{
    public static class OpenApiHelper
    {

        public static OpenApiDocument LoadOpenApiContract(this string pathFile)
        {

            using (FileStream fs = File.Open(pathFile, FileMode.Open))
            {
                var openApiDocument = new OpenApiStreamReader().Read(fs, out var diagnostic);               
                return openApiDocument;
            }

        }


    }

}
