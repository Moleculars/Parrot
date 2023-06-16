using Bb.ComponentModel.Attributes;

namespace Bb.Models.Security
{

    [ExposeClass (Context = "Configuration", ExposedType = typeof(PolicyProfilList), LifeCycle = IocScopeEnum.Singleton)]
    public class PolicyProfilList : List<PolicyProfil>
    {



    }


}
