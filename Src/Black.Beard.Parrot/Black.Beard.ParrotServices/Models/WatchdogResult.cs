using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS8618, CS1591
namespace Bb.Mock
{
    public class WatchdogResult
    {

        public WatchdogResult()
        {
            Items = new List<WatchdogResultItem>();

        }

        public WatchdogResult(params WatchdogResultItem[] items) : this()
        {
            Items = new List<WatchdogResultItem>(items);
        }

        public List<WatchdogResultItem> Items { get; set; }

    }
}