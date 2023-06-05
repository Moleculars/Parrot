using Bb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;

            if (path.StartsWithSegments("/proxy"))
            {


                var services = (ServiceReferential)context.RequestServices.GetService(typeof(ServiceReferential));
                var targetUri = BuildTargetUri(context.Request, services);

                if (targetUri != null)
                {
                    var targetRequestMessage = CreateTargetMessage(context, targetUri);

                    using (var responseMessage = await _httpClient.SendAsync(targetRequestMessage, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted))
                    {
                        context.Response.StatusCode = (int)responseMessage.StatusCode;

                        CopyFromTargetResponseHeaders(context, responseMessage);

                        await ProcessResponseContent(context, responseMessage);
                    }

                    return;
                }

            }

            await _nextMiddleware(context);
        }


        //public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        //{

        //    HttpRequestMessage clone = new HttpRequestMessage(req.Method, req.RequestUri);

        //    // Copy the request's content (via a MemoryStream) into the cloned object
        //    var ms = new MemoryStream();
        //    if (req.Content != null)
        //    {
        //        await req.Content.CopyToAsync(ms).ConfigureAwait(false);
        //        ms.Position = 0;
        //        clone.Content = new StreamContent(ms);
        //        // Copy the content headers
        //        foreach (var h in req.Content.Headers)
        //            clone.Content.Headers.Add(h.Key, h.Value);
        //    }

        //    clone.Version = req.Version;

        //    foreach (KeyValuePair<string, object?> option in req.Options)
        //        clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);

        //    foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
        //        clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        //    return clone;
        //}


        private async Task ProcessResponseContent(HttpContext context, HttpResponseMessage responseMessage)
        {

            var content = await responseMessage.Content.ReadAsByteArrayAsync();

            if (IsContentOfType(responseMessage, "text/html") || IsContentOfType(responseMessage, "text/javascript"))
            {
                var stringContent = Encoding.UTF8.GetString(content);
                var newContent = stringContent.Replace("https://www.google.com", "/google")
                    .Replace("https://www.gstatic.com", "/googlestatic")
                    .Replace("https://docs.google.com/forms", "/googleforms");
                await context.Response.WriteAsync(newContent, Encoding.UTF8);
            } else
            {
                await context.Response.Body.WriteAsync(content);
            }

        }

        private bool IsContentOfType(HttpResponseMessage responseMessage, string type)
        {
            var result = false;

            if (responseMessage.Content?.Headers?.ContentType != null)
            {
                result = responseMessage.Content.Headers.ContentType.MediaType == type;
            }

            return result;
        }

        private HttpRequestMessage CreateTargetMessage(HttpContext context, Uri targetUri)
        {
            var requestMessage = new HttpRequestMessage();
            CopyFromOriginalRequestContentAndHeaders(context, requestMessage);

            targetUri = new Uri(QueryHelpers.AddQueryString(targetUri.OriginalString, new Dictionary<string, string>() { { "entry.1884265043", "John Doe" }}));

            requestMessage.RequestUri = targetUri;
            requestMessage.Headers.Host = targetUri.Host;
            requestMessage.Method = GetMethod(context.Request.Method);
           
            return requestMessage;
        }

        private void CopyFromOriginalRequestContentAndHeaders(HttpContext context, HttpRequestMessage requestMessage)
        {
            var requestMethod = context.Request.Method;

            if (!HttpMethods.IsGet(requestMethod) &&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod) &&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(context.Request.Body);
                requestMessage.Content = streamContent;
            }

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        private void CopyFromTargetResponseHeaders(HttpContext context, HttpResponseMessage responseMessage)
        {
            foreach (var header in responseMessage.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in responseMessage.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
            context.Response.Headers.Remove("transfer-encoding");
        }
        
        private static HttpMethod GetMethod(string method)
        {
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }

        private Uri BuildTargetUri(HttpRequest request, ServiceReferential referential)
        {

            var instance = referential.TryToMatch(request.Path);

            Uri targetUri = null;
            PathString remainingPath;

            if (request.Path.StartsWithSegments("/mock", out remainingPath))
            {
                targetUri = new Uri("https://docs.google.com/forms" + remainingPath);
            }


            if (request.Path.StartsWithSegments("/googleforms", out remainingPath))
            {
                targetUri = new Uri("https://docs.google.com/forms" + remainingPath);
            }

            if (request.Path.StartsWithSegments("/google", out remainingPath))
            {
                targetUri = new Uri("https://www.google.com" + remainingPath);
            }

            if (request.Path.StartsWithSegments("/googlestatic", out remainingPath))
            {
                targetUri = new Uri(" https://www.gstatic.com" + remainingPath);
            }

            return targetUri;
        }


        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly RequestDelegate _nextMiddleware;

    }
}
