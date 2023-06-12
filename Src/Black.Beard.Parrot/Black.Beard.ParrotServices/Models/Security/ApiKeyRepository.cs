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
        /// Delete an item of the referential
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        public ApiResult[] Del(ApiKeyModel[] datas)
        {

            List<ApiResult> results = new List<ApiResult>(datas.Length);

            lock (ApiKeyList._lock)
            {

                foreach (var data in datas)
                {

                    ApiResult r = new ApiResult() { Key = data.Key };

                    var k = data.Key;

                    var item = ApiKeyList
                        .Items
                        .Where(c => c.Key == k)
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
