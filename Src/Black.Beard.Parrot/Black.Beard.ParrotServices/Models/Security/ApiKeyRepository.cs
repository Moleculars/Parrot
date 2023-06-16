using Bb.Json.Jslt.CustomServices.MultiCsv;
using System.Reflection;

namespace Bb.Models.Security
{

    internal class ApiKeyRepository : IApiKeyRepository
    {

        static ApiKeyRepository()
        {
            var currentAssembly = Assembly.GetAssembly(typeof(Program));
            var d = Path.GetDirectoryName(currentAssembly.Location);
            _file = Path.Combine(d, "apikeysettings.json");
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="apiKeyConfiguration"></param>
        public ApiKeyRepository(ApiKeyConfiguration apiKeyConfiguration)
        {
            ApiKeyList = apiKeyConfiguration;
            ApiKeyList.Filename = _file;
        }

        /// <summary>
        /// return the list of api key
        /// </summary>
        /// <returns></returns>
        public ApiItem[] GetList(int page, int count)
        {

            var items = ApiKeyList
                        .Items
                        .Skip(page)
                        .Take(count)
                        .Select(c => c.GetItemForList())
                        .ToArray();

            return items;

        }

        /// <summary>
        /// return the count of pages
        /// </summary>
        /// <returns></returns>
        public int GetPageCount(int count)
        {

            if (count == 0)
                count = 10;

            var items = ApiKeyList.Items.Count;

            var pages = 0;
            if (items > 0) 
                pages = (int)(items / count);

            if (items % count > 0)
                pages++;

            return pages;

        }

        /// <summary>
        /// return the <see cref="T:ApiKey"/> specified by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ApiKey GetByKey(string key)
        {

            var items = ApiKeyList
                      .Items
                      .Where(c => c.Key == key)
                      .FirstOrDefault();

            return items;

        }

        /// <summary>
        /// return the <see cref="T:ApiKey"/> specified by id
        /// </summary>
        /// <param name="id">id of the api key</param>
        /// <returns></returns>
        public ApiKey GetById(Guid id)
        {

            var items = ApiKeyList
                  .Items
                  .Where(c => c.Id == id)
                  .FirstOrDefault();

            return items;

        }


        /// <summary>
        /// Update the referential
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public ApiResult[] Set(ApiKeyModel[] datas)
        {

            List<ApiResult> results = new List<ApiResult>(datas.Length);

            lock (ApiKeyList._lock)
            {

                foreach (var data in datas)
                {

                    ApiResult r = new ApiResult() { Key = data.Key, Info = "None" };

                    var k = data.Key;

                    var item = ApiKeyList
                        .Items
                        .Where(c => c.Key == k)
                        .FirstOrDefault();

                    if (item == null)
                    {

                        var m = data.CreateFrom()
                                    .SetAdmin(ApiKeyList.Items.Count == 0);

                        ApiKeyList.Items.Add(m);
                        r.Success = true;
                        r.Info = "Added";

                    }
                    else
                    {
                        item.Update(data);
                        r.Info = "Updated";
                        r.Success = true;
                    }

                    results.Add(r);

                }

                ApiKeyList.Save();

            }

            return results.ToArray();

        }


        /// <summary>
        /// remove items specified by id
        /// </summary>
        /// <param name="datas"><see cref="T:Guid"/></param>
        /// <returns><see cref="T:ApiResult"/></returns>
        public ApiResult[] DelById(Guid[] datas)
        {

            List<ApiResult> results = new List<ApiResult>(datas.Length);

            lock (ApiKeyList._lock)
            {

                foreach (var data in datas)
                {

                    ApiResult r = new ApiResult() { Key = data.ToString() };

                    var item = ApiKeyList
                        .Items
                        .Where(c => c.Id == data)
                        .FirstOrDefault();

                    if (item != null)
                    {
                        if (item.Admin)
                        {
                            var admin = ApiKeyList
                                .Items
                                .Where(c => c != item && c.Admin)
                                .Any();

                            if (admin)
                            {
                                ApiKeyList.Items.Remove(item);
                                r.Success = true;
                                r.Info = "Removed";
                            }
                            else
                            {
                                r.Success = false;
                                r.Info = "Can't be Removed, because is the last administrator.";
                            }
                        }
                        else
                        {
                            ApiKeyList.Items.Remove(item);
                            r.Success = true;
                            r.Info = "Removed";
                        }

                    }
                    else
                    {
                        r.Info = "Not found";
                        r.Success = true;
                    }

                }

                ApiKeyList.Save();

            }

            return results.ToArray();

        }


        /// <summary>
        /// remove items specified by key
        /// </summary>
        /// <param name="datas">list of api key <see cref="T:string"/> to remove.</param>
        /// <returns><see cref="T:ApiResult"/></returns>
        public ApiResult[] DelByKey(string[] datas)
        {

            List<ApiResult> results = new List<ApiResult>(datas.Length);

            lock (ApiKeyList._lock)
            {

                foreach (var data in datas)
                {

                    ApiResult r = new ApiResult() { Key = data.ToString() };

                    var item = ApiKeyList
                        .Items
                        .Where(c => c.Key == data)
                        .FirstOrDefault();

                    if (item != null)
                    {
                        if (item.Admin)
                        {
                            var admin = ApiKeyList
                                .Items
                                .Where(c => c != item && c.Admin)
                                .Any();

                            if (admin)
                            {
                                ApiKeyList.Items.Remove(item);
                                r.Success = true;
                                r.Info = "Removed";
                            }
                            else
                            {
                                r.Success = false;
                                r.Info = "Can't be Removed, because is the last administrator.";
                            }
                        }
                        else
                        {
                            ApiKeyList.Items.Remove(item);
                            r.Success = true;
                            r.Info = "Removed";
                        }

                    }
                    else
                    {
                        r.Info = "Not found";
                        r.Success = true;
                    }

                }

                ApiKeyList.Save();

            }

            return results.ToArray();

        }


        /// <summary>
        /// return the value of the apikey
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public ApiKey GetApiKeyFromHeaders(HttpContext context) => GetApiKeysFromHeaders(context)?.FirstOrDefault();


        public IEnumerable<ApiKey> GetApiKeysFromHeaders(HttpContext context)
        {
            if (ApiKeyList?.Items == null || !TryGetApiKeyIdFromHeaders(context, out var apiKeyInHeaders))
                return new List<ApiKey>();

            return ApiKeyList.Items?.Where(apiKey =>
                string.Equals(apiKey.Key, apiKeyInHeaders, StringComparison.OrdinalIgnoreCase));
        }


        public bool TryGetApiKeyIdFromHeaders(HttpContext context, out string apiKeyId)
        {
            var result = context.Request.Headers.TryGetValue(ApiKeyList.ApiHeader, out var apiKeyIdInHeaders);
            apiKeyId = apiKeyIdInHeaders;
            return result;
        }


        public override bool Equals(object? obj)
        {
            return obj is ApiKeyRepository repository &&
                   EqualityComparer<ApiKeyConfiguration>.Default.Equals(ApiKeyList, repository.ApiKeyList);
        }



        public ApiKeyConfiguration ApiKeyList { get; }

        private readonly static string _file;

    }

}
