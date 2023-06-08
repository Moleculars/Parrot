namespace Bb.ParrotServices.Middlewares
{

    public static class HttpRequestMessageExtensions
    {

        /// <summary>
        /// create a new HttpMethod
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static HttpMethod GetMethod(this HttpRequest request)
        {
            string method = request.Method;
            if (HttpMethods.IsDelete(method)) return HttpMethod.Delete;
            if (HttpMethods.IsGet(method)) return HttpMethod.Get;
            if (HttpMethods.IsHead(method)) return HttpMethod.Head;
            if (HttpMethods.IsOptions(method)) return HttpMethod.Options;
            if (HttpMethods.IsPost(method)) return HttpMethod.Post;
            if (HttpMethods.IsPut(method)) return HttpMethod.Put;
            if (HttpMethods.IsTrace(method)) return HttpMethod.Trace;
            return new HttpMethod(method);
        }


        /// <summary>
        /// copy all headers from the specified response message in the context
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <param name="context"></param>
        public static void CopyFromTargetResponseHeaders(this HttpResponseMessage responseMessage, HttpContext context)
        {

            if (responseMessage != null)
            {
                foreach (var header in responseMessage.Headers)
                    context.Response.Headers[header.Key] = header.Value.ToArray();

                foreach (var header in responseMessage.Content.Headers)
                    context.Response.Headers[header.Key] = header.Value.ToArray();

                context.Response.Headers.Remove("transfer-encoding");

            }

        }

        /// <summary>
        /// create a copy of the original request
        /// </summary>
        /// <param name="context"></param>
        /// <returns>the cloned request message</returns>
        public static HttpRequestMessage CopyFromOriginalRequestContentAndHeaders(this HttpContext context)
        {
            var requestMessage = new HttpRequestMessage();

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
                requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());

            return requestMessage;

        }

        /// <summary>
        /// return the content or null where is missing
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <returns></returns>
        public static string? ContentType(this HttpResponseMessage responseMessage)
        {

            if (responseMessage.Content?.Headers?.ContentType != null)
                return responseMessage.Content.Headers.ContentType.MediaType;

            return null;
        }

        /// <summary>
        /// return true if the contentType match with specified type
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsContentOfType(this HttpResponseMessage responseMessage, string type)
        {
            return responseMessage.ContentType() == type;
        }

        /// <summary>
        /// return a cloned <see cref="T:HttpRequestMessage"/>
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(HttpRequestMessage req)
        {

            var clone = new HttpRequestMessage(req.Method, req.RequestUri);

            var ms = new MemoryStream();
            if (req.Content != null)
            {
                await req.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;

                if ((ms.Length > 0 || req.Content.Headers.Any()) && clone.Method != HttpMethod.Get)
                {
                    clone.Content = new StreamContent(ms);

                    if (req.Content.Headers != null)
                        foreach (var h in req.Content.Headers)
                            clone.Content.Headers.Add(h.Key, h.Value);
                }
            }

            clone.Version = req.Version;

            foreach (var prop in req.Properties)
                clone.Properties.Add(prop);

            foreach (var header in req.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return clone;

        }

        /// <summary>
        /// return a cloned <see cref="T:HttpResponseMessage"/>
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> CloneResponseAsync(HttpResponseMessage response)
        {

            var newResponse = new HttpResponseMessage(response.StatusCode);
            var ms = new MemoryStream();

            foreach (var v in response.Headers) newResponse.Headers.TryAddWithoutValidation(v.Key, v.Value);
            if (response.Content != null)
            {
                await response.Content.CopyToAsync(ms).ConfigureAwait(false);
                ms.Position = 0;
                newResponse.Content = new StreamContent(ms);

                foreach (var v in response.Content.Headers)
                    newResponse.Content.Headers.TryAddWithoutValidation(v.Key, v.Value);

            }

            return newResponse;
        }

    }

}
