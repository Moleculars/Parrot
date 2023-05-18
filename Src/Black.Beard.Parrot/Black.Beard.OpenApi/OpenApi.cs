using NSwag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bb
{
    public static class OpenApiHelper
    {

        public static OpenApiDocument LoadOpenApiContract(this string pathFile)
        {

            var taskDocument = OpenApiDocument.FromFileAsync(pathFile);
            var awaiter = taskDocument.GetAwaiter();
            var document = awaiter.GetResult();
            //var schema = document.Components.Schemas;

            return document;
        }


    }

}
