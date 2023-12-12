
namespace Bb.Models.Security
{


    public class ApiKey
    {

        public ApiKey()
        {
            Contracts = new List<string>();
            Claims = new List<KeyValuePair<string, string>>();
        }

        public Guid Id { get; set; }

        public string Owner { get; set; }

        public string Key { get; set; }

        public DateTime Created { get; set; }

        public bool Admin { get; set; }

        public List<string> Contracts { get; set; }

        public List<KeyValuePair<string, string>> Claims { get; set; }

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
            this.Claims.Clear();
            this.Claims.AddRange(data.Claims);
            this.Admin = data.Admin;
            return this;
        }

        public ApiItem GetItemForList()
        {

            return new ApiItem()
            {
                Id = this.Id,
                Owner = this.Owner,
                Key = this.Key,
                Created = this.Created,
                Admin = this.Admin,
            };

        }


    }


}
