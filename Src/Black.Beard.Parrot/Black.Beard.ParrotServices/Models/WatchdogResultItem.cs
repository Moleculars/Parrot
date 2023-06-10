using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS8618, CS1591
namespace Bb.Mock
{

    public class WatchdogResultItem
    {


        public WatchdogResultItem()
        {
            
        }


        public WatchdogResultItem(string name, string value, string description = null)
        {
            this.Name = name;   
            this.Value = value;
            this.Description = description;
        }

        public String Name { get; set; }

        public String Value { get; set; }

        public String Description { get; set; }

    }

}