using Bb.Codings;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Linq;

namespace Bb.OpenApiServices
{

    public class GenerateWatchdog : CSharpGeneratorBase
    {

        private GenerateWatchdog(string @namespace, params string[] usings)
           : base(@namespace, usings)
        {
        }

        public static void Generate(ContextGenerator ctx, string @namespace)
        {
            var g = new GenerateWatchdog(@namespace,
                "Microsoft.AspNetCore.Mvc"
                );

            g.Generate(ctx);

        }

        public CSharpArtifact GenerateController()
        {

            var cs = CreateArtifact("WatchdogController", "Controllers");
            var ns = CreateNamespace(cs);
            ns.DisableWarning("CS8618", "CS1591");

            ns.Class("WatchdogController", c =>
            {

                c.Base("Controller");
                c.Field("_logger", "ILogger<WatchdogController>");

                c.Ctor(ctor =>
                {
                    ctor.Body(b =>
                    {
                        "_logger".Identifier().Set("logger".Identifier());
                    });
                })

                .Method("IsUpAndRunning", "ActionResult<WatchdogResult>", m =>
                {
                    m.Attribute("HttpGet");
                    m.Attribute("Produces", a => a.Argument(@"application/json".Literal()));
                    m.Body(b =>
                    {
                        b.TryCatchs(t =>
                        {
                            var argument = "WatchdogResult".AsType().NewObject();
                            t.Return(CodeHelper.This().Call("Ok", argument));
                        }, "Exception".AsType().Catch("ex", lst =>
                        {
                            lst.Add(CodeHelper.DeclareLocalVar("errorId", "Guid".AsType(), "Guid".Identifier().Call("NewGuid")));
                            lst.Add("_logger".Identifier().Call("LogError", "ex".Identifier(), "ex".Identifier().MemberAccess("Message"), "errorId".Identifier()));
                            var arg = "WatchdogResultException".NewObject("errorId".Identifier(),
                                "Sorry, an error has occurred. Please contact our customer service with uuid for assistance.".Literal());
                            lst.Return(SyntaxFactory.ThisExpression().Call("BadRequest", arg));
                        }));
                    });

                });

            });

            return cs;

        }

        public CSharpArtifact GenerateWatchdogResult()
        {

            var cs = CreateArtifact("WatchdogResult", "Models");
            var ns = CreateNamespace(cs);
            ns.DisableWarning("CS8618", "CS1591");                       

            ns.Class("WatchdogResult", c =>
            {
                c.Ctor(ctor =>
                {

                    ctor.Parameter("items", "WatchdogResultItem", p =>
                    {

                        p.Way(ParameterWay.Params);

                    });

                    ctor.Body(b =>
                    {
                        b.Add("Items".Identifier().Set("List<WatchdogResultItem>".AsType().NewObject()));
                    });
                })
                .Property("Items", "List<WatchdogResultItem>", p =>
                {
                    p.AutoGet()
                     .AutoSet();
                });
            });

            return cs;

        }

        public CSharpArtifact WatchdogResultItem()
        {

            var cs = CreateArtifact("WatchdogResultItem", "Models");
            var ns = CreateNamespace(cs);
            ns.DisableWarning("CS8618", "CS1591");

            ns.Class("WatchdogResultItem", c =>
            {

                c.Property("Name", "String", p =>
                {
                    p.AutoGet()
                     .AutoSet();
                })

                .Property("Value", "String", p =>
                {
                    p.AutoGet()
                     .AutoSet();
                })

                .Property("Description", "String", p =>
                {
                    p.AutoGet()
                     .AutoSet();
                })
                ;
            });

            return cs;

        }

        public CSharpArtifact WatchdogResultException()
        {

            var cs = CreateArtifact("WatchdogResultException", "Models");
            var ns = CreateNamespace(cs);
            ns.DisableWarning("CS8618", "CS1591");

            ns.Class("WatchdogResultException", c =>
            {

                c.Ctor(c =>
                {
                    c.Parameter("uuid", "Guid");
                    c.Parameter("message", "String");

                    c.Body(b =>
                    {
                        b.Add("Uuid".Identifier().Set("uuid".Identifier()));
                        b.Add("Message".Identifier().Set("message".Identifier()));
                    });

                });

                c.Property("Uuid", "Guid", p =>
                {
                    p.AutoGet()
                     .AutoSet();
                })

                .Property("Message", "String", p =>
                {
                    p.AutoGet()
                     .AutoSet();
                });

            });

            return cs;

        }

    }


}