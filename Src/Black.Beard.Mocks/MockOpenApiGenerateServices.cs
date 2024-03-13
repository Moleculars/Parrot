using Bb.Codings;
using Bb.ComponentModel.Attributes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.OpenApi.Models;
using Microsoft.VisualBasic;
using System.Diagnostics.Contracts;

namespace Bb.OpenApiServices
{


    public class MockOpenApiGenerateServices : OpenApiGenerateServices
    {

        public MockOpenApiGenerateServices(string contract, string artifactName, string @namespace)
            : base(contract, artifactName, @namespace,
                  "Bb.Json.Jslt.Services",
                    "Bb.ParrotServices"
                  )
        {

        }


        public override string GetServiceRoute(KeyValuePair<string, OpenApiPathItem> self)
        {
            string key = self.Key;
            string pathController = $"/proxy/mock/{_contract}{key}";
            return pathController;
        }


        protected override void GenerateMethod(KeyValuePair<OperationType, OpenApiOperation> self, CodeBlock code, string typeReturn, CsMethodDeclaration method)
        {

            var templateName = Context.GetDataFor(self).GetData<string>("templateName");

            if (string.IsNullOrEmpty(templateName))
            {
                code.Return(SyntaxFactory.ThisExpression().Call("Ok"));
            }

            else
            {

                string diff = Context.GetRelativePath(templateName);

                // var service = new ServiceProcessor<ParcelTrackingList>();
                var type = CodeHelper.AsType("ServiceProcessor", typeReturn);
                code.DeclareLocalVar("var".AsType(), "service", type.NewObject());
                foreach (var item3 in method.Items<CsParameterDeclaration>())
                    code.Add("service".Identifier().Call("Add", item3.Name.Literal(), item3.Name.Identifier()));
                // return service.GetDatas("template.json", "datas.json");
                code.DeclareLocalVar("var".AsType(), "result", "service".Identifier().Call("GetDatas", diff.Literal()));
                code.Return(SyntaxFactory.ThisExpression().Call("Ok", "result".Identifier()));

            }
        }
    }


}