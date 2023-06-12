//using System.Reflection.PortableExecutable;
//using System.Xml.Linq;
//using Bb.Models;
//using Newtonsoft.Json.Linq;
//using Microsoft.Extensions.Primitives;

//namespace Bb.Services
//{



//    public class ApiKeyReferential
//    {

//        public ApiKeyReferential()
//        {
//            this._apiKeyReferential = new Dictionary<string, ApiKeyReferentialDataItem>();
//            _file = Path.Combine(Environment.CurrentDirectory, "apikeysettings.json");
//        }

//        public ApiKeyReferential(ApiKeyReferentialDatas datas) : this()
//        {

//            this.Datas = datas;

//            if (!string.IsNullOrEmpty(this.Datas.ApiKeyName))
//                datas.ApiKeyName = this.Datas.ApiKeyName;

//            foreach (var item in datas.Items)
//                this._apiKeyReferential.Add(item.ApiKey, item);

//        }

//        /// <summary>
//        /// evaluate if the header contains aprikey.
//        /// </summary>
//        /// <param name="headers">hearder that contains the api key</param>
//        /// <param name="roles">role to map</param>
//        /// <returns>if the result is null, it is ok and you can see roles in the out variable</returns>
//        public int? Evaluate(IHeaderDictionary headers, out HashSet<RoleEnum> roles)
//        {

//            roles = new HashSet<RoleEnum>();

//            if (this._apiKeyReferential.Count == 0)
//            {
//                roles = new HashSet<RoleEnum>() { RoleEnum.Administrator };
//                return null;
//            }

//            if (headers.TryGetValue(ApiKeyName, out StringValues extractedApiKey))
//            {


//                try
//                {
//                    _lock.AcquireReaderLock(3000);

//                    if (this._apiKeyReferential.TryGetValue(extractedApiKey, out ApiKeyReferentialDataItem? roleOut))
//                    {
//                        foreach (var item in roleOut.Roles)
//                            roles.Add(item);
//                        return null;

//                    }

//                }
//                finally
//                {
//                    _lock.ReleaseReaderLock();
//                }

//            }

//            return 401;

//        }

//        /// <summary>
//        /// Add new api key
//        /// </summary>
//        /// <param name="key"></param>
//        /// <param name="roles"></param>
//        public void Add(string key, string identityName, params RoleEnum[] roles)
//        {

//            try
//            {

//                _lock.AcquireWriterLock(3000);

//                if (!_apiKeyReferential.TryGetValue(key, out var roleValue))
//                {
//                    roleValue = new ApiKeyReferentialDataItem(key, identityName, roles);
//                    _apiKeyReferential.Add(ApiKeyName, roleValue);
//                    Datas.Items.Add(roleValue);
//                }
//                else
//                {
//                    foreach (var item in roles)
//                        roleValue.Roles.Add(item);
//                }

//                Save();

//            }
//            finally
//            {
//                _lock.ReleaseWriterLock();
//            }

//        }

//        /// <summary>
//        /// Remove specified roles
//        /// </summary>
//        /// <param name="key">api key</param>
//        /// <param name="roles">roles to removes</param>
//        /// <remarks>if the list of role is empty, the api key is removed.</remarks>
//        public void Remove(string key, params RoleEnum[] roles)
//        {

//            if (_apiKeyReferential.TryGetValue(key, out var roleValue))
//            {

//                try
//                {

//                    _lock.AcquireWriterLock(3000);

//                    foreach (var item in roles)
//                        roleValue.Roles.Remove(item);

//                    if (roleValue.Roles.Count == 0)
//                        _apiKeyReferential.Remove(key);

//                    Save();

//                }
//                finally
//                {
//                    _lock.ReleaseWriterLock();
//                }

//            }


//        }

//        /// <summary>
//        /// Remove api key
//        /// </summary>
//        /// <param name="key">api key to remove</param>
//        public void Remove(string key)
//        {

//            try
//            {
//                _lock.AcquireWriterLock(3000);

//                if (_apiKeyReferential.ContainsKey(key))
//                    _apiKeyReferential.Remove(key);

//                Save();

//            }
//            finally
//            {
//                _lock.ReleaseWriterLock();
//            }


//        }

//        private void Save()
//        {
//            var datas = new JObject(new JProperty("apikeys", JObject.FromObject(Datas)));
//            string payload = datas.ToString(Newtonsoft.Json.Formatting.Indented);
//            _file.Save(payload);
//        }

//        /// <summary>
//        /// Key must be specified in the headers
//        /// </summary>
//        public string ApiKeyName { get; set; } = "X-API-KEY";

//        internal ApiKeyReferentialDatas Datas { get; }


//        private Dictionary<string, ApiKeyReferentialDataItem> _apiKeyReferential;
//        private readonly string _file;
//        private volatile ReaderWriterLock _lock = new ReaderWriterLock();

//    }

//}
