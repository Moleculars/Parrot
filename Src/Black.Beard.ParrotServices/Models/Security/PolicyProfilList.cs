using Bb.ComponentModel.Attributes;
using Oldtonsoft.Json.Linq;

namespace Bb.Models.Security
{

    [ExposeClass (Context = Constants.Models.Configuration, ExposedType = typeof(PolicyProfilList), LifeCycle = IocScopeEnum.Singleton)]
    public class PolicyProfilList
    {
                
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyProfilList"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public PolicyProfilList()
        {
            this.Profils = new List<PolicyProfil>();
        }

        public static PolicyProfilList New(List<PolicyModel> self)
        {

            PolicyProfilList _list = new PolicyProfilList();

            foreach (var item in self)
                _list.Profils.Add(item.GetModel());

            return _list;

        }

        public List<PolicyProfil> Profils { get; }
    
        internal void Save(string filename)
        {
            var datas = new JObject(new JProperty(nameof(PolicyProfilList), JToken.FromObject(this)));
            filename.Save(datas.ToString());
        }

    }


}
