using Bb.Json.Jslt.CustomServices;
using Bb.Models;
using Bb.ParrotServices.Controllers;
using Bb.Services;
using Flurl;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bb.Middlewares.ReversProxy
{


    public class ReverseProxyMiddleware
    {


        public ReverseProxyMiddleware(RequestDelegate nextMiddleware)
        {
            _nextMiddleware = nextMiddleware;
            _transformers = new ProxyTransformResponseMatcher()
                .Register<ProxyTransformHtmlResponse>("text/html");
        }

        public async Task Invoke(HttpContext context)
        {

            var path = context.Request.Path;

            if (path.StartsWithSegments("/proxy"))
            {


                if (_services == null)
                {
                    _services = (ServiceReferential)context.RequestServices.GetService(typeof(ServiceReferential));
                    _logger = (ILogger<ReverseProxyMiddleware>)context.RequestServices.GetService(typeof(ILogger<ReverseProxyMiddleware>));
                }

                var contract = _services.TryToMatch(path);

                if (contract != null)
                {

                    AddressTranslator translator = context.Request.IsHttps ? contract.Https : contract.Http;

                    Uri newUri = null;

                    if (context.Request.Path.StartsWithSegments(translator.QuerySource, out PathString relativePath))
                    {
                        var t = context.Request.Path.ToString();
                        if (t.EndsWith(".css") || t.EndsWith(".js") || t.EndsWith(".png"))
                        {
                            var url = Url.Combine(translator.TargetUri.ToString(), "swagger", relativePath.ToString());
                            newUri = new Uri(url);
                        }
                        else
                        {
                            var url = Url.Combine(translator.TargetUrl, relativePath.ToString());
                            newUri = new Uri(url);
                        }
                    }

                    if (newUri != null)
                    {
                        var targetRequestMessage = CreateTargetMessage(context, newUri);
                        using (var responseMessage = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                        {
                            context.Response.StatusCode = (int)responseMessage.StatusCode;
                            if (context.Response.StatusCode != 200)
                            {

                            }
                            else
                            {

                            }

                            // _logger.LogDebug(newUri.ToString() + " return code " + context.Response.StatusCode);
                            responseMessage.CopyFromTargetResponseHeaders(context);
                            await _transformers.Transform(context, responseMessage, translator);
                        }
                    }
                    else
                    {

                    }

                    return;

                }

            }

            else
            {

            }

            await _nextMiddleware(context);
        }

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = context.CopyFromOriginalRequestContentAndHeaders();
            //requestMessage.Version = ???
            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = context.Request.GetMethod();
            return requestMessage;
        }


        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RequestDelegate _nextMiddleware;
        private readonly ProxyTransformResponseMatcher _transformers;
        private ServiceReferential _services;
        private ILogger<ReverseProxyMiddleware>? _logger;

    }

}
