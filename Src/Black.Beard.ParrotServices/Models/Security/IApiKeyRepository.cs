namespace Bb.Models.Security
{

    /// <summary>
    /// Contract for Api key repository
    /// </summary>
    public interface IApiKeyRepository
    {

        /// <summary>
        /// return the list of api key
        /// </summary>
        /// <returns></returns>
        public ApiItem[] GetList(int page, int count);

        /// <summary>
        /// return the count of pages
        /// </summary>
        /// <returns></returns>
        public int GetPageCount(int count);

        /// <summary>
        /// return the <see cref="T:ApiKey"/> specified by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ApiKey GetByKey(string key);

        /// <summary>
        /// return the <see cref="T:ApiKey"/> specified by id
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ApiKey GetById(Guid key);


        /// <summary>
        /// Add or update <see cref="T:ApiKey"/>
        /// </summary>
        /// <param name="datas"></param>
        /// <returns><see cref="T:ApiResult"/></returns>
        public ApiResult[] Set(ApiKeyModel[] datas);

        /// <summary>
        /// remove items specified by ids
        /// </summary>
        /// <param name="datas"><see cref="T:Guid"/></param>
        /// <returns><see cref="T:ApiResult"/></returns>
        public ApiResult[] DelById(Guid[] datas);

        /// <summary>
        /// remove items specified by key
        /// </summary>
        /// <param name="datas">list of api key <see cref="T:string"/> to remove.</param>
        /// <returns><see cref="T:ApiResult"/></returns>
        public ApiResult[] DelByKey(string[] datas);

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
