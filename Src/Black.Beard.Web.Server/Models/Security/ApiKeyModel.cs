namespace Bb.Models.Security
{
    public class ApiKeyModel
    {

        public ApiKeyModel()
        {
            Contracts = new List<string>();
            Claims = new List<KeyValuePair<string, string>>();
        }

        public string Owner { get; set; }

        public string Key { get; set; }

        public bool Admin { get; set; }

        public List<string> Contracts { get; set; }

        public List<KeyValuePair<string, string>> Claims { get; set; }

        public ApiKey CreateFrom()
        {

            var m = new ApiKey()
            {
                Id = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Key = this.Key,
                Owner = this.Owner,
                Admin = this.Admin
            };

            m.Contracts.AddRange(this.Contracts);
            m.Claims.AddRange(this.Claims);

            return m;

        }


    }


}
