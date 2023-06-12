namespace Bb.Models.Security
{

    public interface IApiKeyRepository
    {

        /// <summary>
        /// Add or update
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public ApiResult[] Set(ApiKeyModel[] datas);

        /// <summary>
        /// remove items
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public ApiResult[] Del(ApiKeyModel[] datas);

        /// <summary>
        /// return the apikey for the request context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public ApiKey GetApiKeyFromHeaders(HttpContext context);

        /// <summary>
        /// return the apikey list for the request context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<ApiKey> GetApiKeysFromHeaders(HttpContext context);

        /// <summary>
        /// Test if the api key exists in the referential
        /// </summary>
        /// <param name="context"></param>
        /// <param name="apiKeyId"></param>
        /// <returns></returns>
        public bool TryGetApiKeyIdFromHeaders(HttpContext context, out string apiKeyId);

        /// <summary>
        /// Return the Api key referential
        /// </summary>
        public ApiKeyConfiguration ApiKeyList { get; }

    }

}
