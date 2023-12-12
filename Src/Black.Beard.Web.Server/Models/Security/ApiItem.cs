namespace Bb.Models.Security
{
    public class ApiItem
    {

        public Guid Id { get; set; }

        public string Owner { get; set; }

        public string Key { get; set; }

        public DateTime Created { get; set; }

        public bool Admin { get; set; }


    }


}
