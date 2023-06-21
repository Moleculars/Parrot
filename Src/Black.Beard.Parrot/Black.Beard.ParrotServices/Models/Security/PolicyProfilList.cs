using Bb.ComponentModel.Attributes;
using Bb.Expressions;

namespace Bb.Models.Security
{

    [ExposeClass (Context = Constants.Models.Configuration, ExposedType = typeof(PolicyProfilList), LifeCycle = IocScopeEnum.Singleton)]
    public class PolicyProfilList : ConfigurationBase
    {

        public PolicyProfilList()
        {
            this.Profils = new List<PolicyProfil>();
        }

        public List<PolicyProfil> Profils { get; }
    
    }


}
