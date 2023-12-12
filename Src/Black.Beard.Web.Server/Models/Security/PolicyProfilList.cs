using Bb.ComponentModel.Attributes;
//using Oldtonsoft.Json.Linq;

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

        /// <summary>
        /// Saves the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        internal void Save(string filename)
        {            
            filename.SerializeAndSaveConfiguration(this);
        }

    }


}
