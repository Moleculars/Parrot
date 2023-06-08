using Bb.ParrotServices.Middlewares;
using System.Text;

namespace Bb.Middlewares
{
    /// <summary>
    /// class for select the transform that manage the contentype
    /// </summary>
    public class ProxyTransformResponseMatcher
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyTransformResponseMatcher"/> class.
        /// </summary>
        public ProxyTransformResponseMatcher()
        {
            _items = new Dictionary<string, Func<Uri, string, ProxyTransformResponse>>();
        }

        /// <summary>
        /// Registers the specified mimes for a transformer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mimes">The mimes.</param>
        /// <returns></returns>
        public ProxyTransformResponseMatcher Register<T>(params string[] mimes)
            where T : ProxyTransformResponse, new()
        {

            foreach (var mime in mimes)
            {
                _items.Add(mime, (u, s) =>
                {
                    return new T().Initialize(u, s);
                });
            }

            return this;

        }

        /// <summary>
        /// Transforms the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="targetUri">The target URI.</param>
        /// <param name="AliasUri">The alias URI.</param>
        public async Task Transform(HttpContext context, HttpResponseMessage responseMessage, Uri targetUri, string AliasUri)
        {

            if (responseMessage != null)
            {
                var contentType = responseMessage.ContentType();
                if (contentType != null)
                {


                    if (_items.TryGetValue(contentType, out var item))
                    {
                        var t = item(targetUri, AliasUri);
                        string message = await t.Transform(context, responseMessage);


                        message = message
                            .Replace("\"url\":\"v1/swagger.json\",", $"\"url\":\"{AliasUri}/swagger/v1/swagger.json\",")
                            .Replace("item.url = window.location.href.replace(\"index.html\", item.url).split('#')[0];",
                                $"item.url = window.location.href.replace(\"index.html\", \"{AliasUri.TrimStart('/')}\" + item.url).split('#')[0];")
                            ;


                        await context.Response.WriteAsync(message, Encoding.UTF8);
                    }
                    else
                    {
                        var content = await responseMessage.Content.ReadAsByteArrayAsync();
                        await context.Response.Body.WriteAsync(content);
                    }

                }
            }

        }

        private Dictionary<string, Func<Uri, string, ProxyTransformResponse>> _items;

    }

}
