using Bb.Projects;
using Bb.Services;
using System.Text;

namespace Bb.Middlewares.ReversProxy
{

    /// <summary>
    /// Class bas for implement transformation
    /// </summary>
    public abstract class ProxyTransformResponse
    {

        /// <summary>
        /// Initializes the instance of the transformer.
        /// </summary>
        /// <param name="targetUri">The target URI.</param>
        /// <param name="aliasUri">The alias URI.</param>
        /// <returns></returns>
        public ProxyTransformResponse Initialize(AddressTranslator translator)
        {
            _translator = translator;
            return this;
        }

        internal async Task<string> Transform(HttpContext context, HttpResponseMessage responseMessage)
        {

            var content = await responseMessage.Content.ReadAsByteArrayAsync();
            var stringContent = Encoding.UTF8.GetString(content);

            var newStringContent = Transform(context, stringContent);

            await Task.Yield();
            return newStringContent;

        }

        /// <summary>
        /// Transforms the specified payload.
        /// </summary>
        /// <param name="payload">The payload.</param>
        /// <returns></returns>
        protected abstract string Transform(HttpContext context, string payload);

        protected AddressTranslator _translator;

    }

}
