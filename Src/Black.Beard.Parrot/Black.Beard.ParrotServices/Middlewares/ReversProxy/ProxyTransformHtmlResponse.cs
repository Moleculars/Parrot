using Microsoft.AspNetCore.SignalR;
using SharpYaml;
using static System.Collections.Specialized.BitVector32;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System;

namespace Bb.Middlewares.ReversProxy
{
    public class ProxyTransformHtmlResponse : ProxyTransformResponse
    {

        protected override string Transform(HttpContext context, string payload)
        {

            //var dom = new HtmlDocument();
            //dom.LoadHtml(payload);

            //var visitor = new HtmlVisitor(context, base._translator);
            //dom.DocumentNode.Accept(visitor);
            //return dom.DocumentNode.InnerHtml;

            return payload;

        }

    }


}
