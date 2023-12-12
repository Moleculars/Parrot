namespace Bb.Models.Security
{
    public class PolicyProfil
    {

        public PolicyProfil()
        {
            Routes = new List<PolicyProfilRoute>();
        }

        public string Name { get; set; }

        public List<PolicyProfilRoute> Routes { get; set; }

    }


}
