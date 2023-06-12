using Bb.Codings;

namespace Bb.Models.Security
{

    public class ApiKey
    {

        public ApiKey()
        {
            Contracts = new List<string>();
        }

        public Guid Id { get; set; }

        public string Owner { get; set; }

        public string Key { get; set; }

        public DateTime Created { get; set; }

        public bool Admin { get; set; }

        public List<string> Contracts { get; set; }

        public ApiKey SetAdmin(bool isAdmin)
        {
            this.Admin = isAdmin;
            if (isAdmin)
            {
                this.Contracts.Clear();
            }
            return this;
        }

        public ApiKey Update(ApiKeyModel data)
        {
            this.Contracts.Clear();
            this.Contracts.AddRange(data.Contracts);
            this.Admin = data.Admin;
            return this;
        }

    }



    public class ApiKeyModel
    {

        public ApiKeyModel()
        {
            Contracts = new List<string>();
        }

        public string Owner { get; set; }

        public string Key { get; set; }
        
        public bool Admin { get;  set; }

        public List<string> Contracts { get; set; }

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

            return m;

        }


    }


}
