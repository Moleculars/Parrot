using Bb.Json.Jslt.CustomServices;
using Bb.Middlewares;
using Bb.ParrotServices.Controllers;
using Bb.Services;
using Microsoft.AspNetCore.Http;
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

namespace Bb.ParrotServices.Middlewares
{

    // https://localhost:7033/proxy/parcel/mock
    // https://localhost:7033/proxy/parcel/mock/swagger

    public class ReverseProxyMiddleware
    {


        public ReverseProxyMiddleware(RequestDelegate nextMiddleware)
        {
            _nextMiddleware = nextMiddleware;
            this._transformers = new ProxyTransformResponseMatcher()
                .Register<ProxyTransformHtmlResponse>("text/html");
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;

            if (path.StartsWithSegments("/proxy"))
            {

                Uri targetUri = null;
                string AliasUri = null;

                if (_services == null)
                {
                    _services = (ServiceReferential)context.RequestServices.GetService(typeof(ServiceReferential));
                    _logger = (ILogger<ReverseProxyMiddleware>)context.RequestServices.GetService(typeof(ILogger<ReverseProxyMiddleware>));
                }

                var template = _services.TryToMatch(path);

                if (template != null)
                {

                    List<KeyValuePair<string, Uri>> list;
                    if (context.Request.IsHttps)
                        list = template.HttpsUris;
                    else
                        list = template.HttpUris;

                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];
                        targetUri = item.Value;
                        AliasUri = item.Key;
                    }

                    if (targetUri != null)
                    {
                        if (context.Request.Path.StartsWithSegments(AliasUri, out var relativePath))
                        {

                            //if (relativePath.ToString().StartsWith("/swagger-ui"))
                            //{
                            //    relativePath = "/swagger" + relativePath;
                            //}

                            Uri newUri = new Uri(targetUri, relativePath);
                            var targetRequestMessage = CreateTargetMessage(context, newUri);

                            using (var responseMessage = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                            {

                                context.Response.StatusCode = (int)responseMessage.StatusCode;

                                if (context.Response.StatusCode != 200)
                                {

                                }
                                // _logger.LogDebug(newUri.ToString() + " return code " + context.Response.StatusCode);

                                responseMessage.CopyFromTargetResponseHeaders(context);

                                await _transformers.Transform(context, responseMessage, targetUri, AliasUri);

                            }

                            return;

                        }
                    }
                
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
