using Microsoft.AspNetCore.Mvc;

#pragma warning disable CS8618, CS1591
namespace Bb.Mock
{


    /// <summary>
    /// Watchdog result
    /// </summary>
    public class WatchdogResult
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="WatchdogResult"/> class.
        /// </summary>
        public WatchdogResult()
        {
            Infos = new List<WatchdogResultItem>();

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WatchdogResult"/> class.
        /// </summary>
        /// <param name="items">List of items</param>
        public WatchdogResult(params WatchdogResultItem[] items) : this()
        {
            Infos = new List<WatchdogResultItem>(items);
        }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        public List<WatchdogResultItem> Infos { get; set; }

    }
}