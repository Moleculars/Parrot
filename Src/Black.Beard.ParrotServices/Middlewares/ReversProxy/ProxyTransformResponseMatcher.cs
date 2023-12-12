using Bb.Extensions;
using Bb.Services.ProcessHosting;
using Flurl;
using System.Text;

namespace Bb.Middlewares.ReversProxy
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
            _items = new Dictionary<string, Func<AddressTranslator, ProxyTransformResponse>>();
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
                _items.Add(mime, (u) =>
                {
                    return new T().Initialize(u);
                });
            }

            return this;

        }

        /// <summary>
        /// Transforms the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <param name="translator">The response message translator.</param>
        public async Task Transform(HttpContext context, HttpResponseMessage responseMessage, AddressTranslator translator)
        {

            if (responseMessage != null)
            {
                var contentType = responseMessage.ContentType();
                if (contentType != null)
                {


                    if (_items.TryGetValue(contentType, out var item))
                    {
                        var t = item(translator);
                        string message = await t.Transform(context, responseMessage);

                        var req = context.Request;
                        var _current = new Url(req.Scheme, req.Host.Host, req.Host.Port.HasValue ? req.Host.Port.Value : 80)
                            .AppendPathSegments(translator.QuerySource);

                        message = message
                            .Replace($"\"url\":\"{translator.QuerySource}/swagger/",
                                     $"\"url\":\"{_current}/swagger/"
                                     )
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

        private Dictionary<string, Func<AddressTranslator, ProxyTransformResponse>> _items;

    }

}
